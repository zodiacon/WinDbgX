using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DebuggerEngine.Interop;

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

        private DebugClient(object client, TaskScheduler scheduler) {
            _scheduler = scheduler;

            Client = (IDebugClient5)client;
            Control = (IDebugControl6)client;
            DataSpaces = (IDebugDataSpaces4)client;
            SystemObjects = (IDebugSystemObjects2)client;
            Symbols = (IDebugSymbols5)client;
            Advanced = (IDebugAdvanced3)client;

            Client.SetEventCallbacksWide(new EventCallbacks(Control)).ThrowIfFailed();
            Client.SetOutputCallbacksWide(new OutputCallbacks()).ThrowIfFailed();
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


        private Task<T> RunAsync<T>(Func<T> method) {
            return Task.Factory.StartNew(() => method(), CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private Task RunAsync(Action method) {
            return Task.Factory.StartNew(method, CancellationToken.None, TaskCreationOptions.None, _scheduler);
        }

        private void WaitForEvent(uint msec = uint.MaxValue) {
            Control.WaitForEvent(DEBUG_WAIT.DEFAULT, msec).ThrowIfFailed();
        }

        public Task AttachToLocalKernel() {
            return RunAsync(() => {
                Client.AttachKernel(DEBUG_ATTACH.LOCAL_KERNEL, null).ThrowIfFailed();
                WaitForEvent();
            });
        }

        public Task AttachToKernel(string options) {
            return RunAsync(() => {
                Client.AttachKernelWide(DEBUG_ATTACH.KERNEL_CONNECTION, options).ThrowIfFailed();
                WaitForEvent();
            });
        }

        public Task AttachToKernelWithComPipeAsync(string pipeName, string serverName = ".", bool reconnect = true, int resets = 0) {
            var connection = string.Format(@"com:pipe,port=\\{0}\pipe\{1},resets={2}", serverName, pipeName, resets);
            if(reconnect)
                connection += ",reconnect";
            return AttachToKernel(connection);
        }

        public Task AttachToProcess(int pid, AttachProcessFlags attachFlag = AttachProcessFlags.Invasive) {
            return RunAsync(() => {
                Client.AttachProcess(0, (uint)pid, (DEBUG_ATTACH)attachFlag).ThrowIfFailed();
                WaitForEvent();
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
            for(;;) {
                int hr = Symbols.GetNextSymbolMatchWide(handle, sb, 255, out matchSize, out offset);
                if(hr < 0)
                    break;

                yield return new SymbolInfo {
                    Name = sb.ToString(),
                    Offset = offset
                };
            }
            Symbols.EndSymbolMatch(handle);
        }

        public Task<ModuleInfo[]> GetModules() {
            return RunAsync(() => {
                uint loaded, unloaded;
                Symbols.GetNumberModules(out loaded, out unloaded).ThrowIfFailed();
                for(uint i = 0; i < loaded; i++) {
                    ulong moduleBase;
                    Symbols.GetModuleByIndex(i, out moduleBase).ThrowIfFailed();
                }
                var modules = new DEBUG_MODULE_PARAMETERS[loaded + unloaded];
                Symbols.GetModuleParameters(loaded + unloaded, null, 0, modules).ThrowIfFailed();
                return modules.Select(param => ModuleInfo.FromModuleParameters(param)).ToArray();
            });
        }

        public Task ExecuteAsync(string command) {
            return RunAsync(() => {
                Control.ExecuteWide(DEBUG_OUTCTL.ALL_CLIENTS, command, DEBUG_EXECUTE.ECHO);
            });
        }

        const int S_OK = 0;
        const int E_FAIL = unchecked((int)0x80004005);

        bool FAILED(int hr) => hr < 0;
        bool SUCCEEDED(int hr) => hr >= 0;

        string FixModuleName(string moduleName) {
            if(string.IsNullOrEmpty(moduleName)) {
                return string.Empty;
            }

            if((string.Compare("ntdll", moduleName, StringComparison.OrdinalIgnoreCase) == 0)) {
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
            if(ntdll.StartsWith("nt")) {
                return ntdll;
            }

            return "ntdll";
        }

        unsafe int GetModuleBase(string moduleName, out ulong moduleBase) {
            moduleName = FixModuleName(moduleName);

            int hr;
            ulong tempModuleBase;
            if(SUCCEEDED(hr = Symbols.GetModuleByModuleNameWide(moduleName, 0, null, &tempModuleBase))) {
                if(tempModuleBase == 0) {
                    moduleBase = 0;
                    //Cache.GetModuleBase.Add("-" + moduleName, moduleBase);
                    return E_FAIL;
                }
                moduleBase = IsPointer64Bit() ? tempModuleBase : Utilities.SignExtendAddress(tempModuleBase);
                // Add to cache
                //Cache.GetModuleBase.Add(moduleName, moduleBase);
            }
            else {
                moduleBase = 0;
                //Cache.GetModuleBase.Add("-" + moduleName, moduleBase);
            }
            return hr;
        }

        int GetTypeSize(string moduleName, string typeName, out uint size) {
            if(typeName.EndsWith("*")) {
                if(IsPointer64Bit()) {
                    size = 8;
                }
                else {
                    size = 4;
                }
                return S_OK;
            }

            moduleName = FixModuleName(moduleName);

            //if(Cache.GetTypeSize.TryGetValue(moduleName + "!" + typeName, out size)) {
            //    return S_OK;
            //}

            ulong moduleBase;
            int hr = GetModuleBase(moduleName, out moduleBase);
            if(FAILED(hr)) {
                size = 0;
                return hr;
            }

            uint typeId;
            hr = GetTypeId(moduleName, typeName, out typeId);
            if(FAILED(hr)) {
                size = 0;
                return hr;
            }


            hr = Symbols.GetTypeSize(moduleBase, typeId, out size);
            if(SUCCEEDED(hr)) {
                //Cache.GetTypeSize.Add(moduleName + "!" + typeName, size);
            }
            return hr;
        }

        /// <summary>
        ///     Reads a single pointer from the target.
        ///     NOTE: POINTER VALUE IS SIGN EXTENDED TO 64-BITS WHEN NECESSARY!
        /// </summary>
        /// <param name="offset">The address to read the pointer from</param>
        /// <param name="value">A ulong to receive the pointer</param>
        /// <returns>HRESULT of the operation</returns>
        public int ReadPointer(ulong offset, out ulong value) {
            var pointerArray = new ulong[1];
            int hr = DataSpaces.ReadPointersVirtual(1, offset, pointerArray);
            //ReadPointersVirtual is supposed to sign-extend but isn't.
            //value = SUCCEEDED(hr) ? IsPointer64Bit() ? pointerArray[0] : SignExtendAddress(pointerArray[0]) : 0UL;
            value = SUCCEEDED(hr) ? pointerArray[0] : 0UL;
            return hr;
        }

        /// <summary>
        ///     Reads a single pointer from the target.
        ///     NOTE: POINTER VALUE IS SIGN EXTENDED TO 64-BITS WHEN NECESSARY!
        /// </summary>
        /// <param name="offset">The address to read the pointer from</param>
        /// <returns>The pointer</returns>
        public ulong ReadPointer(ulong offset) {
            var pointerArray = new ulong[1];
            DataSpaces.ReadPointersVirtual(1, offset, pointerArray).ThrowIfFailed();

            return pointerArray[0];
        }


        /// <summary>
        ///     Reads the specified number of pointers from the target
        /// </summary>
        /// <param name="offset">The address from which to begin reading</param>
        /// <param name="count">The number of pointers to be read</param>
        /// <param name="values">Pointers being returned</param>
        /// <returns>HRESULT of the operation</returns>
        public int ReadPointers(ulong offset, uint count, out ulong[] values) {

            int hr = ReadPointers(offset, count, out values);
            if(FAILED(hr)) {
                values = null;
            }
            return hr;
        }

        public unsafe int GetSymbolTypeIdWide(string symbolName, out UInt32 typeId, out ulong moduleBase) {
            //TypeInfoCache tInfo;
            symbolName = symbolName.TrimEnd();
            while(symbolName.EndsWith("*")) {
                symbolName = symbolName.Substring(0, symbolName.Length - 1).TrimEnd();
            }

            if(symbolName.EndsWith("]")) {
                symbolName = symbolName.Substring(0, symbolName.LastIndexOf('[')).TrimEnd();
            }

            if((symbolName.StartsWith("<") && symbolName.EndsWith(">")) || symbolName == "void" || symbolName.EndsWith("!void") || symbolName.Contains("!unsigned ") || symbolName.StartsWith("unsigned")) {
                typeId = 0;
                moduleBase = 0;
                return E_FAIL;
            }

            //if(Cache.GetSymbolTypeIdWide.TryGetValue(symbolName, out tInfo)) {
            //    typeId = tInfo.TypeId;
            //    moduleBase = tInfo.Modulebase;
            //    return MexFrameworkClass.S_OK;
            //}

            //if (Cache.GetSymbolTypeIdWide.TryGetValue("-" + symbolName, out tInfo))
            //{
            //    typeId = 0;
            //    moduleBase = 0;
            //    return MexFrameworkClass.E_FAIL;
            //}
            int hr;
            ulong module;
            hr = Symbols.GetSymbolTypeIdWide(symbolName, out typeId, &module);
            if(FAILED(hr)) {
                if(typeId == 0) {
                    if(symbolName.Contains(" __ptr64") || symbolName.Contains(" __ptr32")) {
                        string newsymbolName = symbolName.Replace(" __ptr64", string.Empty).Replace(" __ptr32", string.Empty);
                        hr = Symbols.GetSymbolTypeIdWide(newsymbolName, out typeId, &module);
                    }
                }
            }

            if(FAILED(hr)) {
                moduleBase = module;
            }
            else {
                moduleBase = module;
                //tInfo.Modulebase = module;
                //moduleBase = module;
                //tInfo.TypeId = typeId;
                //Cache.GetSymbolTypeIdWide.Add(symbolName, tInfo);
            }
            return hr;
        }
        public int GetFieldOffset(string symbolName, string fieldName, out UInt32 offset) {
            var part1 = string.Empty;
            var part2 = symbolName;

            if(symbolName.Contains("!")) {
                var symbolparts = symbolName.Split("!".ToCharArray(), 2);
                part1 = symbolparts[0];
                part2 = symbolparts[1];
            }
            return GetFieldOffset(part1, part2, fieldName, out offset);
        }

        public unsafe int GetFieldOffset(ulong moduleBase, uint typeId, string fieldName, out UInt32 offset) {
            uint fieldOffset;
            int hr = Symbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, null, &fieldOffset);
            if(SUCCEEDED(hr)) {
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

            if(symbolName.Contains("!")) {
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
            if(FAILED(hr)) {
                offset = 0;
                return hr;
            }

            uint fieldOffset;
            hr = Symbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, null, &fieldOffset);
            if(SUCCEEDED(hr)) {
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

            if(FAILED(hr = Symbols.GetTypeIdWide(0, fqModule, out typeId))) {
                //OutputErrorLine("Failed: {0} {1} {2}", moduleName, typeName, typeId);
                typeId = 0;
            }

            return hr;
        }

        ///     Gets the value of any field. Especially useful for bitfields.
        ///     MemberName is smart. It will take value.value, and if it encounters a pointer, it will dereference them.  ie :
        ///     Value.Value->Value, would look like: Value.Value.Value in the MemberName argument
        /// </summary>
        unsafe public int GetFieldValue(ulong moduleBase, uint typeId, string fieldName, UInt64 structureAddress, out ulong fieldValue) {
            int hr;
            fieldValue = 0;

            // Make a new Typed Data structure.
            _EXT_TYPED_DATA symbolTypedData;

            // Fill it in from ModuleBase and TypeID
            // Note, we could use EXT_TDOP_SET_PTR_FROM_TYPE_ID_AND_U64 if this was a pointer to the object
            symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;

            symbolTypedData.InData.ModBase = moduleBase;
            symbolTypedData.InData.TypeId = typeId;

            symbolTypedData.InData.Offset = structureAddress;

            if(FAILED(hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null))) {
                return hr;
            }

            IntPtr buffer = IntPtr.Zero;
            IntPtr memPtr = IntPtr.Zero;
            try {
                _EXT_TYPED_DATA temporaryTypedDataForBufferConstruction;

                // Allocate Buffers.

                memPtr = Marshal.StringToHGlobalAnsi(fieldName);
                int totalSize = sizeof(_EXT_TYPED_DATA) + fieldName.Length + 1; //+1 to account for the null terminator
                buffer = Marshal.AllocHGlobal(totalSize);

                // Get_Field. This does all the magic.
                temporaryTypedDataForBufferConstruction.Operation = _EXT_TDOP.EXT_TDOP_GET_FIELD;

                // Pass in the OutData from the first call to Request(), so it knows what symbol to use
                temporaryTypedDataForBufferConstruction.InData = symbolTypedData.OutData;

                // The index of the string will be immediately following the _EXT_TYPED_DATA structure
                temporaryTypedDataForBufferConstruction.InStrIndex = (uint)sizeof(_EXT_TYPED_DATA);

                // Source is our _EXT_TYPED_DATA structure, Dest is our empty allocated buffer
                Utilities.CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Copy the ANSI string of our member name immediately after the TypedData Structure.
                // Source is our ANSI Buffer, Dest is the byte immediately after the last byte from the previous copy

                // This fails if we use i<MemberName.Length, made it i<= to copy the null terminator.
                Utilities.CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if(FAILED(hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null))) {
                    return hr;
                }

                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

                // OutData.Data has our field value.  This will always be a ulong
                fieldValue = typedDataInClassForm.OutData.Data;
                if(fieldName.Contains("RunningThreadGoal")) {
                }
            }
            finally {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(memPtr);
            }

            return S_OK;
        }

        /// <summary>
        ///     Gets the virtual Address of a field.  Useful for Static Fields
        ///     MemberName is smart. It will take value.value, and if it encounters a pointer, it will dereference them.  ie :
        ///     Value.Value->Value, would look like: Value.Value.Value in the MemberName argument
        /// </summary>
        unsafe public Task<ulong> GetFieldVirtualAddress(string moduleName, string typeName, string fieldName, ulong structureAddress) {
            return RunAsync(() => GetFieldVirtualAddressInternal(moduleName, typeName, fieldName, structureAddress));
        } 
        /// <summary>
        ///     Retrieves the address of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="address">UInt64 to receive the address</param>
        /// <returns>HRESULT</returns>
        public Task<ulong> GetGlobalAddress(string moduleName, string globalName) {
            return RunAsync(() => GetGlobalAddressInternal(moduleName, globalName));
        }

        ulong GetGlobalAddressInternal(string moduleName, string globalName) {
            moduleName = FixModuleName(moduleName);

            UInt64 tempAddress;
            int hr = Symbols.GetOffsetByNameWide(moduleName + "!" + globalName, out tempAddress);
            if(SUCCEEDED(hr)) {
                if(tempAddress != 0) {
                    return IsPointer64Bit() ? tempAddress : Utilities.SignExtendAddress(tempAddress);
                }
            }
            return 0;
        }

        private unsafe ulong GetFieldVirtualAddressInternal(string moduleName, string typeName, string fieldName, ulong structureAddress) {
            moduleName = FixModuleName(moduleName);

            ulong FieldAddress = 0;

            uint typeId;

            // Get the ModuleBase
            ulong moduleBase;
            typeName = typeName.TrimEnd();
            if(typeName.EndsWith("*")) {
                typeName = typeName.Substring(0, typeName.Length - 1).TrimEnd();
                ReadPointer(structureAddress, out structureAddress);
                if(structureAddress == 0) {
                    return 0;
                }
            }

            ulong savedStructAddr = structureAddress;
            bool slow = false;
            uint offset = 0;

            int hr = GetFieldOffset(moduleName, typeName, fieldName, out offset);

            if(hr < 0) {
                return 0;
            }

            if(offset == 0) {
                slow = true;
            }
            else {
                uint typeSize = 0;
                hr = GetTypeSize(moduleName, typeName, out typeSize);
                if(typeSize == 0 || offset > typeSize) {
                    slow = true;
                }
            }

            if(slow == false) {
                return FieldAddress = structureAddress + offset;
            }

            hr = GetSymbolTypeIdWide(typeName, out typeId, out moduleBase);

            if(hr < 0) {
                GetModuleBase(moduleName, out moduleBase);
                hr = GetTypeId(moduleName, typeName, out typeId);
                if(hr < 0) {
                    return 0;
                }
            }

            // Make a new Typed Data structure.
            _EXT_TYPED_DATA symbolTypedData;

            // Fill it in from ModuleBase and TypeID
            // Note, we could use EXT_TDOP_SET_PTR_FROM_TYPE_ID_AND_U64 if this was a pointer to the object
            symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;

            symbolTypedData.InData.ModBase = moduleBase;
            symbolTypedData.InData.TypeId = typeId;

            symbolTypedData.InData.Offset = structureAddress;

            if((hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)) < 0) {
                if(offset == 0) {
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

                // Get_Field. This does all the magic.
                temporaryTypedDataForBufferConstruction.Operation = _EXT_TDOP.EXT_TDOP_GET_FIELD;

                // Pass in the OutData from the first call to Request(), so it knows what symbol to use
                temporaryTypedDataForBufferConstruction.InData = symbolTypedData.OutData;

                // The index of the string will be immediately following the _EXT_TYPED_DATA structure
                temporaryTypedDataForBufferConstruction.InStrIndex = (uint)sizeof(_EXT_TYPED_DATA);

                // Source is our _EXT_TYPED_DATA structure, Dest is our empty allocated buffer

                Utilities.CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Copy the ANSI string of our member name immediately after the TypedData Structure.
                // Source is our ANSI Buffer, Dest is the byte immediately after the last byte from the previous copy

                // This fails if we use fieldName.Length, made it +1 to copy the null terminator.
                Utilities.CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if((hr = Advanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)) < 0) {
                    if(offset == 0) {
                        return FieldAddress = savedStructAddr;
                    }
                    return FieldAddress;
                }

                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

                // OutData.Data has our field value.  This will always be a ulong

                FieldAddress = typedDataInClassForm.OutData.Offset;

                if(FieldAddress < savedStructAddr && offset == 0) {
                    FieldAddress = savedStructAddr;
                }
            }
            finally {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(memPtr);
            }

            return FieldAddress;
        }

        public void Dispose() {
            Dispose(true);
        }

        ~DebugClient() {
            Dispose(false);
        }

        protected async virtual void Dispose(bool isDisposing) {
            if(isDisposing) {
                GC.SuppressFinalize(this);
            }
            await RunAsync(() => Client.DetachProcesses().ThrowIfFailed());
            ((IDisposable)_scheduler)?.Dispose();
        }
    }
}
