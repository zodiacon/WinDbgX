using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Zodiacon.WPF;

namespace WinDbgEx.ViewModels {
	sealed class RunExecutableViewModel : DialogViewModelBase {
		readonly IFileDialogService _fileDialogService;

		public RunExecutableViewModel(Window dialog, IFileDialogService fileDialogService) : base(dialog) {
			_fileDialogService = fileDialogService;

			CanExecuteOKCommand = () => !string.IsNullOrWhiteSpace(ExecutablePath);
			OKCommand.ObservesProperty(() => ExecutablePath);
		}

		public ResizeMode ResizeMode => ResizeMode.NoResize;
		public SizeToContent SizeToContent => SizeToContent.WidthAndHeight;
		public string Title => "Run Executable";

		private string _executablePath;

		public string ExecutablePath {
			get { return _executablePath; }
			set { SetProperty(ref _executablePath, value); }
		}

		private string _startDirectory;

		public string StartDirectory {
			get { return _startDirectory; }
			set { SetProperty(ref _startDirectory, value); }
		}

		private string _commandLine;

		public string CommandLine {
			get { return _commandLine; }
			set { SetProperty(ref _commandLine, value); }
		}

		private bool _debugChildren;

		public bool DebugChildren {
			get { return _debugChildren; }
			set { SetProperty(ref _debugChildren, value); }
		}

		public ICommand BrowseCommand => new DelegateCommand(() => {
			var filename = _fileDialogService.GetFileForOpen("Executable Files|*.*|All Files|*.*", "Select Executable");
			if (filename != null)
				ExecutablePath = filename;
		});

		public ICommand BrowseDirectoryCommand => new DelegateCommand(() => {
			var folder = _fileDialogService.GetFolder();
			if (folder != null) {
				StartDirectory = folder;
			}
		});
	}
}
