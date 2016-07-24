using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Diagnostics.Runtime.Interop;

namespace DebuggerEngine {
    public class ModuleInfo {
        public string Name { get; internal set; }
        public ulong BaseAddress { get; internal set; }
        public uint Checksum { get; internal set; }

        internal static ModuleInfo FromModuleParameters(DEBUG_MODULE_PARAMETERS parameters) {
            return new ModuleInfo {
                BaseAddress = parameters.Base,
                Checksum = parameters.Checksum
            };
        }
    }
}
