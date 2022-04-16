using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public static class TaskSchedulerFactory
    {
        public static TaskScheduler Create(TaskSchedulerType type, int maximumConcurrency = int.MaxValue,
            CancellationToken cancellationToken = default)
        {
            if (maximumConcurrency <= 0)
            {
                maximumConcurrency = int.MaxValue;
            }

            switch (type)
            {
                case TaskSchedulerType.MainThread:
                    return new SyncTaskScheduler(maximumConcurrency, cancellationToken);

                case TaskSchedulerType.ManagedThreadPool:
                    return new ThreadPoolTaskScheduler(maximumConcurrency, cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Invalid TaskSchedulerType");
            }
        }
    }
}