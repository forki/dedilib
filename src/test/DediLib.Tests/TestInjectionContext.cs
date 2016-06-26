using System;
using System.Diagnostics;
using NUnit.Framework;

namespace DediLib.Tests
{
    [TestFixture]
    public class TestInjectionContext
    {
        [Test]
        public void Get_InterfaceNotRegistered_ThrowsInvalidOperationException()
        {
            using (var context = new InjectionContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Get<ITestInterface>());
            }
        }

        [Test]
        public void TryGet_InterfaceNotRegistered_Null()
        {
            using (var context = new InjectionContext())
            {
                Assert.IsNull(context.TryGet<ITestInterface>());
            }
        }

        [Test]
        public void Get_InterfaceRegistered_InstanceOfRegisteredInterface()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());
                Assert.IsInstanceOf<TestClass>(context.Get<ITestInterface>());
            }
        }

        [Test]
        public void TryGet_InterfaceRegistered_InstanceOfRegisteredInterface()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());
                Assert.IsInstanceOf<TestClass>(context.TryGet<ITestInterface>());
            }
        }

        [Test]
        public void Get_ClassWithNonPublicConstructor_ThrowsInvalidOperationException()
        {
            using (var context = new InjectionContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Get<TestClassWithNonPublicConstructor>());
            }
        }

        [Test]
        public void TryGet_ClassWithNonPublicConstructor_Null()
        {
            using (var context = new InjectionContext())
            {
                Assert.IsNull(context.TryGet<TestClassWithNonPublicConstructor>());
            }
        }

        [Test]
        public void Get_ClassWithInterfaceConstructor_InstanceForInterfaceIsCreated()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());

                var instance = context.Get<TestClassWithInterfaceConstructor>();
                Assert.IsInstanceOf<TestClass>(instance.TestInterface);
            }
        }

        [Test]
        public void TryGet_ClassWithInterfaceConstructor_InstanceForInterfaceIsCreated()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());

                var instance = context.TryGet<TestClassWithInterfaceConstructor>();
                Assert.IsInstanceOf<TestClass>(instance.TestInterface);
            }
        }

        [Test]
        public void Get_ClassWithIInjectionContextConstructor_InjectionContextIsPassedIntoConstructor()
        {
            using (var context = new InjectionContext())
            {
                var instance = context.Get<TestClassWithIInjectionContext>();
                Assert.AreSame(context, instance.InjectionContext);
            }
        }

        [Test]
        public void TryGet_ClassWithIInjectionContextConstructor_InjectionContextIsPassedIntoConstructor()
        {
            using (var context = new InjectionContext())
            {
                var instance = context.TryGet<TestClassWithIInjectionContext>();
                Assert.AreSame(context, instance.InjectionContext);
            }
        }

        [Test]
        public void Register_GenericTypeNotAnInterface_InvalidOperationException()
        {
            using (var context = new InjectionContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Register(c => new TestClass()));
            }
        }

        [Test]
        public void Register_GetTwice_DifferentInstances()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());

                var instance1 = context.Get<ITestInterface>();
                var instance2 = context.Get<ITestInterface>();

                Assert.IsInstanceOf<TestClass>(instance1);
                Assert.IsInstanceOf<TestClass>(instance2);

                Assert.That(instance1, Is.Not.SameAs(instance2));
            }
        }

        [Test]
        public void RegisterGeneric_GetTwice_DifferentInstances()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface, TestClass>();

                var instance1 = context.Get<ITestInterface>();
                var instance2 = context.Get<ITestInterface>();

                Assert.IsInstanceOf<TestClass>(instance1);
                Assert.IsInstanceOf<TestClass>(instance2);

                Assert.That(instance1, Is.Not.SameAs(instance2));
            }
        }

        [Test]
        public void Register_OverrideRegistration_LastRegisterWins()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());
                context.Register<ITestInterface>(c => new OtherTestClass());
                Assert.IsInstanceOf<OtherTestClass>(context.Get<ITestInterface>());
            }
        }

        [Test]
        public void RegisterGeneric_OverrideRegistration_LastRegisterWins()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface, TestClass>();
                context.Register<ITestInterface, OtherTestClass>();
                Assert.IsInstanceOf<OtherTestClass>(context.Get<ITestInterface>());
            }
        }

        [Test]
        public void Singleton_GenericTypeNotAnInterface_InvalidOperationException()
        {
            using (var context = new InjectionContext())
            {
                Assert.Throws<InvalidOperationException>(() => context.Singleton(c => new TestClass()));
            }
        }

        [Test]
        public void Singleton_GetTwice_SameInstance()
        {
            using (var context = new InjectionContext())
            {
                context.Singleton<ITestInterface>(c => new TestClass());

                var instance1 = context.Get<ITestInterface>();
                var instance2 = context.Get<ITestInterface>();

                Assert.That(instance1, Is.SameAs(instance2));
            }
        }

        [Test]
        public void SingletonGeneric_GetTwice_SameInstance()
        {
            using (var context = new InjectionContext())
            {
                context.Singleton<ITestInterface, TestClass>();

                var instance1 = context.Get<ITestInterface>();
                var instance2 = context.Get<ITestInterface>();

                Assert.That(instance1, Is.SameAs(instance2));
            }
        }

        [Test]
        public void Singleton_OverrideRegistration_LastRegisterWins()
        {
            using (var context = new InjectionContext())
            {
                context.Singleton<ITestInterface>(c => new TestClass());
                context.Singleton<ITestInterface>(c => new OtherTestClass());
                Assert.IsInstanceOf<OtherTestClass>(context.Get<ITestInterface>());
            }
        }

        [Test]
        public void SingletonGeneric_OverrideRegistration_LastRegisterWins()
        {
            using (var context = new InjectionContext())
            {
                context.Singleton<ITestInterface, TestClass>();
                context.Singleton<ITestInterface, OtherTestClass>();
                Assert.IsInstanceOf<OtherTestClass>(context.Get<ITestInterface>());
            }
        }

        [Test]
        public void CreateScope_inherits_registered_components_from_parent_scope()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());

                using (var subContext = context.CreateScope())
                {
                    Assert.IsInstanceOf<TestClass>(subContext.Get<ITestInterface>());
                }
            }
        }

        [Test]
        public void CreateScope_child_scope_overrides_existing_parent_registration()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());

                using (var subContext = context.CreateScope())
                {
                    subContext.Register<ITestInterface>(c => new OtherTestClass());

                    Assert.IsInstanceOf<OtherTestClass>(subContext.Get<ITestInterface>());
                }
            }
        }

        [Test]
        public void CreateScope_parent_scope_changes_do_not_affect_child_scopes()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface>(c => new TestClass());

                using (var subContext = context.CreateScope())
                {
                    context.Register<ITestInterface>(c => new OtherTestClass());

                    Assert.IsInstanceOf<OtherTestClass>(context.Get<ITestInterface>());
                    Assert.IsInstanceOf<TestClass>(subContext.Get<ITestInterface>());
                }
            }
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void Benchmark_RegisterGet_TestClass()
        {
            using (var context = new InjectionContext())
            {
                context.Register<ITestInterface, TestClass>();

                const int count = 10000000;
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < count; i++)
                {
                    context.Get<ITestInterface>();
                }
                sw.Stop();

                var opsPerSec = count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
                Assert.Inconclusive($"{sw.Elapsed} ({opsPerSec.ToString("N0")} ops/sec)");
            }
        }

        [Category("Benchmark")]
        [Explicit]
        [Test]
        public void Benchmark_SingletonGet_TestClass()
        {
            using (var context = new InjectionContext())
            {
                context.Singleton<ITestInterface>(c => new TestClass());

                const int count = 10000000;
                var sw = Stopwatch.StartNew();
                for (var i = 0; i < count; i++)
                {
                    context.Get<ITestInterface>();
                }
                sw.Stop();

                var opsPerSec = count / (sw.ElapsedMilliseconds + 0.001m) * 1000m;
                Assert.Inconclusive($"{sw.Elapsed} ({opsPerSec.ToString("N0")} ops/sec)");
            }
        }

        private class TestClass : ITestInterface
        {
        }

        private class OtherTestClass : ITestInterface
        {
        }

        // ReSharper disable once ClassNeverInstantiated.Local
        private class TestClassWithNonPublicConstructor
        {
            private TestClassWithNonPublicConstructor()
            {
            }
        }

        public class TestClassWithIInjectionContext
        {
            public IInjectionContext InjectionContext { get; }

            public TestClassWithIInjectionContext(IInjectionContext injectionContext)
            {
                InjectionContext = injectionContext;
            }
        }

        public class TestClassWithInterfaceConstructor
        {
            public ITestInterface TestInterface { get; }

            public TestClassWithInterfaceConstructor(ITestInterface testInterface)
            {
                TestInterface = testInterface;
            }
        }

        public interface ITestInterface
        {
        }
    }
}
