using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.Models {
    public class Settings {
        public ObservableCollection<Executable> RecentExecutables { get; set; }

        public void Save(string path) {
            using (var stm = File.Open(path, FileMode.Create)) {
                var serializer = new DataContractSerializer(GetType());
                serializer.WriteObject(stm, this);
            }
        }

        public static Settings Load(string path) {
            try {
                using (var stm = File.Open(path, FileMode.Open)) {
                    var serializer = new DataContractSerializer(typeof(Settings));
                    var settings = (Settings)serializer.ReadObject(stm);
                    return settings;
                }
            }
            catch {
                return null;
            }
        }
    }
}
