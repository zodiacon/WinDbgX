using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Mex.Extensions;
using Microsoft.Mex.Framework;

namespace Microsoft.Mex.DotNetDbg
{
    public class MexLinkedList
    {
        public readonly int EntryOffset;
        public readonly ulong Head;
        private readonly DebugUtilities _d;
        public readonly bool SkipListHead;


        public MexLinkedList(ulong head, int entryOffset, bool skipListHead, DebugUtilities d)
        {
            _d = d;
            Head = head;
            EntryOffset = entryOffset;
            SkipListHead = skipListHead;
        }

        public IEnumerator<ulong> GetEnumerator()
        {
            return new ListEntryEnumerator(Head, EntryOffset, SkipListHead, _d);
        }
    }

    public class ListEntryEnumerator : IEnumerator<ulong>
    {
        private readonly DebugUtilities _d;
        private readonly int _entryOffset;
        private readonly ulong _head;
        private readonly bool _skipListHead;

        private ulong _current;
        private bool _headEnumerated;
        private bool _isDisposed;
        private bool _isReset;
        private ulong _next;

        public ListEntryEnumerator(ulong head, int entryOffset, bool skipListHead, DebugUtilities d)
        {
            _d = d;
            _head = head;
            _entryOffset = entryOffset;
            _isDisposed = false;
            _skipListHead = skipListHead;
            _headEnumerated = false;

            Reset();
        }

        public ulong Current
        {
            get
            {
                CheckDisposed();

                return (_current == 0) ? 0 : (_current - (ulong)_entryOffset);
            }
        }

        public void Dispose()
        {
            CheckDisposed();

            _isDisposed = true;
        }

        object IEnumerator.Current
        {
            get
            {
                CheckDisposed();

                return Current;
            }
        }

        public bool MoveNext()
        {
            CheckDisposed();

            if (!_isReset && (_next == _head))
            {
                _next = 0;
            }

            _isReset = false;

            _current = _next;

            if (_current != 0)
            {
                for (;;)
                {
                    int result = _d.ReadPointer(_current, out _next);

                    if (result != MexFrameworkClass.S_OK)
                    {
                        //OutputVerboseLine("Cannot read list item at {0:x}", _current);
                        return false;
                    }

                    if (_current == _head)
                    {
                        if (_headEnumerated)
                        {
                            _current = 0;
                            _next = 0;
                            break;
                        }

                        _headEnumerated = true;

                        if (_skipListHead)
                        {
                            _current = _next;
                            continue;
                        }
                    }

                    break;
                }
            }

            return _current != 0;
        }

        public void Reset()
        {
            CheckDisposed();

            _current = 0;
            _next = _head;
            _isReset = true;
            _headEnumerated = false;
        }

        private void CheckDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }

    public static class RtlHashTable
    {
        const uint KDEXT_RTL_HT_SECOND_LEVEL_DIR_SHIFT = 7;
        const uint KDEXT_RTL_HT_SECOND_LEVEL_DIR_SIZE = (1 << 7);
        const uint BUCKET_ARRAY = 1;
        const uint FIRST_LEVEL_DIR = 2;

        public static List<ulong> EnumerateHashTable(DebugUtilities d, StructSymbolPlus HashTable)
        {
            // typedef struct _RTL_DYNAMIC_HASH_TABLE
            // {
            // 
            //     // Entries initialized at creation
            //     ULONG Flags;
            //     ULONG Shift;
            // 
            //     // Entries used in bucket computation.
            //     ULONG TableSize;
            //     ULONG Pivot;
            //     ULONG DivisorMask;
            // 
            //     // Counters
            //     ULONG NumEntries;
            //     ULONG NonEmptyBuckets;
            //     ULONG NumEnumerators;
            // 
            //     // The directory. This field is for internal use only.
            //     PVOID Directory;
            // 
            // }
            // RTL_DYNAMIC_HASH_TABLE, *PRTL_DYNAMIC_HASH_TABLE;

            var entries = new List<ulong>();
            var tableSize = HashTable.GetFieldValue("TableSize");

            ulong secondLevelDir = 0;
            ulong dirIndex = 0;
            ulong secondLevelIndex = 0;
            ulong bucket = 0;
            ulong pDirectory = 0;

            uint indirection = 0;

            if (tableSize <= KDEXT_RTL_HT_SECOND_LEVEL_DIR_SIZE)
                indirection = BUCKET_ARRAY;
            else
                indirection = FIRST_LEVEL_DIR;  // First level dir

            pDirectory = HashTable.GetFieldValue("Directory");

            for (bucket = 0; bucket < tableSize; bucket++)
            {
                ComputeDirIndices(bucket, out dirIndex, out secondLevelIndex);

                if (0 == secondLevelIndex)
                {
                    if (indirection == BUCKET_ARRAY)
                    {
                        secondLevelDir = pDirectory;
                    }
                    else // FIRST_LEVEL_DIR
                    {
                        var hr = d.ReadPointer(pDirectory + (dirIndex * d.PointerSize()),
                            out secondLevelDir);
                        if (DebugUtilities.FAILED(hr))
                        {
                            d.OutputErrorLine("Failed to read second-level dir 0x{0:x}\n", dirIndex);
                            break;
                        }
                    }
                }

                //
                // Read the list head
                // 

                var bucketHead = secondLevelDir + secondLevelIndex * (2 * d.PointerSize());
                var bucketEntries = d.WalkList(bucketHead);

                entries.AddRange(bucketEntries);
            }

            foreach (var entry in entries)
            {
                //d.OutputDMLLine("Hash entry: 0x{0:x}", "!ddt netio!WFP_HASH_ENTRY 0x{0:x}", entry);
            }
            return entries;
        }

