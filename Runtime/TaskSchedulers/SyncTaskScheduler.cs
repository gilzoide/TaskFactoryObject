using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public class SyncTaskScheduler : TaskScheduler
    {
        public override int MaximumConcurrencyLevel => _maximumConcurrency;

        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly int _maximumConcurrency = int.MaxValue;
        private readonly CancellationToken _cancellationToken;
        private readonly TaskScheduler _runnerTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public SyncTaskScheduler(int maximumConcurrency, CancellationToken cancellationToken = default,
            TaskScheduler runnerScheduler = null)
        {
            _maximumConcurrency = maximumConcurrency;
            _cancellationToken = cancellationToken;
            _runnerTaskScheduler = runnerScheduler ?? TaskScheduler.FromCurrentSynchronizationContext();
            RunTasksAsyncLoop();
        }

        public SyncTaskScheduler()
        {
            RunTasksAsyncLoop();
        }
    
        protected override void QueueTask(Task task)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new NotSupportedException("Cannot queue task to a canceled scheduler");
            }

            RunAsync(() => { _tasks.AddLast(task); });
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        protected override bool TryDequeue(Task task)
        {
            return _tasks.Remove(task);
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(_tasks, ref lockTaken);
                if (lockTaken) return _tasks;
                else throw new NotSupportedException();
            }
            finally
            {
                if (lockTaken) Monitor.Exit(_tasks);
            }
        }

        private async void RunAsync(Action action)
        {
            try
            {
                await Task.Factory.StartNew(action, _cancellationToken, default, _runnerTaskScheduler);
            }
            catch (OperationCanceledException) {}
        }

        private async void RunTasksAsyncLoop()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                for (int i = 0; i < _maximumConcurrency && _tasks.TryRemoveFirst(out Task task); i++)
                {
                    if (!TryExecuteTask(task))
                    {
                        break;
                    }
                }
                await Task.Yield();
            }
        }
    }
}