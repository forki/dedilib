using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using DediLib.Collections;

namespace DediLib
{
    public interface IInjectionContext : IDisposable
    {
        T Get<T>() where T : class;
        object ResolveType(Type type);

        T TryGet<T>() where T : class;
        object TryResolveType(Type type);

        bool IsRegistered<T>();
        bool IsRegistered(Type interfaceType);

        void Register(Type type, Func<IInjectionContext, object> createFunc);
        void Register<T>(Func<IInjectionContext, T> createFunc) where T : class;
        void Register(Type interfaceType, Type instanceType);
        void Register<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface;

        void Singleton(Type type, Func<IInjectionContext, object> createFunc);
        void Singleton<T>(Func<IInjectionContext, T> createFunc) where T : class;
        void Singleton(Type interfaceType, Type instanceType);
        void Singleton<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface;

        void RegisterAllUniqueInterfaceImplementations(bool overwriteExistingRegistrations = false,
            params Assembly[] assemblies);

        IInjectionContext CreateScope();
    }

    public class InjectionContext : IInjectionContext
    {
        private readonly Dictionary<Type, Func<IInjectionContext, object>> _actions;

        public InjectionContext()
        {
            _actions = new Dictionary<Type, Func<IInjectionContext, object>>();
        }

        public InjectionContext(InjectionContext parentContext)
        {
            _actions = parentContext?._actions.ToDictionary(x => x.Key, x => x.Value) ?? new Dictionary<Type, Func<IInjectionContext, object>>();
        }

        public void Dispose()
        {
        }

        [TargetedPatchingOptOut("")]
        public T Get<T>() where T : class
        {
            object result;

            var type = typeof (T);
            if (type.IsInterface)
            {
                result = TryGetInterface(type);
                if (result == null)
                    throw new InvalidOperationException(
                        $"Type '{type.FullName}' not registered for Get<T>(), please use Register<T> first");
                return (T)result;
            }

            result = TryCreateFromConstructor(type);
            if (result == null)
                throw new InvalidOperationException(
                    $"Type '{type.FullName}' cannot be instantiated as it must have a constructor with only interface parameters");

            return (T)result;
        }

