using Autofac;
using Microsoft.HackChecklist.Services;
using Microsoft.HackChecklist.Services.Contracts;
using Microsoft.HackChecklist.UWP.Contracts;
using Microsoft.HackChecklist.UWP.Services;
using Microsoft.HackChecklist.UWP.ViewModels;

namespace Microsoft.HackChecklist.UWP
{
    public class IoCConfiguration
    {
        private static IContainer _container;

        public static void Configure()
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<JsonSerializerService>().As<IJsonSerializerService>().SingleInstance();
            builder.RegisterType<AppDataService>().As<IAppDataService>().SingleInstance();
            builder.RegisterType<AnalyticsService>().As<IAnalyticsService>().SingleInstance();

            builder.RegisterType<MainViewModel>().SingleInstance();

            _container = builder.Build();
        }

        public static object GetType<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
