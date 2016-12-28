using MahApps.Metro;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace WinDbgX.ViewModels {
	class AccentViewModel : BindableBase {
		Accent _accent;
		public AccentViewModel(Accent accent) {
			_accent = accent;
		}

		public string Name => _accent.Name;
		public Brush Brush => _accent.Resources["AccentColorBrush"] as Brush;

		private bool _isCurrent;

		public bool IsCurrent {
			get { return _isCurrent; }
			set { SetProperty(ref _isCurrent, value); }
		}

		public void ChangeAccentColor(Window window) {
			ThemeManager.ChangeAppStyle(window.Resources, _accent, ThemeManager.DetectAppStyle().Item1);
		}
	}
}
