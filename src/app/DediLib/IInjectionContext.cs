using System;

namespace DediLib
{
    public interface IInjectionContext : IDisposable
    {
        T Get<T>() where T : class;
        object ResolveType(Type type);

        T TryGet<T>() where T : class;
        object TryResolveType(Type type);

        void Register(Type type, Func<IInjectionContext, object> createFunc);
        void Register<T>(Func<IInjectionContext, T> createFunc) where T : class;
        void Register(Type interfaceType, Type instanceType);
        void Register<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface;

        void Singleton(Type type, Func<IInjectionContext, object> createFunc);
        void Singleton<T>(Func<IInjectionContext, T> createFunc) where T : class;
        void Singleton(Type interfaceType, Type instanceType);
        void Singleton<TInterface, TInstance>() where TInterface : class where TInstance : class, TInterface;

        IInjectionContext CreateScope();
    }
}
