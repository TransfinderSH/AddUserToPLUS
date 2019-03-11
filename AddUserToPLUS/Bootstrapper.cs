using Prism.Unity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Events;
using System.Reflection;
using AddUserToPLUS.ViewModels;
using AddUserToPLUS.Services;

namespace AddUserToPLUS
{
	class Bootstrapper : UnityBootstrapper
	{
		protected override DependencyObject CreateShell()
		{
			Container.Resolve<IEventAggregator>();
			RegisterTypes();
			var vm = Container.Resolve<MainWindowViewModel>();
			return vm.View;
		}

		private void RegisterTypes()
		{
			RegistServicesAndStart();
		}

		private void RegistServicesAndStart()
		{
			Assembly ass = Assembly.GetExecutingAssembly();
			Type[] types = ass.GetTypes();
			foreach (var type in types)
			{
				if (type.GetInterface("IService") != null)
				{
					Container.RegisterType(type, new ContainerControlledLifetimeManager());
					IService service = Container.Resolve(type) as IService;
					if (service.AutoStart) service.Start();
				}
			}
		}

		protected override void InitializeShell()
		{
			(this.Shell as Window).Show();
		}
	}
}
