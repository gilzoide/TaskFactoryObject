using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject
{
    public interface ITaskFactoryObject
    {
        public TaskFactoryConfig FactoryConfig { get; }
        public TaskScheduler Scheduler { get; }
        public TaskFactory Factory { get; }
    }
}