        private static void ComputeDirIndices(ulong BucketIndex, out ulong FirstLevelIndex, out ulong SecondLevelIndex)
        {
            if (!OSInfo.Win81OrNewer)  // OLDER than WinBlue
            {
                SecondLevelIndex = BucketIndex % KDEXT_RTL_HT_SECOND_LEVEL_DIR_SIZE;
                FirstLevelIndex = BucketIndex / KDEXT_RTL_HT_SECOND_LEVEL_DIR_SIZE;
            }
            else
            {
                uint AbsoluteIndex = (uint)BucketIndex + KDEXT_RTL_HT_SECOND_LEVEL_DIR_SIZE;

                BitScanReverse(out FirstLevelIndex, AbsoluteIndex);
                SecondLevelIndex = (ulong)(AbsoluteIndex ^ (1 << (int)FirstLevelIndex));
                FirstLevelIndex -= KDEXT_RTL_HT_SECOND_LEVEL_DIR_SHIFT;
            }
        }

        private static void BitScanReverse(out ulong Index, ulong Mask)
        {
            Index = 0;
            if (Mask == 0) return;

            while ((Mask & 0x1) == 1)
            {
                Index++; Mask >>= 1;
            }
            return;
        }
    }


    /// DebugUtilities
    public partial class DebugUtilities
    {
        /// <summary>
        ///     This overload is for walking lists where the Next entry is not at offset 0.  Returns address of all entries in the
        ///     list.
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="moduleName"></param>
        /// <param name="typeName"></param>
        /// <param name="fieldName"></param>
        /// <param name="skipListHead"></param>
        /// <param name="entries"></param>
        /// <param name="entryOffset"></param>
        /// <returns></returns>
        public int WalkMexLinkedList(ulong addr, string moduleName, string typeName, string fieldName, bool skipListHead, out List<ulong> entries, uint entryOffset = 0)
        {
            entries = new List<ulong>();

            int hr;
            uint fieldOffset;
            if (FAILED(hr = GetFieldOffset(moduleName, typeName, fieldName, out fieldOffset)))
            {
                return hr;
            }

            return WalkMexLinkedList(addr, skipListHead, out entries, fieldOffset);
        }


        /// <summary>
        ///     Walks a linked list using the MexLinkedList implementation
        /// </summary>
        /// <param name="addr">Address of first element in the list</param>
        /// <param name="skipListHead">Skip the list head?  Set to true if the head element is empty.</param>
        /// <param name="entries">Addresses of list entries.</param>
        /// <param name="entryOffset">Offset to apply, if any.</param>
        /// <returns></returns>
        public int WalkMexLinkedList(ulong addr, bool skipListHead, out List<ulong> entries, uint entryOffset = 0)
        {
            entries = new List<ulong>();

            // Checks for duplicates/circular lists
            var visited = new HashSet<ulong>();

            var itemList = new MexLinkedList(addr, (int)entryOffset, skipListHead, this);


            foreach (ulong item in itemList)
            {
                if (ShouldBreak())
                {
                    break;
                }

                if (visited.Contains(item))
                {
                    // List loop detected
                    // Don't error out, just return the values we have found so far.
                    break;
                }

                visited.Add(item);
                entries.Add(item);
            }

            if (entries.Count > 0)
            {
                return S_OK;
            }
            return S_FALSE;
        }

