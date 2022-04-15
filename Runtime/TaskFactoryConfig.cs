using System;
using System.Threading;
using System.Threading.Tasks;
using Gilzoide.TaskFactoryObject.TaskSchedulers;

namespace Gilzoide.TaskFactoryObject
{
    [Serializable]
    public class TaskFactoryConfig
    {
        public enum ThreadingMode
        {
            MainThread,
            ThreadPool,
        }

        public int MaximumConcurrency = 0;
        public ThreadingMode Mode;
        public TaskCreationOptions DefaultTaskCreationOptions = TaskCreationOptions.None;
        public TaskContinuationOptions DefaultTaskContinuationOptions = TaskContinuationOptions.None;

        public TaskScheduler CreateScheduler(CancellationToken cancellationToken = default)
        {
            int maxConcurrency = MaximumConcurrency > 0 ? MaximumConcurrency : int.MaxValue;
            switch (Mode)
            {
                case ThreadingMode.MainThread:
                    return new SyncTaskScheduler(maxConcurrency, cancellationToken);

                case ThreadingMode.ThreadPool:
                    return new ThreadPoolTaskScheduler(maxConcurrency, cancellationToken);

                default:
                    throw new ArgumentOutOfRangeException(nameof(Mode), "Invalid threading mode");
            }
        }

        public TaskFactory CreateFactory(TaskScheduler taskScheduler, CancellationToken cancellationToken = default)
        {
            return new TaskFactory(
                cancellationToken,
                DefaultTaskCreationOptions,
                DefaultTaskContinuationOptions,
                taskScheduler
            );
        }

        public TaskFactory CreateFactory(CancellationToken cancellationToken = default)
        {
            return CreateFactory(CreateScheduler(cancellationToken), cancellationToken);
        }
    }
}