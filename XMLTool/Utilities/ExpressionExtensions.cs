namespace ObjectiveXML.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;

    public static class ExpressionExtensions
    {
        private static readonly string expressionCannotBeNullMessage = "The expression cannot be null.";
        private static readonly string invalidExpressionMessage = "Invalid expression.";

        public static string GetMemberName<T>(Expression<Func<T, object>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string GetMemberName<T>(Expression<Func<T, bool>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static string GetMemberName<T>(this T instance, Expression<Action<T>> expression)
        {
            return GetMemberName(expression.Body);
        }

        public static object GetMemberIdentity(this Expression exp)
        {
            if (exp is ConstantExpression)
            {
                var constant = (ConstantExpression)exp;
                return constant.Value;
            }

            if (exp is MemberExpression)
            {
                // Reference type property or field
                var memberExpression = (MemberExpression)exp;
                return memberExpression.Member.Name;
            }

            throw new ArgumentException(invalidExpressionMessage);
        }

        public static List<string> GetMemberNames<T>(this T instance, params Expression<Func<T, object>>[] expressions)
        {
            List<string> memberNames = new List<string>();
            foreach (var cExpression in expressions)
            {
                memberNames.Add(GetMemberName(cExpression.Body));
            }

            return memberNames;
        }

        private static string GetMemberName(Expression expression)
        {
            if (expression == null)
            {
                throw new ArgumentException(expressionCannotBeNullMessage);
            }

            if (expression is ConstantExpression)
            {
                var constant = (ConstantExpression) expression;
                return (string)constant.Value;
            }

            if (expression is MemberExpression)
            {
                // Reference type property or field
                var memberExpression = (MemberExpression)expression;
                return memberExpression.Member.Name;
            }

            if (expression is MethodCallExpression)
            {
                // Reference type method
                var methodCallExpression = (MethodCallExpression)expression;
                return methodCallExpression.Method.Name;
            }

            if (expression is UnaryExpression)
            {
                // Property, field of method returning value type
                var unaryExpression = (UnaryExpression)expression;
                return GetMemberName(unaryExpression);
            }

            throw new ArgumentException(invalidExpressionMessage);
        }

        private static string GetMemberName(UnaryExpression unaryExpression)
        {
            if (unaryExpression.Operand is MethodCallExpression)
            {
                var methodExpression = (MethodCallExpression)unaryExpression.Operand;
                return methodExpression.Method.Name;
            }

            return ((MemberExpression)unaryExpression.Operand).Member.Name;
        }
    }
}
