using System.Linq.Expressions;

namespace Chibest.Service.Utilities;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        if (left == null) return right;
        if (right == null) return left;

        var parameter = Expression.Parameter(typeof(T));
        var leftBody = new ParameterReplacer(parameter).Visit(left.Body);
        var rightBody = new ParameterReplacer(parameter).Visit(right.Body);
        var body = Expression.AndAlso(leftBody!, rightBody!);
        return Expression.Lambda<Func<T, bool>>(body, parameter);
    }

    private class ParameterReplacer : ExpressionVisitor
    {
        private readonly ParameterExpression _parameter;
        public ParameterReplacer(ParameterExpression parameter) => _parameter = parameter;
        protected override Expression VisitParameter(ParameterExpression node) => _parameter;
    }
}
