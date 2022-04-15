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
        public TaskFactory Factory { get; private set; }

        private CancellationTokenSource _cancellationTokenSource;
        
        void OnEnable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            Factory = TaskFactoryConfig.CreateFactory(_cancellationTokenSource.Token);
        }

        void OnDisable()
        {
            _cancellationTokenSource.Cancel();
            _cancellationTokenSource.Dispose();
            Factory = null;
        }
    }
}