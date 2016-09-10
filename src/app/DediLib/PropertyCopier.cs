// Source: http://stackoverflow.com/questions/930433/apply-properties-values-from-one-object-to-another-of-the-same-type-automaticall
// + performance improved, more flexibility, refactoring

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime;

namespace DediLib
{
    /// <summary>
    /// Non-generic class allowing properties to be copied from one instance
    /// to another existing instance of a potentially different type.
    /// </summary>
    public static class PropertyCopy
    {
        /// <summary>
        /// Copies all public, readable properties from the source object to the
        /// target. The target type does not have to have a parameterless constructor,
        /// as no new instance needs to be created.
        /// </summary>
        /// <remarks>Only the properties of the source and target types themselves
        /// are taken into account, regardless of the actual types of the arguments.</remarks>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="source">Source to copy properties from</param>
        /// <param name="target">Target to copy properties to</param>
        [TargetedPatchingOptOut("")]
        public static void CopyFull<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            PropertyCopier<TSource, TTarget>.CopyFull(source, target);
        }

        /// <summary>
        /// Copies matching public, readable properties from the source object to the
        /// target. The target type does not have to have a parameterless constructor,
        /// as no new instance needs to be created.
        /// </summary>
        /// <remarks>Only the properties of the source and target types themselves
        /// are taken into account, regardless of the actual types of the arguments.</remarks>
        /// <typeparam name="TSource">Type of the source</typeparam>
        /// <typeparam name="TTarget">Type of the target</typeparam>
        /// <param name="source">Source to copy properties from</param>
        /// <param name="target">Target to copy properties to</param>
        [TargetedPatchingOptOut("")]
        public static void CopyMatchingOnly<TSource, TTarget>(TSource source, TTarget target)
            where TSource : class
            where TTarget : class
        {
            PropertyCopier<TSource, TTarget>.CopyMatching(source, target);
        }
    }

    /// <summary>
    /// Generic class which copies to its target type from a source
    /// type specified in the Copy method. The types are specified
    /// separately to take advantage of type inference on generic
    /// method arguments.
    /// </summary>
    public static class PropertyCopy<TTarget> where TTarget : class, new()
    {
        /// <summary>
        /// Tries to cast source to target and if not possible, copies all readable properties 
        /// from the source to a new instance of TTarget.
        /// </summary>
        [TargetedPatchingOptOut("")]
        public static TTarget CastOrCopyFull<TSource>(TSource source) where TSource : class
        {
            var result = source as TTarget;
            if (result != null) return result;
            return PropertyCopier<TSource, TTarget>.CopyFull(source);
        }

        /// <summary>
        /// Tries to cast source to target and if not possible, copies matching readable properties 
        /// from the source to a new instance of TTarget.
        /// </summary>
        [TargetedPatchingOptOut("")]
        public static TTarget CastOrCopyMatching<TSource>(TSource source) where TSource : class
        {
            var result = source as TTarget;
            if (result != null) return result;
            return PropertyCopier<TSource, TTarget>.CopyMatching(source);
        }

        /// <summary>
        /// Copies all readable properties from the source to a new instance
        /// of TTarget.
        /// </summary>
        [TargetedPatchingOptOut("")]
        public static TTarget CopyFull<TSource>(TSource source) where TSource : class
        {
            return PropertyCopier<TSource, TTarget>.CopyFull(source);
        }

        /// <summary>
        /// Copies matching readable properties from the source to a new instance
        /// of TTarget.
        /// </summary>
        [TargetedPatchingOptOut("")]
        public static TTarget CopyMatching<TSource>(TSource source) where TSource : class
        {
            return PropertyCopier<TSource, TTarget>.CopyMatching(source);
        }
    }

    /// <summary>
    /// Static class to efficiently store the compiled delegate which can
    /// do the copying. We need a bit of work to ensure that exceptions are
    /// appropriately propagated, as the exception is generated at type initialization
    /// time, but we wish it to be thrown as an ArgumentException.
    /// Note that this type we do not have a constructor constraint on TTarget, because
    /// we only use the constructor when we use the form which creates a new instance.
    /// </summary>
    internal static class PropertyCopier<TSource, TTarget>
    {
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        /// <summary>
        /// Delegate to create a new instance of the target type given an instance of the
        /// source type. This is a single delegate from an expression tree.
        /// </summary>
        private static Func<TSource, TTarget> _creatorFull = s => { _creatorFull = BuildCreator(true); return _creatorFull(s); };
        internal static Func<TSource, TTarget> CreatorFull => _creatorFull;

        private static Func<TSource, TTarget> _creatorMatching = s => { _creatorMatching = BuildCreator(false); return _creatorMatching(s); };
        internal static Func<TSource, TTarget> CreatorMatching => _creatorMatching;

        private static Action<TSource, TTarget> _mapperFull = (s, t) => { _mapperFull = BuildMapper(true); _mapperFull(s, t); };
        internal static Action<TSource, TTarget> MapperFull => _mapperFull;

        private static Action<TSource, TTarget> _mapperMatching = (s, t) => { _mapperMatching = BuildMapper(false); _mapperMatching(s, t); };
        internal static Action<TSource, TTarget> MapperMatching => _mapperMatching;
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        internal static TTarget CopyFull(TSource source)
        {
            if (ReferenceEquals(source, null))
                throw new ArgumentNullException(nameof(source));

            return CreatorFull(source);
        }

        internal static TTarget CopyMatching(TSource source)
        {
            if (ReferenceEquals(source, null))
                throw new ArgumentNullException(nameof(source));

            return CreatorMatching(source);
        }

        internal static void CopyFull(TSource source, TTarget target)
        {
            if (ReferenceEquals(source, null))
                throw new ArgumentNullException(nameof(source));

            MapperFull(source, target);
        }

        internal static void CopyMatching(TSource source, TTarget target)
        {
            if (ReferenceEquals(source, null))
                throw new ArgumentNullException(nameof(source));

            MapperMatching(source, target);
        }

        private static Func<TSource, TTarget> BuildCreator(bool includeAllProperties)
        {
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var bindings = new List<MemberBinding>();
            foreach (var sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!sourceProperty.CanRead) continue;

                var targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);
                if (targetProperty == null)
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not present and accessible in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.CanWrite)
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not writable in " + typeof(TTarget).FullName);
                }
                var setMethod = targetProperty.GetSetMethod();
                if (setMethod == null || setMethod.Attributes.HasFlag(MethodAttributes.Static))
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " is static or has no setter in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " has an incompatible type in " + typeof(TTarget).FullName);
                }
                bindings.Add(Expression.Bind(targetProperty, Expression.Property(sourceParameter, sourceProperty)));
            }
            Expression initializer = Expression.MemberInit(Expression.New(typeof(TTarget)), bindings);
            return Expression.Lambda<Func<TSource, TTarget>>(initializer, sourceParameter).Compile();
        }

        private static Action<TSource, TTarget> BuildMapper(bool includeAllProperties)
        {
            var sourceParameter = Expression.Parameter(typeof(TSource), "source");
            var targetParameter = Expression.Parameter(typeof(TTarget), "target");

            var expressions = new List<Expression>();
            foreach (var sourceProperty in typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!sourceProperty.CanRead) continue;

                var targetProperty = typeof(TTarget).GetProperty(sourceProperty.Name);
                if (targetProperty == null)
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not present and accessible in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.CanWrite)
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " is not writable in " + typeof(TTarget).FullName);
                }
                var setMethod = targetProperty.GetSetMethod();
                if (setMethod != null && setMethod.Attributes.HasFlag(MethodAttributes.Static))
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " is static in " + typeof(TTarget).FullName);
                }
                if (!targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    if (!includeAllProperties) continue;
                    throw new ArgumentException("Property " + sourceProperty.Name + " has an incompatible type in " + typeof(TTarget).FullName);
                }

                expressions.Add(Expression.Assign(Expression.Property(targetParameter, targetProperty), Expression.Property(sourceParameter, sourceProperty)));
            }

            if (expressions.Count <= 0) return (source, target) => { }; // no properties => do nothing
            Expression block = Expression.Block(expressions);
            return Expression.Lambda<Action<TSource, TTarget>>(block, sourceParameter, targetParameter).Compile();
        }
    }
}
