using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using WinDbgX.Models;
using WinDbgX.UICore;
using WinDbgX.ViewModels;
using WinDbgX.Windows;
using Zodiacon.WPF;

namespace WinDbgX {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
		public static CompositionContainer Container { get; private set; }

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            var container = new CompositionContainer(
                new AggregateCatalog(
                    new AssemblyCatalog(typeof(IFileDialogService).Assembly),
                    new AssemblyCatalog(Assembly.GetExecutingAssembly())));

			var defaults = new UIServicesDefaults();
			container.ComposeExportedValue(defaults.DialogService);
			container.ComposeExportedValue(defaults.FileDialogService);
			container.ComposeExportedValue(defaults.MessageBoxService);
			container.ComposeExportedValue(container);

			var appManager = container.GetExportedValue<AppManager>();

			Container = container;
			var win = new MainWindow();
			var vm = new MainViewModel(true, new IWindowImpl(win));
			win.DataContext = vm;

			win.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
			Container.GetExportedValue<DebugManager>().Dispose();
            base.OnExit(e);
        }
    }
}
