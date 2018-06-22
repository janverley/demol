using DeMol.ViewModels;
using Caliburn.Micro;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Threading;

namespace DeMol
{
    //Basic AppBootstrapper
    public class Bootstrapper : BootstrapperBase
    {
        private SimpleContainer container;

        public Bootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            container = new SimpleContainer();

            container.Instance(container);

            container
                .Singleton<IWindowManager, WindowManager>()
                .Singleton<IEventAggregator, EventAggregator>()
                .Singleton<ShellViewModel>()
                .Singleton<MenuViewModel>();

            container
               .PerRequest<InvalidateViewModel>()
               .PerRequest<QuizNaamViewModel>()
               .PerRequest<QuizBenJijDeMolViewModel>()
               .PerRequest<QuizWieIsDeMolViewModel>()
               .PerRequest<QuizOuttroViewModel>()
               .PerRequest<ValidateViewModel>()
               .PerRequest<ResultViewModel>()
               .PerRequest<QuizVragenViewModel>();
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<ShellViewModel>();
        }

        protected override object GetInstance(Type service, string key)
        {
            return container.GetInstance(service, key);
        }

        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return container.GetAllInstances(service);
        }

        protected override void BuildUp(object instance)
        {
            container.BuildUp(instance);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            MessageBox.Show($"{e.Exception.Message}\n{e.Exception.InnerException?.Message ?? ""}", "An error as occurred", MessageBoxButton.OK);
        }

    }
}
