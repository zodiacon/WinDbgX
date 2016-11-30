using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DebuggerEngine.Interop;
using System.Diagnostics;

namespace DebuggerEngine {
	public enum AttachProcessFlags : uint {
		Invasive = 0,
		NonInvasive = DEBUG_ATTACH.NONINVASIVE,
		NonInvasiveNoSuspend = DEBUG_ATTACH.NONINVASIVE_NO_SUSPEND,
		InvasiveNoInitialBreak = DEBUG_ATTACH.INVASIVE_NO_INITIAL_BREAK,
		InvasiveResumeProcess = DEBUG_ATTACH.INVASIVE_RESUME_PROCESS,
		Existing = DEBUG_ATTACH.EXISTING
	}

	public partial class DebugClient : CriticalFinalizerObject, IDisposable {
		internal readonly IDebugClient5 Client;
		internal readonly IDebugAdvanced3 Advanced;
		internal readonly IDebugDataSpaces4 DataSpaces;
		internal readonly IDebugSymbols5 Symbols;
		internal readonly IDebugSystemObjects2 SystemObjects;
		internal readonly IDebugControl6 Control;

		readonly TaskScheduler _scheduler;

		public event EventHandler<ErrorEventArgs> Error;

		void OnError(ErrorEventArgs e) {
			Error?.Invoke(this, e);
		}

		private DebugClient(object client, TaskScheduler scheduler) {
			_scheduler = scheduler;

			Client = (IDebugClient5)client;
			Control = (IDebugControl6)client;
			DataSpaces = (IDebugDataSpaces4)client;
			SystemObjects = (IDebugSystemObjects2)client;
			Symbols = (IDebugSymbols5)client;
			Advanced = (IDebugAdvanced3)client;

			Client.SetEventCallbacksWide(this).ThrowIfFailed();
			Client.SetOutputCallbacksWide(this).ThrowIfFailed();

			Control.AddEngineOptions(DEBUG_ENGOPT.INITIAL_BREAK);
		}

		public Task Break() {
			return Task.Run(() => Control.SetInterrupt(DEBUG_INTERRUPT.ACTIVE));
		}

		public Task Stop() {
			return Task.Run(() => {
				Control.SetInterrupt(DEBUG_INTERRUPT.ACTIVE);
				Client.TerminateProcesses().ThrowIfFailed();
			});
		}


		[DllImport("dbgeng", PreserveSig = true)]
		private static extern int DebugCreate(ref Guid iid, [MarshalAs(UnmanagedType.Interface)] out object iface);

		public static Task<DebugClient> CreateAsync() {
			var scheduler = new SingleThreadedTaskScheduler();
			return Task.Factory.StartNew(() => {
				var iid = typeof(IDebugClient).GUID;
				object client;
				DebugCreate(ref iid, out client).ThrowIfFailed();
				return new DebugClient(client, scheduler);
			}, CancellationToken.None, TaskCreationOptions.None, scheduler);
		}


		internal Task<T> RunAsync<T>(Func<T> method) {
			return Task.Factory.StartNew(() => method(), CancellationToken.None, TaskCreationOptions.None, _scheduler);
		}

		internal Task RunAsync(Action method) {
			return Task.Factory.StartNew(method, CancellationToken.None, TaskCreationOptions.None, _scheduler);
		}

		private int WaitForEvent(uint msec = uint.MaxValue) {
			return Control.WaitForEvent(DEBUG_WAIT.DEFAULT, msec);
		}

		public void AttachToLocalKernel() {
			RunAsync(() => {
				int hr = Client.AttachKernel(DEBUG_ATTACH.LOCAL_KERNEL, null);
				if (FAILED(hr))
					OnError(new ErrorEventArgs(DebuggerError.LocalKernelAttachFailed, hr));
				else
					WaitForEvent();
			});
		}

		public void AttachToKernel(string options) {
			RunAsync(() => {
				int hr = Client.AttachKernelWide(DEBUG_ATTACH.KERNEL_CONNECTION, options);
				if (FAILED(hr)) {
					OnError(new ErrorEventArgs(DebuggerError.KernelAttachFailed, hr, options));
					return;
				}
				WaitForEvent();
			});
		}

