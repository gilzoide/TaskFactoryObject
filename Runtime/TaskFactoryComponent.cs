using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Gilzoide.TaskFactoryObject
{
    public class TaskFactoryComponent : MonoBehaviour, ITaskFactoryObject
    {
        public enum CreateLifecycle
        {
            Awake,
            Start,
            OnEnable,
        }

        public enum DestroyLifecycle
        {
            OnDestroy,
            OnDisable,
        }

        public TaskFactoryConfig TaskFactoryConfig;
        public CreateLifecycle CreateFactoryLifecycle;
        public DestroyLifecycle DestroyFactoryLifecycle;

        public TaskFactoryConfig FactoryConfig => TaskFactoryConfig;
        public TaskFactory Factory { get; private set; }

        private CancellationTokenSource _cancellationTokenSource;
        
        void Awake()
        {
            if (CreateFactoryLifecycle == CreateLifecycle.Awake)
            {
                CreateFactoryIfNeeded();
            }
        }

        void Start()
        {
            if (CreateFactoryLifecycle == CreateLifecycle.Start)
            {
                CreateFactoryIfNeeded();
            }
        }

        void OnEnable()
        {
            if (CreateFactoryLifecycle == CreateLifecycle.OnEnable)
            {
                CreateFactoryIfNeeded();
            }
        }

        void OnDisable()
        {
            if (DestroyFactoryLifecycle == DestroyLifecycle.OnDisable)
            {
                DestroyFactory();
            }
        }

        void OnDestroy()
        {
            if (DestroyFactoryLifecycle == DestroyLifecycle.OnDestroy)
            {
                DestroyFactory();
            }
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