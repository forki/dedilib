using System;
using System.Linq.Expressions;
using System.Reflection;

namespace DediLib
{
    /// <summary>
    /// Provides convenience methods for working with Reflection.
    /// </summary>
    /// <typeparam name="T">The type to reflect on.</typeparam>
    public static class NameOf<T>
    {
        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyAccessExpression">The property access expression.</param>
        /// <returns></returns>
        /// <remarks>
        /// This method is significantly slower than simply using property name strings. But the
        /// benefit of refactoring support, outweighs the cost of any single call. DO cache the result
        /// of this method for performance.
        /// </remarks>
        public static string Property<TProperty>(Expression<Func<T, TProperty>> propertyAccessExpression)
        {
            // get MemberExpression from the lambda expression
            var expr = propertyAccessExpression.Body as MemberExpression;
            if (expr == null)
                throw new ArgumentException("Expression must be a property access.", nameof(propertyAccessExpression));

            // verify that the member is a property
            if ((expr.Member.MemberType & MemberTypes.Property) != MemberTypes.Property && 
                (expr.Member.MemberType & MemberTypes.Field) != MemberTypes.Field)
                throw new ArgumentException("Accessed member must be a property.", nameof(propertyAccessExpression));
            return expr.Member.Name;
        }
    }
}
