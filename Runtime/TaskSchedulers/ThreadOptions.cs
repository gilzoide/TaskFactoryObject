using System;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    [Serializable]
    public struct ThreadOptions
    {
        public bool useBackgroundThreads;
        public string threadNamePrefix;
    }
}