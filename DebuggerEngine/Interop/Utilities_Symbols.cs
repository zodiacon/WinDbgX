using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Mex.Framework;
using StackFrame = System.Diagnostics.StackFrame;

namespace Microsoft.Mex.DotNetDbg
{
    public unsafe partial class DebugUtilities
    {
        public enum ReadUNICODE_STRINGOptions
        {
            Escaped = 0,
            Raw,
            Truncated,
        }

        private readonly Dictionary<string, UInt32> _knownRegisterIndexes = new Dictionary<string, UInt32>();
        private readonly StringBuilder _lookupSymbolStringBuilder = new StringBuilder(2048);

        private static string[] nameSplit(string name)
        {

            string[] stringParts = name.Split('!');

            if (stringParts.Count() != 2)
            {
                throw new Exception("string passed is invalid");
            }

            return stringParts;
        }

        /// <summary>
        ///     Sets whether the debugger engine should attempt to get symbols from network paths.
        ///     Allows you to override scenarios when the engineer disables network shares by default.
        ///     WARNING! Can cause the debugger to hang if used improperly! USE CAUTION!!!
        /// </summary>
        /// <param name="enabled">True to enable network paths, false to disable</param>
        /// <returns>Last HRESULT value returned</returns>
        public int SetNetworkSymbolsState(bool enabled)
        {
            /* This is the equivalent of executing .netsyms 0|1 */

            DEBUG_ENGOPT options;
            DebugControl.GetEngineOptions(out options);
            if (enabled)
            {
                options &= ~DEBUG_ENGOPT.DISALLOW_NETWORK_PATHS;
                options |= DEBUG_ENGOPT.ALLOW_NETWORK_PATHS;
            }
            else
            {
                options &= ~DEBUG_ENGOPT.ALLOW_NETWORK_PATHS;
                options |= DEBUG_ENGOPT.DISALLOW_NETWORK_PATHS;
            }
            int hr = DebugControl.SetEngineOptions(options);

            return hr;

            //return RunCommandAndGo(enabled ? ".netsyms 1" : ".netsyms 0", (char**)NULL);
        }

        /// <summary>
        ///     Gets whether the debugger engine should attempt to get symbols from network paths.
        /// </summary>
        /// <returns>bool value returned</returns>
        public bool GetNetworkSymbolsState()
        {
            DEBUG_ENGOPT options;
            DebugControl.GetEngineOptions(out options);
            if (!options.HasFlag(DEBUG_ENGOPT.ALLOW_NETWORK_PATHS) || (options.HasFlag(DEBUG_ENGOPT.DISALLOW_NETWORK_PATHS)))
            {
                return false;
            }
            return true;
        }
        public string GetSymbolPath()
        {

            uint bufferSize;
            string symbolPath = null;
            DebugSymbols.GetSymbolPathWide(null, 0, &bufferSize);
            if (bufferSize != 0)
            {
                ++bufferSize;
                var symbolPathBuffer = new StringBuilder((int)bufferSize);
                DebugSymbols.GetSymbolPathWide(symbolPathBuffer, symbolPathBuffer.Capacity, null);
                symbolPath = symbolPathBuffer.ToString();
            }
            return symbolPath;
        }

        public void SetSymbolPath(string path)
        {
            OutputDebugLine("Setting sympath to {0}", path);
            DebugSymbols.SetSymbolPathWide(path);
           
        }

        /// <summary>
        ///     Load symbols for a specific module.
        /// </summary>
        /// <param name="moduleName">"Name of the module to load symbols for."</param>
        /// <param name="allowNetworkPaths">Whether network paths should be searched for the symbols. Defaults to true.</param>
        /// <returns>Last HRESULT value returned</returns>
        public int LoadSymbolFor(string moduleName, bool allowNetworkPaths = true)
        {

            if (moduleName == null)
            {
                return E_FAIL;
            }

            string appendedName = "/f " + moduleName;

            /* Ignoring the errors */
            //			#if DEBUG
            RunCommandInternal("!sym noisy", false, 500);
            //			#endif

            SetNetworkSymbolsState(allowNetworkPaths);

            string executingDirectory;
            if (GetDirectoryForModule(null, out executingDirectory) == false)
            {
                return E_FAIL;
            }
            StringBuilder cachePath = new StringBuilder("cache*").Append(executingDirectory);

            uint bufferSize;
            DebugSymbols.GetSymbolPathWide(null, 0, &bufferSize);
            if (bufferSize != 0)
            {
                ++bufferSize;
                var symbolPathBuffer = new StringBuilder((int)bufferSize);
                DebugSymbols.GetSymbolPathWide(symbolPathBuffer, symbolPathBuffer.Capacity, null);
                string existingSymbolPath = symbolPathBuffer.ToString();
                string existingSymbolPathUpper = existingSymbolPath.ToUpperInvariant();
                if (existingSymbolPathUpper.Contains("CACHE*") == false)
                {
                    DebugSymbols.SetSymbolPathWide(cachePath + ";" + existingSymbolPath);
                }
                if (existingSymbolPathUpper.Contains("HTTP://MSDL.MICROSOFT.COM/DOWNLOAD/SYMBOLS") == false)
                {
                    DebugSymbols.AppendSymbolPathWide("srv*http://msdl.microsoft.com/download/symbols");
                }
            }
            else
            {
                DebugSymbols.AppendSymbolPathWide(cachePath.ToString());
#if INTERNAL
                DebugSymbols.AppendSymbolPathWide(@"srv*\\symbols\symbols");
#endif
                DebugSymbols.AppendSymbolPathWide(@"srv*http://msdl.microsoft.com/download/symbols");
            }

            int hr = DebugSymbols.ReloadWide(appendedName);

            return hr;
        }


        /// <summary>
        ///     Gets the ntdll name for the current effective architecture (x86, x64, etc).
        /// </summary>
        /// <returns>string</returns>
        public string GetNtdllName()
        {
            var sb = new StringBuilder(64);
            DebugControl.GetTextReplacementWide("$ntdllsym", 0, null, 0, null, sb, 128, null);
            string ntdll = sb.ToString();
            if (ntdll.StartsWith("nt"))
            {
                return ntdll;
            }

            return "ntdll";
        }


        /// <summary>
        ///     Gets the 32 bit ntdll name
        /// </summary>
        /// <returns>string</returns>
        public string GetNtdll32Name()
        {
            uint wow64_NtDll32Base;
            int hr = ReadGlobalAsUInt32("wow64", "NtDll32Base", out wow64_NtDll32Base);
            if (hr !=0 || wow64_NtDll32Base == 0)
            {
                OutputDebugLine("Failed to read wow64!NtDll32Base");
               // RunCommand("x wow64!NtDll32Base");
               // RunCommand("!mex.p");
                //RunCommand("!mex.context");
                throw new Exception("wow64!NtDll32Base");
            }
            return "ntdll_" + wow64_NtDll32Base.ToString("x");
        }

        /// <summary>
        ///     Gets the correct ntdll name if moduleName is ntdll, returns moduleName module otherwise
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <returns>string</returns>
        public string FixModuleName(string moduleName)
        {
            if (String.IsNullOrEmpty(moduleName))
            {
                return string.Empty;
            }

            if ((string.Compare("ntdll", moduleName, StringComparison.OrdinalIgnoreCase) == 0))
            {
                string ntdllname = GetNtdllName();
                OutputDebugLine("Returning $ntdllsym name due to '{0}' matching ntdll. New Name is {1}", moduleName, ntdllname);
                return ntdllname;
            }

            return moduleName;
        }


        /// <summary>
        ///     Get the base address for a module
        /// </summary>
        /// <param name="moduleName">Name of the module</param>
        /// <param name="moduleBase">UInt64 to receive the base address</param>
        /// <returns>HRESULT</returns>
        public int GetModuleBase(string moduleName, out UInt64 moduleBase)
        {
            moduleName = FixModuleName(moduleName);

            //if ((string.Compare("ntdll", moduleName, StringComparison.OrdinalIgnoreCase) == 0) && (wow64exts.IsEffectiveProcessorSameAsActualProcessor(this) == false))
            //{
            //    moduleName = GetNtdllName();

            //    //List<ModuleInfo> moduleList = Modules.GetLoadedModuleList(this, "^ntdll_[0-9a-fA-F]+$");
            //    //if ((moduleList != null) && (moduleList.Count > 0))
            //    //{
            //    //    moduleName = moduleList[0].Name;
            //    //    OutputVerboseLine("DotNetDbg.GetModuleBase: Wow64 detected, changing name 'ntdll' to '{0}'", moduleName);
            //    //}
            //}

            object cacheGet = Cache.GetModuleBase.Get(moduleName);
            if (cacheGet != null)
            {
                moduleBase = (UInt64)cacheGet;
                return S_OK;
            }

            cacheGet = Cache.GetModuleBase.Get("-" + moduleName);
            if (cacheGet != null)
            {
                moduleBase = (UInt64)cacheGet;
                return E_FAIL;
            }
            // Check Cache, return if cached.


            int hr;
            UInt64 tempModuleBase = 0;
            if (SUCCEEDED(hr = DebugSymbols.GetModuleByModuleNameWide(moduleName, 0, null, &tempModuleBase)))
            {
                if (tempModuleBase == 0)
                {
                    moduleBase = 0;
                    Cache.GetModuleBase.Add("-" + moduleName, moduleBase);
                    return E_FAIL;
                }
                moduleBase = IsPointer64Bit() ? tempModuleBase : SignExtendAddress(tempModuleBase);
                // Add to cache
                Cache.GetModuleBase.Add(moduleName, moduleBase);
            }
            else
            {
                moduleBase = 0;
                Cache.GetModuleBase.Add("-" + moduleName, moduleBase);
            }
            return hr;
        }


        ///// <summary>
        ///// Gets the ID for a type in a module
        ///// </summary>
        ///// <param name="moduleBase">Base address of the module</param>
        ///// <param name="typeName">Name of the type</param>
        ///// <param name="typeId">UInt32 to receive the type's ID</param>
        ///// <returns>HRESULT</returns>
        //public int GetTypeId(UInt64 moduleBase, string typeName, out UInt32 typeId)
        //{
        //    int hr;

        //    if (Cache.GetTypeId.TryGetValue(moduleBase.ToString("x") + "#" + typeName, out typeId))
        //    {
        //        return MexMain.S_OK;
        //    }

        //    if (FAILED(hr = DebugSymbols.GetTypeIdWide(moduleBase, typeName, out typeId)))
        //    {
        //        typeId = 0;
        //    }
        //    else
        //    {
        //        Cache.GetTypeId.Add(moduleBase.ToString("x") + "#" + typeName, typeId);
        //    }
        //    return hr;
        //}


        /// <summary>
        ///     Determine if a type is valid
        /// </summary>
        /// <param name="moduleName">Base name of the module</param>
        /// <param name="typeName">Name of the type</param>
        /// <returns>bool</returns>
        public bool IsTypeValid(string moduleName, string typeName)
        {
            moduleName = FixModuleName(moduleName);

            uint typeId;
            string fqModule = moduleName + "!" + typeName;

            if (Cache.GetTypeId.TryGetValue(fqModule, out typeId))
            {
                return true;
            }

            if (Cache.GetTypeId.TryGetValue("-" + fqModule, out typeId))
            {
                return false;
            }

            if (FAILED(DebugSymbols.GetTypeIdWide(0, fqModule, out typeId)))
            {
                //OutputErrorLine("Failed: {0} {1} {2}", moduleName, typeName, typeId);
                OutputVerboseLine("GetTypeIdWide failed for {0}", fqModule);
                Cache.GetTypeId.Add("-" + fqModule, typeId);
                return false;
            }
            //OutputErrorLine("Added: {0} {1} {2}", moduleName, typeName, typeId);
            Cache.GetTypeId.Add(fqModule, typeId);
            return true;
        }

        /// <summary>
        ///     Gets the ID for a type in a module
        /// </summary>
        /// <param name="moduleName">Base name of the module</param>
        /// <param name="typeName">Name of the type</param>
        /// <param name="typeId">UInt32 to receive the type's ID</param>
        /// <returns>HRESULT</returns>
        public int GetTypeId(string moduleName, string typeName, out UInt32 typeId)
        {
            moduleName = FixModuleName(moduleName);

            int hr;

            string fqModule = moduleName + "!" + typeName;

            // 
            // Removing caching from this function as it causes unexplained errors on the second call to extensions (???)
            // 

            if (Cache.GetTypeId.TryGetValue(fqModule, out typeId))
            {
                return MexFrameworkClass.S_OK;
            }

            if (Cache.GetTypeId.TryGetValue("-" + fqModule, out typeId))
            {
                typeId = 0;
                return MexFrameworkClass.E_FAIL;
            }

            if (FAILED(hr = DebugSymbols.GetTypeIdWide(0, fqModule, out typeId)))
            {
                //OutputErrorLine("Failed: {0} {1} {2}", moduleName, typeName, typeId);
                typeId = 0;
                OutputVerboseLine("GetTypeIdWide failed for {0}", fqModule);
                Cache.GetTypeId.Add("-" + fqModule, typeId);
            }
            else
            {
                //OutputErrorLine("Added: {0} {1} {2}", moduleName, typeName, typeId);
                Cache.GetTypeId.Add(fqModule, typeId);
            }


            return hr;
        }



