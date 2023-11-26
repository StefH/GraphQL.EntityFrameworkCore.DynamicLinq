using System;
using System.Linq;
using System.Reflection;
using GraphQL.Types;

namespace GraphQL.EntityFrameworkCore.DynamicLinq.Extensions;

internal static class TypeExtensions
{
    public static bool IsNonNullGraphType(this Type? type)
    {
        return type?.GetTypeInfo().BaseType == typeof(NonNullGraphType);
    }

    public static Type GraphType(this Type type)
    {
        return type.IsNonNullGraphType() ? type.GetGenericArguments().First() : type;
    }

    public static bool IsObjectGraphType(this Type? type)
    {
        var baseType = type?.GetTypeInfo().BaseType;
        return baseType is { Name: "ObjectGraphType`1" };
    }

    public static bool IsListGraphType(this Type? type)
    {
        if (type == null)
        {
            return false;
        }

        var baseType = type.GetTypeInfo().BaseType;
        return baseType != null && baseType == typeof(ListGraphType);
    }

    public static Type ModelType(this Type type)
    {
        return type.GraphType().GetTypeInfo().BaseType!.GetGenericArguments().First();
    }
}