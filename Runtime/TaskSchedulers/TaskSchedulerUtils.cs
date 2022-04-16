using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
    }
}