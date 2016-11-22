using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Mex.Extensions;
using Microsoft.Mex.Framework;
using Microsoft.Mex.RangeList;

namespace Microsoft.Mex.DotNetDbg
{
    /// <summary>
    ///     Simple class to provide Windows major and minor version and platform ID information.
    /// </summary>
    public class SimpleVersion
    {
        public readonly uint Major;
        public readonly uint Minor;
        public readonly uint PlatformId;

        public SimpleVersion(uint platId, uint vMajor, uint vMinor)
        {
            PlatformId = platId;
            Major = vMajor;
            Minor = vMinor;
        }
    }

    public unsafe partial class DebugUtilities
    {
        private static readonly UInt16[] DefaultLanguages = {0x0409, 0x0000, 0x0809};
        private static readonly UInt16[] DefaultCodepages = {1200, 1252, 0};
        private SimpleVersion _simpleVersion;

        public SimpleVersion TargetWindowsVersion
        {
            get
            {
                if (_simpleVersion == null)
                {
                    uint platformId = 0;
                    uint vMajor = 0;
                    uint vMinor = 0;

                    DebugControl.GetSystemVersionValues(out platformId, out vMajor, out vMinor, null, null);

                    _simpleVersion = new SimpleVersion(platformId, vMajor, vMinor);
                }

                return _simpleVersion;
            }
        }

        /// <summary>
        /// Does not always return the same number of bytes you requested for large reads. Check the return array size to make sure it matches what you asked for. Don't know why this happens yet.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] ReadBytes(ulong addr, int count)
        {
            IntPtr buffer = IntPtr.Zero;

            try
            {
                buffer = Marshal.AllocHGlobal(count);

                uint cbRead = 0;
                ReadVirtual(addr, (uint)count, buffer, &cbRead);

                var arr = new byte[cbRead];

                Marshal.Copy(buffer, arr, 0, (int)cbRead);

                return arr;
            }
            finally
            {
                if (buffer != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }


        /// <summary>
        ///     A wrapper around IDebugDataSpaces::ReadVirtual
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="size">How many bytes to read</param>
        /// <param name="buffer">Buffer to receive the data</param>
        /// <param name="bytesRead">UInt32 to receive the number of bytes read</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual(UInt64 address, UInt32 size, IntPtr buffer, UInt32* bytesRead)
        {
            return DebugDataSpaces.ReadVirtual(address, buffer, size, bytesRead);
        }

        /// <summary>
        ///     Reads a 8-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">SByte to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual8(UInt64 address, out SByte value)
        {
            SByte tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(SByte), null);
            value = SUCCEEDED(hr) ? tempValue : ((SByte)0);
            return hr;
        }

        /// <summary>
        ///     Reads a 8-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Byte to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual8(UInt64 address, out Byte value)
        {
            Byte tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(Byte), null);
            value = SUCCEEDED(hr) ? tempValue : ((Byte)0);
            return hr;
        }

        /// <summary>
        ///     Reads a 16-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Int16 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual16(UInt64 address, out Int16 value)
        {
            Int16 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(Int16), null);
            value = SUCCEEDED(hr) ? tempValue : ((Int16)0);
            return hr;
        }