		public void AttachToKernelWithComPipe(string pipeName, string serverName = ".", bool reconnect = true, int resets = 0) {
			var connection = string.Format(@"com:pipe,port=\\{0}\pipe\{1},resets={2}", serverName, pipeName, resets);
			if (reconnect)
				connection += ",reconnect";
			AttachToKernel(connection);
		}

		public Task AttachToProcess(int pid, AttachProcessFlags attachFlag = AttachProcessFlags.Invasive) {
			return RunAsync(() => {
				Client.AttachProcess(0, (uint)pid, (DEBUG_ATTACH)attachFlag).ThrowIfFailed();
				WaitForEvent().ThrowIfFailed();
			});
		}

		public Task DebugProcess(string commandLine, string args, AttachProcessFlags attachFlags, bool debugChildProcesses = false) {
			return RunAsync(() => {
				var options = new DEBUG_CREATE_PROCESS_OPTIONS {
					CreateFlags = DEBUG_CREATE_PROCESS.DEBUG_PROCESS
				};
				if (!debugChildProcesses)
					options.CreateFlags |= DEBUG_CREATE_PROCESS.DEBUG_ONLY_THIS_PROCESS;

				Client.CreateProcessAndAttach2Wide(0, commandLine + " " + (args ?? string.Empty), ref options, (uint)Marshal.SizeOf<DEBUG_CREATE_PROCESS_OPTIONS>(), 
					null, null, 0, (DEBUG_ATTACH)attachFlags).ThrowIfFailed();
				WaitForEvent().ThrowIfFailed();
			});
		}

		public Task OpenDumpFile(string file) {
			return RunAsync(() => {
				Client.OpenDumpFileWide(file, 0).ThrowIfFailed();
				WaitForEvent();
			});
		}

		public IEnumerable<SymbolInfo> FindSymbols(string pattern) {
			ulong handle;
			Symbols.StartSymbolMatchWide(pattern, out handle).ThrowIfFailed();
			var sb = new StringBuilder(256);
			uint matchSize;
			ulong offset;
			for (;;) {
				int hr = Symbols.GetNextSymbolMatchWide(handle, sb, 255, out matchSize, out offset);
				if (hr < 0)
					break;

				yield return new SymbolInfo {
					Name = sb.ToString(),
					Offset = offset
				};
			}
			Symbols.EndSymbolMatch(handle);
		}

		public Task<TargetModule[]> GetModules() {
			return RunAsync(() => {
				uint loaded, unloaded;
				Symbols.GetNumberModules(out loaded, out unloaded).ThrowIfFailed();
				for (uint i = 0; i < loaded; i++) {
					ulong moduleBase;
					Symbols.GetModuleByIndex(i, out moduleBase).ThrowIfFailed();
				}
				var modules = new DEBUG_MODULE_PARAMETERS[loaded + unloaded];
				Symbols.GetModuleParameters(loaded + unloaded, null, 0, modules).ThrowIfFailed();
				return modules.Select(param => TargetModule.FromModuleParameters(param)).ToArray();
			});
		}

		public Task<bool> Execute(string command) {
			return RunAsync(() => {
				Control.ExecuteWide(DEBUG_OUTCTL.ALL_CLIENTS, command, DEBUG_EXECUTE.DEFAULT);
				return DoPostCommand();
			});
		}

		public Task<Breakpoint> CreateBreakpoint(DEBUG_BREAKPOINT_TYPE type, uint id = uint.MaxValue) {
			return RunAsync(() => {
				IDebugBreakpoint bp;
				Control.AddBreakpoint(type, id, out bp).ThrowIfFailed();
				return new Breakpoint(this, (IDebugBreakpoint3)bp);
			});
		}