        public bool IsSymbolValid(string symbolName)
        {
            uint typeId;
            ulong moduleBase;
            if (FAILED(GetSymbolTypeIdWide(symbolName, out typeId, out moduleBase)))
            {
                return false;
            }
            if (moduleBase == 0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        ///     Gets the ID for a symbol type. Wrapper for GetSymbolTypeIdWide that caches.
        /// </summary>
        /// <param name="symbolName">Full Symbol Name</param>
        /// <param name="typeId">UInt32 to receive the type's ID</param>
        /// <param name="moduleBase">UInt64 to receive the module base address</param>
        /// <returns>HRESULT</returns>
        public int GetSymbolTypeIdWide(string symbolName, out UInt32 typeId, out ulong moduleBase)
        {
            TypeInfoCache tInfo;
            symbolName = symbolName.TrimEnd();
            while (symbolName.EndsWith("*"))
            {
                symbolName = symbolName.Substring(0, symbolName.Length - 1).TrimEnd();
            }

            if (symbolName.EndsWith("]"))
            {
                symbolName = symbolName.Substring(0, symbolName.LastIndexOf('[')).TrimEnd();
            }

            if ((symbolName.StartsWith("<") && symbolName.EndsWith(">")) || symbolName == "void" || symbolName.EndsWith("!void") || symbolName.Contains("!unsigned ") || symbolName.StartsWith("unsigned"))
            {
                typeId = 0;
                moduleBase = 0;
                return MexFrameworkClass.E_FAIL;
            }

            if (Cache.GetSymbolTypeIdWide.TryGetValue(symbolName, out tInfo))
            {
                typeId = tInfo.TypeId;
                moduleBase = tInfo.Modulebase;
                return MexFrameworkClass.S_OK;
            }

            //if (Cache.GetSymbolTypeIdWide.TryGetValue("-" + symbolName, out tInfo))
            //{
            //    typeId = 0;
            //    moduleBase = 0;
            //    return MexFrameworkClass.E_FAIL;
            //}
            int hr;
            ulong module;
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                hr = DebugSymbols.GetSymbolTypeIdWide(symbolName, out typeId, &module);
                if (FAILED(hr))
                {
                    if (typeId == 0)
                    {
                        if (symbolName.Contains(" __ptr64") || symbolName.Contains(" __ptr32"))
                        {
                            string newsymbolName = symbolName.Replace(" __ptr64", string.Empty).Replace(" __ptr32", string.Empty);
                            hr = DebugSymbols.GetSymbolTypeIdWide(newsymbolName, out typeId, &module);
                        }
                    }
                }
            }
            if (FAILED(hr))
            {
                moduleBase = module;
                OutputVerboseLine("GetSymbolTypeIdWide failed for {0} 0x{1:x}", symbolName, hr);
                if (ShouldBreak(true) == false)
                {
                    Cache.GetSymbolTypeIdWide.Add("-" + symbolName, tInfo);
                }
            }
            else
            {
                tInfo.Modulebase = module;
                moduleBase = module;
                tInfo.TypeId = typeId;
                Cache.GetSymbolTypeIdWide.Add(symbolName, tInfo);
            }
            return hr;
        }


        /// <summary>
        ///     Gets the Name of a field by offset. Currently only work on SymbolNames (Like ntdll!LdrpLoaderLock).
        ///     Email timhe if you need this expanded to work on a Type Name
        /// </summary>
        /// <param name="SymbolName">Name of the symbol (eg ntdll!LdrpLoaderLock, this)</param>
        /// <param name="Offset">Field Offset</param>
        /// <returns>
        ///     String FieldName (includes [x].membername,or just .membername if not an array, or String.Empty on failure or
        ///     fieldnotfound
        /// </returns>
        public string GetFieldNameByOffset(string SymbolName, uint Offset)
        {
            // Get the ModuleBase

            // Get the TypeID for the parent structure
            uint arraymember = 0;
            if (!SymbolName.Contains("!"))
            {
                return string.Empty;
            }
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                ulong moduleBase;
                uint typeId;
                int hr = GetSymbolTypeIdWide(SymbolName, out typeId, out moduleBase);

                string name;
                GetModuleName(moduleBase, out name);

                var sb = new StringBuilder(256);

                hr = DebugSymbols.GetTypeName(moduleBase, typeId, sb, 256, null);

                bool array = false;
                if (SUCCEEDED(hr))
                {
                    if (sb.ToString().Contains("[]") && !sb.ToString().Contains("char[]"))
                    {
                        array = true;
                    }
                }

                if (array)
                {
                    uint typeSize = 0;
                    GetTypeSize(moduleBase, typeId, out typeSize);
                    string typeName = sb.ToString().Replace("[]", string.Empty);
                    uint typeSize2 = 0;
                    GetTypeSize(name, typeName, out typeSize2);
                    //---timhe
                    if (typeSize2 == 0)
                    {
                        arraymember = 0;
                        Offset = 0;
                    }
                    else
                    {
                        arraymember = Offset/typeSize2;
                        Offset = Offset%typeSize2;
                    }
                    OutputVerboseLine("Size = {0}, Size2 = {3},member = {1}, Offset = {2}", typeSize, arraymember, Offset, typeSize2);
                }

                string tName = sb.ToString();

                OutputVerboseLine("Type Name :{0}", tName);

                sb.Clear();
                sb.Append(name + "!");
                uint i = 0;

                while (hr == 0)
                {
                    hr = DebugSymbols.GetFieldNameWide(moduleBase, typeId, i, sb, sb.Capacity, null);
                    if (FAILED(hr))
                    {
                        break;
                    }
                    uint offset;
                    GetFieldOffset(name, tName, sb.ToString(), out offset);

                    if (offset == Offset)
                    {
                        if (array)
                        {
                            sb.Insert(0, "[" + arraymember + "].");
                        }
                        else
                        {
                            sb.Insert(0, ".");
                        }
                        return sb.ToString();
                    }

                    i++;
                }
            }
            return string.Empty;
        }


