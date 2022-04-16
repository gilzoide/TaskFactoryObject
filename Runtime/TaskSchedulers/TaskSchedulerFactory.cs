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
            int? maxConcurrencyArg = maximumConcurrency > 0 ? maximumConcurrency : null;
            switch (type)
            {
                case TaskSchedulerType.MainThread:
                    return new SyncTaskScheduler(maxConcurrencyArg, cancellationToken);

                case TaskSchedulerType.ManagedThreadPool:
                    return new ManagedThreadPoolTaskScheduler(maxConcurrencyArg, cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException(nameof(type), "Invalid TaskSchedulerType");
            }
        }
    }
}