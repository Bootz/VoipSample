﻿using Autofac;
using CyclopsToolkit.WinPhone.Navigation;
using VoipTranslator.Client.Core.Common;
using VoipTranslator.Client.Core.Compasition;
using VoipTranslator.Client.Core.Contracts;
using VoipTranslator.Client.WinPhone.Infrastructure;
using VoipTranslator.Client.WinPhone.Navigation;
using VoipTranslator.Client.WinPhone.ViewModels;
using VoipTranslator.Client.WinPhone.Views;
using VoipTranslator.Infrastructure.Logging;

namespace VoipTranslator.Client.WinPhone
{
    public class ApplicationModule : LayerModule
    {
        private NavigationBuilder _navigationBuilder = null;

        protected override void OnMap(ContainerBuilder builder)
        {
            _navigationBuilder = new NavigationBuilder(new ContainerBuilderAdapter(builder));
            builder.RegisterInstance(_navigationBuilder).SingleInstance();

            builder.RegisterType<AgentsController>().SingleInstance();
            builder.RegisterType<VoipTranslatorNavigator>().As<NavigationManagerBase>().SingleInstance();
            builder.RegisterType<KeyValueStorage>().As<IKeyValueStorage>().SingleInstance();
            builder.RegisterType<Dispatcher>().As<IUIDispatcher>().SingleInstance();
            builder.RegisterType<TransportResource>().As<ITransportResource>().SingleInstance();
            builder.RegisterType<PhoneLogger>().As<ILogger>().SingleInstance();
            builder.RegisterType<AudioDeviceResource>().As<IAudioDeviceResource>().SingleInstance();
            builder.RegisterType<DeviceInfoProvider>().As<IDeviceInfoProvider>().SingleInstance();

            _navigationBuilder
                .RegisterViewModel<RegistrationViewModel>().ForView<RegistrationPage>()
                .RegisterViewModel<MainViewModel>().Singleton().ForView<MainPage>()
                .RegisterViewModel<FrameViewModel>().StaticResource().WithoutView()
                .RegisterViewModel<KeypadViewModel>().StaticResource().WithoutView()
                .RegisterViewModel<CallViewModel>().StaticResource().WithoutView()
                ;

            base.OnMap(builder);
        }

        public override void OnPostContainerBuild(IContainer container)
        {
            _navigationBuilder.RegisterStaticViewModels(container.Resolve);
        }
    }
}
