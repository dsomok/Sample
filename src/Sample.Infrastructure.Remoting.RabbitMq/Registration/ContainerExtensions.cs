using System;
using Autofac;
using Sample.Infrastructure.Hosting.Host;
using Sample.Infrastructure.Remoting.Registration;

namespace Sample.Infrastructure.Remoting.Rabbit.Registration
{
    public static class ContainerExtensions
    {
        public static ContainerBuilder WithRabbitRemoting(
            this ContainerBuilder builder, Action<IServiceConfigurator> action)
        {
            builder.RegisterModule<RabbitModule>();
            var configurator = new ServiceConfigurator(builder);
            action(configurator);

            return builder;
        }

        public static IHostBuilder WithRabbitRemoting(
            this IHostBuilder hostBuilder,
            Action<IServiceConfigurator> action
        )
        {
            return hostBuilder.With(builder => builder.WithRabbitRemoting(action));
        }
    }
}