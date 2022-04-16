using System;
using System.Threading;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    [Serializable]
    public struct ThreadOptions
    {
        public bool useBackgroundThreads;
        public string threadNamePrefix;
    }
}