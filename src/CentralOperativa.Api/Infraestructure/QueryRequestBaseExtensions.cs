using ServiceStack.OrmLite;

namespace CentralOperativa.Infraestructure
{
    public static class QueryRequestBaseExtensions
    {
        public static SqlExpression<T> GetLimit<T>(this QueryRequestBase<T> item, SqlExpression<T> q)
        {
            var skip = 0;
            var take = 100;
            if (item.Skip.HasValue)
            {
                skip = item.Skip.Value;
            }

            if (item.Take.HasValue)
            {
                take = item.Take.Value;
            }

            return q.Limit(skip, take);
        }
    }
}