        /// <summary>
        ///     Gets the Name of a field by offset. Currently only work on SymbolNames (Like ntdll!LdrpLoaderLock).
        ///     Email timhe if you need this expanded to work on a Type Name
        /// </summary>
        /// <param name="TypeId">Type ID of the symbols</param>
        /// <param name="Offset">Field Offset</param>
        /// <param name="ModuleBase">ModuleBase of the symbol</param>
        /// <returns>
        ///     String FieldName (includes [x].membername,or just .membername if not an array, or String.Empty on failure or
        ///     fieldnotfound
        /// </returns>
        public string GetFieldNameByOffset(ulong ModuleBase, uint TypeId, uint Offset)
        {
            uint arraymember = 0;
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                string name;
                GetModuleName(ModuleBase, out name);

                var sb = new StringBuilder(256);

                int hr = DebugSymbols.GetTypeName(ModuleBase, TypeId, sb, 256, null);

                bool array = false;
                if (SUCCEEDED(hr))
                {
                    if (sb.ToString().Contains("[]") && !sb.ToString().Contains("char[]"))
                    {
                        array = true;
                    }
                }

                if (array)
                {
                    uint typeSize = 0;
                    GetTypeSize(ModuleBase, TypeId, out typeSize);
                    uint typeSize2 = 0;
                    GetTypeSize(ModuleBase, TypeId, out typeSize2);
                    //---timhe
                    if (typeSize2 == 0)
                    {
                        arraymember = 0;
                        Offset = 0;
                    }
                    else
                    {
                        arraymember = Offset/typeSize2;
                        Offset = Offset%typeSize2;
                    }
                    OutputVerboseLine("Size = {0}, Size2 = {3},member = {1}, Offset = {2}", typeSize, arraymember, Offset, typeSize2);
                }

                string tName = sb.ToString();

                OutputVerboseLine("Type Name :{0}", tName);

                sb.Clear();
                sb.Append(name + "!");
                uint i = 0;

                while (hr == 0)
                {
                    hr = DebugSymbols.GetFieldNameWide(ModuleBase, TypeId, i, sb, sb.Capacity, null);
                    if (FAILED(hr))
                    {
                        break;
                    }
                    uint offset;
                    GetFieldOffset(ModuleBase, TypeId, sb.ToString(), out offset);

                    if (offset == Offset)
                    {
                        if (array)
                        {
                            sb.Insert(0, "[" + arraymember + "].");
                        }
                        else
                        {
                            sb.Insert(0, ".");
                        }
                        return sb.ToString();
                    }

                    i++;
                }
            }
            return string.Empty;
        }


        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="symbolName">fully qualified symbol name</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="offset">UInt32 to receive the offset</param>
        /// <returns>HRESULT</returns>
        public int GetFieldOffset(string symbolName, string fieldName, out UInt32 offset)
        {
            string part1 = "";
            string part2 = symbolName;

            if (symbolName.Contains("!"))
            {
                string[] symbolparts = symbolName.Split("!".ToCharArray(), 2);
                part1 = symbolparts[0];
                part2 = symbolparts[1];
            }
            return GetFieldOffset(part1, part2, fieldName, out offset);
        }

        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="symbolName">fully qualified symbol name</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Offset</returns>
        public UInt32 GetFieldOffset(string symbolName, string fieldName)
        {
            string part1 = "";
            string part2 = symbolName;
            UInt32 offset;

            if (symbolName.Contains("!"))
            {
                string[] symbolparts = symbolName.Split("!".ToCharArray(), 2);
                part1 = symbolparts[0];
                part2 = symbolparts[1];
            }
            int hr = GetFieldOffset(part1, part2, fieldName, out offset);

            if (FAILED(hr))
            {
                ThrowExceptionHere(hr);
            }

            return offset;
        }


        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="offset">UInt32 to receive the offset</param>
        /// <param name="moduleBase">module address of the symbol</param>
        /// <returns>HRESULT</returns>
        public int GetFieldOffset(ulong moduleBase, uint typeId, string fieldName, out UInt32 offset)
        {
            // Check the Cache.  If Cache contains modulename + TypeName + fieldname, return that value.

            string cacheName = moduleBase.ToString("x") + "!" + typeId.ToString("x") + "." + fieldName;

            if (Cache.GetFieldOffset.TryGetValue(cacheName, out offset))
            {
                return S_OK;
            }

            if (Cache.GetFieldOffset.TryGetValue("-" + cacheName, out offset))
            {
                return E_FAIL;
            }

            uint fieldOffset;
            int hr = DebugSymbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, null, &fieldOffset);
            if (SUCCEEDED(hr))
            {
                offset = fieldOffset;
                DebugLogging.OutputVerboseLine(this, "GetFieldOffset", "{0} is at offset {1:x}", cacheName, offset);
                Cache.GetFieldOffset.Add(cacheName, offset);
            }
            else
            {
                offset = 0;
                if (ShouldBreak(true) == false)
                {
                    Cache.GetFieldOffset.Add("-" + cacheName, offset);

#if DEBUG
                    if (!HasPrivateSymbols(moduleBase))
                    {
                        OutputErrorLine("GetFieldOffset: Could not find Private Symbols for module: {0:x}", moduleBase);
                    }
#endif
                }
            }
            return hr;
        }


        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="offset">UInt32 to receive the offset</param>
        /// <returns>HRESULT</returns>
        public int GetFieldOffset(string moduleName, string typeName, string fieldName, out UInt32 offset)
        {
            //OutputDebugLine("GetFieldOffset - {0}!{1}.{2}", moduleName, typeName, fieldName);
            moduleName = FixModuleName(moduleName);

            // Check the Cache.  If Cache contains modulename + TypeName + fieldname, return that value.

            if (Cache.GetFieldOffset.TryGetValue(moduleName + "!" + typeName + "." + fieldName, out offset))
            {
                return S_OK;
            }

            if (Cache.GetFieldOffset.TryGetValue("-" + moduleName + "!" + typeName + "." + fieldName, out offset))
            {
                return E_FAIL;
            }
            UInt64 moduleBase;
            UInt32 typeId;
            int hr = GetSymbolTypeIdWide(moduleName + "!" + typeName, out typeId, out moduleBase);
            if (FAILED(hr))
            {
#if DEBUG
                if (DumpInfo.IsKernelMode && moduleName == "ntdll")
                {
                }
                else
                {
                    OutputErrorLine("Debug: GetFieldOffset: Could not find: ({0}!{1}.{2})", moduleName, typeName, fieldName);
                }
#endif
                offset = 0;
                Cache.GetFieldOffset.Add("-" + moduleName + "!" + typeName + "." + fieldName, offset);
                return hr;
            }

            uint fieldOffset;
            hr = DebugSymbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, null, &fieldOffset);
            if (SUCCEEDED(hr))
            {
                offset = fieldOffset;
                DebugLogging.OutputVerboseLine(this, "GetFieldOffset", "{0} is at offset {1:x}", moduleName + "!" + typeName + "." + fieldName, offset);
                Cache.GetFieldOffset.Add(moduleName + "!" + typeName + "." + fieldName, offset);
            }
            else
            {
#if DEBUG
                if (!HasPrivateSymbols(moduleBase))
                {
                    OutputErrorLine("GetFieldOffset: Could not find Private Symbols for module: {0}", moduleName);
                }
#endif
                offset = 0;
                Cache.GetFieldOffset.Add("-" + moduleName + "!" + typeName + "." + fieldName, offset);
            }
            return hr;
        }


        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="symbolName">fully qualified symbol name</param>
        /// <param name="fieldName">Name of the field</param>
        /// <returns>Offset</returns>
        public UInt32 GetFieldTypeId(string symbolName, string fieldName)
        {
            string part1 = "";
            string part2 = symbolName;
            UInt32 offset;

            if (symbolName.Contains("!"))
            {
                string[] symbolparts = symbolName.Split("!".ToCharArray(), 2);
                part1 = symbolparts[0];
                part2 = symbolparts[1];
            }
            int hr = GetFieldTypeId(part1, part2, fieldName, out offset);

            if (FAILED(hr))
            {
                ThrowExceptionHere(hr);
            }

            return offset;
        }


        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="id">UInt32 to receive the id</param>
        /// <param name="moduleBase">module address of the symbol</param>
        /// <returns>HRESULT</returns>
        public int GetFieldTypeId(ulong moduleBase, uint typeId, string fieldName, out UInt32 id)
        {
            // Check the Cache.  If Cache contains modulename + TypeName + fieldname, return that value.

            string cacheName = moduleBase.ToString("x") + "!" + typeId.ToString("x") + "." + fieldName;

            if (Cache.GetFieldTypeId.TryGetValue(cacheName, out id))
            {
                return S_OK;
            }

            if (Cache.GetFieldTypeId.TryGetValue("-" + cacheName, out id))
            {
                return E_FAIL;
            }

            uint fieldID;
            int hr = DebugSymbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, &fieldID, null);
            if (SUCCEEDED(hr))
            {
                id = fieldID;
                DebugLogging.OutputVerboseLine(this, "GetFieldTypeId", "{0} is at offset {1:x}", cacheName, id);
                if (fieldID != 0)
                {
                    Cache.GetFieldTypeId.Add(cacheName, id);
                }
            }
            else
            {
                id = 0;
                if (ShouldBreak(true) == false)
                {
                    Cache.GetFieldTypeId.Add("-" + cacheName, id);
                }
            }
            return hr;
        }


        /// <summary>
        ///     Gets the offset of a field in a type
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="id">UInt32 to receive the offset</param>
        /// <returns>HRESULT</returns>
        public int GetFieldTypeId(string moduleName, string typeName, string fieldName, out UInt32 id)
        {
            moduleName = FixModuleName(moduleName);

            // Check the Cache.  If Cache contains modulename + TypeName + fieldname, return that value.

            if (Cache.GetFieldTypeId.TryGetValue(moduleName + "!" + typeName + "." + fieldName, out id))
            {
                return S_OK;
            }

            if (Cache.GetFieldTypeId.TryGetValue("-" + moduleName + "!" + typeName + "." + fieldName, out id))
            {
                return E_FAIL;
            }
            UInt64 moduleBase;
            UInt32 typeId;
            int hr = GetSymbolTypeIdWide(moduleName + "!" + typeName, out typeId, out moduleBase);
            if (FAILED(hr))
            {
                id = 0;
                Cache.GetFieldTypeId.Add("-" + moduleName + "!" + typeName + "." + fieldName, id);
                return hr;
            }

            uint fieldID;
            hr = DebugSymbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, &fieldID, null);
            if (SUCCEEDED(hr))
            {
                id = fieldID;
                DebugLogging.OutputDebugLine(this, "GetFieldTypeId", "{0} is at offset {1:x}", moduleName + "!" + typeName + "." + fieldName, id);
                Cache.GetFieldTypeId.Add(moduleName + "!" + typeName + "." + fieldName, id);
            }
            else
            {
                id = 0;
                Cache.GetFieldTypeId.Add("-" + moduleName + "!" + typeName + "." + fieldName, id);
            }
            return hr;
        }

        /// <summary>
        ///     Gets the size of a type
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <returns>Size</returns>
        public UInt32 GetTypeSize(string moduleName, string typeName)
        {
            int hr;
            uint size;

            if (FAILED(hr = GetTypeSize(moduleName, typeName, out size)))
            {
                ThrowExceptionHere(hr);
            }
            return size;
        }

        /// <summary>
        ///     Gets the size of a type
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="size">UInt32 to receive the size</param>
        /// <returns>HRESULT</returns>
        public int GetTypeSize(string moduleName, string typeName, out UInt32 size)
        {
            if (typeName.EndsWith("*"))
            {
                if (IsPointer64Bit())
                {
                    size = 8;
                }
                else
                {
                    size = 4;
                }
                return S_OK;
            }

            moduleName = FixModuleName(moduleName);

            if (Cache.GetTypeSize.TryGetValue(moduleName + "!" + typeName, out size))
            {
                return S_OK;
            }

            UInt64 moduleBase;
            int hr = GetModuleBase(moduleName, out moduleBase);
            if (FAILED(hr))
            {
                size = 0;
                return hr;
            }

            UInt32 typeId;
            hr = GetTypeId(moduleName, typeName, out typeId);
            if (FAILED(hr))
            {
                size = 0;
                return hr;
            }


            hr = DebugSymbols.GetTypeSize(moduleBase, typeId, out size);
            if (SUCCEEDED(hr))
            {
                Cache.GetTypeSize.Add(moduleName + "!" + typeName, size);
            }
            return hr;
        }

        /// <summary>
        ///     Gets the size of a type
        /// </summary>
        /// <param name="typeId">type ID of the symbol</param>
        /// <param name="size">UInt32 to receive the size</param>
        /// <param name="moduleBase">Module Base of the symbol</param>
        /// <returns>HRESULT</returns>
        public int GetTypeSize(ulong moduleBase, uint typeId, out UInt32 size)
        {
            string cacheName = moduleBase.ToString("x") + "!" + typeId.ToString("x");

            if (Cache.GetTypeSize.TryGetValue(cacheName, out size))
            {
                return S_OK;
            }

            int hr = DebugSymbols.GetTypeSize(moduleBase, typeId, out size);
            if (SUCCEEDED(hr))
            {
                Cache.GetTypeSize.Add(cacheName, size);
            }
            return hr;
        }

        /// <summary>
        ///     Gets the size of a type
        /// </summary>
        /// <param name="typeId">type ID of the symbol</param>
        /// <param name="size">UInt32 to receive the size</param>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <returns>HRESULT</returns>
        public int GetTypeSize(String moduleName, uint typeId, out UInt32 size)
        {
            moduleName = FixModuleName(moduleName);

            UInt64 moduleBase;
            int hr = GetModuleBase(moduleName, out moduleBase);
            if (FAILED(hr))
            {
                size = 0;
                return hr;
            }

            return GetTypeSize(moduleBase, typeId, out size);
        }

        /// <summary>
        ///     Reads an enum from a structure and returns the value as a string.
        ///     If the function is unable to map the value to a name the output contains a hex representation of the value.
        ///     NOTE! Success is returned if we are able to read the value but not perform a lookup.
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="structureTypeName">Name of the type that contains the field</param>
        /// <param name="structureFieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="enumTypeName">Name of the type of the enum</param>
        /// <param name="valueAsText">The name of the enum, or a text representation of the </param>
        /// <returns>HRESULT</returns>
        public int ReadEnum32FromStructure(string moduleName, string structureTypeName, string structureFieldName, UInt64 structureAddress, string enumTypeName, out string valueAsText)
        {
            int hr;
            uint fieldOffset;

            moduleName = FixModuleName(moduleName);

            if (Cache.ReadEnum32FromStructure.TryGetValue(moduleName + "!" + structureTypeName + "." + structureFieldName + "." + enumTypeName + "@" + structureAddress.ToString("x"), out valueAsText))
            {
                return S_OK;
            }

            if (FAILED(hr = GetFieldOffset(moduleName, structureTypeName, structureFieldName, out fieldOffset)))
            {
                valueAsText = "FIELD_OFFSET_ERROR!";
                return hr;
            }
            UInt32 valueAsInt;
            hr = ReadVirtual32(structureAddress + fieldOffset, out valueAsInt);
            if (FAILED(hr))
            {
                valueAsText = "READ_FAILURE!";
                return hr;
            }
            hr = GetEnumName(moduleName, enumTypeName, valueAsInt, out valueAsText);
            if (FAILED(hr) || (string.IsNullOrEmpty(valueAsText)))
            {
                valueAsText = valueAsInt.ToString("x8", CultureInfo.InvariantCulture);
            }

            Cache.ReadEnum32FromStructure.Add(moduleName + "!" + structureTypeName + "." + structureFieldName + "." + enumTypeName + "@" + structureAddress.ToString("x"), valueAsText);
            return S_OK;
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadEnum32FromStructure_Silent(string moduleName, string structureTypeName, string structureFieldName, UInt64 structureAddress, string enumTypeName, out string valueAsText)
        {
            moduleName = FixModuleName(moduleName);

            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadEnum32FromStructure(moduleName, structureTypeName, structureFieldName, structureAddress, enumTypeName, out valueAsText);
            }
        }

        /// <summary>
        ///     Takes the type and value of an enum and tries to get the symbolic name for the value.
        /// </summary>
        public int GetEnumName(string moduleName, string typeName, ulong enumValue, out string enumName)
        {
            moduleName = FixModuleName(moduleName);

            if (Cache.GetEnumName.TryGetValue(moduleName + "!" + typeName + ":" + enumValue.ToString("x"), out enumName))
            {
                return S_OK;
            }

            UInt64 moduleBase;
            //We can get this information from the next call.  Should improve performance.
            //hr = GetModuleBase(moduleName, out moduleBase);
            //if (FAILED(hr))
            //{
            //    enumName = "";
            //    return hr;
            //}

            UInt32 typeId;
            int hr = GetSymbolTypeIdWide(moduleName + "!" + typeName, out typeId, out moduleBase);

            if (FAILED(hr))
            {
                enumName = "";
                return hr;
            }
            uint nameSize = 0;
            var sb = new StringBuilder(1024);
            hr = DebugSymbols.GetConstantNameWide(moduleBase, typeId, enumValue, sb, sb.Capacity, &nameSize);
            enumName = SUCCEEDED(hr) ? sb.ToString() : "";

            if (SUCCEEDED(hr))
            {
                Cache.GetEnumName.Add(moduleName + "!" + typeName + ":" + enumValue.ToString("x"), enumName);
            }

            return hr;
        }


        /// <summary>
        ///     Gets the debugger index of a register
        /// </summary>
        /// <param name="registerName">Name of the register (case sensitive, normally lower-case)</param>
        /// <param name="index">UInt32 to receive the index</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterIndex(string registerName, out UInt32 index)
        {
            if (_knownRegisterIndexes.TryGetValue(registerName, out index))
            {
                return S_OK;
            }

            int hr = DebugRegisters.GetIndexByNameWide(registerName, out index);
            if (SUCCEEDED(hr))
            {
                _knownRegisterIndexes.Add(registerName, index);
            }
            return hr;
        }

        /// <summary>
        ///     Gets the value of a register
        ///     WARNING!!! VALUE MUST BE SIGN EXTENDED ON 32-BIT SYSTEMS!!!
        /// </summary>
        /// <param name="registerName">Name of the register (case sensitive, normally lower-case)</param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterValue(string registerName, out DEBUG_VALUE value)
        {
            UInt32 registerIndex;
            int hr = GetRegisterIndex(registerName, out registerIndex);
            if (FAILED(hr))
            {
                value = default(DEBUG_VALUE);
                return hr;
            }

            return DebugRegisters.GetValue(registerIndex, out value);
        }


        /// <summary>
        ///     Gets the value of a register
        ///     WARNING!!! VALUE MUST BE SIGN EXTENDED ON 32-BIT SYSTEMS!!!
        /// </summary>
        /// <param name="registerName">Name of the register (case sensitive, normally lower-case)</param>
        /// <param name="FrameNum" />
        /// Frame Number/param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterValueFromFrameContext(string registerName, uint FrameNum, out DEBUG_VALUE value)
        {
            uint currentframe;
            ExpressionToUInt32("@$frame", out currentframe);
            RunCommandSilent(String.Format(".frame 0x{0:x}", FrameNum));
            int hr = GetRegisterValueFromFrameContext(registerName, out value);
            RunCommandSilent(String.Format(".frame 0x{0:x}", currentframe));

            return hr;
        }


        /// <summary>
        ///     Gets the value of a register
        ///     WARNING!!! VALUE MUST BE SIGN EXTENDED ON 32-BIT SYSTEMS!!!
        /// </summary>
        /// <param name="registerName">Name of the register (case sensitive, normally lower-case)</param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterValueFromFrameContext(string registerName, out DEBUG_VALUE value)
        {
            UInt32 registerIndex;
            int hr = GetRegisterIndex(registerName, out registerIndex);
            if (FAILED(hr))
            {
                value = default(DEBUG_VALUE);
                OutputVerboseLine("Failed to find index of register '{0}'", registerName);
                return hr;
            }

            hr = GetRegisterValueFromIndex(registerIndex, out value);

            return hr;
        }

        /// <summary>
        ///     Gets the value of a register
        ///     WARNING!!! VALUE MUST BE SIGN EXTENDED ON 32-BIT SYSTEMS!!!
        /// </summary>
        /// <param name="index">index of the register</param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterValueFromIndex(uint index, out DEBUG_VALUE value)
        {
            var indexArray = new uint[1];
            var debugValues = new DEBUG_VALUE[1];

            indexArray[0] = index;

            int hr = DebugRegisters.GetValues2((uint)DEBUG_REGSRC.FRAME, 1, indexArray, 0, debugValues);
            value = debugValues[0];
            return hr;
        }

        /// <summary>
        ///     Gets the value of a register
        ///     WARNING!!! VALUE MUST BE SIGN EXTENDED ON 32-BIT SYSTEMS!!!
        /// </summary>
        /// <param name="registerIndex">Index of the register</param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterValueFromFrameContext(uint registerIndex, out DEBUG_VALUE value)
        {
            var indexArray = new uint[1];
            var debugValues = new DEBUG_VALUE[1];

            indexArray[0] = registerIndex;

            int hr = DebugRegisters.GetValues2((uint)DEBUG_REGSRC.FRAME, 1, indexArray, 0, debugValues);

            value = debugValues[0];

            return hr;
        }


        /// <summary>
        ///     Gets the value of a register
        ///     WARNING!!! VALUE MUST BE SIGN EXTENDED ON 32-BIT SYSTEMS!!!
        /// </summary>
        /// <param name="registerIndex">Index of the register</param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterValue(uint registerIndex, out DEBUG_VALUE value)
        {
            return DebugRegisters.GetValue(registerIndex, out value);
        }

        /// <summary>
        ///     Gets the number of registers in the system
        /// </summary>
        /// <param name="numberOfRegisters">Number of registers in the system</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterCount(out uint numberOfRegisters)
        {
            return DebugRegisters.GetNumberRegisters(out numberOfRegisters);
        }

        /// <summary>
        ///     Gets the value of a register and sign extends for 32-bit target
        /// </summary>
        /// <param name="registerName">Name of the register (case sensitive, normally lower-case)</param>
        /// <param name="value">DEBUG_VALUE struct to receive the value</param>
        /// <returns>HRESULT</returns>
        public int GetSignExtendedRegisterValue(string registerName, out UInt64 value)
        {
            int hr;
            DEBUG_VALUE debugValue;

            UInt32 registerIndex;
            if (FAILED(hr = GetRegisterIndex(registerName, out registerIndex)) || FAILED(hr = DebugRegisters.GetValue(registerIndex, out debugValue)))
            {
                value = 0UL;
                return hr;
            }

            value = IsPointer64Bit() ? debugValue.I64 : SignExtendAddress(debugValue.I64);
            return hr;
        }

        /// <summary>
        ///     Retrieves the address of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="address">UInt64 to receive the address</param>
        /// <returns>HRESULT</returns>
        public int GetGlobalAddress(string moduleName, string globalName, out UInt64 address)
        {
            moduleName = FixModuleName(moduleName);

            if (Cache.GetGlobalAddress.TryGetValue(moduleName + "!" + globalName, out address))
            {
                return S_OK;
            }

            if (Cache.GetGlobalAddress.TryGetValue("-" + moduleName + "!" + globalName, out address))
            {
                return E_FAIL;
            }

            UInt64 tempAddress;
            int hr = DebugSymbols.GetOffsetByNameWide(moduleName + "!" + globalName, out tempAddress);
            if (SUCCEEDED(hr))
            {
                if (tempAddress != 0)
                {
                    address = IsPointer64Bit() ? tempAddress : SignExtendAddress(tempAddress);
                    Cache.GetGlobalAddress.Add(moduleName + "!" + globalName, address);
                }
                else
                {
                    address = 0;
                    OutputWarningLine("IDebugSymbols::GetOffsetByName() for {0}!{1} returned success ({2:x8}) but returned an address of 0. Overriding return status to E_FAIL.", moduleName, globalName, hr);
                    Cache.GetGlobalAddress.Add("-" + moduleName + "!" + globalName, address);
                    hr = E_FAIL;
                }
            }
            else
            {
                address = 0;
                Cache.GetGlobalAddress.Add("-" + moduleName + "!" + globalName, address);
            }
            OutputDebugLine("GetGlobalAddress: Read " + moduleName + "!" + globalName + " as {0:p}", address);
            return hr;
        }

        /// <summary>
        ///     Breaks symbol name into module & variable with some error checking
        /// </summary>
        /// <param name="symbolName"></param>
        /// <returns>string[0] = module; string[1] = variable</returns>
        private string[] BreakSymbolName(string symbolName)
        {
            string[] parts = symbolName.Split(new[] {'!'});

            if (parts.Length != 2)
            {
                throw new Exception("Error processing symbolName " + symbolName + ": " + parts.Length + " found when we expected 2.");
            }
            return parts;
        }

        /// <summary>
        ///     Retrieves the address of a global variable (exception throwing variety).
        /// </summary>
        /// <param name="symbolName">Name of the global, e.g. "nt!MmMaximumNonPagedPoolInBytes"</param>
        /// <returns>Address of structure</returns>
        public UInt64 GetGlobalAddress(string symbolName)
        {
            UInt64 addr = 0;

            string[] parts = BreakSymbolName(symbolName);

            int hr = GetGlobalAddress(parts[0], parts[1], out addr);

            if (SUCCEEDED(hr))
            {
                return addr;
            }
            //RunCommand("x " + symbolName);
            //RunCommand("!context");
            //RunCommand("!p");            
            ThrowExceptionHere("Failed to find Global: " + symbolName, hr);
            return 0;
        }

        /// <summary>
        /// Retrieves the value of a global pointer-sized variable.
        /// </summary>
        /// <param name="globalName">full name of global variable. e.g. module!g_Info</param>
        /// <returns></returns>
        public ulong ReadGlobalAsPointer(string globalName)
        {
            string[] stringParts = nameSplit(globalName);
            return ReadGlobalAsPointer(stringParts[0], stringParts[1]);
        }

        

        /// <summary>
        ///     Retrieves the value of a global pointer-sized variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <returns>Value of global as a pointer</returns>
        public ulong ReadGlobalAsPointer(string moduleName, string globalName)
        {
            int hr;
            UInt64 value;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out value)) || FAILED(hr = ReadPointer(value, out value)))
            {
                ThrowExceptionHere("Could not find or read from global: " + moduleName+ "!" + globalName, hr);
            }
            return value;
        }

        /// <summary>
        ///     Retrieves the value of a global pointer-sized variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt64 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsPointer(string moduleName, string globalName, out UInt64 value)
        {
            int hr;
            UInt64 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out temp)) || FAILED(hr = ReadPointer(temp, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global pointer-sized variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt64 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsNativeUInt(string moduleName, string globalName, out UInt64 value)
        {
            int hr;
            UInt64 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out temp)) || FAILED(hr = ReadNativeUInt(temp, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global pointer-sized variable.
        /// </summary>
        /// <param name="symbolName">Name of the symbol, e.g. nt!MmMaximumNonPagedPoolInBytes</param>
        /// <returns>UInt64 value</returns>
        public UInt64 ReadGlobalAsNativeUInt(string symbolName)
        {
            int hr;
            UInt64 temp;

            string[] parts = BreakSymbolName(symbolName);

            if (FAILED(hr = GetGlobalAddress(parts[0], parts[1], out temp)) || FAILED(hr = ReadNativeUInt(temp, out temp)))
            {
                ThrowExceptionHere(hr);
                return 0;
            }
            return temp;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">SByte to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsInt8(string moduleName, string globalName, out SByte value)
        {
            int hr;
            UInt64 tempAddress;
            SByte temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual8(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">Byte to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsUInt16(string moduleName, string globalName, out Byte value)
        {
            int hr;
            UInt64 tempAddress;
            Byte temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual8(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">Int16 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsInt16(string moduleName, string globalName, out Int16 value)
        {
            int hr;
            UInt64 tempAddress;
            Int16 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual16(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt16 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsUInt16(string moduleName, string globalName, out UInt16 value)
        {
            int hr;
            UInt64 tempAddress;
            UInt16 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual16(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">Int32 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsInt32(string moduleName, string globalName, out Int32 value)
        {
            int hr;
            UInt64 tempAddress;
            Int32 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual32(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }


        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">byte to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsByte(string moduleName, string globalName, out byte value)
        {
            int hr;
            UInt64 tempAddress;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual8(tempAddress, out value)))
            {
                value = 0;
                return hr;
            }
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <returns>Value of the global</returns>
        public byte ReadGlobalAsByte(string moduleName, string globalName)
        {
            int hr;
            UInt64 tempAddress;
            byte tempVal;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)))
            {
                ThrowExceptionHere(hr);
            }

            if (FAILED(hr = ReadVirtual8(tempAddress, out tempVal)))
            {
                ThrowExceptionHere(hr);
            }

            return tempVal;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">string to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsUnicode(string moduleName, string globalName, out string value)
        {
            int hr;
            UInt64 tempAddress;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadUnicodeString(tempAddress, 1024, out value)))
            {
                value = "";
                return hr;
            }
            return hr;
        }

        /// <summary>
        ///     Throws a new exception with the name of the parent function and value of the HR
        /// </summary>
        /// <param name="hr">Error # to include</param>
        internal void ThrowExceptionHere(int hr)
        {
            if (BreakStatus) // Dont Throw on Ctrl-Break
            {
                return;
            }
            var stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            string name = "Unknown Frame";
            try
            {
                name = stackFrame.GetMethod().Name;
            }
            catch {}
            throw new Exception(String.Format("Error in {0}: 0x{1:x}", name, hr));
        }

        /// <summary>
        ///     Throws a new exception with the name of the parent function and value of the HR
        /// </summary>
        /// <param name="message"></param>
        /// <param name="hr">Error # to include</param>
        internal void ThrowExceptionHere(string message, int hr)
        {
            if (BreakStatus) // Dont Throw on Ctrl-Break
            {
                return;
            }
            var stackTrace = new StackTrace();
            StackFrame stackFrame = stackTrace.GetFrame(1);
            throw new Exception(String.Format("Error in {0}: {1} 0x{2:x}", stackFrame.GetMethod().Name, message, hr));
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt32 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsUInt32(string moduleName, string globalName, out UInt32 value)
        {
            int hr;
            UInt64 tempAddress;
            UInt32 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual32(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        public UInt32 ReadGlobalAsUInt32(string symbolName)
        {
            int hr;
            UInt64 tempAddress;
            UInt32 temp;

            string[] parts = BreakSymbolName(symbolName);

            if (FAILED(hr = GetGlobalAddress(parts[0], parts[1], out tempAddress)) || FAILED(hr = ReadVirtual32(tempAddress, out temp)))
            {
                ThrowExceptionHere("Could not read Global: " + symbolName, hr);
                return 0;
            }
            return temp;
        }


        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">Int64 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsInt64(string moduleName, string globalName, out Int64 value)
        {
            int hr;
            UInt64 tempAddress;
            Int64 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual64(tempAddress, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt64 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsUInt64(string moduleName, string globalName, out UInt64 value)
        {
            int hr;
            UInt64 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out temp)) || FAILED(hr = ReadVirtual64(temp, out temp)))
            {
                value = 0;
                return hr;
            }
            value = temp;
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        /// </summary>
        /// <param name="symbolName">Name of the symbol, e.g. nt!MmMaximumNonPagedPoolInBytes</param>
        /// <returns>value</returns>
        public ulong ReadGlobalAsUInt64(string symbolName)
        {
            int hr;
            UInt64 temp;

            string[] parts = BreakSymbolName(symbolName);

            if (FAILED(hr = GetGlobalAddress(parts[0], parts[1], out temp)) || FAILED(hr = ReadVirtual64(temp, out temp)))
            {
                ThrowExceptionHere("Could not read Global: " + symbolName, hr);
                return 0;
            }
            return temp;
        }


        /// <summary>
        ///     Retrieves the value of a global variable.
        ///     NOTE: This is the 4-byte Windows BOOL, not the 1-byte C/C++ bool
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt32 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsBOOL(string moduleName, string globalName, out bool value)
        {
            int hr;
            UInt64 tempAddress;
            UInt32 temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual32(tempAddress, out temp)))
            {
                value = false;
                return hr;
            }
            value = (temp != 0);
            return hr;
        }

        /// <summary>
        ///     Retrieves the value of a global variable.
        ///     /// NOTE: This is the 1-byte C/C++ bool, not the 4-byte Windows BOOL
        /// </summary>
        /// <param name="moduleName">Name of the module the global resides in</param>
        /// <param name="globalName">Name of the global</param>
        /// <param name="value">UInt32 to receive the value of the global</param>
        /// <returns>HRESULT</returns>
        public int ReadGlobalAsBool(string moduleName, string globalName, out bool value)
        {
            int hr;
            UInt64 tempAddress;
            Byte temp;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out tempAddress)) || FAILED(hr = ReadVirtual8(tempAddress, out temp)))
            {
                value = false;
                return hr;
            }
            value = (temp != 0);
            return hr;
        }

        /// <summary>
        ///     Caching version of GetNameByOffsetWide
        /// </summary>
        /// <param name="offset">Address of the symbol to look up.</param>
        /// <param name="displacement">number of bytes of offset after symbol name</param>
        /// <returns>HR of DebugSymbols.GetNameByOffsetWide call</returns>
        private int GetNameByOffsetWide(UInt64 offset, out UInt64 displacement)
        {
            SymbolInfoCache symbol;
            _lookupSymbolStringBuilder.Length = 0;
            if (Cache.GetNameByOffsetWide.TryGetValue(offset.ToString("x"), out symbol))
            {
                _lookupSymbolStringBuilder.Append(symbol.SymbolName);
                displacement = symbol.Offset;
                return S_OK;
            }

            if (Cache.GetNameByOffsetWide.TryGetValue("!" + offset.ToString("x"), out symbol))
            {
                _lookupSymbolStringBuilder.Append("unknown");
                displacement = 0;
                return E_FAIL;
            }

            UInt64 tempdisplacement;
            int hr = DebugSymbols.GetNameByOffsetWide(offset, _lookupSymbolStringBuilder, _lookupSymbolStringBuilder.Capacity, null, &tempdisplacement);
            if (hr < 0)
            {
                _lookupSymbolStringBuilder.Length = 0;
                displacement = 0;
                symbol.Offset = 0;
                symbol.SymbolName = "unknown";
                Cache.GetNameByOffsetWide.Add("!" + offset.ToString("x"), symbol);
                return hr;
            }

            symbol.Offset = tempdisplacement;
            symbol.SymbolName = _lookupSymbolStringBuilder.ToString();

            displacement = tempdisplacement;

            Cache.GetNameByOffsetWide.Add(offset.ToString("x"), symbol);

            return hr;
        }

        /// <summary>
        ///     Looks up a symbol for an address.
        /// </summary>
        /// <param name="offset">Address of the symbol to look up.</param>
        /// <returns>String or NULL if an error occurs</returns>
        public string LookupSymbol(UInt64 offset)
        {
            lock (_lookupSymbolStringBuilder)
            {
                UInt64 displacement;
                _lookupSymbolStringBuilder.Length = 0;
                int hr = GetNameByOffsetWide(offset, out displacement);
                if (hr < 0)
                {
                    return null;
                }
                if (displacement != 0)
                {
                    _lookupSymbolStringBuilder.Append("+0x");
                    _lookupSymbolStringBuilder.Append(displacement.ToString("x", CultureInfo.InvariantCulture.NumberFormat));
                }
                return _lookupSymbolStringBuilder.ToString();
            }
        }

        /// <summary>
        ///     Looks up a symbol for an address.
        /// </summary>
        /// <param name="offset">Address of the symbol to look up.</param>
        /// <param name="Displacement">displacement from symbol name.</param>
        /// <returns>String or NULL if an error occurs</returns>
        public string LookupSymbol(UInt64 offset, out uint Displacement)
        {
            lock (_lookupSymbolStringBuilder)
            {
                UInt64 displacement;
                _lookupSymbolStringBuilder.Length = 0;
                int hr = GetNameByOffsetWide(offset, out displacement);
                if (hr < 0)
                {
                    Displacement = 0;
                    return null;
                }

                Displacement = (uint)displacement;
                return _lookupSymbolStringBuilder.ToString();
            }
        }

        public string LookupMemberSymbol(UInt64 offset)
        {
            uint displacement;
            string symbolicName = LookupSymbol(offset, out displacement);
            if (string.IsNullOrEmpty(symbolicName))
            {
                symbolicName = "unknown";
            }
            else if (displacement > 0)
            {
                string memberName = GetFieldNameByOffset(symbolicName, displacement);

                if (string.IsNullOrEmpty(memberName))
                {
                    symbolicName = symbolicName + "+0x" + displacement.ToString("x");
                }
                else
                {
                    symbolicName = symbolicName + memberName;
                }
            }

            return symbolicName;
        }

        /// <summary>
        ///     Looks up a symbol for an address, but only returns the symbol if there is the address is an exact match.
        /// </summary>
        /// <param name="offset">Address of the symbol to look up.</param>
        /// <returns>String or NULL if an error occurs</returns>
        public string LookupSymbolExactOnly(UInt64 offset)
        {
            lock (_lookupSymbolStringBuilder)
            {
                UInt64 displacement;
                _lookupSymbolStringBuilder.Length = 0;
                int hr = GetNameByOffsetWide(offset, out displacement);
                if ((hr < 0) || (displacement != 0))
                {
                    return null;
                }
                return _lookupSymbolStringBuilder.ToString();
            }
        }

        /// <summary>
        ///     Gets the TEB for the current thread. In a WOW64 process the 32-bit TEB will be returned.
        /// </summary>
        /// <param name="teb">UInt64 to receive the teb address</param>
        /// <param name="tebType">The type of TEB structure returned. Normally _TEB, but can be _TEB32 if inside of WOW</param>
        /// <returns>HRESULT</returns>
        public int GetTeb(out UInt64 teb, out string tebType)
        {
            string ntTibType;
            return GetTeb(out teb, out tebType, out ntTibType);
        }

        /// <summary>
        ///     Gets the TEB for the current thread. In a WOW64 process the 32-bit TEB will be returned.
        /// </summary>
        /// <param name="teb">UInt64 to receive the teb address</param>
        /// <param name="tebType">The type of TEB structure returned. Normally _TEB, but can be _TEB32 if inside of WOW</param>
        /// <param name="ntTibType">
        ///     The type of the _NT_TIB contained in the TEB. Normally _NT_TIB, but can be _NT_TIB32 if inside
        ///     of WOW
        /// </param>
        /// <returns>HRESULT</returns>
        public int GetTeb(out UInt64 teb, out string tebType, out string ntTibType)
        {
            UInt64 tebAddress;

            /*
                This seems to be a bit buggy in kernel and can succeed but return a TEB of 0
                int hr = debugSystemObjects.GetCurrentThreadTeb(out teb);
            */

            int hr = ExpressionToPointer("@$teb", out tebAddress);
            if (FAILED(hr))
            {
                teb = 0;
                tebType = "";
                ntTibType = "";
                return hr;
            }

            bool wowPresent = WowPresent();
            if (wowPresent)
            {
                hr = ReadPointer(tebAddress, out teb);
                if (FAILED(hr))
                {
                    teb = 0;
                    tebType = "";
                    ntTibType = "";
                    return hr;
                }
                tebType = "_TEB32";
                ntTibType = "_NT_TIB32";
            }
            else
            {
                teb = tebAddress;
                tebType = "_TEB";
                ntTibType = "_NT_TIB";
            }

            return hr;
        }

        private bool _AddressIsUserMode(UInt64 address)
        {
            switch (Wow64Exts.ActualCPUType)
            {
                case IMAGE_FILE_MACHINE.I386:
                    {
                        return address < 0x80000000;
                    }
                case IMAGE_FILE_MACHINE.IA64:
                    {
                        return address < 0xE000000000000000;
                    }
                case IMAGE_FILE_MACHINE.AMD64:
                    {
                        if (OSInfo.Win8OrNewer)
                        {
                            return address < 0xFFFF800000000000;
                        }
                        else
                        {
                            return address < 0xFFFFF80000000000;
                        }
                    }
                case IMAGE_FILE_MACHINE.UNKNOWN:
                    {
                        return address < 0xBADF00;
                    }
                default:
                    {
                        return address < 0xFFFFFFFF80000000;
                    }
            }
        }

        /// <summary>
        ///     Gets the TEB when debugging usermode, or ETHREAD in kernelmode.
        ///     NOTE: If Wow is loaded the TEB returned for usermode will be the application TEB, not the external WOW TEB.
        /// </summary>
        /// <param name="threadData">A variable to receive the TEB (Usermode) or ETHREAD (Kernelmode)</param>
        /// <returns>HRESULT</returns>
        public int GetTebOrEThread(out UInt64 threadData)
        {
            UInt64 dataAddress;

            int hr = ExpressionToPointer("@$thread", out dataAddress);
            if (FAILED(hr))
            {
                threadData = 0;
                return hr;
            }

            if (_AddressIsUserMode(dataAddress) && WowPresent())
            {
                /* If usermode must be a TEB, do Wow translation */

                hr = ReadPointer(dataAddress, out threadData);
                if (FAILED(hr))
                {
                    threadData = 0;
                    return hr;
                }
            }
            else
            {
                threadData = dataAddress;
            }

            return hr;
        }

        /// <summary>
        ///     Determines if the process is using a WOW64 environment.
        /// </summary>
        /// <returns>True if WOW, False otherwise or if an error occurs.</returns>
        public bool WowPresent()
        {
            UInt64 peb32Address;
            return SUCCEEDED(GetGlobalAddress("wow64", "Peb32", out peb32Address));
        }

        /// <summary>
        ///     Trims prefixed "class" or "struct" from string returned from ReadTypeNameFromStructureMember
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="memberTypeName">The type name read from symbols</param>
        /// <returns>HRESULT</returns>
        public int ReadTypeNameFromStructureMemberClean(string moduleName, string typeName, string fieldName, ulong structureAddress, out string memberTypeName)
        {
            int hr = ReadTypeNameFromStructureMember(moduleName, typeName, fieldName, structureAddress, out memberTypeName);
            if (memberTypeName.StartsWith("class "))
            {
                memberTypeName = memberTypeName.Substring("class ".Length);
            }
            else if (memberTypeName.StartsWith("struct "))
            {
                memberTypeName = memberTypeName.Substring("struct ".Length);
            }
            else if (memberTypeName.StartsWith("union "))
            {
                memberTypeName = memberTypeName.Substring("union ".Length);
            }

            return hr;
        }


        //
        /// <summary>
        ///     Gets the type information from a structure member
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="memberTypeName">The type name read from symbols</param>
        /// <returns>HRESULT</returns>
        public int ReadTypeNameFromStructureMember(string moduleName, string typeName, string fieldName, ulong structureAddress, out string memberTypeName)
        {
            moduleName = FixModuleName(moduleName);

            const int MAX_TYPE_SIZE = 512; // even most templated types shouldn't be 512 chars
            memberTypeName = "";

            // Make a new Typed Data structure.
            _EXT_TYPED_DATA symbolTypedData;

            // Get the ModuleBase
            ulong moduleBase;
            GetModuleBase(moduleName, out moduleBase);

            // Get the TypeID for the parent structure
            uint typeId;
            GetTypeId(moduleName, typeName, out typeId);

            // get the field offset and typeid for the member
            uint fieldTypeId;
            int hr;
            try
            {
                uint offset;
                hr = DebugSymbols.GetFieldTypeAndOffsetWide(moduleBase, typeId, fieldName, &fieldTypeId, &offset);
                if (FAILED(hr))
                {
                    OutputVerboseLine("GetFieldTypeAndOffset Failed:  {0:x}", hr);
                    return hr;
                }
            }
            catch
            {
                OutputErrorLine("[ReadTypeNameFromStructureMember] ERROR: IDebugSymbols.GetFieldTypeAndOffset threw an exception.  Your Debugger is probably out of date and does not support the IDebugSymbols5 interface");
                return S_FALSE;
            }

            // Set up the first operation to get symbol data
            symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;
            symbolTypedData.InData.ModBase = moduleBase;
            symbolTypedData.InData.Offset = structureAddress;
            symbolTypedData.InData.TypeId = fieldTypeId;

            //d.OutputVerboseLine("FieldTypeId:{0:x}  ModuleBase:{1:x}   Offset:{2:x}", FieldTypeId, ModuleBase, offset);


            if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)))
            {
                OutputVerboseLine("[ReadTypeNameFromStructureMember] DebugAdvanced.Request 1 Failed to get {1}!{2}.{3} hr={0:x}", hr, moduleName, typeName, fieldName);
                return hr;
            }


            IntPtr buffer = IntPtr.Zero;

            try
            {
                _EXT_TYPED_DATA temporaryTypedDataForBufferConstruction;

                // Allocate Buffers
                int totalSize = sizeof(_EXT_TYPED_DATA) + MAX_TYPE_SIZE;
                buffer = Marshal.AllocHGlobal(totalSize);

                // Set up the parameters for the 2nd request call to get the type
                temporaryTypedDataForBufferConstruction.Operation = _EXT_TDOP.EXT_TDOP_GET_TYPE_NAME;

                // Pass in the OutData from the first call to Request(), so it knows what symbol to use
                temporaryTypedDataForBufferConstruction.InData = symbolTypedData.OutData;

                // The index of the string will be immediately following the _EXT_TYPED_DATA structure
                temporaryTypedDataForBufferConstruction.StrBufferIndex = (uint)sizeof(_EXT_TYPED_DATA);
                temporaryTypedDataForBufferConstruction.StrBufferChars = MAX_TYPE_SIZE;


                // I suck at moving buffers around in C#.. but this seems to work :)
                // Copy TemporaryTypedDataForBufferConstruction into the Buffer.

                // Source is our _EXT_TYPED_DATA structure, Dest is our empty allocated buffer


                CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)))
                {
                    OutputVerboseLine("[ReadTypeNameFromStructureMember]DebugAdvanced.Request 2 Failed to get {1}!{2}.{3} hr={0:x}", hr, moduleName, typeName, fieldName);
                    return hr;
                }

                var typedDataInClassForm = new EXT_TYPED_DATA();
                // Convert the returned buffer to a _EXT_TYPED_Data _CLASS_ (since it wont let me convert to a struct)
                Marshal.PtrToStructure(buffer, typedDataInClassForm);

                memberTypeName = Marshal.PtrToStringAnsi((IntPtr)(buffer.ToInt64() + typedDataInClassForm.StrBufferIndex));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return hr;
        }


        /// <summary>
        ///     Trims prefixed "class" or "struct" from string returned from ReadTypeNameFromStructureMember
        /// </summary>
        /// <param name="symbolName">Symbol Name</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="memberTypeName">The type name read from symbols</param>
        /// <returns>HRESULT</returns>
        public int ReadTypeNameClean(string symbolName, ulong structureAddress, out string memberTypeName)
        {
            int hr = ReadTypeName(symbolName, structureAddress, out memberTypeName);
            if (memberTypeName.StartsWith("class "))
            {
                memberTypeName = memberTypeName.Substring("class ".Length);
            }
            else if (memberTypeName.StartsWith("struct "))
            {
                memberTypeName = memberTypeName.Substring("struct ".Length);
            }

            return hr;
        }


        //
        /// <summary>
        ///     Gets the type information
        /// </summary>
        /// <param name="symbolName">Symbol Name</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="memberTypeName">The type name read from symbols</param>
        /// <returns>HRESULT</returns>
        public int ReadTypeName(string symbolName, ulong structureAddress, out string memberTypeName)
        {
            const int MAX_TYPE_SIZE = 512; // even most templated types shouldn't be 512 chars
            memberTypeName = "";

            // Make a new Typed Data structure.
            _EXT_TYPED_DATA symbolTypedData;

            // Get the ModuleBase
            ulong moduleBase;

            // Get the TypeID for the parent structure
            uint typeId;
            int hr = GetSymbolTypeIdWide(symbolName, out typeId, out moduleBase);

            if (FAILED(hr))
            {
                OutputVerboseLine("[ReadTypeName] GetSymbolTypeIdWide Failed to get {1} hr={0:x}", hr, symbolName);
                return hr;
            }

            // Set up the first operation to get symbol data
            symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;
            symbolTypedData.InData.ModBase = moduleBase;
            symbolTypedData.InData.Offset = structureAddress;
            symbolTypedData.InData.TypeId = typeId;

            //d.OutputVerboseLine("FieldTypeId:{0:x}  ModuleBase:{1:x}   Offset:{2:x}", FieldTypeId, ModuleBase, offset);

            if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)))
            {
                OutputVerboseLine("[ReadTypeName] DebugAdvanced.Request 1 Failed to get {1} hr={0:x}", hr, symbolName);
                return hr;
            }


            IntPtr buffer = IntPtr.Zero;

            try
            {
                _EXT_TYPED_DATA temporaryTypedDataForBufferConstruction;

                // Allocate Buffers
                int totalSize = sizeof(_EXT_TYPED_DATA) + MAX_TYPE_SIZE;
                buffer = Marshal.AllocHGlobal(totalSize);

                // Set up the parameters for the 2nd request call to get the type
                temporaryTypedDataForBufferConstruction.Operation = _EXT_TDOP.EXT_TDOP_GET_TYPE_NAME;

                // Pass in the OutData from the first call to Request(), so it knows what symbol to use
                temporaryTypedDataForBufferConstruction.InData = symbolTypedData.OutData;

                // The index of the string will be immediately following the _EXT_TYPED_DATA structure
                temporaryTypedDataForBufferConstruction.StrBufferIndex = (uint)sizeof(_EXT_TYPED_DATA);
                temporaryTypedDataForBufferConstruction.StrBufferChars = MAX_TYPE_SIZE;


                // I suck at moving buffers around in C#.. but this seems to work :)
                // Copy TemporaryTypedDataForBufferConstruction into the Buffer.

                // Source is our _EXT_TYPED_DATA structure, Dest is our empty allocated buffer

                CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Hack alert.
                // I had to make a new class called EXT_TYPED_DATA so i could call Marshal.PtrToStructure.. The struct wouldn't work.. so we have a struct and a class with the same(ish) fields.

                var typedDataInClassForm = new EXT_TYPED_DATA();

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)))
                {
                    OutputVerboseLine("[ReadTypeNameFromStructureMember]DebugAdvanced.Request 2 Failed to get {1} hr={0:x}", hr, symbolName);
                    return hr;
                }


                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                Marshal.PtrToStructure(buffer, typedDataInClassForm);

                memberTypeName = Marshal.PtrToStringAnsi((IntPtr)(buffer.ToInt64() + typedDataInClassForm.StrBufferIndex));
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }

            return hr;
        }


        /// <summary>
        ///     Reads a 8-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt8FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out SByte value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual8(fieldAddress, out value);
        }


        /// <summary>
        ///     Reads a 8-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt8FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out SByte value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual8(fieldAddress, out value);
        }


        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadInt8FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out SByte value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadInt8FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }


        /// <summary>
        ///     Reads a 8-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt8FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out Byte value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual8(fieldAddress, out value);
        }


        /// <summary>
        ///     Reads a 8-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt8FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out Byte value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual8(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadUInt8FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out Byte value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadUInt8FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 16-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt16FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out Int16 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual16(fieldAddress, out value);
        }


        /// <summary>
        ///     Reads a 16-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt16FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out Int16 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual16(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadInt16FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out Int16 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadInt16FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 16-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt16FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out UInt16 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual16(fieldAddress, out value);
        }

        /// <summary>
        ///     Reads a 16-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt16FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out UInt16 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual16(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadUInt16FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out UInt16 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadUInt16FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 32-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt32FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out Int32 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual32(fieldAddress, out value);
        }


        /// <summary>
        ///     Reads a 32-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt32FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out Int32 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual32(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadInt32FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out Int32 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadInt32FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 32-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt32FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out UInt32 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual32(fieldAddress, out value);
        }

        /// <summary>
        ///     Reads a 32-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <returns>value</returns>
        public UInt32 ReadUInt32FromStructure(string moduleName, string typeName, string fieldName, UInt64 address)
        {
            int hr;
            ulong fieldAddress;
            UInt32 value;

            hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress);

            if (FAILED(hr))
                ThrowExceptionHere(hr);

            hr = ReadVirtual32(fieldAddress, out value);

            if (FAILED(hr))
                ThrowExceptionHere(hr);

            return value;
        }


        /// <summary>
        ///     Reads a 32-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt32FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out UInt32 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual32(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadUInt32FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out UInt32 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadUInt32FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 64-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt64FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out Int64 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual64(fieldAddress, out value);
        }


        /// <summary>
        ///     Reads a 64-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadInt64FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out Int64 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual64(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadInt64FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out Int64 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadInt64FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 64-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt64FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out UInt64 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual64(fieldAddress, out value);
        }

        /// <summary>
        ///     Reads a 64-bit value from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <returns>value</returns>
        public ulong ReadUInt64FromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address)
        {
            int hr;
            ulong value;
            hr = ReadUInt64FromStructure(moduleBase, typeId, fieldName, address, out value);

            if (FAILED(hr))
                ThrowExceptionHere(hr);

            return value;              
        }

        /// <summary>
        ///     Reads a 64-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadUInt64FromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out UInt64 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadVirtual64(fieldAddress, out value);
        }

        /// <summary>
        ///     Reads a 64-bit value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <returns>value</returns>
        public ulong ReadUInt64FromStructure(string moduleName, string typeName, string fieldName, UInt64 address)
        {
            int hr;
            ulong fieldAddress;
            UInt64 value;

            hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress);

            if (FAILED(hr))
                ThrowExceptionHere(hr);

            hr = ReadVirtual64(fieldAddress, out value);

            if (FAILED(hr))
                ThrowExceptionHere(hr);            

            return value;
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadUInt64FromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out UInt64 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadUInt64FromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a Double value from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadDoubleFromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out Double value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }

            return ReadDouble(fieldAddress, out value);
        }

        /// <summary>
        ///     Reads a 32-bit value from a structure
        ///     NOTE: This is the 4-byte Windows BOOL, not the 1-byte C/C++ bool
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadBOOLFromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out bool value)
        {
            int hr;
            ulong fieldAddress;
            UInt32 tempValue;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)) || FAILED(hr = ReadVirtual32(fieldAddress, out tempValue)))
            {
                value = false;
                return hr;
            }
            value = (tempValue != 0);
            return hr;
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadBOOLFromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out bool value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadBOOLFromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a 32-bit value from a structure
        ///     NOTE: This is the 1-byte C/C++ bool, not the 4-byte Windows BOOL
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadBoolFromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out bool value)
        {
            int hr;
            ulong fieldAddress;
            Byte tempValue;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)) || FAILED(hr = ReadVirtual8(fieldAddress, out tempValue)))
            {
                value = false;
                return hr;
            }
            value = (tempValue != 0);
            return hr;
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadBoolFromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out bool value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadBoolFromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a pointer from a structure
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadPointerFromStructure(string moduleName, string typeName, string fieldName, UInt64 address, out UInt64 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }

            hr = ReadPointer(fieldAddress, out value);
            return hr;
        }

        /// <summary>
        ///     Reads a pointer from a structure
        /// </summary>
        /// <param name="moduleBase">Module Base</param>
        /// <param name="typeId">Type ID of the symbol</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="value">The value read from memory</param>
        /// <returns>HRESULT</returns>
        public int ReadPointerFromStructure(ulong moduleBase, uint typeId, string fieldName, UInt64 address, out UInt64 value)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleBase, typeId, fieldName, address, out fieldAddress)))
            {
                value = 0;
                return hr;
            }
            return ReadPointer(fieldAddress, out value);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadPointerFromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 address, out UInt64 value)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadPointerFromStructure(moduleName, typeName, fieldName, address, out value);
            }
        }

        /// <summary>
        ///     Reads a IPv6 byte array and returns a string representing the address (optionally including port)
        /// </summary>
        /// <param name="IPv6MemoryAddress">Address of the 16-byte IPv6 Array</param>
        /// <param name="PortValue">Value of the port (to be included in the string)</param>
        /// <returns>FE80.0000.0000.0000.EAB7.48FF.FE64.4A01:80</returns>
        public string IPv6ByteArrayAsString(ulong IPv6MemoryAddress, ulong PortValue)
        {
            byte[] addr_Array;
            ReadByteArrayFromAddress(IPv6MemoryAddress, 16, out addr_Array);

            var sb = new StringBuilder();
            sb.AppendFormat("{0:X2}{1:X2}.{2:X2}{3:X2}.{4:X2}{5:X2}.{6:X2}{7:X2}.{8:X2}{9:X2}.{10:X2}{11:X2}.{12:X2}{13:X2}.{14:X2}{15:X2}:{16}",
                addr_Array[0], addr_Array[1], addr_Array[2], addr_Array[3],
                addr_Array[4], addr_Array[5], addr_Array[6], addr_Array[7],
                addr_Array[8], addr_Array[9], addr_Array[10], addr_Array[11],
                addr_Array[12], addr_Array[13], addr_Array[14], addr_Array[15], PortValue);

            return sb.ToString();
        }

        /// <summary>
        ///     Reads a byte array from a memory address
        /// </summary>
        /// <param name="Address">Address to read from</param>
        /// <param name="cb">Number of bytes to read</param>
        /// <param name="bin">The data that was retrieved</param>
        /// <returns>HRESULT</returns>
        public int ReadByteArrayFromAddress(UInt64 Address, uint cb, out byte[] bin)
        {
            int hr;

            IntPtr buffer = Marshal.AllocHGlobal((int)cb);
            UInt32 nRead = 0;
            if (FAILED(hr = ReadVirtual(Address, cb, buffer, &nRead)))
            {
                bin = null;
                return hr;
            }

            bin = new byte[cb];
            Marshal.Copy(buffer, bin, 0, (int)nRead);
            Marshal.FreeHGlobal(buffer);
            return S_OK;
        }


        /// <summary>
        ///     Reads a byte array from a memory address
        /// </summary>
        /// <param name="Address">Address to read from</param>
        /// <param name="cBits">Number of bits to read</param>
        /// <returns>BitArray</returns>
        public BitArray ReadBitsFromAddress(UInt64 Address, uint cBits)
        {
            int hr;
            uint cBytes = cBits/8;

            IntPtr buffer = Marshal.AllocHGlobal((int)cBytes);
            UInt32 nRead = 0;
            if (FAILED(hr = ReadVirtual(Address, cBytes, buffer, &nRead)))
            {
                //bin = null;
                ThrowExceptionHere(hr);
                return null;
            }

            var bin = new byte[cBytes];
            Marshal.Copy(buffer, bin, 0, (int)nRead);
            Marshal.FreeHGlobal(buffer);

            var ba = new BitArray(bin);
            return ba;
        }

        /// <summary>
        ///     Reads a unicode string from a structure
        ///     NOTE: This function assumes the structure has an pointer to the unicode string, NOT a string embedded within the
        ///     structure itself!
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="maxSize">Maximum number of characters to read</param>
        /// <param name="output">The data that was retrieved</param>
        /// <returns>HRESULT</returns>
        public int ReadUnicodeStringFromStructure(string moduleName, string typeName, string fieldName, UInt64 structureAddress, uint maxSize, out string output)
        {
            int hr;
            UInt64 stringPointer;
            if (FAILED(hr = ReadPointerFromStructure(moduleName, typeName, fieldName, structureAddress, out stringPointer)))
            {
                output = null;
                return hr;
            }
            return ReadUnicodeString(stringPointer, maxSize, out output);
        }

        /// <summary>
        ///     Reads a unicode string from a structure
        ///     NOTE: This function assumes the structure has an pointer to the unicode string, NOT a string embedded within the
        ///     structure itself!
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="maxSize">Maximum number of characters to read</param>
        /// <param name="output">The data that was retrieved</param>
        /// <returns>HRESULT</returns>
        public int ReadUnicodeStringFromStructure_WOW(string moduleName, string typeName, string fieldName, UInt64 structureAddress, uint maxSize, out string output)
        {
            int hr;
            UInt64 stringPointer;
            if (FAILED(hr = ReadPointerFromStructure(moduleName, typeName, fieldName, structureAddress, out stringPointer)))
            {
                output = null;
                return hr;
            }

            var peFile = new PEFile(this, moduleName);
            if (peFile.GetMachineType() == IMAGE_FILE_MACHINE.I386)
            {
                stringPointer &= 0xFFFFFFFF;
            }

            return ReadUnicodeString(stringPointer, maxSize, out output);
        }

        /// <summary>
        ///     Reads a unicode string from a structure
        ///     NOTE: This function assumes a string embedded within the structure itself!
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="address">Address of the structure</param>
        /// <param name="maxSize">Maximum number of characters to read</param>
        /// <param name="output">The data that was retrieved</param>
        /// <returns>HRESULT</returns>
        public int ReadUnicodeStringFromStructure_Embedded(string moduleName, string typeName, string fieldName, UInt64 address, uint maxSize, out string output)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, address, out fieldAddress)))
            {
                output = string.Empty;
                return hr;
            }
            return ReadUnicodeString(fieldAddress, maxSize, out output);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadUnicodeStringFromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 structureAddress, uint maxSize, out string output)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadUnicodeStringFromStructure(moduleName, typeName, fieldName, structureAddress, maxSize, out output);
            }
        }

        /// <summary>
        ///     Reads an ANSI string from a structure
        ///     NOTE: This function assumes the structure has an pointer to the ansi string, NOT a string embedded within the
        ///     structure itself!
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="maxSize">Maximum number of characters to read</param>
        /// <param name="output">The data that was retrieved</param>
        /// <returns>HRESULT</returns>
        public int ReadAnsiStringFromStructure(string moduleName, string typeName, string fieldName, UInt64 structureAddress, uint maxSize, out string output)
        {
            int hr;
            UInt64 stringPointer;
            if (FAILED(hr = ReadPointerFromStructure(moduleName, typeName, fieldName, structureAddress, out stringPointer)))
            {
                output = null;
                return hr;
            }
            return ReadAnsiString(stringPointer, maxSize, out output);
        }

        /// <summary>
        ///     Wraps the core function, blocking all output
        /// </summary>
        public int ReadAnsiStringFromStructure_Silent(string moduleName, string typeName, string fieldName, UInt64 structureAddress, uint maxSize, out string output)
        {
            using (InstallIgnoreFilter_WRAP_WITH_USING())
            {
                return ReadAnsiStringFromStructure(moduleName, typeName, fieldName, structureAddress, maxSize, out output);
            }
        }

        /// <summary>
        ///     Reads a UNICODE_STRING from a structure
        ///     NOTE: This function assumes that the UNICODE_STRING structure is embedded in the parent struction, NOT that the
        ///     parent has a pointer to a UNICODE_STRING!
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="output">The data that was retrieved</param>
        /// <param name="options"></param>
        /// <returns>HRESULT</returns>
        public int ReadUNICODE_STRINGFromStructure_Embedded(string moduleName, string typeName, string fieldName, UInt64 structureAddress, out string output, ReadUNICODE_STRINGOptions options = ReadUNICODE_STRINGOptions.Escaped)
        {
            int hr;
            ulong fieldAddress;
            if (FAILED(hr = GetFieldVirtualAddress(moduleName, typeName, fieldName, structureAddress, out fieldAddress)))
            {
                output = string.Empty;
                return hr;
            }
            return ReadUNICODE_STRING(fieldAddress, out output, options);
        }


        /// <summary>
        ///     Reads a UNICODE_STRING from a structure
        ///     NOTE: This function assumes that the structure contains a pointer to the UNICODE_STRING, NOT that the
        ///     UNICODE_STRING is embedded!
        /// </summary>
        /// <param name="moduleName">Name of the module that contains the type</param>
        /// <param name="typeName">Name of the type that contains the field</param>
        /// <param name="fieldName">Name of the field</param>
        /// <param name="structureAddress">Address of the structure</param>
        /// <param name="output">The data that was retrieved</param>
        /// <param name="options"></param>
        /// <returns>HRESULT</returns>
        public int ReadUNICODE_STRINGFromStructure_Pointer(string moduleName, string typeName, string fieldName, UInt64 structureAddress, out string output, ReadUNICODE_STRINGOptions options = ReadUNICODE_STRINGOptions.Escaped)
        {
            int hr;
            UInt64 stringPointer;
            if (FAILED(hr = ReadPointerFromStructure(moduleName, typeName, fieldName, structureAddress, out stringPointer)))
            {
                output = null;
                return hr;
            }
            return ReadUNICODE_STRING(stringPointer, out output, options);
        }


        /// <summary>
        ///     Returns true of the debugger has access to the kernel. If in Mex, you should use DumpInfo.IsKernelMode
        /// </summary>
        /// <returns></returns>
        internal bool IsKernelMode()
        {
            DEBUG_CLASS debuggeeClass;
            DEBUG_CLASS_QUALIFIER debuggeeClassQualifier;
            int hr = DebugControl.GetDebuggeeType(out debuggeeClass, out debuggeeClassQualifier);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Can't tell is debug target is kernel or usermode, assuming user: {0:x8}", hr);
                return false;
            }
            if (debuggeeClass == DEBUG_CLASS.UNINITIALIZED)
            {
                OutputVerboseLine("ERROR! Debugger target is not initialized!");
                return false;
            }

            return (debuggeeClass == DEBUG_CLASS.KERNEL);
        }

        /// <summary>
        ///     Wraps IDebugSymbols2::GetSymbolOptions
        /// </summary>
        /// <param name="options">The current symbol options</param>
        /// <returns>HRESULT</returns>
        public int GetSymbolOptions(out SYMOPT options)
        {
            object cacheget = Cache.GetSymbolOptions.Get("SymOpt");

            if (cacheget == null)
            {
                int hr = DebugSymbols.GetSymbolOptions(out options);
                if (SUCCEEDED(hr))
                {
                    Cache.GetSymbolOptions.Add("SymOpt", options);
                }
                return hr;
            }
            options = (SYMOPT)cacheget;
            return S_OK;
        }

        /// <summary>
        ///     Wraps IDebugSymbols3::GetNameByOffset
        /// </summary>
        /// <param name="address">Address to lookup</param>
        /// <param name="name">The returned name for the address passed in</param>
        /// <param name="displacement">Optional displacement from the address passed in</param>
        /// <returns>HRESULT</returns>
        public int GetNameByOffset(UInt64 address, out string name, ulong* displacement)
        {
            var sb = new StringBuilder(1024);

            int hr = DebugSymbols.GetNameByOffsetWide(address, sb, sb.Capacity, null, displacement);
            name = SUCCEEDED(hr) ? sb.ToString() : "";
            return hr;
        }

        /// <summary>
        ///     Wraps IDebugSymbols5::GetNameByInlineContext, or IDebugSymbols3::GetNameByOffset if not available
        /// </summary>
        /// <param name="address">Address to lookup</param>
        /// <param name="inlineContext">inlineContext</param>
        /// <param name="name">The returned name for the address passed in</param>
        /// <param name="displacement">Optional displacement from the address passed in</param>
        /// <returns>HRESULT</returns>
        public int GetNameByInlineContext(UInt64 address, UInt32 inlineContext, out string name, ulong* displacement)
        {
            SymbolInfoCache si;
            int hr = S_OK;
            if (Cache.GetSymbolName.TryGetValue(address.ToString("x") + "+" + inlineContext.ToString("x"), out si))
            {
                name = si.SymbolName;
                *displacement = si.Offset;
                return hr;
            }

            if (DebugSymbols5 == null)
            {
                hr = GetNameByOffset(address, out name, displacement);
                if (SUCCEEDED(hr))
                {
                    si.SymbolName = name;
                    si.Offset = *displacement;

                    Cache.GetSymbolName.Add(address.ToString("x") + "+" + inlineContext.ToString("x"), si);
                }
                return hr;
            }

            var sb = new StringBuilder(1024);

            hr = DebugSymbols5.GetNameByInlineContextWide(address, inlineContext, sb, sb.Capacity, null, displacement);
            name = SUCCEEDED(hr) ? sb.ToString() : "";
            if (SUCCEEDED(hr))
            {
                si.SymbolName = name;
                si.Offset = *displacement;

                Cache.GetSymbolName.Add(address.ToString("x") + "+" + inlineContext.ToString("x"), si);
            }
            return hr;
        }

        /// <summary>
        ///     Wraps IDebugSymbols2::GetLineByOffset
        /// </summary>
        /// <param name="address">Address to lookup</param>
        /// <param name="fileName">Name of the file the address resides in</param>
        /// <param name="lineNumber">Line number inside the file</param>
        /// <param name="displacement">Optional displacement from the address passed in</param>
        /// <returns>HRESULT</returns>
        public int GetLineByOffset(UInt64 address, out string fileName, out uint lineNumber, ulong* displacement)
        {
            var sb = new StringBuilder(1024);
            uint line = 0;

            int hr = DebugSymbols.GetLineByOffsetWide(address, &line, sb, sb.Capacity, null, displacement);
            fileName = SUCCEEDED(hr) ? sb.ToString() : "";
            lineNumber = line;
            return hr;
        }

        /// <summary>
        ///     Wraps IDebugSymbols2::GetLineByInlineContext
        /// </summary>
        /// <param name="address">Address to lookup</param>
        /// <param name="inlineContext">inlineContext</param>
        /// <param name="fileName">Name of the file the address resides in</param>
        /// <param name="lineNumber">Line number inside the file</param>
        /// <param name="displacement">Optional displacement from the address passed in</param>
        /// <returns>HRESULT</returns>
        public int GetLineByInlineContext(UInt64 address, uint inlineContext, out string fileName, out uint lineNumber, ulong* displacement)
        {
            if (DebugSymbols5 == null)
            {
                return GetLineByOffset(address, out fileName, out lineNumber, displacement);
            }

            var sb = new StringBuilder(1024);
            uint line = 0;

            int hr = DebugSymbols5.GetLineByInlineContextWide(address, inlineContext, &line, sb, sb.Capacity, null, displacement);
            fileName = SUCCEEDED(hr) ? sb.ToString() : "";
            lineNumber = line;
            return hr;
        }

        /// <summary>
        ///     Tries to determine if a module has private symbols
        /// </summary>
        public bool HasPrivateSymbols(UInt64 moduleBaseAddress, string moduleNameWithExtension)
        {
            IMAGEHLP_MODULE64 imageHlp;

            DebugAdvanced.GetSymbolInformationWide(DEBUG_SYMINFO.IMAGEHLP_MODULEW64, moduleBaseAddress, 0, &imageHlp, sizeof(IMAGEHLP_MODULE64), null, null, 0, null);

            if (imageHlp.SymType == DEBUG_SYMTYPE.DEFERRED)
            {
                OutputVerboseLine("HasPrivateSymbols: Loading Symbols for {0}", Path.GetFileName(moduleNameWithExtension));
                RunCommandSilent("ld /f {0}", Path.GetFileName(moduleNameWithExtension));
                //ReloadSymbols("/f " + Path.GetFileName(moduleNameWithExtension), false);
                DebugAdvanced.GetSymbolInformationWide(DEBUG_SYMINFO.IMAGEHLP_MODULEW64, moduleBaseAddress, 0, &imageHlp, sizeof(IMAGEHLP_MODULE64), null, null, 0, null);
            }

            return imageHlp.GlobalSymbols;
        }

        /// <summary>
        ///     Tries to determine if a module has private symbols
        /// </summary>
        public bool HasPrivateSymbols(UInt64 moduleBaseAddress)
        {
            IMAGEHLP_MODULE64 imageHlp;

            DebugAdvanced.GetSymbolInformationWide(DEBUG_SYMINFO.IMAGEHLP_MODULEW64, moduleBaseAddress, 0, &imageHlp, sizeof(IMAGEHLP_MODULE64), null, null, 0, null);

            if (imageHlp.SymType == DEBUG_SYMTYPE.DEFERRED)
            {
                OutputVerboseLine("HasPrivateSymbols: Symbols for {0} are deferred.  Please try using the symbols before checking if they are private", imageHlp.ImageName);
            }

            return imageHlp.GlobalSymbols;
        }

        /// <summary>
        ///     Calls IDebugDataSpaces.QueryVirtual with a correctly aligned MEMORY_BASIC_INFORMATION64 structure
        /// </summary>
        public int QueryVirtual(UInt64 address, out MEMORY_BASIC_INFORMATION64 memoryInfo)
        {
            // This is only needed when we need a 16 byte aligned buffer on 32 bit, as is the case when calling SSE instructions.
            IntPtr rawPointer = Marshal.AllocHGlobal(sizeof(MEMORY_BASIC_INFORMATION64) + 15);
            var alignedPointer = new IntPtr((rawPointer.ToInt64() + 15L) & ~15L);

            int hr = DebugDataSpaces.QueryVirtual(address, alignedPointer);
            //memoryInfo = SUCCEEDED(hr) ? (MEMORY_BASIC_INFORMATION64)Marshal.PtrToStructure(alignedPointer, typeof(MEMORY_BASIC_INFORMATION64)) : new MEMORY_BASIC_INFORMATION64();
            memoryInfo = SUCCEEDED(hr) ? *(MEMORY_BASIC_INFORMATION64*)alignedPointer.ToPointer() : new MEMORY_BASIC_INFORMATION64();

            Marshal.FreeHGlobal(rawPointer);
            return hr;
        }

        /// <summary>
        ///     Gets the value of any field. Especially useful for bitfields.
        ///     MemberName is smart. It will take value.value, and if it encounters a pointer, it will dereference them.  ie :
        ///     Value.Value->Value, would look like: Value.Value.Value in the MemberName argument
        /// </summary>
        public int GetFieldValue(string SymbolName, string fieldName, UInt64 structureAddress, out ulong fieldValue)
        {
            string part1 = "";
            string part2 = SymbolName;

            if (SymbolName.Contains("!"))
            {
                string[] symbol = SymbolName.Split("!".ToCharArray());
                part1 = symbol[0];
                part2 = symbol[1];
            }

            return GetFieldValue(part1, part2, fieldName, structureAddress, out fieldValue);
        }

        /// <summary>
        ///     Gets the value of any field. Especially useful for bitfields.
        ///     MemberName is smart. It will take value.value, and if it encounters a pointer, it will dereference them.  ie :
        ///     Value.Value->Value, would look like: Value.Value.Value in the MemberName argument
        /// </summary>
        public int GetFieldValue(string moduleName, string typeName, string fieldName, UInt64 structureAddress, out ulong fieldValue)
        {
            moduleName = FixModuleName(moduleName);

            fieldValue = 0;
            // Begin the runner up for the ugliest code ever:
            // The below code implements "GetFieldValue()" -- Proof of concept only. Trevor should clean this up and add it to the library.
            uint typeId;
            // Get the ModuleBase
            ulong moduleBase;

            bool pointer = false;
            typeName = typeName.TrimEnd();
            if (typeName.EndsWith("*"))
            {
                typeName = typeName.Substring(0, typeName.Length - 1).TrimEnd();
                pointer = true;
            }

            int hr = GetSymbolTypeIdWide(moduleName + "!" + typeName, out typeId, out moduleBase);

            if (FAILED(hr))
            {
                GetModuleBase(moduleName, out moduleBase);
                hr = GetTypeId(moduleName, typeName, out typeId);
            }

            if (FAILED(hr))
            {
                return hr;
            }

            // Make a new Typed Data structure.
            _EXT_TYPED_DATA symbolTypedData;

            // Fill it in from ModuleBase and TypeID
            // Note, we could use EXT_TDOP_SET_PTR_FROM_TYPE_ID_AND_U64 if this was a pointer to the object
            symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;

            symbolTypedData.InData.ModBase = moduleBase;
            symbolTypedData.InData.TypeId = typeId;


            if (pointer)
            {
                ReadPointer(structureAddress, out structureAddress);
            }
            symbolTypedData.InData.Offset = structureAddress;


            if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)))
            {
                OutputVerboseLine("GetFieldValue: DebugAdvanced.Request Failed to get {1}!{2}.{3} hr={0:x}", hr, moduleName, typeName, fieldName);
                return hr;
            }

            IntPtr buffer = IntPtr.Zero;
            IntPtr memPtr = IntPtr.Zero;
            try
            {
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
                CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Copy the ANSI string of our member name immediately after the TypedData Structure.
                // Source is our ANSI Buffer, Dest is the byte immediately after the last byte from the previous copy

                // This fails if we use i<MemberName.Length, made it i<= to copy the null terminator.
                CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)))
                {
                    OutputVerboseLine("GetFieldValue2: DebugAdvanced.Request Failed to get {1}!{2}.{3} hr={0:x}", hr, moduleName, typeName, fieldName);
                    return hr;
                }

                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));
                // OutData.Data has our field value.  This will always be a ulong
                fieldValue = typedDataInClassForm.OutData.Data;
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(memPtr);
            }

            return S_OK;
        }

        /// <summary>
        ///     Gets the value of any field. Especially useful for bitfields.
        ///     MemberName is smart. It will take value.value, and if it encounters a pointer, it will dereference them.  ie :
        ///     Value.Value->Value, would look like: Value.Value.Value in the MemberName argument
        /// </summary>
        public int GetFieldValue(ulong moduleBase, uint typeId, string fieldName, UInt64 structureAddress, out ulong fieldValue)
        {
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

            if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)))
            {
                OutputVerboseLine("GetFieldValue3: DebugAdvanced.Request Failed to get {1:x}!{2}.{3} hr={0:x}", hr, moduleBase, typeId, fieldName);
                return hr;
            }

            IntPtr buffer = IntPtr.Zero;
            IntPtr memPtr = IntPtr.Zero;
            try
            {
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
                CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Copy the ANSI string of our member name immediately after the TypedData Structure.
                // Source is our ANSI Buffer, Dest is the byte immediately after the last byte from the previous copy

                // This fails if we use i<MemberName.Length, made it i<= to copy the null terminator.
                CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)))
                {
                    OutputVerboseLine("GetFieldValue4: DebugAdvanced.Request Failed to get {1:x}!{2}.{3} hr={0:x}", hr, moduleBase, typeId, fieldName);
                    return hr;
                }

                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

                // OutData.Data has our field value.  This will always be a ulong
                fieldValue = typedDataInClassForm.OutData.Data;
                if (fieldName.Contains("RunningThreadGoal"))
                {
                    OutputLine("Size of {1} - typedDataInClassForm.OutData.Size {0}", typedDataInClassForm.OutData.Size, fieldName);
                }
            }
            finally
            {
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
        public int GetFieldVirtualAddress(string moduleName, string typeName, string fieldName, UInt64 structureAddress, out ulong FieldAddress)
        {
            DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", "GetFieldVirtualAddress: Module: {0}, Type {1}, Field {2}, StuctAddr: {3:x}", moduleName, typeName, fieldName, structureAddress);
            moduleName = FixModuleName(moduleName);

            FieldAddress = 0;

            uint typeId;
            // Get the ModuleBase
            ulong moduleBase;
            typeName = typeName.TrimEnd();
            if (typeName.EndsWith("*"))
            {
                typeName = typeName.Substring(0, typeName.Length - 1).TrimEnd();
                ReadPointer(structureAddress, out structureAddress);
                if (structureAddress == 0)
                {
                    DebugLogging.OutputVerboseLine(this, "GetFieldVirtualAddress", " -- GetFieldVirtualAddress: Null pointer {0}, Type {1}, Field {2}, StuctAddr: {3:x}", moduleName, typeName, fieldName, structureAddress);
                    FieldAddress = 0;
                    return E_FAIL;
                }
                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", " -- GetFieldVirtualAddress: DEREF POINTER: Module: {0}, Type {1}, Field {2}, StuctAddr: {3:x}", moduleName, typeName, fieldName, structureAddress);
            }

            ulong savedStructAddr = structureAddress;
            bool slow = false;
            uint offset = 0;

            int hr = GetFieldOffset(moduleName, typeName, fieldName, out offset);

            if (FAILED(hr))
            {
                OutputVerboseLine("GetFieldOffset returned {0:x}", hr);
                return hr;
            }

            if (offset == 0)
            {
                slow = true;
                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", " -- GetFieldVirtualAddress: Offset is 0, Slow is TRUE: Module: {0}, Type {1}, Field {2}, StuctAddr: {3:x}", moduleName, typeName, fieldName, structureAddress);
            }
            else
            {
                uint typeSize = 0;
                hr = GetTypeSize(moduleName, typeName, out typeSize);
                if (typeSize == 0 || offset > typeSize)
                {
                    slow = true;
                }
            }

            if (slow == false)
            {
                FieldAddress = structureAddress + offset;
                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", " *** Final Fixed up Field Offset value {1} = 0x{0:x} ***", FieldAddress, fieldName);
                return S_OK;
            }

            hr = GetSymbolTypeIdWide(typeName, out typeId, out moduleBase);

            if (FAILED(hr))
            {
                GetModuleBase(moduleName, out moduleBase);
                hr = GetTypeId(moduleName, typeName, out typeId);
                if (FAILED(hr))
                {
                    return hr;
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

            if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)))
            {
                DebugLogging.OutputVerboseLine(this, "GetFieldVirtualAddress", "GetFieldVirtualAddress: DebugAdvanced.Request Failed to get {1}!{2}.{3} hr={0:x}", hr, moduleName, typeName, fieldName);
                if (offset == 0)
                {
                    FieldAddress = savedStructAddr;
                    return S_OK;
                }
                return hr;
            }

            IntPtr buffer = IntPtr.Zero;
            IntPtr memPtr = IntPtr.Zero;
            try
            {
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

                CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Copy the ANSI string of our member name immediately after the TypedData Structure.
                // Source is our ANSI Buffer, Dest is the byte immediately after the last byte from the previous copy

                // This fails if we use fieldName.Length, made it +1 to copy the null terminator.
                CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)))
                {
                    DebugLogging.OutputVerboseLine(this, "GetFieldVirtualAddress", "GetFieldVirtualAddress2: DebugAdvanced.Request Failed to get {1}!{2}.{3} hr={0:x}", hr, moduleName, typeName, fieldName);
                    if (offset == 0)
                    {
                        FieldAddress = savedStructAddr;
                        return S_OK;
                    }
                    return hr;
                }

                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", "Field Offset value {1} = {0} (Struct Address = {2:x}, offset ={3})", typedDataInClassForm.OutData.Offset, fieldName, savedStructAddr, offset);
                // OutData.Data has our field value.  This will always be a ulong

                FieldAddress = typedDataInClassForm.OutData.Offset;

                if (FieldAddress < savedStructAddr && offset == 0)
                {
                    FieldAddress = savedStructAddr;
                }

                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", " *** Final Fixed up Field Offset value {1} = 0x{0:x} ***", FieldAddress, fieldName);
            }
            finally
            {
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
        public int GetFieldVirtualAddress(ulong moduleBase, uint typeId, string fieldName, UInt64 structureAddress, out ulong FieldAddress)
        {
            DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", "Module: {0:x}, Type {1}, Field {2}, StuctAddr: {3:x}", moduleBase, typeId, fieldName, structureAddress);
            FieldAddress = 0;

            ulong savedStructAddr = structureAddress;
            bool slow = false;
            uint offset = 0;
            uint typeSize = 0;
            int hr = GetFieldOffset(moduleBase, typeId, fieldName, out offset);

            if (FAILED(hr))
            {
                DebugLogging.OutputVerboseLine(this, "GetFieldVirtualAddress", "GetFieldOffset returned {0:x}", hr);
                return hr;
            }

            if (offset == 0)
            {
                slow = true;
                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", " -- GetFieldVirtualAddress: Offset is 0, Slow is TRUE: Module: {0:x}, Type {1}, Field {2}, StuctAddr: {3:x}", moduleBase, typeId, fieldName, structureAddress);
            }
            else
            {
                hr = GetTypeSize(moduleBase, typeId, out typeSize);
                if (typeSize == 0 || offset > typeSize)
                {
                    slow = true;
                }
            }

            DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", " -- GetFieldVirtualAddress: Offset = {0}, TypeSize = {1}", offset, typeSize);

            if (slow == false)
            {
                FieldAddress = structureAddress + offset;
                return S_OK;
            }

            // Make a new Typed Data structure.
            _EXT_TYPED_DATA symbolTypedData;

            // Fill it in from ModuleBase and TypeID
            // Note, we could use EXT_TDOP_SET_PTR_FROM_TYPE_ID_AND_U64 if this was a pointer to the object
            symbolTypedData.Operation = _EXT_TDOP.EXT_TDOP_SET_FROM_TYPE_ID_AND_U64;

            symbolTypedData.InData.ModBase = moduleBase;
            symbolTypedData.InData.TypeId = typeId;

            symbolTypedData.InData.Offset = structureAddress;

            if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, &symbolTypedData, sizeof(_EXT_TYPED_DATA), &symbolTypedData, sizeof(_EXT_TYPED_DATA), null)))
            {
                DebugLogging.OutputVerboseLine(this, "GetFieldVirtualAddress", "GetFieldVirtualAddress3: DebugAdvanced.Request Failed to get {1:x}!{2}.{3} hr={0:x}", hr, moduleBase, typeId, fieldName);
                if (offset == 0)
                {
                    FieldAddress = savedStructAddr;
                    return S_OK;
                }
                return hr;
            }

            IntPtr buffer = IntPtr.Zero;
            IntPtr memPtr = IntPtr.Zero;
            try
            {
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

                CopyMemory(buffer, (IntPtr)(&temporaryTypedDataForBufferConstruction), sizeof(_EXT_TYPED_DATA));

                // Copy the ANSI string of our member name immediately after the TypedData Structure.
                // Source is our ANSI Buffer, Dest is the byte immediately after the last byte from the previous copy

                // This fails if we use fieldName.Length, made it +1 to copy the null terminator.
                CopyMemory(buffer + sizeof(_EXT_TYPED_DATA), memPtr, fieldName.Length + 1);

                // Call Request(), Passing in the buffer we created as the In and Out Parameters
                if (FAILED(hr = DebugAdvanced.Request(DEBUG_REQUEST.EXT_TYPED_DATA_ANSI, (void*)buffer, totalSize, (void*)buffer, totalSize, null)))
                {
                    DebugLogging.OutputVerboseLine(this, "GetFieldVirtualAddress", "GetFieldVirtualAddress4: DebugAdvanced.Request Failed to get {1:x}!{2}.{3} hr={0:x}", hr, moduleBase, typeId, fieldName);
                    if (offset == 0)
                    {
                        FieldAddress = savedStructAddr;
                        return S_OK;
                    }
                    return hr;
                }

                // Convert the returned buffer to a _EXT_TYPED_Data CLASS (since it wont let me convert to a struct)
                var typedDataInClassForm = (_EXT_TYPED_DATA)Marshal.PtrToStructure(buffer, typeof(_EXT_TYPED_DATA));

                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", "Field Offset value {1} = {0} (Struct Address = {2:x}, offset ={3})", typedDataInClassForm.OutData.Offset, fieldName, savedStructAddr, offset);
                // OutData.Data has our field value.  This will always be a ulong

                FieldAddress = typedDataInClassForm.OutData.Offset;

                if (FieldAddress < savedStructAddr && offset == 0)
                {
                    FieldAddress = savedStructAddr;
                }

                DebugLogging.OutputDebugLine(this, "GetFieldVirtualAddress", "Final Fixed up Field Offset value {1} = {0}", FieldAddress, fieldName);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
                Marshal.FreeHGlobal(memPtr);
            }

            return S_OK;
        }
    }
}