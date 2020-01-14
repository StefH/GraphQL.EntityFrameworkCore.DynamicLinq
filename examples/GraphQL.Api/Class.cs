using System;

namespace GraphQL.Types
{
    public class ListGraphType2<T> : ListGraphType2
        where T : IGraphType
    {
        public ListGraphType2()
            : base(typeof(T))
        {
        }
    }

    public class ListGraphType2 : GraphType
    {
        public ListGraphType2(IGraphType type)
        {
            ResolvedType = type;
        }

        protected ListGraphType2(Type type)
        {
            Type = type;
        }

        public Type Type { get; private set; }

        public IGraphType ResolvedType { get; set; }

        public override string CollectTypes(TypeCollectionContext context)
        {
            var innerType = context.ResolveType(Type);
            ResolvedType = innerType;
            var name = innerType.CollectTypes(context);
            context.AddType(name, innerType, context);
            return "[{0}]".ToFormat(name);
        }

        public override string ToString() => $"[{ResolvedType}]";
    }
}