using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;

namespace Gilzoide.TaskFactoryObject
{
    [CreateAssetMenuAttribute(fileName = "TaskFactory", menuName = "Task Scheduler/TaskFactory")]
    public class TaskFactoryScriptableObject : ScriptableObject, ITaskFactoryObject
    {
        public TaskFactoryConfig TaskFactoryConfig;

        public TaskFactoryConfig FactoryConfig => TaskFactoryConfig;
        public TaskFactory Factory { get; private set; }

        private CancellationTokenSource _cancellationTokenSource;
        
        void OnEnable()
        {
            CreateFactoryIfNeeded();
        }

        void OnDisable()
        {
            DestroyFactory();
        }

        public void CreateFactoryIfNeeded()
        {
            if (Factory != null)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            Factory = TaskFactoryConfig.CreateFactory(_cancellationTokenSource.Token);
        }

        public void DestroyFactory()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
            }
            Factory = null;
        }
    }
}