		public IReadOnlyList<Breakpoint> GetBreakpoints() {
			return RunAsync(() => {
				uint count;
				Control.GetNumberBreakpoints(out count).ThrowIfFailed();
				var breakpoints = new List<Breakpoint>((int)count);

				for (uint i = 0; i < count; i++) {
					IDebugBreakpoint bp;
					Control.GetBreakpointByIndex(i, out bp);
					breakpoints.Add(new Breakpoint(this, (IDebugBreakpoint3)bp));
				}
				return breakpoints;
			}).Result;
		}

		public Breakpoint GetBreakpointById(uint breakpointId) {
			return RunAsync(() => {
				IDebugBreakpoint bp;
				Control.GetBreakpointById(breakpointId, out bp).ThrowIfFailed();
				return new Breakpoint(this, (IDebugBreakpoint3)bp);
			}).Result; 
		}

		public void DeleteBreakpoint(Breakpoint bp) {
			RunAsync(() => bp.Remove());
		}

		private bool DoPostCommand() {
			var status = UpdateStatus();

			Control.OutputPromptWide(DEBUG_OUTCTL.THIS_CLIENT, null);

			if (status == DEBUG_STATUS.NO_DEBUGGEE) {
				Client.EndSession(DEBUG_END.ACTIVE_TERMINATE);
				return false;
			}

			if (status == DEBUG_STATUS.GO || status == DEBUG_STATUS.STEP_BRANCH || status == DEBUG_STATUS.STEP_INTO || status == DEBUG_STATUS.STEP_OVER) {
				if (Control.WaitForEvent(DEBUG_WAIT.DEFAULT, uint.MaxValue) < 0) {
					UpdateStatus();
					if (Status == DEBUG_STATUS.NO_DEBUGGEE)
						return false;
				}
				Control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);
				_stateChanged = true;
			}

			if (_stateChanged) {
				//UpdateStatus();
				if (Status == DEBUG_STATUS.NO_DEBUGGEE)
					return false;

				_stateChanged = false;
				if (_breakpointHit) {
					//Control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);
					_breakpointHit = false;

					UpdateStatus();
				}
				Control.OutputPromptWide(DEBUG_OUTCTL.THIS_CLIENT, null);
			}
			return true;
		}

		const int S_OK = 0;
		const int E_FAIL = unchecked((int)0x80004005);

		bool FAILED(int hr) => hr < 0;
		bool SUCCEEDED(int hr) => hr >= 0;

		string FixModuleName(string moduleName) {
			if (string.IsNullOrEmpty(moduleName)) {
				return string.Empty;
			}

			if ((string.Compare("ntdll", moduleName, StringComparison.OrdinalIgnoreCase) == 0)) {
				string ntdllname = GetNtdllName();
				return ntdllname;
			}

			return moduleName;
		}

		bool IsPointer64Bit() {
			return Control.IsPointer64Bit() == S_OK;
		}

		unsafe string GetNtdllName() {
			var sb = new StringBuilder(64);
			uint srcSize, dstSize;
			Control.GetTextReplacementWide("$ntdllsym", 0, null, 0, &srcSize, sb, 128, &dstSize);
			string ntdll = sb.ToString();
			if (ntdll.StartsWith("nt")) {
				return ntdll;
			}

			return "ntdll";
		}

		unsafe int GetModuleBase(string moduleName, out ulong moduleBase) {
			moduleName = FixModuleName(moduleName);

			int hr;
			ulong tempModuleBase;
			if (SUCCEEDED(hr = Symbols.GetModuleByModuleNameWide(moduleName, 0, null, &tempModuleBase))) {
				if (tempModuleBase == 0) {
					moduleBase = 0;
					return E_FAIL;
				}
				moduleBase = IsPointer64Bit() ? tempModuleBase : Utilities.SignExtendAddress(tempModuleBase);
			}
			else {
				moduleBase = 0;
			}
			return hr;
		}

