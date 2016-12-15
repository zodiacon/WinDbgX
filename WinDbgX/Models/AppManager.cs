using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinDbgX.UICore;

namespace WinDbgX.Models {
	[Export]
	class AppManager : IPartImportsSatisfiedNotification, IDisposable {
		[Import]
		public DebugManager Debug { get; private set; }

		[Import]
		public UIManager UI { get; private set; }

		public static AppManager Instance { get; private set; }

		[Import]
		public CompositionContainer Container { get; private set; }

		Settings _settings;
		public Settings Settings => _settings ?? (_settings = new Settings());

		string GetSettingsFile() {
			var folder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"\WinDbgX";
			if (!Directory.Exists(folder))
				Directory.CreateDirectory(folder);
			return folder + @"\settings.xml";
		}

		public void OnImportsSatisfied() {
		}

		internal AppManager() {
			Instance = this;
			_settings = Settings.Load(GetSettingsFile());
		}

		internal void SaveSettings() {
			Settings.Save(GetSettingsFile());
		}

        public void Dispose() {
            SaveSettings();
            Debug.Dispose();
        }
    }
}