        [TargetedPatchingOptOut("")]
        public object ResolveType(Type type)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));

            object result;

            if (type.IsInterface)
            {
                result = TryGetInterface(type);
                if (result == null)
                    throw new InvalidOperationException(
                        $"Type '{type.FullName}' not registered for ResolveType(), please use Register<T> first");
                return result;
            }

            result = TryCreateFromConstructor(type);
            if (result == null)
                throw new InvalidOperationException(
                    $"Type '{type.FullName}' cannot be instantiated as it must have a constructor with only interface parameters");

            return result;
        }

        [TargetedPatchingOptOut("")]
        public T TryGet<T>() where T : class
        {
            var type = typeof (T);
            if (type.IsInterface)
            {
                return (T)TryGetInterface(type);
            }

            return (T)TryCreateFromConstructor(type);
        }

        [TargetedPatchingOptOut("")]
        public object TryResolveType(Type type)
        {
            if (type == null) return null;

            if (type.IsInterface)
            {
                return TryGetInterface(type);
            }

            return TryCreateFromConstructor(type);
        }

        [TargetedPatchingOptOut("")]
        public bool IsRegistered<T>()
        {
            return _actions.ContainsKey(typeof(T));
        }

        [TargetedPatchingOptOut("")]
        public bool IsRegistered(Type interfaceType)
        {
            return _actions.ContainsKey(interfaceType);
        }

        private object TryGetInterface(Type type)
        {
            Func<IInjectionContext, object> func;
            if (!_actions.TryGetValue(type, out func))
                return null;

            return func(this);
        }

        private readonly Dictionary<Type, Tuple<ConstructorInfo, ParameterInfo[]>[]> _cachedConstructors = new Dictionary<Type, Tuple<ConstructorInfo, ParameterInfo[]>[]>();
        private object TryCreateFromConstructor(Type type)
        {
            Tuple<ConstructorInfo, ParameterInfo[]>[] constructors;
            if (!_cachedConstructors.TryGetValue(type, out constructors))
            {
                constructors = type.GetConstructors()
                    .Where(x => !x.IsGenericMethod)
                    .Where(
                        x =>
                            x.GetParameters()
                                .All(p => p.ParameterType.IsInterface || p.ParameterType == typeof (InjectionContext)))
                    .OrderBy(x => x.GetParameters().Length)
                    .Select(x => new Tuple<ConstructorInfo, ParameterInfo[]>(x, x.GetParameters()))
                    .ToArray();
                _cachedConstructors[type] = constructors;
            }

            foreach (var tuple in constructors)
            {
                var parameters = tuple.Item2.Select(p =>
                {
                    if (p.ParameterType == typeof(InjectionContext) || p.ParameterType == typeof(IInjectionContext))
                        return this;

                    return ResolveType(p.ParameterType);
                }).ToArray();

                return tuple.Item1.Invoke(parameters);
            }

            return null;
        }

        [TargetedPatchingOptOut("")]
        public void Register(Type type, Func<IInjectionContext, object> createFunc)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));

            if (!type.IsInterface)
                throw new InvalidOperationException(
                    $"Registered type for Register() must be an interface (but type was: {type.FullName})");

            _actions[type] = createFunc;
        }

        [TargetedPatchingOptOut("")]
        public void Register<T>(Func<IInjectionContext, T> createFunc) where T : class
        {
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));

            if (!typeof(T).IsInterface)
                throw new InvalidOperationException(
                    $"Generic type <T> for Register<T> must be an interface (but type was: {typeof(T).FullName})");

            Func<IInjectionContext, object> registerAction = createFunc;
            _actions[typeof(T)] = registerAction;
        }

        [TargetedPatchingOptOut("")]
        public void Register(Type interfaceType, Type instanceType)
        {
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException(
                    $"Registered interface type for Register must be an interface (but type was: {interfaceType.FullName})");

            if (instanceType.IsInterface)
                throw new InvalidOperationException(
                    $"Registered instance type for Register must be an instance (but type was: {interfaceType.FullName})");

            Func<IInjectionContext, object> registerAction = c => c.ResolveType(instanceType);
            _actions[interfaceType] = registerAction;
        }

        [TargetedPatchingOptOut("")]
        public void Register<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface
        {
            var constructorWithoutParams = typeof (TInstance).GetConstructors().FirstOrDefault(x => x.GetParameters().Length == 0);
            if (constructorWithoutParams != null)
            {
                Register<TInterface>(c => (TInstance)Activator.CreateInstance(typeof(TInstance)));
                return;
            }

            Register<TInterface>(c => c.Get<TInstance>());
        }

        [TargetedPatchingOptOut("")]
        public void Singleton(Type type, Func<IInjectionContext, object> createFunc)
        {
            if (type == null) throw new ArgumentNullException(nameof(type));
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));

            if (!type.IsInterface)
                throw new InvalidOperationException(
                    $"Registered type for Singleton() must be an interface (but type was: {type.FullName})");

            var singleton = createFunc(this);
            _actions[type] = c => singleton;
        }

        [TargetedPatchingOptOut("")]
        public void Singleton<T>(Func<IInjectionContext, T> createFunc) where T : class
        {
            if (createFunc == null) throw new ArgumentNullException(nameof(createFunc));

            if (!typeof(T).IsInterface)
                throw new InvalidOperationException(
                    $"Generic type <T> for Singleton<T> must be an interface (but type was: {typeof(T).FullName})");

            object singleton = createFunc(this);
            _actions[typeof(T)] = c => singleton;
        }

        [TargetedPatchingOptOut("")]
        public void Singleton(Type interfaceType, Type instanceType)
        {
            if (!interfaceType.IsInterface)
                throw new InvalidOperationException(
                    $"Registered interface type for Singleton must be an interface (but type was: {interfaceType.FullName})");

            if (instanceType.IsInterface)
                throw new InvalidOperationException(
                    $"Registered instance type for Singleton must be an instance (but type was: {interfaceType.FullName})");

            var singleton = ResolveType(instanceType);
            _actions[interfaceType] = c => singleton;
        }

        [TargetedPatchingOptOut("")]
        public void Singleton<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface
        {
            Singleton<TInterface>(c => c.Get<TInstance>());
        }

        [TargetedPatchingOptOut("")]
        public void RegisterAllUniqueInterfaceImplementations(bool overwriteExistingRegistrations = false, params Assembly[] assemblies)
        {
            if (assemblies == null || !assemblies.Any())
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic).ToArray();
            }

            var classTypes = assemblies.SelectMany(x => x.GetTypes()).Where(x => !x.IsInterface && !x.IsAbstract).ToList();
            var interfaces = new HashSet<Type>(assemblies.SelectMany(x => x.GetTypes()).Where(x => x.IsInterface));

            var interfaceMap = new HashSetDictionary<Type, Type>();

            foreach (var classType in classTypes)
            {
                foreach (var interfaceType in classType.GetInterfaces().Where(x => interfaces.Contains(x)))
                {
                    interfaceMap.Add(interfaceType, classType);
                }
            }

            foreach (var interfaceType in interfaceMap.Keys)
            {
                var values = interfaceMap.GetValuesAsHashSet(interfaceType);
                if (values.Count != 1) continue;

                if (overwriteExistingRegistrations || !_actions.ContainsKey(interfaceType))
                {
                    Register(interfaceType, values.First());
                }
            }
        }

        [TargetedPatchingOptOut("")]
        public IInjectionContext CreateScope()
        {
            return new InjectionContext(this);
        }
    }
}
