using System;
using System.Threading;
using System.Threading.Tasks;
using Gilzoide.TaskFactoryObject.TaskSchedulers;

namespace Gilzoide.TaskFactoryObject
{
    [Serializable]
    public class TaskFactoryConfig
    {
        public int MaximumConcurrency = 0;
        public TaskSchedulerType SchedulerType;
        public TaskCreationOptions DefaultTaskCreationOptions = TaskCreationOptions.None;
        public TaskContinuationOptions DefaultTaskContinuationOptions = TaskContinuationOptions.None;
        public ThreadOptions OwnThreadsOptions;

        public TaskScheduler CreateScheduler(CancellationToken cancellationToken = default)
        {
            return TaskSchedulerFactory.Create(SchedulerType, MaximumConcurrency, cancellationToken, OwnThreadsOptions);
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