        /// <summary>
        ///     Walks a linked list using the MexLinkedList implementation
        /// </summary>
        /// <param name="addr">Address of first element in the list</param>
        /// <param name="skipListHead">Skip the list head?  Set to true if the head element is empty.</param>
        /// <param name="entryOffset">Offset to apply, if any.</param>
        /// <returns>Addresses of list entries</returns>
        public List<ulong> WalkMexLinkedList(ulong addr, bool skipListHead, uint entryOffset = 0)
        {
            var entries = new List<ulong>();
            int hr = WalkMexLinkedList(addr, skipListHead, out entries, entryOffset);

            if (FAILED(hr))
            {
                ThrowExceptionHere(hr);
            }

            return entries;
        }


        /// <summary>
        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY where Flink is at offset 0 and returns each node in an array.
        /// </summary>
        /// <returns>
        /// The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        /// </returns>
        public List<ulong> WalkList(UInt64 listAddress)
        {
            return WalkList(listAddress, 0);
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY and returns each node in an array
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public List<ulong> WalkList(UInt64 listAddress, uint offsetToSubtract)
        {
            int hr = S_OK;
            UInt64 next = listAddress;
            var entries = new List<ulong>(32);

            if (listAddress == 0)
            {
                goto Exit;
            }

            for (;;)
            {
                if (ShouldBreak())
                {
                    hr = E_FAIL; 
                    goto Exit;
                }
                if (FAILED(hr = ReadPointer(next, out next)))
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                if ((next == 0) || (next == listAddress))
                {
                    goto Exit;
                }
                if (entries.Contains(next - offsetToSubtract))
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                entries.Add(next - offsetToSubtract);
            }

            Exit:
            if (FAILED(hr))
            {
                ThrowExceptionHere(hr);
            }
            return entries;
        }


        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY and returns each node in an array
        /// The output array will contain pointers to the LIST_ENTRY field, not the beginning of the structure! Use the other overload for that.
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkList(UInt64 listAddress, out UInt64[] listEntries)
        {
            return WalkList(listAddress, out listEntries, 0);
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY and returns each node in an array
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkList(UInt64 listAddress, out UInt64[] listEntries, uint offsetToSubtract)
        {
            int hr = S_OK;
            UInt64 next = listAddress;
            var entries = new List<UInt64>(32);

            for (;;)
            {
                if (ShouldBreak())
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                if (FAILED(hr = ReadPointer(next, out next)))
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                if ((next == 0) || (next == listAddress))
                {
                    goto Exit;
                }
                if (entries.Contains(next - offsetToSubtract))
                {
                    hr = E_FAIL;
                    goto Exit;                    
                }
                entries.Add(next - offsetToSubtract);
            }

            Exit:
            listEntries = entries.ToArray();
            return hr;
        }


        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY for the specified count of entries and returns each node in an array
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkList(UInt64 listAddress, out UInt64[] listEntries, uint offsetToSubtract, uint count)
        {
            int hr = S_OK;
            UInt64 next = listAddress;
            var entries = new List<UInt64>((int)count);

            for (uint i = 1; i <= count; i++)
            {
                if (ShouldBreak())
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                if (FAILED(hr = ReadPointer(next, out next)))
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                if ((next == 0) || (next == listAddress))
                {
                    goto Exit;
                }
                if (entries.Contains(next - offsetToSubtract))
                {
                    hr = E_FAIL;
                    goto Exit;
                }
                entries.Add(next - offsetToSubtract);
            }

            Exit:
            listEntries = entries.ToArray();
            return hr;
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY and returns each node in an array
        /// This overload takes a type and field name for the list entries and automatically subtracts the offsets
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkList(UInt64 listAddress, out UInt64[] listEntries, string moduleName, string entryTypeName, string entryLinkFieldName)
        {
            int hr;
            uint entryLinkFieldOffset;
            if (FAILED(hr = GetFieldOffset(moduleName, entryTypeName, entryLinkFieldName, out entryLinkFieldOffset)))
            {
                listEntries = new UInt64[0];
                return hr;
            }
            return WalkList(listAddress, out listEntries, entryLinkFieldOffset);
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY embedded in a structure
        /// The output array will contain pointers to the LIST_ENTRY field, not the beginning of the structure! Use the other overload for that.
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkListInStructure(string moduleName, string typeName, string fieldName, UInt64 address, out UInt64[] listEntries, uint maxEntries = 0)
        {
            return WalkListInStructure(moduleName, typeName, fieldName, address, out listEntries, null, null, maxEntries);
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY embedded in a structure
        /// This overload takes a type and field name for the list entries and automatically subtracts the offsets
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        /// 
        public int WalkListInStructure(string moduleName, string typeName, string fieldName, UInt64 address, out UInt64[] listEntries, string entryTypeName, string entryLinkFieldName, uint maxEntries = 0)
        {
            int hr;
            uint fieldOffset, entryLinkFieldOffset;
            if (FAILED(hr = GetFieldOffset(moduleName, typeName, fieldName, out fieldOffset)))
            {
                listEntries = new UInt64[0];
                return hr;
            }
            if ((entryTypeName != null) && (entryLinkFieldName != null))
            {
                if (FAILED(hr = GetFieldOffset(moduleName, entryTypeName, entryLinkFieldName, out entryLinkFieldOffset)))
                {
                    listEntries = new UInt64[0];
                    return hr;
                }
            }
            else
            {
                entryLinkFieldOffset = 0;
            }
            if (maxEntries > 0)
            {
                return WalkList(address + fieldOffset, out listEntries, entryLinkFieldOffset, maxEntries); 
            }
            return WalkList(address + fieldOffset, out listEntries, entryLinkFieldOffset);
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY stored in a global
        /// The output array will contain pointers to the LIST_ENTRY field, not the beginning of the structure! Use the other overload for that.
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkListGlobal(string moduleName, string globalName, out UInt64[] listEntries)
        {
            return WalkListGlobal(moduleName, globalName, out listEntries, null, null);
        }


        /// <summary>
        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY stored in a global for the specified count
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        /// </summary>
        /// <param name="moduleName"></param>
        /// <param name="globalName"></param>
        /// <param name="listEntries">This output array will contain pointers to the LIST_ENTRY field, not the beginning of the structure! Use the other overload for that.</param>
        /// <param name="count">Maximum number of elements to enumerate</param>
        /// <returns></returns>
        public int WalkListGlobal(string moduleName, string globalName, out UInt64[] listEntries, uint count)
        {
            return WalkListGlobal(moduleName, globalName, out listEntries, null, null, count);
        }

        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY stored in a global
        /// This overload takes a type and field name for the list entries and automatically subtracts the offsets
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkListGlobal(string moduleName, string globalName, out UInt64[] listEntries, string entryTypeName, string entryLinkFieldName)
        {
            int hr;
            UInt64 globalAddress;
            uint entryLinkFieldOffset;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out globalAddress)))
            {
                listEntries = new UInt64[0];
                return hr;
            }
            if ((entryTypeName != null) && (entryLinkFieldName != null))
            {
                if (FAILED(hr = GetFieldOffset(moduleName, entryTypeName, entryLinkFieldName, out entryLinkFieldOffset)))
                {
                    listEntries = new UInt64[0];
                    return hr;
                }
            }
            else
            {
                entryLinkFieldOffset = 0;
            }
            return WalkList(globalAddress, out listEntries, entryLinkFieldOffset);
        }


        /// Walks a LIST_ENTRY or SINGLE_LIST_ENTRY stored in a global for the specified count of entries (count should NOT be 0)
        /// This overload takes a type and field name for the list entries and automatically subtracts the offsets
        /// NOTE! The output list may have entries even if this function fails, as the failure could be due to a memory read after multiple successful reads.
        public int WalkListGlobal(string moduleName, string globalName, out UInt64[] listEntries, string entryTypeName, string entryLinkFieldName, uint count)
        {
            int hr;
            UInt64 globalAddress;
            uint entryLinkFieldOffset;
            if (FAILED(hr = GetGlobalAddress(moduleName, globalName, out globalAddress)))
            {
                listEntries = new UInt64[0];
                return hr;
            }
            if ((entryTypeName != null) && (entryLinkFieldName != null))
            {
                if (FAILED(hr = GetFieldOffset(moduleName, entryTypeName, entryLinkFieldName, out entryLinkFieldOffset)))
                {
                    listEntries = new UInt64[0];
                    return hr;
                }
            }
            else
            {
                entryLinkFieldOffset = 0;
            }
            return WalkList(globalAddress, out listEntries, entryLinkFieldOffset, count);
        }



        

    }
}