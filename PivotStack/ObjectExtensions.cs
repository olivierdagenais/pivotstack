using System;

namespace PivotStack
{
    public static class ObjectExtensions
    {
        public static T FromDBNull<T>(this object data)
        {
            if (data == DBNull.Value)
            {
                return default (T);
            }
            return (T)data;
        }
    }
}
