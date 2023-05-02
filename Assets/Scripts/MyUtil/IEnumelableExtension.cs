using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace MyUtil
{
    public static class IEnumelableExtension
    {
        public static T RandomGet<T>(this IEnumerable<T> list)
        {
            var rand = new Random();
            return list.ElementAt(rand.Next(list.Count()));
        }
    }

}
