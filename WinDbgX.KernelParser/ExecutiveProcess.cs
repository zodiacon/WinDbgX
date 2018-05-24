using DebuggerEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.KernelParser {
	public sealed class ExecutiveProcess {
		public static uint EProcessTypeId;
		public static ulong KernelBase;
		public static uint ListEntryOffset;

		public uint ProcessId { get; private set; }
		public uint ParentProcessId { get; private set; }
		public ExecutiveProcessFlags Flags { get; private set; }
		public ExecutiveProcessFlags2 Flags2 { get; private set; }
		public TimeSpan CreateTime { get; private set; }
		public ulong PeakVirtualSize { get; private set; }
		public ulong VirtualSize { get; private set; }
		public ulong EProcess { get; }
		public string Name { get; private set; }

		private ExecutiveProcess(ulong eprocess) {
			EProcess = eprocess;
		}

		public async static Task<ExecutiveProcess> FromListEntry(DebugClient debugger, ulong address) {
			if (EProcessTypeId == 0)
				await GetTypeIds(debugger);

			address -= ListEntryOffset;
			return await CreateInternal(debugger, address);
		}

		private static async Task<ExecutiveProcess> CreateInternal(DebugClient debugger, ulong address) {
			var process = new ExecutiveProcess(address) {
				ProcessId = (uint)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "UniqueProcessId", address),
				ParentProcessId = (uint)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "InheritedFromUniqueProcessId", address),
				Flags = (ExecutiveProcessFlags)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "Flags", address),
				Flags2 = (ExecutiveProcessFlags2)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "Flags2", address),
				CreateTime = new TimeSpan((long)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "CreateTime", address)),
				PeakVirtualSize = (ulong)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "PeakVirtualSize", address),
				VirtualSize = (ulong)await debugger.GetFieldValue(KernelBase, EProcessTypeId, "VirtualSize", address),
				//Name = new string((char*)await debugger.GetFieldValue(_kernelBase, _EProcessTypeId, "ImageFileName", address)),
			};

			return process;
		}

		private async static Task GetTypeIds(DebugClient debugger) {
			(EProcessTypeId, KernelBase) = await debugger.GetSymbolTypeId("nt!_EPROCESS");
			ListEntryOffset = await debugger.GetFieldOffset("nt!_EPROCESS", "ActiveProcessLinks");
		}
	}
}
