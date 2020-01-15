using System;
using System.Linq;
#if !NET451
using System.Reflection;
#endif
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Extensions
{
    internal static class TypeExtensions
    {
        public static bool IsNonNullGraphType(this Type type)
        {
            return type.GetTypeInfo().BaseType == typeof(NonNullGraphType);
        }

        public static Type GraphType(this Type type)
        {
            return type.IsNonNullGraphType() ? type.GetGenericArguments().First() : type;
        }

        public static bool IsObjectGraphType(this Type type)
        {
            Type baseType = type.GetTypeInfo().BaseType;
            return baseType != null && baseType.Name == "ObjectGraphType`1";
        }

        public static bool IsListGraphType(this Type? type)
        {
            if (type == null)
            {
                return false;
            }

            Type baseType = type.GetTypeInfo().BaseType;
            return baseType != null && baseType == typeof(ListGraphType);
        }

        public static Type ModelType(this Type type)
        {
            return type.GraphType().GetTypeInfo().BaseType.GetGenericArguments().First();
        }

#if NET451
        // https://github.com/castleproject/Core/blob/netcore/src/Castle.Core/Compatibility/IntrospectionExtensions.cs
        // This allows us to use the new reflection API which separates Type and TypeInfo
        // while still supporting .NET 3.5 and 4.0. This class matches the API of the same
        // class in .NET 4.5+, and so is only needed on .NET Framework versions before that.
        //
        // Return the System.Type for now, we will probably need to create a TypeInfo class
        // which inherits from Type like .NET 4.5+ and implement the additional methods and
        // properties.
        public static Type GetTypeInfo(this Type type)
        {
            return type;
        }
#endif
    }
}