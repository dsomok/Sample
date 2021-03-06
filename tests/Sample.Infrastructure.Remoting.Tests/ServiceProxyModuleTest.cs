﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Sample.Infrastructure.Remoting.Client;
using Sample.Infrastructure.Remoting.Communication;
using Shouldly;
using Xunit;

namespace Sample.Infrastructure.Remoting.Tests
{
    public class ServiceProxyModuleTest
    {
        internal interface ITestInterface
        {
            Task<bool> AsyncBooleanMethod(string input);
            Task AsyncVoidMethod();
            void VoidMethod();
            DateTime SyncMethod();
            Task<object> AsyncObjectMethod();
        }

        [Fact]
        public async void TestCorrectBooleanInvocation()
        {
            var proxy = RegisterProxy<ITestInterface>(() => true);

            var result = await proxy.AsyncBooleanMethod("adf");

            result.ShouldBe(true);
        }

        [Fact]
        public async void TestAsyncVoidInvocation()
        {
            var proxy = RegisterProxy<ITestInterface>(() => true);

            await Assert.ThrowsAsync<InvalidOperationException>(async () => await proxy.AsyncVoidMethod());
        }

        [Fact]
        public void TestVoidInvocation()
        {
            var proxy = RegisterProxy<ITestInterface>(() => true);

            Assert.Throws<InvalidOperationException>(() => proxy.SyncMethod());
        }

        [Fact]
        public async void TestFaultedInvocation()
        {
            const string errorMsg = "someerror";
            var proxy = RegisterProxy<ITestInterface>(() => new ArgumentNullException(errorMsg));

            var targetInvocationEx = await Assert.ThrowsAsync<TargetInvocationException>(async () => await proxy.AsyncObjectMethod());

            var sourceEx = targetInvocationEx.InnerException.ShouldBeAssignableTo<ArgumentNullException>();
            sourceEx.Message.ShouldBe(errorMsg);
        }

        [Fact]
        public void TestFaultedBlockingInvocation()
        {
            const string errorMsg = "someerror";
            var proxy = RegisterProxy<ITestInterface>(() => new ArgumentNullException(errorMsg));

            var ex = Assert.Throws<AggregateException>(() => proxy.AsyncObjectMethod().Wait());

            var targetInvocationEx = ex.InnerExceptions.First().ShouldBeAssignableTo<TargetInvocationException>();
            var sourceEx = targetInvocationEx.InnerException.ShouldBeAssignableTo<ArgumentNullException>();
            sourceEx.Message.ShouldBe(errorMsg);
        }

        [Fact]
        public void TestTrue()
        {
            true.ShouldBe(true);
        }

        private TInterface RegisterProxy<TInterface>(Func<dynamic> processAction)
        {
            var builder = new ContainerBuilder();
            builder.RegisterMockTransport<TInterface>(e => new RemoteResponse(processAction()));
            builder.RegisterType<ResponseAwaitersRegistry<RemoteResponse>>().AsSelf();
            builder.RegisterType<RemoteProcedureExecutor<TInterface>>().AsSelf();
            builder.RegisterType<ResponseConverter>().AsSelf();
            var container = builder.Build();
            var proxy = ServiceProxyFactory.Create(
                container.Resolve<RemoteProcedureExecutor<TInterface>>(),
                container.Resolve<ResponseConverter>());
            return proxy;
        }
    }
}