		int GetTypeSize(string moduleName, string typeName, out uint size) {
			if (typeName.EndsWith("*")) {
				if (IsPointer64Bit()) {
					size = 8;
				}
				else {
					size = 4;
				}
				return S_OK;
			}

			moduleName = FixModuleName(moduleName);

			ulong moduleBase;
			int hr = GetModuleBase(moduleName, out moduleBase);
			if (FAILED(hr)) {
				size = 0;
				return hr;
			}

			uint typeId;
			hr = GetTypeId(moduleName, typeName, out typeId);
			if (FAILED(hr)) {
				size = 0;
				return hr;
			}


			hr = Symbols.GetTypeSize(moduleBase, typeId, out size);
			return hr;
		}

		public int ReadPointer(ulong offset, out ulong value) {
			var pointerArray = new ulong[1];
			int hr = DataSpaces.ReadPointersVirtual(1, offset, pointerArray);
			value = SUCCEEDED(hr) ? pointerArray[0] : 0UL;
			return hr;
		}

		public ulong ReadPointer(ulong offset) {
			var pointerArray = new ulong[1];
			DataSpaces.ReadPointersVirtual(1, offset, pointerArray).ThrowIfFailed();

			return pointerArray[0];
		}

		public int ReadPointers(ulong offset, uint count, out ulong[] values) {

			int hr = ReadPointers(offset, count, out values);
			if (FAILED(hr)) {
				values = null;
			}
			return hr;
		}

		public unsafe int GetSymbolTypeIdWide(string symbolName, out uint typeId, out ulong moduleBase) {
			symbolName = symbolName.TrimEnd();
			while (symbolName.EndsWith("*")) {
				symbolName = symbolName.Substring(0, symbolName.Length - 1).TrimEnd();
			}

			if (symbolName.EndsWith("]")) {
				symbolName = symbolName.Substring(0, symbolName.LastIndexOf('[')).TrimEnd();
			}

			if ((symbolName.StartsWith("<") && symbolName.EndsWith(">")) || symbolName == "void" || symbolName.EndsWith("!void") || symbolName.Contains("!unsigned ") || symbolName.StartsWith("unsigned")) {
				typeId = 0;
				moduleBase = 0;
				return E_FAIL;
			}

			int hr;
			ulong module;
			hr = Symbols.GetSymbolTypeIdWide(symbolName, out typeId, &module);
			if (FAILED(hr)) {
				if (typeId == 0) {
					if (symbolName.Contains(" __ptr64") || symbolName.Contains(" __ptr32")) {
						string newsymbolName = symbolName.Replace(" __ptr64", string.Empty).Replace(" __ptr32", string.Empty);
						hr = Symbols.GetSymbolTypeIdWide(newsymbolName, out typeId, &module);
					}
				}
			}

			moduleBase = module;
			return hr;
		}
		public int GetFieldOffset(string symbolName, string fieldName, out uint offset) {
			var part1 = string.Empty;
			var part2 = symbolName;

			if (symbolName.Contains("!")) {
				var symbolparts = symbolName.Split("!".ToCharArray(), 2);
				part1 = symbolparts[0];
				part2 = symbolparts[1];
			}
			return GetFieldOffset(part1, part2, fieldName, out offset);
		}

