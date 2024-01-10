using System;
using System.Collections.Generic;
using System.Threading;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public static class TaskSchedulerUtils
    {
        public static T ReturnIfMonitorEnter<T>(T value) where T : class
        {
            bool lockTaken = false;
            try
            {
                Monitor.TryEnter(value, ref lockTaken);
                if (lockTaken)
                {
                    return value;
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(value);
                }
            }
        }

        #region LinkedList extensions

        public static bool TryRemoveFirst<T>(this LinkedList<T> list, out T value)
        {
            if (list.First is LinkedListNode<T> node)
            {
                value = node.Value;
                list.RemoveFirst();
                return true;
            }

            value = default;
            return false;
        }

        #endregion

        #region Thread extensions

        public static void SetupWithOptions(this Thread thread, ThreadOptions options, int indexInPool)
        {
            thread.IsBackground = options.useBackgroundThreads;
            
            if (!string.IsNullOrEmpty(options.threadNamePrefix))
            {
                thread.Name = options.threadNamePrefix + "-" + indexInPool;
            }
        }

        #endregion
    }
}