using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace INLINQ.Orc.Helpers
{
    internal static class NullableHelper
    {
        public static bool IsNullable(Type propertyInfoType)
        {
            bool isNullable =
                propertyInfoType == typeof(string) ||
                propertyInfoType.BaseType == typeof(Array) || 
                Nullable.GetUnderlyingType(propertyInfoType) != null;
            return isNullable;
        }
    }
}
