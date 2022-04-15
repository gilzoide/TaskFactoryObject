using System;
using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject
{
    public interface ITaskFactoryObject
    {
        public TaskFactory Factory { get; }
        public TaskFactoryConfig FactoryConfig { get; }
    }
}