# TaskFactoryObject
Brings configurable `MonoBehaviour` and `ScriptableObject` subclasses wrapping
[TaskFactory](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskfactory?view=netstandard-2.0)
objects, easing the creation and sharing of task factories with custom
schedulers in Unity.


## Installing the package
This package can be installed on Unity projects using the [Unity Package Manager](https://docs.unity3d.com/Manual/Packages.html).
Just [add a package using this repository URL](https://docs.unity3d.com/Manual/upm-ui-giturl.html):

```
https://github.com/gilzoide/TaskFactoryObject.git
```


## TaskSchedulers
The [TaskSchedulers module](Runtime/TaskSchedulers/) comes with 3
[TaskScheduler](https://docs.microsoft.com/en-us/dotnet/api/system.threading.tasks.taskscheduler?view=netstandard-2.0)
implementations:

- [SyncTaskScheduler](Runtime/TaskSchedulers/SyncTaskScheduler.cs): run tasks
  in the current `SynchronizationContext`, which by default is Unity's Main
  Thread. Default maximum concurrency is `int.MaxValue`, that is, process every
  queued task in a single frame.
- [ManagedThreadPoolTaskScheduler](Runtime/TaskSchedulers/ManagedThreadPoolTaskScheduler.cs):
  run tasks in the [Managed Thread Pool](https://docs.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool).
  Default maximum concurrency is `Environment.ProcessorCount`.
- [OwnThreadsTaskScheduler](Runtime/TaskSchedulers/OwnThreadsTaskScheduler.cs):
  creates it's own threads and run tasks on them. Uses a
  [SemaphoreSlim](https://docs.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim?view=netstandard-2.0)
  for sleeping threads until there is work to be done. Threads can have their
  name and background flag configured. Default maximum concurrency is
  `Environment.ProcessorCount`.


## TaskFactoryComponent
The [TaskFactoryComponent](Runtime/TaskFactoryComponent.cs) is a
`MonoBehaviour` subclass with configurations for the created `TaskFactory`.
It also lets one choose in what lifecycle events the factory will be created
and destroyed. The `TaskScheduler` used by the factory is tied to these
lifetimes, so that when the factory is destroyed, no more tasks can be
scheduled and pending ones will be dropped silently.

If `MaximumConcurrency` is 0 or a negative number, the created `TaskScheduler`
will use it's own default.

Usage example:
```cs
using System.Threading.Tasks;
using UnityEngine;
using Gilzoide.TaskFactoryObject;

public class SomeOtherScript : MonoBehaviour
{
    public TaskFactoryComponent FactoryComponent;

    async void Start()
    {
        TaskFactory taskFactory = FactoryComponent.Factory;
        await taskFactory.StartNew(() =>
        {
            Debug.Log("This runs in the configured TaskScheduler!");
        });
        Debug.Log("Done!");
    }
}
```

![](Extras~/TaskFactoryComponent.png)


## TaskFactoryScriptableObject
The [TaskFactoryScriptableObject](Runtime/TaskFactoryScriptableObject.cs) is a
`ScriptableObject` subclass with configurations for the created `TaskFactory`.
The factory is created on the `OnEnable` event and destroyed on the `OnDisable`
event of this scriptable object. When the factory is destroyed, no more tasks
can be scheduled and pending ones will be dropped silently.

If `MaximumConcurrency` is 0 or a negative number, the created `TaskScheduler`
will use it's own default.

Usage example:
```cs
using System.Threading.Tasks;
using UnityEngine;
using Gilzoide.TaskFactoryObject;

public class SomeOtherScript : MonoBehaviour
{
    public TaskFactoryScriptableObject FactorySO;

    async void Start()
    {
        TaskFactory taskFactory = FactorySO.Factory;
        await taskFactory.StartNew(() =>
        {
            Debug.Log("This runs in the configured TaskScheduler!");
        });
        Debug.Log("Done!");
    }
}
```

![](Extras~/TaskFactoryScriptableObject.png)


## TaskFactoryConfig
The [TaskFactoryConfig](Runtime/TaskFactoryConfig.cs) is a serializable class
with configurations for creating `TaskScheduler` and `TaskFactory` objects.
This is used by both `TaskFactoryComponent` and `TaskFactoryScriptableObject`.

Methods:
- `TaskScheduler CreateScheduler(CancellationToken cancellationToken = default)`:
  creates a new `TaskScheduler` with the defined configuration.
- `TaskFactory CreateFactory(CancellationToken cancellationToken = default)`:
  creates a new `TaskFactory` with the defined configuration. A `TaskScheduler`
  is also created via `CreateScheduler` and passed to it.
- `TaskFactory CreateFactory(TaskScheduler taskScheduler, CancellationToken cancellationToken = default)`:
  creates a new `TaskFactory` with the defined configuration, using a specific
  scheduler object.
