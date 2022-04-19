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
        private readonly int _maximumConcurrency;
        private readonly CancellationToken _cancellationToken;
        private readonly TaskScheduler _runnerTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

        public SyncTaskScheduler(int? maximumConcurrency, CancellationToken cancellationToken = default)
        {
            _maximumConcurrency = maximumConcurrency ?? int.MaxValue;
            _cancellationToken = cancellationToken;
        }

        public SyncTaskScheduler() : this(null) {}
    
        #region TaskScheduler implementation

        protected override void QueueTask(Task task)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new NotSupportedException("Cannot queue task to a canceled scheduler");
            }

            RunAsync(async () =>
            {
                _tasks.AddLast(task);

                if (_tasks.Count == 1)
                {
                    await WorkerLoopAsync();
                }
            });
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
            return TaskSchedulerUtils.ReturnIfMonitorEnter(_tasks);
        }

        #endregion

        private async void RunAsync(Action action)
        {
            try
            {
                await Task.Factory.StartNew(action, _cancellationToken, default, _runnerTaskScheduler);
            }
            catch (OperationCanceledException) {}
        }

        private async Task WorkerLoopAsync()
        {
            while (!_cancellationToken.IsCancellationRequested)
            {
                await Task.Yield();

                for (int i = 0; i < _maximumConcurrency; i++)
                {
                    _cancellationToken.ThrowIfCancellationRequested();

                    if (!_tasks.TryRemoveFirst(out Task task))
                    {
                        return;
                    }
                    if (!TryExecuteTask(task))
                    {
                        break;
                    }
                }
            }
        }
    }
}