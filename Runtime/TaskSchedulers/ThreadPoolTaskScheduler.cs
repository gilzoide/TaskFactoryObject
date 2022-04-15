using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public class ThreadPoolTaskScheduler : TaskScheduler
    {
        public override int MaximumConcurrencyLevel => _maximumConcurrency;

        [ThreadStatic] private static bool _currentThreadIsProcessingItems;
        private readonly LinkedList<Task> _tasks = new LinkedList<Task>();
        private readonly int _maximumConcurrency = int.MaxValue;
        private readonly CancellationToken _cancellationToken;
        private int _delegatesQueuedOrRunning = 0;

        public ThreadPoolTaskScheduler(int maximumConcurrency, CancellationToken cancellationToken)
        {
            _maximumConcurrency = maximumConcurrency;
            _cancellationToken = cancellationToken;
        }

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

        private void AddWorkItemToThreadPool()
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                _currentThreadIsProcessingItems = true;

                while (!_cancellationToken.IsCancellationRequested)
                {
                    Task task;
                    lock (_tasks)
                    {
                        if (!_tasks.TryRemoveFirst(out task))
                        {
                            _delegatesQueuedOrRunning--;
                            break;
                        }
                    }

                    TryExecuteTask(task);
                }
                
                _currentThreadIsProcessingItems = false;
            }, null);
        }
    }
}