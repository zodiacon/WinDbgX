using DebuggerEngine.Interop;

namespace DebuggerEngine {
    public sealed class TargetModule {
        public string Name { get; internal set; }
        public ulong BaseAddress { get; internal set; }
        public uint Checksum { get; internal set; }
		public string ImageName { get; internal set; }
		public uint Size { get; internal set; }
		public uint TimeStamp { get; internal set; }
		public ulong Handle { get; internal set; }
		public uint ProcessIndex { get; internal set; }
		public uint PID { get; internal set; }

        internal static TargetModule FromModuleParameters(DEBUG_MODULE_PARAMETERS parameters) {
            return new TargetModule {
                BaseAddress = parameters.Base,
                Checksum = parameters.Checksum
            };
        }
    }
}
