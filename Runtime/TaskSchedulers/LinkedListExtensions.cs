using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gilzoide.TaskFactoryObject.TaskSchedulers
{
    public static class LinkedListExtensions
    {
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
    }
}