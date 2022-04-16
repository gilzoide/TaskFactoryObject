using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public class OwnThreadsTaskScheduler : TaskScheduler
    {
        public override int MaximumConcurrencyLevel => _maximumConcurrency;
        public IReadOnlyList<Thread> Threads => _threads;

        [ThreadStatic] private static bool _currentThreadIsProcessingItems;
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly int _maximumConcurrency;
        private readonly CancellationToken _cancellationToken;
        private readonly Thread[] _threads;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(0);

        public OwnThreadsTaskScheduler(int? maximumConcurrency, CancellationToken cancellationToken = default,
            ThreadOptions threadOptions = default)
        {
            _maximumConcurrency = maximumConcurrency ?? Environment.ProcessorCount;
            _cancellationToken = cancellationToken;
            _threads = new Thread[_maximumConcurrency];
            for (int i = 0; i < _maximumConcurrency; i++)
            {
                var thread = new Thread(WorkerLoop);
                thread.SetupWithOptions(threadOptions, i);
                thread.Start();
                _threads[i] = thread;
            }
        }

        public OwnThreadsTaskScheduler() : this(null) {}

        protected override void QueueTask(Task task)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new NotSupportedException("Cannot queue task to a canceled scheduler");
            }

            lock (_tasks)
            {
                _tasks.AddLast(task);
            }
            _semaphore.Release();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return !_cancellationToken.IsCancellationRequested
                && _currentThreadIsProcessingItems
                && (!taskWasPreviouslyQueued || TryDequeue(task))
                && TryExecuteTask(task);
        }

        protected override bool TryDequeue(Task task)
        {
            lock (_tasks)
            {
                return _tasks.Remove(task);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return TaskSchedulerUtils.ReturnIfMonitorEnter(_tasks);
        }

        private void WorkerLoop()
        {
            _currentThreadIsProcessingItems = true;

            while (!_cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _semaphore.Wait(_cancellationToken);
                    
                    Task task;
                    lock (_tasks)
                    {
                        if (!_tasks.TryRemoveFirst(out task))
                        {
                            continue;
                        }
                    }

                    TryExecuteTask(task);
                }
                catch (OperationCanceledException) {}
            }

            _currentThreadIsProcessingItems = false;
        }
    }
}