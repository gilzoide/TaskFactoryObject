using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Gilzoide.TaskFactoryObject
{
    public class TaskFactoryComponent : MonoBehaviour, ITaskFactoryObject
    {
        public TaskFactoryConfig TaskFactoryConfig;
        
        public TaskFactoryConfig FactoryConfig => TaskFactoryConfig;
        public TaskScheduler Scheduler
        {
            get
            {
                if (_taskScheduler == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                    _taskScheduler = TaskFactoryConfig.CreateScheduler(_cancellationTokenSource.Token);
                }
                return _taskScheduler;
            }
        }
        public TaskFactory Factory
        {
            get
            {
                if (_taskFactory == null)
                {
                    TaskScheduler scheduler = Scheduler;
                    _taskFactory = TaskFactoryConfig.CreateFactory(scheduler, _cancellationTokenSource.Token);
                }
                return _taskFactory;
            }
        }

        private CancellationTokenSource _cancellationTokenSource;
        private TaskScheduler _taskScheduler;
        private TaskFactory _taskFactory;

        void OnDestroy()
        {
            DestroyFactory();
        }

        public void DestroyFactory()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
            _taskFactory = null;
            _taskScheduler = null;
        }
    }
}