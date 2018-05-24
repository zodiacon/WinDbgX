using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WinDbgX.KernelParser {
	[Flags]
	public enum ExecutiveProcessFlags : uint {
		CreateReported = 1,
		NoDebugInherit = 2,
		ProcessExiting = 4,
		ProcessDelete = 8,
		ControlFlowGuardEnabled = 0x10,
		VmDeleted = 0x20,
		OutswapEnabled = 0x40,
		Outswapped = 0x80,
		FailFastOnCommitFail = 0x100,
		Wow64VaSpace4Gb = 0x200,
		AddressSpaceInitialized1 = 0x400,
		AddressSpaceInitialized2 = 0x800,
		AddressSpaceInitialized3 = 0xc00,
		SetTimerResolution = 0x1000,
		BreakOnTermination = 0x2000,
		DeprioritizeViews = 0x4000,
		WriteWatch       = 0x8000,
		ProcessInSession = 0x10000,
		OverrideAddressSpace = 0x20000,
		HasAddressSpace  = 0x40000,
		LaunchPrefetched = 0x80000,
		Background       = 0x100000,
		VmTopDown        = 0x200000,
		ImageNotifyDone  = 0x400000,
		PdeUpdateNeeded			= 0x800000,
		VdmAllowed				= 0x1000000,
		ProcessRundown			= 0x2000000,
		ProcessInserted			= 0x4000000,
		DefaultIoPriority0		= 0x8000000,
		//DefaultIoPriority1		= 0x8000000,
		//DefaultIoPriority2		= 0x8000000,
		//DefaultIoPriority3		= 0x8000000,
		//DefaultIoPriority4		= 0x8000000,
		//DefaultIoPriority5		= 0x8000000,
		//DefaultIoPriority6		= 0x8000000,
		//DefaultIoPriority7		= 0x8000000,

		ProcessSelfDelete = 0x40000000,
		SetTimerResolutionLink	= 0x80000000
	}

	[Flags]
	public enum ExecutiveProcessFlags2 : uint {
		JobNotReallyActive			= 1,
	}
}
