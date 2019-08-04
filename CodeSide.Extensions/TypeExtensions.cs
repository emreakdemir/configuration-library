using System;

namespace CodeSide.Extensions
{
    public static class TypeExtensions
    {
        public static Type GetUnderlyingType(this Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return Nullable.GetUnderlyingType(type) ?? type;
            
            return type;
        }
    }
}