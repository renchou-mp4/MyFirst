using System;

namespace yxy
{
    public static class EnumHelper
    {
        public static string[] GetEnumNames<T>() where T : struct, Enum
        {
            return Enum.GetNames(typeof(T));
        }
    }
}
