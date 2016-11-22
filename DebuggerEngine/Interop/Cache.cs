using System;
using Microsoft.Mex.Extensions;
using Microsoft.Mex.Framework;

namespace Microsoft.Mex.DotNetDbg
{
    public partial class DebugUtilities
    {
        public static class Cache
        {
            public static GenericCache GetModuleBase = GenericCache.Create("GetModuleBase");
            public static GenericCache GetTypeId = GenericCache.Create("GetTypeId");
            public static GenericCache GetFieldOffset = GenericCache.Create("GetFieldOffset");
            public static GenericCache GetFieldTypeId = GenericCache.Create("GetFieldTypeId");
            public static GenericCache GetTypeSize = GenericCache.Create("GetTypeSize");
            public static GenericCache GetGlobalAddress = GenericCache.Create("GetGlobalAddress");
            public static GenericCache ReadEnum32FromStructure = GenericCache.Create("ReadEnum32FromStructure");
            public static GenericCache GetEnumName = GenericCache.Create("GetEnumName");
            public static GenericCache GetNameByOffsetWide = GenericCache.Create("GetNameByOffsetWide");
            public static GenericCache GetSymbolName = GenericCache.Create("GetSymbolName");
            public static GenericCache GetSymbolTypeIdWide = GenericCache.Create("GetSymbolTypeIdWide");
            public static GenericCache GetSymbolOptions = GenericCache.Create("GetSymbolOptions");
            static Cache()
            {
            }

            public static void ClearCache(string reason, bool fullClear = true)
            {
                if (reason == "SymOpt")
                {
                     GetSymbolOptions.ClearCache(reason);
                }

                if (fullClear)
                {
                    GetTypeId.ClearCache(reason);
                    GetSymbolTypeIdWide.ClearCache(reason);
                    GetFieldTypeId.ClearCache(reason);

                    GetModuleBase.ClearCache(reason);
                    GetGlobalAddress.ClearCache(reason);
                    ReadEnum32FromStructure.ClearCache(reason);
                    GetNameByOffsetWide.ClearCache(reason);
                    GetFieldOffset.ClearCache(reason);
                    GetTypeSize.ClearCache(reason);
                    GetEnumName.ClearCache(reason);
                    GetSymbolName.ClearCache(reason);
                }
            }
        }

        public struct SymbolInfoCache
        {
            public ulong Offset;
            public String SymbolName;

            public override string ToString()
            {
                return SymbolName + "+0x" + Offset.ToString("x");
            }
        }

        //hr = DebugSymbols.GetSymbolTypeIdWide(typeName, out typeId, &moduleBase);

        public struct TypeInfoCache
        {
            public ulong Modulebase;
            public UInt32 TypeId;

            public override string ToString()
            {
                return "0x" + Modulebase.ToString("x") + "!" + TypeId;
            }
        }
    }
}