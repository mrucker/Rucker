using System;
using System.Linq;
using System.Collections.Generic;

namespace Data.Core
{
    public static class ListExtensions
    {
        /// <summary>
        /// Moves the item from the old index to the new index
        /// </summary>
        public static IList<T> MoveSingle<T>(this IList<T> list, Predicate<T> predicate, int newIndex)
        {
            var item = list.SingleOrDefault(i => predicate(i));

            if (item == null || item.Equals(default(T))) return list;

            var oldIndex = list.IndexOf(item);

            if (newIndex < 0) newIndex = list.Count - newIndex;
   
            list.RemoveAt(oldIndex);

            //if oldIndex was came before the newIndex within the list
            //then removing the oldIndex will have shifted the newIndex
            if (oldIndex < newIndex) newIndex--;

            if (list.Count <= newIndex)
            {
                //if the index is beyond the end of the list then stick the item on the end
                list.Add(item);
            }
            else
            {
                //if the index isn't beyond the end of the list then stick the item at the index
                list.Insert(newIndex, item);    
            }

            return list;
        }

        public static IList<T> RemoveSingle<T>(this IList<T> list, Predicate<T> predicate)
        {
            var item = list.SingleOrDefault(i => predicate(i));

            if (item == null || item.Equals(default(T))) return list;

            list.RemoveAt(list.IndexOf(item));

            return list;
        }
    }
}