		public unsafe int GetFieldOffset(ulong moduleBase, uint typeId, string fieldName, out uint offset) {
			uint fieldOffset;
			int hr = Symbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, null, &fieldOffset);
			if (SUCCEEDED(hr)) {
				offset = fieldOffset;
			}
			else {
				offset = 0;
			}
			return hr;
		}

		public uint GetFieldOffset(string symbolName, string fieldName) {
			string part1 = "";
			string part2 = symbolName;
			UInt32 offset;

			if (symbolName.Contains("!")) {
				string[] symbolparts = symbolName.Split("!".ToCharArray(), 2);
				part1 = symbolparts[0];
				part2 = symbolparts[1];
			}
			GetFieldOffset(part1, part2, fieldName, out offset).ThrowIfFailed();

			return offset;
		}

		public unsafe int GetFieldOffset(string moduleName, string typeName, string fieldName, out uint offset) {
			moduleName = FixModuleName(moduleName);

			ulong moduleBase;
			uint typeId;
			int hr = GetSymbolTypeIdWide(moduleName + "!" + typeName, out typeId, out moduleBase);
			if (FAILED(hr)) {
				offset = 0;
				return hr;
			}

			uint fieldOffset;
			hr = Symbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, null, &fieldOffset);
			if (SUCCEEDED(hr)) {
				offset = fieldOffset;
			}
			else {
				offset = 0;
			}
			return hr;
		}

		int GetTypeId(string moduleName, string typeName, out UInt32 typeId) {
			moduleName = FixModuleName(moduleName);

			int hr;

			string fqModule = moduleName + "!" + typeName;

			if (FAILED(hr = Symbols.GetTypeIdWide(0, fqModule, out typeId))) {
				Debug.WriteLine($"GetTypeId Failed: {moduleName} {typeName} {typeId}");
				typeId = 0;
			}

			return hr;
		}

		unsafe public int GetFieldValue(ulong moduleBase, uint typeId, string fieldName, ulong structureAddress, out ulong fieldValue) {
			int hr;
			fieldValue = 0;

			_EXT_TYPED_DATA symbolTypedData;

			symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;

			symbolTypedData.InData.ModBase = moduleBase;
			symbolTypedData.InData.TypeId = typeId;

			symbolTypedData.InData.Offset = structureAddress;

			if (FAILED(hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null))) {
				return hr;
			}

			IntPtr buffer = IntPtr.Zero;
			IntPtr memPtr = IntPtr.Zero;
			try {
				_EXT_TYPED_DATA temporaryTypedDataForBufferConstruction;

				memPtr = Marshal.StringToHGlobalAnsi(fieldName);
				int totalSize = sizeof(_EXT_TYPED_DATA) + fieldName.Length + 1; //+1 to account for the null terminator
				buffer = Marshal.AllocHGlobal(totalSize);

				temporaryTypedDataForBufferConstruction.Operation = _EXT_TDOP.EXT_TDOP_GET_FIELD;
				temporaryTypedDataForBufferConstruction.InData = symbolTypedData.OutData;
				temporaryTypedDataForBufferConstruction.InStrIndex = (uint)sizeof(_EXT_TYPED_DATA);
				Utilities.CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));
				Utilities.CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);
				if (FAILED(hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null))) {
					return hr;
				}

				var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

				fieldValue = typedDataInClassForm.OutData.Data;
			}
			finally {
				Marshal.FreeHGlobal(buffer);
				Marshal.FreeHGlobal(memPtr);
			}

			return S_OK;
		}

		unsafe public Task<ulong> GetFieldVirtualAddress(string moduleName, string typeName, string fieldName, ulong structureAddress) {
			return RunAsync(() => GetFieldVirtualAddressInternal(moduleName, typeName, fieldName, structureAddress));
		}

		public Task<ulong> GetGlobalAddress(string moduleName, string globalName) {
			return RunAsync(() => GetGlobalAddressInternal(moduleName, globalName));
		}

		ulong GetGlobalAddressInternal(string moduleName, string globalName) {
			moduleName = FixModuleName(moduleName);

			ulong tempAddress;
			int hr = Symbols.GetOffsetByNameWide(moduleName + "!" + globalName, out tempAddress);
			if (SUCCEEDED(hr)) {
				if (tempAddress != 0) {
					return IsPointer64Bit() ? tempAddress : Utilities.SignExtendAddress(tempAddress);
				}
			}
			return 0;
		}

		private unsafe ulong GetFieldVirtualAddressInternal(string moduleName, string typeName, string fieldName, ulong structureAddress) {
			moduleName = FixModuleName(moduleName);

			ulong FieldAddress = 0;

			uint typeId;

			ulong moduleBase;
			typeName = typeName.TrimEnd();
			if (typeName.EndsWith("*")) {
				typeName = typeName.Substring(0, typeName.Length - 1).TrimEnd();
				ReadPointer(structureAddress, out structureAddress);
				if (structureAddress == 0) {
					return 0;
				}
			}

			ulong savedStructAddr = structureAddress;
			bool slow = false;
			uint offset = 0;

			int hr = GetFieldOffset(moduleName, typeName, fieldName, out offset);

			if (hr < 0) {
				return 0;
			}

			if (offset == 0) {
				slow = true;
			}
			else {
				uint typeSize = 0;
				hr = GetTypeSize(moduleName, typeName, out typeSize);
				if (typeSize == 0 || offset > typeSize) {
					slow = true;
				}
			}

			if (slow == false) {
				return FieldAddress = structureAddress + offset;
			}

			hr = GetSymbolTypeIdWide(typeName, out typeId, out moduleBase);

			if (hr < 0) {
				GetModuleBase(moduleName, out moduleBase);
				hr = GetTypeId(moduleName, typeName, out typeId);
				if (hr < 0) {
					return 0;
				}
			}

			_EXT_TYPED_DATA symbolTypedData;

			symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;

			symbolTypedData.InData.ModBase = moduleBase;
			symbolTypedData.InData.TypeId = typeId;

			symbolTypedData.InData.Offset = structureAddress;

			if ((hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)) < 0) {
				if (offset == 0) {
					return FieldAddress = savedStructAddr;
				}
				return 0;
			}

			IntPtr buffer = IntPtr.Zero;
			IntPtr memPtr = IntPtr.Zero;
			try {
				_EXT_TYPED_DATA temporaryTypedDataForBufferConstruction;

				// Allocate Buffers.

				memPtr = Marshal.StringToHGlobalAnsi(fieldName);
				int totalSize = sizeof(_EXT_TYPED_DATA) + fieldName.Length + 1;
				buffer = Marshal.AllocHGlobal(totalSize);

				temporaryTypedDataForBufferConstruction.Operation = _EXT_TDOP.EXT_TDOP_GET_FIELD;

				temporaryTypedDataForBufferConstruction.InData = symbolTypedData.OutData;

				temporaryTypedDataForBufferConstruction.InStrIndex = (uint)sizeof(_EXT_TYPED_DATA);

				Utilities.CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

				Utilities.CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

				if ((hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)) < 0) {
					if (offset == 0) {
						return FieldAddress = savedStructAddr;
					}
					return FieldAddress;
				}

				var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

				FieldAddress = typedDataInClassForm.OutData.Offset;

				if (FieldAddress < savedStructAddr && offset == 0) {
					FieldAddress = savedStructAddr;
				}
			}
			finally {
				Marshal.FreeHGlobal(buffer);
				Marshal.FreeHGlobal(memPtr);
			}

			return FieldAddress;
		}

		public Task OutputPrompt(DEBUG_OUTCTL outControl = DEBUG_OUTCTL.THIS_CLIENT, string format = null) {
			return RunAsync(() => Control.OutputPromptWide(outControl, format));
		}

		public Task Detach() {
			return RunAsync(() => Client.DetachCurrentProcess());
		}

		public Task DetachAll() {
			return RunAsync(() => Client.DetachProcesses());
		}

		public event EventHandler<StatusChangedEventArgs> StatusChanged;

		private void OnStatusChanged(StatusChangedEventArgs e) {
			StatusChanged?.Invoke(this, e);
		}

		public DEBUG_STATUS Status { get; private set; } = DEBUG_STATUS.NO_DEBUGGEE;

		private DEBUG_STATUS UpdateStatus(bool force = false) {
			DEBUG_STATUS status;
			Control.GetExecutionStatus(out status);
			if (Status != status) {
				var args = new StatusChangedEventArgs(Status, status);
				Status = status;
				OnStatusChanged(args);
			}
			return Status;
		}

		public void Dispose() {
			Dispose(true);
		}

		~DebugClient() {
			Dispose(false);
		}

		protected async virtual void Dispose(bool isDisposing) {
			if (isDisposing) {
				GC.SuppressFinalize(this);
			}
			await RunAsync(() => Client.DetachProcesses());
			((IDisposable)_scheduler)?.Dispose();
		}
	}
}
