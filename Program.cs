using System;

namespace ConsoleApp2
{
    public abstract class Container
    { 
        public interface IContainerFactory
        {
            IContainer NewContainer();
        }

        protected interface IContainerBuilder
        {
            void RegisterSingleton<TC, TI>() where TI : class, TC where TC : class;
            void RegisterSingleton<TC>(Func<IContainer, TC> ctor) where TC : class;

            void RegisterScoped<TC, TI>() where TI : class, TC where TC : class;
            void RegisterScoped<TC>(Func<IContainer, TC> ctor) where TC : class;

            void RegisterTransient<TC, TI>() where TI : class, TC where TC : class;
            void RegisterTransient<TC>(Func<IContainer, TC> ctor) where TC : class;
        }

        protected abstract void Bootstrap(IContainerBuilder constructor);

        public sealed override bool Equals(object obj) => base.Equals(obj);
        public sealed override int GetHashCode() => base.GetHashCode();
        public sealed override string ToString() => base.ToString();

        public static IContainer Build<TContainer>()
            where TContainer : Container, new()
        {
            return ((IContainerFactory)new TContainer()).NewContainer();
        }
    }

    public interface IContainer : IDisposable
    {
        IScope CreateScope();
    }

    public interface IScope : IDisposable
    {
        object Resolve(Type type);
    }

    internal class MyContainer : Container, Container.IContainerFactory
    {
        static MyContainer()
        {
            //call real bootstrap that populates fields
        }

        #region generated

        private class MyContainerScope : IScope
        {
            //fields (properties?) with scope implementations

            private MyContainerRoot myContainerRoot;

            public MyContainerScope(MyContainerRoot myContainerRoot)
            {
                this.myContainerRoot = myContainerRoot;
            }

            public void Dispose()
            {
                //dispose transient collection
                //dispose fields
                throw new NotImplementedException();
            }

            public object Resolve(Type type)
            {
                //incode tree search for field
                throw new NotImplementedException();
            }
        }

        private class MyContainerRoot : IContainer
        {
            //fields (properties?) with singleton implementations

            public IScope CreateScope()
            {
                var scope = new MyContainerScope(this);
                return scope;
            }

            public void Dispose()
            {
                //dispose scopes
                //dispose singletons                  
                throw new NotImplementedException();
            }
        }

        IContainer IContainerFactory.NewContainer()
        {
            return new MyContainerRoot();
        }

        #endregion

        
        protected override void Bootstrap(IContainerBuilder builder)
        {
            builder.RegisterSingleton<ITestIface, TestImpl>();
            builder.RegisterSingleton<ITestIface>(c => new TestImpl(4,c.ToString()));
            builder.RegisterSingleton<ITestIface>(c => new TestImpl());
            builder.RegisterSingleton<ITestIface>(c => new TestImpl(c.ToString()));
            builder.RegisterSingleton<ITestIface>(c => new TestImpl(4));
        }
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = Container.Build<MyContainer>();
            Console.ReadLine();
        }
    }

    internal interface ITestIface
    {

    }

    internal class TestImpl : ITestIface
    {
        private int v1;
        private string v2;
        private int v;
        private string v3;

        public TestImpl()
        {
        }

        public TestImpl(string v)
        {
        }

        public TestImpl(int v)
        {
            this.v = v;
        }

        public TestImpl(int v, string v3) : this(v)
        {
            this.v3 = v3;
        }

        public TestImpl(string v, int v1, string v2) : this(v)
        {
            this.v1 = v1;
            this.v2 = v2;
        }
    }
}
