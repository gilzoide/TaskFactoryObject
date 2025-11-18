using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public class ManagedThreadPoolTaskScheduler : TaskScheduler
    {
        public override int MaximumConcurrencyLevel => _maximumConcurrency;

        [ThreadStatic] private static bool _currentThreadIsProcessingItems;
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly int _maximumConcurrency;
        private readonly CancellationToken _cancellationToken;
        private int _delegatesQueuedOrRunning = 0;

        public ManagedThreadPoolTaskScheduler(int? maximumConcurrency, CancellationToken cancellationToken = default)
        {
            _maximumConcurrency = maximumConcurrency ?? Environment.ProcessorCount;
            _cancellationToken = cancellationToken;
        }

        public ManagedThreadPoolTaskScheduler() : this(null) {}

        #region TaskScheduler implementation

        protected override void QueueTask(Task task)
        {
            if (_cancellationToken.IsCancellationRequested)
            {
                throw new NotSupportedException("Cannot queue task to a canceled scheduler");
            }

            lock (_tasks)
            {
                _tasks.AddLast(task);
                if (_delegatesQueuedOrRunning < _maximumConcurrency)
                {
                    _delegatesQueuedOrRunning++;
                    AddWorkItemToThreadPool();
                }
            }
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

        #endregion

        private void AddWorkItemToThreadPool()
        {
            ThreadPool.QueueUserWorkItem(static state =>
            {
                ManagedThreadPoolTaskScheduler self = (ManagedThreadPoolTaskScheduler) state;
                _currentThreadIsProcessingItems = true;

                while (!self._cancellationToken.IsCancellationRequested)
                {
                    Task task;
                    lock (self._tasks)
                    {
                        if (!self._tasks.TryRemoveFirst(out task))
                        {
                            self._delegatesQueuedOrRunning--;
                            break;
                        }
                    }

                    self.TryExecuteTask(task);
                }
                
                _currentThreadIsProcessingItems = false;
            }, this);
        }
    }
}