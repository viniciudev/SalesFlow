using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Npgsql.EntityFrameworkCore.PostgreSQL.Query.Internal;
using System;
using System.Linq;
using System.Linq.Expressions;

public class CaseInsensitiveQueryRewriter : ExpressionVisitor
{
    protected override Expression VisitMethodCall(MethodCallExpression node)
    {
        // Converte .Contains() para ILike no PostgreSQL
        if (node.Method.Name == "Contains" &&
            node.Arguments.Count == 1 &&
            node.Object?.Type == typeof(string))
        {
            var property = node.Object;
            var searchTerm = node.Arguments[0];

            // Tenta obter o valor do termo de busca
            if (TryGetConstantValue(searchTerm, out string termValue))
            {
                // Cria EF.Functions.ILike(property, "%value%")
                var efFunctions = Expression.Property(null, typeof(EF).GetProperty("Functions"));
                var iLikeMethod = typeof(NpgsqlDbFunctionsExtensions).GetMethod("ILike",
                    new[] { typeof(DbFunctions), typeof(string), typeof(string) });

                var pattern = Expression.Constant($"%{termValue}%");
                return Expression.Call(iLikeMethod, efFunctions, property, pattern);
            }
        }

        return base.VisitMethodCall(node);
    }

    private bool TryGetConstantValue(Expression expression, out string value)
    {
        value = null;

        if (expression is ConstantExpression constant)
        {
            value = constant.Value?.ToString();
            return true;
        }

        // Tenta avaliar expressões mais complexas
        try
        {
            var lambda = Expression.Lambda<Func<string>>(expression);
            var compiled = lambda.Compile();
            value = compiled();
            return true;
        }
        catch
        {
            return false;
        }
    }
}

// Para usar, crie uma extensão de IQueryable
public static class NpgsqlQueryableExtensions
{
    public static IQueryable<T> WithCaseInsensitive<T>(this IQueryable<T> query)
    {
        var expression = new CaseInsensitiveQueryRewriter().Visit(query.Expression);
        return query.Provider.CreateQuery<T>(expression);
    }
}