        /// <summary>
        ///     Reads a 16-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">UInt16 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual16(UInt64 address, out UInt16 value)
        {
            UInt16 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(UInt16), null);
            value = SUCCEEDED(hr) ? tempValue : ((UInt16)0);
            return hr;
        }

        /// <summary>
        ///     Reads a 32-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Int32 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual32(UInt64 address, out Int32 value)
        {
            Int32 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(Int32), null);
            value = SUCCEEDED(hr) ? tempValue : 0;
            return hr;
        }

        /// <summary>
        ///     Reads a 32-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">UInt32 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual32(UInt64 address, out UInt32 value)
        {
            UInt32 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(UInt32), null);
            value = SUCCEEDED(hr) ? tempValue : 0;
            return hr;
        }

        /// <summary>
        ///     Reads a 32-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>UInt32</returns>
        public UInt32 ReadUInt32(UInt64 address)
        {
            UInt32 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(UInt32), null);
            if (!SUCCEEDED(hr))
            {
                ThrowExceptionHere(hr);
            }
            return tempValue;
        }


        /// <summary>
        ///     Reads a GUID from an address and returns it in string format.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>Guid in format {b519f3ca-a2b9-4697-966d-628d6d79fdf4}</returns>
        public string ReadGuid(UInt64 address)
        {
            try
            {
                var data1 = ReadUInt32(address);
                var data2 = ReadUInt16(address + 0x4);
                var data3 = ReadUInt16(address + 0x6);
                var data4 = ReadBytes(address + 0x8, 8);

                var sb = new DbgStringBuilder(this);
                sb.Append(String.Format("{{{0,8:x8}-{1,4:x4}-{2,4:x4}-", data1, data2, data3));

                for (int i = 0; i < 2; i++)
                {
                    sb.Append(String.Format("{0,2:x2}", data4[i]));
                }

                sb.Append("-");

                for (int i = 2; i < 8; i++)
                {
                    sb.Append(String.Format("{0,2:x2}", data4[i]));
                }

                sb.Append("}");

                return sb.ToString();
            }
            catch (Exception e)
            {
                ThrowExceptionHere(e.ToString(), 0);
                return "";
            }
        }




        /// <summary>
        ///     Reads a 64-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>UInt64</returns>
        public UInt64 ReadUInt64(UInt64 address)
        {
            UInt64 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(UInt64), null);
            if (!SUCCEEDED(hr))
            {
                ThrowExceptionHere(hr);
            }
            return tempValue;
        }

        /// <summary>
        ///     Reads a 32-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>UInt16</returns>
        public UInt16 ReadUInt16(UInt64 address)
        {
            UInt16 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(UInt16), null);
            if (!SUCCEEDED(hr))
            {
                ThrowExceptionHere(hr);
            }
            return tempValue;
        }

        /// <summary>
        ///     Reads a 32-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <returns>UInt32</returns>
        public Int32 ReadInt32(UInt64 address)
        {
            Int32 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(Int32), null);
            if (!SUCCEEDED(hr))
            {
                ThrowExceptionHere(hr);
            }
            return tempValue;
        }

        /// <summary>
        ///     Reads a 64-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Int64 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual64(UInt64 address, out Int64 value)
        {
            Int64 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(Int64), null);
            value = SUCCEEDED(hr) ? tempValue : 0;
            return hr;
        }

        /// <summary>
        ///     Reads a 64-bit value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">UInt64 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadVirtual64(UInt64 address, out UInt64 value)
        {
            UInt64 tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(UInt64), null);
            value = SUCCEEDED(hr) ? tempValue : 0;
            return hr;
        }

        /// <summary>
        ///     Reads a single native unsigned integer from the target.
        /// </summary>
        /// <param name="offset">The address to read the value from</param>
        /// <param name="value">A UInt64 to receive the value</param>
        /// <returns>HRESULT of the operation</returns>
        public int ReadNativeUInt(UInt64 offset, out UInt64 value)
        {
            if (IsPointer64Bit())
            {
                return ReadVirtual64(offset, out value);
            }
            UInt32 tempValue;
            int hr = ReadVirtual32(offset, out tempValue);
            value = tempValue;
            return hr;
        }

        /// <summary>
        ///     Reads a single pointer from the target.
        ///     NOTE: POINTER VALUE IS SIGN EXTENDED TO 64-BITS WHEN NECESSARY!
        /// </summary>
        /// <param name="offset">The address to read the pointer from</param>
        /// <param name="value">A UInt64 to receive the pointer</param>
        /// <returns>HRESULT of the operation</returns>
        public int ReadPointer(UInt64 offset, out UInt64 value)
        {
            var pointerArray = new UInt64[1];
            int hr = DebugDataSpaces.ReadPointersVirtual(1, offset, pointerArray);
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
        public UInt64 ReadPointer(UInt64 offset)
        {
            var pointerArray = new UInt64[1];
            int hr = DebugDataSpaces.ReadPointersVirtual(1, offset, pointerArray);
            if (FAILED(hr))
            {
                ThrowExceptionHere(string.Format("Pointer Address: 0x{0:x}", offset), hr);
            }

            return pointerArray[0];
        }


        /// <summary>
        ///     Reads the specified number of pointers from the target
        /// </summary>
        /// <param name="offset">The address from which to begin reading</param>
        /// <param name="count">The number of pointers to be read</param>
        /// <param name="values">Pointers being returned</param>
        /// <returns>HRESULT of the operation</returns>
        public int ReadPointers(UInt64 offset, UInt32 count, out List<ulong> values)
        {
            ulong[] valuesArray;
            values = new List<ulong>();

            int hr = ReadPointers(offset, count, out valuesArray);
            if (FAILED(hr))
            {
                values = null;
            }
            else
            {
                foreach (ulong v in valuesArray)
                {
                    values.Add(v);
                }
            }
            return hr;
        }

        /// <summary>
        ///     Reads the specified number of pointers from the target
        /// </summary>
        /// <param name="offset">The address from which to begin reading</param>
        /// <param name="count">The number of pointers to be read</param>
        /// <param name="values">A UInt64[] that receives the pointers</param>
        /// <returns>HRESULT of the operation</returns>
        public int ReadPointers(UInt64 offset, UInt32 count, out UInt64[] values)
        {
            values = new UInt64[count];
            int hr = DebugDataSpaces.ReadPointersVirtual(count, offset, values);
            if (FAILED(hr))
            {
                values = null;
            }
            return hr;
        }

        /// <summary>
        ///     Reads a Double value from the target's virtual address space.
        /// </summary>
        /// <param name="address">Address to read from</param>
        /// <param name="value">Int64 to receive the value</param>
        /// <returns>HRESULT</returns>
        public int ReadDouble(UInt64 address, out Double value)
        {
            Double tempValue;
            int hr = DebugDataSpaces.ReadVirtual(address, (IntPtr)(&tempValue), sizeof(Double), null);
            value = SUCCEEDED(hr) ? tempValue : 0;
            return hr;
        }

        /// <summary>
        ///     Retrieves the name of a module in the target process.
        /// </summary>
        /// <param name="moduleBase">Base address of the module</param>
        /// <param name="name">String ot receive the module name</param>
        /// <returns>HRESULT</returns>
        public int GetModuleName(UInt64 moduleBase, out string name)
        {
            var sb = new StringBuilder(1024);

            int hr;
            if (FAILED(hr = DebugSymbols.GetModuleNameStringWide(DEBUG_MODNAME.MODULE, DEBUG_ANY_ID, moduleBase, sb, sb.Capacity, null)))
            {
                name = null;
            }
            else
            {
                name = sb.ToString();
            }

            return hr;
        }

        /// <summary>
        ///     Retrieves the name of a module in the target process.
        /// </summary>
        /// <param name="moduleBase">Base address of the module</param>
        /// <param name="path">String to receive the module path</param>
        /// <param name="name">String to receive the module name</param>
        /// <returns>HRESULT</returns>
        public int GetModuleName(UInt64 moduleBase, out string path, out string name)
        {
            var sbPath = new StringBuilder(1024);
            var sbName = new StringBuilder(1024);

            int hr;
            if (FAILED(hr = DebugSymbols.GetModuleNameStringWide(DEBUG_MODNAME.IMAGE, DEBUG_ANY_ID, moduleBase, sbPath, sbPath.Capacity, null)))
            {
                path = null;
            }
            else
            {
                path = sbPath.ToString();
            }
            if (FAILED(hr = DebugSymbols.GetModuleNameStringWide(DEBUG_MODNAME.MODULE, DEBUG_ANY_ID, moduleBase, sbName, sbName.Capacity, null)))
            {
                name = null;
            }
            else
            {
                name = sbName.ToString();
            }

            return hr;
        }

        /// <summary>
        ///     Wraps IDebugSymbols2::GetModuleNameString
        /// </summary>
        /// <param name="moduleBase">Base address of the module</param>
        /// <param name="moduleNameString">String to receive the module name string</param>
        /// <returns>HRESULT</returns>
        public int GetModuleNameString(UInt64 moduleBase, out string moduleNameString)
        {
            int hr;
            var sb = new StringBuilder(1024);
            if (FAILED(hr = DebugSymbols.GetModuleNameStringWide(DEBUG_MODNAME.IMAGE, DEBUG_ANY_ID, moduleBase, sb, sb.Capacity, null)))
            {
                OutputVerboseLine("ERROR! IDebugSymbols2::GetModuleNameString failed for module {0:p}: {1:x8}", moduleBase, hr);
                moduleNameString = "";
            }
            else
            {
                moduleNameString = sb.ToString();
            }
            return hr;
        }

        /// <summary>
        ///     Get the number of modules in the process
        /// </summary>
        /// <param name="loaded">Number of loaded modules</param>
        /// <param name="unloaded">Number of unloaded modules</param>
        /// <returns>HRESULT</returns>
        public int GetModuleCount(out uint loaded, out uint unloaded)
        {
            int hr;
            if (FAILED(hr = DebugSymbols.GetNumberModules(out loaded, out unloaded)))
            {
                OutputVerboseLine("ERROR! Failed getting the number of loaded/unloaded modules: {0:x8}", hr);
            }
            return hr;
        }

        /// <summary>
        ///     Wraps IDebugSymbols2::GetModuleParameters
        /// </summary>
        /// <param name="count">Count</param>
        /// <param name="bases">Bases</param>
        /// <param name="start">Start</param>
        /// <param name="parameters">Parameters</param>
        /// <returns>HRESULT</returns>
        public int GetModuleParameters(uint count, ulong[] bases, uint start, DEBUG_MODULE_PARAMETERS[] parameters)
        {
            int hr;
            if (FAILED(hr = DebugSymbols.GetModuleParameters(count, bases, start, parameters)))
            {
                OutputVerboseLine("ERROR! Failed getting module parameters: {0:x8}", hr);
            }
            return hr;
        }

        /// <summary>
        ///     Gets the description of a registers
        /// </summary>
        /// <param name="register">The register number to query</param>
        /// <param name="registerName">The name of the register</param>
        /// <param name="registerDescription">A register description structure</param>
        /// <returns>HRESULT</returns>
        public int GetRegisterDescription(uint register, out string registerName, DEBUG_REGISTER_DESCRIPTION* registerDescription)
        {
            int hr;
            var sb = new StringBuilder(128);
            if (FAILED(hr = DebugRegisters.GetDescriptionWide(register, sb, sb.Capacity, null, registerDescription)))
            {
                OutputVerboseLine("Failed getting register name: {0:x8}", hr);
                registerName = "Register " + register;
            }
            else
            {
                registerName = sb.ToString();
            }
            return hr;
        }

        /// <summary>
        ///     Gets the size of a module in the target process
        /// </summary>
        /// <param name="moduleBase">Base address of the module</param>
        /// <param name="moduleSize">Size of the module in bytes</param>
        /// <returns>HRESULT</returns>
        public int GetModuleSize(UInt64 moduleBase, out UInt32 moduleSize)
        {
            int hr;
            var paramsArray = new DEBUG_MODULE_PARAMETERS[1];
            var basesArray = new[] {moduleBase};

            if (FAILED(hr = DebugSymbols.GetModuleParameters(1, basesArray, 0, paramsArray)))
            {
                moduleSize = 0;
            }
            else
            {
                moduleSize = paramsArray[0].Size;
            }

            return hr;
        }

        ///// <summary>
        ///// Determines whether an address is in user or kernel space
        ///// </summary>
        ///// <param name="address">Address to check</param>
        ///// <returns>True if user-mode, False if kernel</returns>
        //public bool IsAddressUserMode(UInt64 address)
        //{
        //    UInt64 mmSystemRangeStart;
        //    if (SUCCEEDED(ReadGlobalAsPointer("nt", "MmSystemRangeStart", out mmSystemRangeStart)) && (mmSystemRangeStart != 0))
        //    {
        //        return (address < mmSystemRangeStart);
        //    }

        //    UInt64 kernelSystemRangeStart;
        //    if (SUCCEEDED(ReadGlobalAsPointer("kernel32", "SystemRangeStart", out kernelSystemRangeStart)) && (kernelSystemRangeStart != 0))
        //    {
        //        return (address < kernelSystemRangeStart);
        //    }

        //    UInt64 moduleBase;
        //    if (SUCCEEDED(GetModuleBase("hal", out moduleBase)) || SUCCEEDED(GetModuleBase("nt", out moduleBase)))
        //    {
        //        return address < moduleBase;
        //    }

        //    return true;
        //}


        ///// <summary>
        ///// Determines whether an address is in kernel space
        ///// </summary>
        ///// <param name="address">Address to check</param>
        ///// <returns>True if kernel, false if usermode</returns>
        //public bool IsAddressKernelMode(UInt64 address)
        //{
        //    UInt64 mmSystemRangeStart;
        //    if (SUCCEEDED(ReadGlobalAsPointer("nt", "MmSystemRangeStart", out mmSystemRangeStart)) && (mmSystemRangeStart != 0))
        //    {
        //        return (address > mmSystemRangeStart);
        //    }

        //    UInt64 kernelSystemRangeStart;
        //    if (SUCCEEDED(ReadGlobalAsPointer("kernel32", "SystemRangeStart", out kernelSystemRangeStart)) && (kernelSystemRangeStart != 0))
        //    {
        //        return (address > kernelSystemRangeStart);
        //    }

        //    UInt64 moduleBase;
        //    if (SUCCEEDED(GetModuleBase("hal", out moduleBase)) || SUCCEEDED(GetModuleBase("nt", out moduleBase)))
        //    {
        //        return address > moduleBase;
        //    }

        //    return false;
        //}

        //public bool IsAddressPool(UInt64 address)
        //{
        //    string poolValOutput = string.Empty;
        //    bool isPool = true;

        //    RunCommandSaveOutput(out poolValOutput, "!head 1 !poolval {0:p}", address);

        //    if (poolValOutput.Contains("Unknown"))
        //    {
        //        isPool = false;
        //    }


        //    return isPool;
        //}


        /// <summary>
        ///     Reads either an IMAGE_THUNK_DATA64 or an IMAGE_THUNK_DATA32 from an address in memory.
        /// </summary>
        /// <param name="thunkAddress">Address of the thunk</param>
        /// <param name="thunkIs64bit">True if we should read a 64-bit thunk, false for 32-bit</param>
        /// <param name="thunkOutput">Pointer to a structure to receive the thunk data</param>
        /// <returns>HRESULT</returns>
        public int ReadImageDataThunk(UInt64 thunkAddress, bool thunkIs64bit, IMAGE_THUNK_DATA64* thunkOutput)
        {
            int hr;
            var thunkSize = (UInt32)(thunkIs64bit ? Marshal.SizeOf(typeof(IMAGE_THUNK_DATA64)) : Marshal.SizeOf(typeof(IMAGE_THUNK_DATA32)));

            if (FAILED(hr = ReadVirtual(thunkAddress, thunkSize, (IntPtr)thunkOutput, null)))
            {
                return hr;
            }

            if (thunkIs64bit == false)
            {
                thunkOutput->Function = SignExtendAddress(thunkOutput->Function);
            }

            return hr;
        }

        /// <summary>
        ///     Provides a wrapped version of IDebugSymbols2.GetModuleVersionInformation that allocates a buffer of the correct
        ///     size for the requested data.
        ///     NOTE: The output buffer can be set to null if querying the path succeeds but returns a size of 0. This scenario
        ///     returns a SUCCESS HRESULT.
        /// </summary>
        /// <param name="moduleBase">Base address of the module to read from</param>
        /// <param name="requestedInfo">String containing the path to query</param>
        /// <param name="buffer">Allocated buffer containing the data</param>
        /// <param name="bufferSize">Size of the allocated buffer</param>
        /// <returns>HRESULT</returns>
        public int GetModuleVersionInformation(UInt64 moduleBase, string requestedInfo, out IntPtr buffer, out UInt32 bufferSize)
        {
            /* NOTE! If this is every converted to use the wide version in IDebugSymbols3 then GetAdvancedModuleInfo must be changed as well! */

            int allocationSize = 2048;
            IntPtr tempBuffer = Marshal.AllocHGlobal(allocationSize);
            UInt32 bytesRead = 0;
            for (;;)
            {
                int hr = DebugSymbols.GetModuleVersionInformation(DEBUG_ANY_ID, moduleBase, requestedInfo, tempBuffer, allocationSize, &bytesRead);
                if (FAILED(hr))
                {
                    Marshal.FreeHGlobal(tempBuffer);
                    buffer = IntPtr.Zero;
                    bufferSize = 0;
                    return hr;
                }
                if (hr == S_FALSE)
                {
                    Marshal.FreeHGlobal(tempBuffer);
                    tempBuffer = Marshal.AllocHGlobal(allocationSize <<= 2);
                }
                    //else if (hr == S_OK)
                else
                {
                    buffer = tempBuffer;
                    bufferSize = bytesRead;
                    return hr;
                }
            }
        }

        /// <summary>
        ///     Provides a wrapped version of IDebugSymbols2.GetModuleVersionInformation that can be used when requesting string
        ///     information.
        /// </summary>
        /// <param name="moduleBase">Base address of the module to read from</param>
        /// <param name="requestedInfo">String containing the path to query</param>
        /// <param name="data">String to receive the data</param>
        /// <returns>HRESULT</returns>
        public int GetModuleVersionInformationAsString(UInt64 moduleBase, string requestedInfo, out string data)
        {
            IntPtr infoBuffer;
            UInt32 infoBufferSize;
            int hr;
            if (SUCCEEDED(hr = GetModuleVersionInformation(moduleBase, requestedInfo, out infoBuffer, out infoBufferSize)) && (infoBufferSize > 0))
            {
                data = Marshal.PtrToStringAnsi(infoBuffer);
            }
            else
            {
                data = String.Empty;
            }
            if (infoBuffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(infoBuffer);
            }
            return hr;
        }

        /// <summary>
        ///     Retrieves a string in the modules \StringFileInfo\[translation]\ block.
        ///     NOTE: Do not specify the full path, just what would come after the hex language and code page
        /// </summary>
        /// <param name="moduleBase">Base address of the module to read from</param>
        /// <param name="requestedInfo">String containing the path to query</param>
        /// <param name="output">The output string</param>
        /// <returns>HRESULT</returns>
        public int GetModuleStringFileInfo(UInt64 moduleBase, string requestedInfo, out string output)
        {
            int hr;
            string fvpath;

            foreach (ushort language in DefaultLanguages) {
                foreach (ushort codePage in DefaultCodepages) {
                    fvpath = FormatString(@"\StringFileInfo\{0:x4}{1:x4}\{2}", language, codePage, requestedInfo);
                    string stringData;
                    if (SUCCEEDED(hr = GetModuleVersionInformationAsString(moduleBase, fvpath, out stringData)))
                    {
                        output = stringData;
                        return hr;
                    }
                }
            }

            IntPtr buffer = IntPtr.Zero;
            UInt32 bufferSize;
            if (SUCCEEDED(hr = GetModuleVersionInformation(moduleBase, @"\VarFileInfo\Translation", out buffer, out bufferSize)))
            {
                if (bufferSize == 0)
                {
                    goto Error;
                }

                var languages = (LANGANDCODEPAGE*)buffer.ToPointer();
                for (int i = 0; i < (bufferSize/Marshal.SizeOf(typeof(LANGANDCODEPAGE))); ++i)
                {
                    fvpath = FormatString(@"\StringFileInfo\{0:x4}{1:x4}\{2}", languages[i].wLanguage, languages[i].wCodePage, requestedInfo);
                    string stringData;
                    if (SUCCEEDED(hr = GetModuleVersionInformationAsString(moduleBase, fvpath, out stringData)))
                    {
                        output = stringData;
                        goto Exit;
                    }
                }

                Marshal.FreeHGlobal(buffer);
                buffer = IntPtr.Zero;
            }

            Error:
            /* If we get here we couldn't find any valid language, hr should be a failure unless Translation succeeded but returned 0 bytes. */
            output = "";
            Exit:
            if (buffer != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(buffer);
            }
            return hr;
        }

        /// <summary>
        ///     Wraps a call to IDebugSymbols2.GetModuleVersionInformation with a path of "\"
        /// </summary>
        /// <param name="moduleBase">Base address of the module to read from</param>
        /// <param name="fileInfo">The requested information</param>
        /// <returns>HRESULT</returns>
        public int GetModuleVersionInformation(UInt64 moduleBase, out VS_FIXEDFILEINFO fileInfo)
        {
            fixed (void* dataPtr = &fileInfo)
            {
                return DebugSymbols.GetModuleVersionInformation(DEBUG_ANY_ID, moduleBase, @"\", (IntPtr)dataPtr, Marshal.SizeOf(typeof(VS_FIXEDFILEINFO)), null);
            }
        }

        /// <summary>
        ///     Gets the size of a memory page.
        /// </summary>
        /// <returns>Page size</returns>
        public uint GetPageSize()
        {
            uint pageSize;
            int hr;

            if (FAILED(hr = DebugControl.GetPageSize(out pageSize)))
            {
                ThrowExceptionHere(hr);
            }
            return pageSize;
        }

        /// <summary>
        ///     Gets the size of a memory page.
        ///     If an error occurs the output value is 4096 as that is the most common page size currently in use.
        /// </summary>
        /// <param name="pageSize">
        ///     The size of a page in memory. If a failure occurs this will be set to 4096 as that is the most
        ///     common value.
        /// </param>
        /// <returns>HRESULT</returns>
        public int GetPageSize(out UInt32 pageSize)
        {
            int hr = DebugControl.GetPageSize(out pageSize);
            if (FAILED(hr))
            {
                pageSize = 4096;
            }
            return hr;
        }

        /// <summary>
        ///     Tries to determine the value of the kernel start address
        /// </summary>
        /// <param name="kernelStart">The start of the kernel address range</param>
        /// <returns>HRESULT</returns>
        public int GetKernelStartAddress(out UInt64 kernelStart)
        {
            UInt64 mmSystemRangeStart;
            if (SUCCEEDED(ReadGlobalAsPointer("nt", "MmSystemRangeStart", out mmSystemRangeStart)) && (mmSystemRangeStart != 0))
            {
                kernelStart = mmSystemRangeStart;
                return S_OK;
            }

            UInt64 kernelSystemRangeStart;
            if (SUCCEEDED(ReadGlobalAsPointer("kernel32", "SystemRangeStart", out kernelSystemRangeStart)) && (kernelSystemRangeStart != 0))
            {
                kernelStart = kernelSystemRangeStart;
                return S_OK;
            }

            kernelStart = 0;
            return E_FAIL;
        }

        /// <summary>
        ///     Treats the data as array of ANSI character values and returns the length as if it is a null-terminated string
        /// </summary>
        /// <param name="rawTextBytes">Input string as bytes</param>
        /// <returns>Length without terminating null</returns>
        public static int StrlenANSI(byte[] rawTextBytes)
        {
            for (int i = 0; i < rawTextBytes.Length; ++i)
            {
                if (rawTextBytes[i] == 0)
                {
                    return i;
                }
            }
            return rawTextBytes.Length;
        }

        /// <summary>
        ///     Checks if a byte array contains any null bytes
        /// </summary>
        /// <param name="data">Byte array to check</param>
        /// <returns>True if any byte is 0, False otherwise</returns>
        public static bool ContainsNulls(byte[] data)
        {
            foreach (byte t in data) {
                if (t == 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Attempts to determine if a string contains single-byte characters or double-byte characters
        ///     NOTES:
        ///     If the input contains a UTF-8 or UTF-16 Byte Order Mark then that will be the sole determinant
        ///     The input array is assumed to contain a single, non-null terminated string
        ///     Pure binary data with no embedded nulls will be detected as single-byte
        /// </summary>
        /// <param name="rawTextBytes">Input string as bytes</param>
        /// <returns>True if the data seems to be DBCS, False otherwise</returns>
        public static bool IsDBCS(byte[] rawTextBytes)
        {
            if (rawTextBytes.Length >= 2)
            {
                /* UTF-16 BOM, 2-bytes */
                if (((rawTextBytes[0] == 0xff) && (rawTextBytes[1] == 0xfe)) || ((rawTextBytes[0] == 0xfe) || (rawTextBytes[1] == 0xff)))
                {
                    return true;
                }
                /* UTF-8 BOM, 3-bytes */
                if (rawTextBytes.Length >= 3)
                {
                    if ((rawTextBytes[0] == 0xef) && (rawTextBytes[1] == 0xbb) && (rawTextBytes[2] == 0xbf))
                    {
                        return false;
                    }
                }
            }
            return ContainsNulls(rawTextBytes);
        }

        /// <summary>
        ///     Gets the memory ranges of the process heaps.
        ///     NOTE! If two heaps are completely adjacent they will be merged into a single range!
        /// </summary>
        public int GetHeapRanges(out RangeList_UInt64 heapRanges)
        {
            heapRanges = new RangeList_UInt64();

            UInt64 peb;
            int hr = ExpressionToPointer("$peb", out peb);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed evaluating \"$peb\": {0:x8}", hr);
                return hr;
            }
            if (peb == 0)
            {
                OutputVerboseLine("ERROR! $peb evaluated to 0");
                return E_FAIL;
            }

            uint numberOfHeapsOffset;
            hr = GetFieldOffset("ntdll", "_PEB", "NumberOfHeaps", out numberOfHeapsOffset);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed getting the offset of _PEB!NumberOfHeaps: {0:x8}", hr);
                return hr;
            }
            OutputVerboseLine("GetHeapRanges: ntdll!_PEB.NumberOfHeaps offset: {0:x8}", numberOfHeapsOffset);

            uint processHeapsOffset;
            hr = GetFieldOffset("ntdll", "_PEB", "ProcessHeaps", out processHeapsOffset);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed getting the offset of _PEB!ProcessHeaps: {0:x8}", hr);
                return hr;
            }
            OutputVerboseLine("GetHeapRanges: ntdll!_PEB.ProcessHeaps offset: {0:x8}", processHeapsOffset);

            UInt64 processHeaps;
            hr = ReadPointer(peb + processHeapsOffset, out processHeaps);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed retrieving the value of _PEB!ProcessHeaps: {0:x8}", hr);
                return hr;
            }
            if (processHeaps == 0)
            {
                OutputVerboseLine("ERROR! _PEB!ProcessHeaps was read as 0");
                return E_FAIL;
            }
            OutputVerboseLine("GetHeapRanges: ntdll!_PEB.ProcessHeaps: {0:x8}", processHeaps);

            UInt32 numberOfHeaps;
            hr = ReadVirtual32(peb + numberOfHeapsOffset, out numberOfHeaps);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed retrieving the value of _PEB!NumberOfHeaps: {0:x8}", hr);
                return hr;
            }
            if (numberOfHeaps == 0)
            {
                OutputVerboseLine("ERROR! _PEB!NumberOfHeaps was read as 0");
                return E_FAIL;
            }
            OutputVerboseLine("GetHeapRanges: ntdll!_PEB.NumberOfHeaps: {0:x8}", numberOfHeaps);

            UInt64 pointerSize = PointerSize();
            UInt32 pageSize;
            hr = GetPageSize(out pageSize);
            if (FAILED(hr) || (pageSize == 0))
            {
                OutputWarningLine("WARNING: Failed determining the size of a page, will continue assuming 4096 bytes: {0:x8}", hr);
                pageSize = 4096;
            }

            uint segmentListOffset = 0;
            uint segmentListEntryOffset = 0;
            uint segmentsOffset = 0;
            if (SUCCEEDED(GetFieldOffset("ntdll", "_HEAP", "SegmentList", out segmentListOffset)) && (segmentListOffset != 0))
            {
                if (FAILED(hr = GetFieldOffset("ntdll", "_HEAP_SEGMENT", "SegmentListEntry", out segmentListEntryOffset)))
                {
                    OutputVerboseLine("ERROR! Failed getting the offset of ntdll!_HEAP_SEGMENT.SegmentListEntry: {0:x8}", hr);
                    return E_FAIL;
                }
            }
            else if (SUCCEEDED(GetFieldOffset("ntdll", "_HEAP", "Segments", out segmentsOffset)) && (segmentsOffset != 0))
            {
                segmentListOffset = 0;
                /* Nothing to do here */
            }
            else
            {
                OutputVerboseLine("ERROR! ntdll!_HEAP has neither SegmentList nor Segments field");
                return E_FAIL;
            }

            for (uint i = 0; i < numberOfHeaps; ++i)
            {
                UInt64 heapPtr;
                hr = ReadPointer(processHeaps + (i*pointerSize), out heapPtr);
                if (FAILED(hr))
                {
                    OutputVerboseLine("ERROR! Failed reading heap pointer at _PEB!ProcessHeaps[{0}]: {1:x8}", i, hr);
                    continue;
                }
                OutputVerboseLine("GetHeapRanges: Process heap {0}: {1:p}", i, heapPtr);

                if (segmentListOffset != 0)
                {
                    UInt64[] segments;
                    hr = WalkList(heapPtr + segmentListOffset, out segments, segmentListEntryOffset);
                    if (FAILED(hr))
                    {
                        OutputVerboseLine("WARNING! GetHeapRanges: Failed walking SegmentList: {0:x8}", hr);
                        continue;
                    }
                    foreach (ulong segment in segments) {
                        GetHeapRangesHelper(pageSize, segment, heapRanges);
                    }
                }
                else
                {
                    //FIX ME!!! Hardcoding 64. This is not used in modern OSs so this probably will never change.
                    for (uint x = 0; x < 64; ++x)
                    {
                        UInt64 segment;
                        hr = ReadPointer(heapPtr + segmentsOffset + (x*pointerSize), out segment);
                        if (FAILED(hr) || (segment == 0))
                        {
                            break;
                        }
                        GetHeapRangesHelper(pageSize, segment, heapRanges);
                    }
                }
            }

            return S_OK;
        }

        private void GetHeapRangesHelper(uint pageSize, ulong heapSegment, RangeList_UInt64 heapRanges)
        {
            UInt64 baseAddress;
            int hr = ReadPointerFromStructure("ntdll", "_HEAP_SEGMENT", "BaseAddress", heapSegment, out baseAddress);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed reading the base address from heap {0:p}: {1:x8}", baseAddress, hr);
                return;
            }
            if (baseAddress == 0)
            {
                OutputWarningLine("WARNING: Base address for heap {0:p} was read as 0, skipping");
                return;
            }

            UInt32 numberOfPages;
            hr = ReadUInt32FromStructure("ntdll", "_HEAP_SEGMENT", "NumberOfPages", heapSegment, out numberOfPages);
            if (FAILED(hr))
            {
                OutputVerboseLine("ERROR! Failed reading the number of pages from heap {0:p}: {1:x8}", baseAddress, hr);
                return;
            }
            if (numberOfPages == 0)
            {
                OutputWarningLine("WARNING: Heap {0:p} has no pages associated with it, skipping");
                return;
            }

            heapRanges.AddLength(baseAddress, pageSize*numberOfPages);

            OutputVerboseLine("Heap {0} {1:p} : BaseAddr={2:p}, NumPages={3}", heapRanges.Count - 1, heapSegment, baseAddress, numberOfPages);
        }


        /// <summary>
        ///     Use this for walking POINTERS in an array at an arbitrary address
        ///     Returns the address and count of elements in the specified module!entry
        /// </summary>
        /// <param name="addr">address</param>
        /// <param name="count">number of array entries to read</param>
        /// <param name="pointers">return pointers</param>
        /// <returns></returns>
        public int WalkArrayOfPointersAtAddress(ulong addr, uint count, out List<ulong> pointers)
        {
            int hr = S_OK;
            ulong addrBase = addr;
            ulong ptrSize = PointerSize();
            pointers = new List<ulong>();

            for (ulong i = 0; i < count; i++)
            {
                ulong addrThis = addrBase + ptrSize*i;
                ulong ptr;

                hr = ReadPointer(addrThis, out ptr);

                if (FAILED(hr))
                {
                    return hr;
                }

                pointers.Add(ptr);
            }

            return hr;
        }

        /// <summary>
        ///     Use this for walking an array of POINTERS stored in a field in a module!_STRUCTURE
        ///     Returns the address and count of elements in the specified module!entry
        /// </summary>
        /// <param name="structAddr">Address of the object/structure this is in</param>
        /// <param name="symbolName">module.  Ex: nt!_DRIVER_OBJECT</param>
        /// <param name="fieldName">field.  Ex: MajorFunction</param>
        /// <param name="count">number of array pointers to read</param>
        /// <param name="pointers">return pointers</param>
        /// <returns></returns>
        public int WalkArrayOfPointersInStructure(ulong structAddr, string symbolName, string fieldName, uint count, out List<ulong> pointers)
        {
            int hr = S_OK;
            pointers = new List<ulong>();

            uint offset;
            ulong addrBase = structAddr;
            uint ptrSize = PointerSize();
            hr = GetFieldOffset(symbolName, fieldName, out offset);

            if (FAILED(hr))
            {
                return hr;
            }
            addrBase = addrBase + offset;

            for (ulong i = 0; i < count; i++)
            {
                ulong addrThis = addrBase + ptrSize*i;
                ulong ptr;

                hr = ReadPointer(addrThis, out ptr);

                if (FAILED(hr))
                {
                    return hr;
                }

                pointers.Add(ptr);
            }

            return hr;
        }


        /// <summary>
        ///     Use this for walking an array of UINT32s stored in a field in a module!_STRUCTURE
        ///     Returns the address and count of elements in the specified module!entry
        /// </summary>
        /// <param name="structAddr">Address of the object/structure this is in</param>
        /// <param name="symbolName">module.  Ex: nt!_DRIVER_OBJECT</param>
        /// <param name="fieldName">field.  Ex: MajorFunction</param>
        /// <param name="count">number of array UInt32s to read</param>
        /// <param name="values">return values</param>
        /// <returns></returns>
        public int WalkArrayOfUInt32InStructure(ulong structAddr, string symbolName, string fieldName, uint count, out List<UInt32> values)
        {
            int hr = S_OK;
            values = new List<UInt32>();

            uint offset;
            ulong addrBase = structAddr;
            uint ptrSize = PointerSize();
            hr = GetFieldOffset(symbolName, fieldName, out offset);

            if (FAILED(hr))
            {
                return hr;
            }
            addrBase = addrBase + offset;

            for (ulong i = 0; i < count; i++)
            {
                ulong addrThis = addrBase + ptrSize*i;
                UInt32 val;

                hr = ReadVirtual32(addrThis, out val);

                if (FAILED(hr))
                {
                    return hr;
                }

                values.Add(val);
            }

            return hr;
        }


        /// <summary>
        ///     Use this overload for walking an array of STRUCTURES embedded in a module!global
        ///     Returns the address and count of elements in the specified module!entry
        /// </summary>
        /// <param name="moduleName">Module name</param>
        /// <param name="arrayName">Name of the array</param>
        /// <param name="typeName">Type of the array structures</param>
        /// <param name="arrayEntries">Returns POINTERS to the structures</param>
        /// <returns></returns>
        public int WalkArrayInGlobalStructure(string moduleName, string arrayName, string typeName, out List<ulong> arrayEntries)
        {
            int hr = S_OK;
            ulong numberOfElements = 0;
            arrayEntries = new List<ulong>();

            ulong globalAddress;

            if (FAILED(hr = GetGlobalAddress(moduleName, arrayName, out globalAddress)))
            {
                return hr;
            }

            uint array_Size;
            if (FAILED(hr = GetTypeSize(moduleName, arrayName, out array_Size)))
            {
                return hr;
            }

            uint entry_Size;
            if (FAILED(hr = GetTypeSize(moduleName, typeName, out entry_Size)))
            {
                return hr;
            }

            // Well, this is almost too easy...
            numberOfElements = array_Size/entry_Size;

            for (ulong i = 0; i < numberOfElements; i++)
            {
                arrayEntries.Add(globalAddress + i*entry_Size);
            }

            return hr;
        }

        /// <summary>
        ///     Use this for walking an array of STRUCTURES at a given memory address
        ///     This returns a list of pointers to each of your objects in the array.
        /// </summary>        
        /// <param name="arrayType">Name of the type in the array</param>
        /// <param name="startingAddress">Address of the first element in the array</param>
        /// <param name="numberOfElements">This cannot be zero.</param>
        /// <returns></returns>
        public List<ulong> WalkArray(string arrayType, ulong startingAddress, ulong numberOfElements = 0)
        {
            if (numberOfElements == 0)
                throw new Exception("Number of elements in array cannot be zero");

            var arrayEntries = new List<ulong>();
            int hr = S_OK;

            string arrayStructModName = "";
            string arrayStructTypeName = "";

            if (arrayType.Contains("!"))
            {
                String[] parts = arrayType.Split('!');
                arrayStructModName = parts[0];
                arrayStructTypeName = parts[1];
            }

            uint entry_Size;
            if (FAILED(hr = GetTypeSize(arrayStructModName, arrayStructTypeName, out entry_Size)))
            {
                ThrowExceptionHere(hr);
            }            

            for (ulong i = 0; i < numberOfElements; i++)
            {
                arrayEntries.Add(startingAddress + (i * entry_Size));
            }

            return arrayEntries;
        }


        /// <summary>
        ///     Use this for walking an array of STRUCTURES embedded in another STRUCTURE
        ///     This returns a list of pointers to each of your objects in the array.
        /// </summary>
        /// <param name="parentStructure">Structure Type that has the array member</param>
        /// <param name="memberArrayName">Name of the structures member that contains the array</param>
        /// <param name="arrayMemberType">Name of the type in the array</param>
        /// <param name="parentAdddress"></param>
        /// <param name="arrayEntries">Returns POINTERS to the structures</param>
        /// <param name="numberOfElements"></param>
        /// <returns></returns>
        public int WalkArrayInStructure(string parentStructure, string memberArrayName, string arrayMemberType, ulong parentAdddress, out List<ulong> arrayEntries, ulong numberOfElements = 0)
        {
            arrayEntries = new List<ulong>();
            int hr = S_OK;
            uint arrayOffset;

            GetFieldOffset(parentStructure, memberArrayName, out arrayOffset);

            string arrayStructModName = "";
            string arrayStructTypeName = "";

            if (arrayMemberType.Contains("!"))
            {
                String[] parts = arrayMemberType.Split('!');
                arrayStructModName = parts[0];
                arrayStructTypeName = parts[1];
            }

            uint entry_Size;
            if (FAILED(hr = GetTypeSize(arrayStructModName, arrayStructTypeName, out entry_Size)))
            {
                return hr;
            }

            if (numberOfElements == 0)
            {
                uint array_Size;
                string szArraySize = RunCommandSaveOutput("!mex.cut -f 3 ??#RTL_FIELD_SIZE({0},{1})", parentStructure, memberArrayName);
                uint.TryParse(szArraySize.Replace("0x", ""), NumberStyles.HexNumber, null, out array_Size);

                numberOfElements = array_Size/entry_Size;
            }

            for (ulong i = 0; i < numberOfElements; i++)
            {
                arrayEntries.Add((parentAdddress + arrayOffset) + i*entry_Size);
            }
            return hr;
        }


        /// <summary>
        ///     Maps a bit mask to which values are are set. This takes a string array that contains the names of each value in
        ///     order of least significant to most
        ///     significant bit and the input bit mask
        /// </summary>
        /// <param name="flagNames">an array of strings that name each value represented by the bits in the bit mask</param>
        /// <param name="flags">the bit mask to be decoded</param>
        /// <returns>a comma separated string value that represents which flags are enabled in the provided bit mask</returns>
        public string DecodeFlags(string[] flagNames, UInt32 flags)
        {
            //convert flag value into flag names

            //setup the bit test controller
            UInt32 flagTest = 1;
            string decodedFlags = "";

            //go through each flag only do this is flags is not null
            if (flags != 0)
            {
                //march through each bit comparing the flagTest to the flags value and if the bit is on, use i as the index into the array of flag names to build our flag values
                for (int i = 0; i < flagNames.Length; i++, flagTest <<= 1)
                {
                    if ((flags & flagTest) != 0)
                    {
                        if (decodedFlags.Length > 0)
                        {
                            decodedFlags += ", ";
                        }

                        decodedFlags += flagNames[i];
                    }
                }
            }

            return decodedFlags;
        }


        /// Attempts to get the PEB using a number of different methods.
        /// 
        /// WARNING! This should only be called in a usermode dump! In kernel mode this will probably return a KPROCESS that may or may not be for the current process
        public int GetPEB_UserMode(out UInt64 peb)
        {
            int hr;

            if (Wow64Exts.IsEffectiveProcessorSameAsActualProcessor(this) == false)
            {
                uint peb32;
                if (SUCCEEDED(hr = ReadGlobalAsUInt32("wow64", "Peb32", out peb32)))
                {
                    peb = peb32;
                    return hr;
                }
            }

            if (SUCCEEDED(hr = DebugSystemObjects.GetCurrentProcessPeb(out peb)) && (peb != 0UL))
            {
                return hr;
            }
            if (SUCCEEDED(hr = DebugSystemObjects.GetCurrentProcessDataOffset(out peb)) && (peb != 0UL))
            {
                return hr;
            }
            if (SUCCEEDED(hr = ExpressionToPointer("$peb", out peb)) && (peb != 0UL))
            {
                return hr;
            }
            if (FAILED(hr))
            {
                OutputVerboseLine("DotNetDbg.GetPEB_Usermode: Failed evaluating $peb: {0:x8}", hr);
            }
            else
            {
                OutputVerboseLine("DotNetDbg.GetPEB_Usermode: $peb evaluated to 0");
                hr = E_FAIL;
            }
            return hr;
        }

        public List<UInt64> ReadArrayOfUInt64s(UInt64 address, uint count)
        {
            var array = new List<ulong>();

            for (uint i = 0; i < count; i++)
            {
                ulong value;
                int hr = ReadVirtual64(address + i * 8, out value);
                if (FAILED(hr))
                {
                    ThrowExceptionHere(hr);
                }

                array.Add(value);
            }

            return array;
        }

        public List<UInt32> ReadArrayOfUInt32s(UInt64 address, uint count)
        {
            var array = new List<uint>();

            for (uint i = 0; i < count; i++)
            {
                uint value;
                int hr = ReadVirtual32(address + i*4, out value);
                if (FAILED(hr))
                {
                    ThrowExceptionHere(hr);
                }

                array.Add(value);
            }

            return array;
        }

        public List<UInt16> ReadArrayOfUInt16s(UInt64 address, uint count)
        {
            var array = new List<UInt16>();

            for (uint i = 0; i < count; i++)
            {
                UInt16 value;
                int hr = ReadVirtual16(address + i*2, out value);
                if (FAILED(hr))
                {
                    ThrowExceptionHere(hr);
                }

                array.Add(value);
            }

            return array;
        }

        public int ReadArrayOfPointers(UInt64 address, uint count, out UInt64[] pointers)
        {
            pointers = new UInt64[count];
            uint size = count*PointerSize();

            if (PointerSize() == 4)
            {
                var pointers_Temp = new UInt32[count];

                fixed (UInt32* pointersPtr = pointers_Temp)
                {
                    uint bytesRead = 0;
                    int hr = ReadVirtual(address, size, (IntPtr)pointersPtr, &bytesRead);
                    if (FAILED(hr))
                    {
                        pointers = null;
                        return hr;
                    }

                    for (int i = 0; i < count; i++)
                    {
                        pointers[i] = SignExtendAddress(pointers_Temp[i]);
                    }

                    return hr;
                }
            }
            fixed (UInt64* pointersPtr = pointers)
            {
                uint bytesRead = 0;
                int hr = ReadVirtual(address, size, (IntPtr)pointersPtr, &bytesRead);
                if (FAILED(hr))
                {
                    pointers = null;
                }
                return hr;
            }
        }

        public static string FormatTimeSpan(TimeSpan ts)
        {
            string format = string.Empty;
            if (ts.Days > 0)
            {
                format = string.Format("{0}{1}{2}{3}.{4:000}",
                    ts.Days + "d.",
                    ts.Hours.ToString("00") + ":",
                    ts.Minutes.ToString("00") + ":",
                    ts.Seconds.ToString("00"),
                    ts.Milliseconds);
            }
            else if (ts.Hours > 0)
            {
                format = string.Format("{0}{1}{2}{3}.{4:000}",
                    string.Empty,
                    ts.Hours + "h:",
                    ts.Minutes.ToString("00") + ":",
                    ts.Seconds.ToString("00"),
                    ts.Milliseconds);
            }
            else if (ts.Minutes > 0)
            {
                format = string.Format("{0}{1}{2}{3}.{4:000}",
                    string.Empty,
                    string.Empty,
                    ts.Minutes + "m:",
                    ts.Seconds.ToString("00"),
                    ts.Milliseconds);
            }
            else if (ts.Seconds > 0)
            {
                format = string.Format("{0}{1}{2}{3}.{4:000}",
                    string.Empty,
                    string.Empty,
                    string.Empty,
                    ts.Seconds + "s",
                    ts.Milliseconds);
            }
            else if (ts.Milliseconds > 0)
            {
                format = string.Format("{0}{1}{2}{3}{4}",
                    ts.Days > 0 ? ts.Days + "." : string.Empty,
                    ts.Hours > 0 ? ts.Hours + ":" : string.Empty,
                    ts.Minutes > 0 ? ts.Minutes + ":" : string.Empty,
                    ts.Seconds > 0 ? ts.Seconds.ToString(CultureInfo.InvariantCulture) : string.Empty,
                    ts.Milliseconds + "ms");
            }
            else
            {
                format = "0";
            }

            return format;
        }
    }
}