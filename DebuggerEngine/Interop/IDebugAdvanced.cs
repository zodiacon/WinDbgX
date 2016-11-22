using System;
using System.Text;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace DebuggerEngine.Interop
{
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("f2df5f53-071f-47bd-9de6-5734c3fed689")]
	public unsafe interface IDebugAdvanced
	{
		[PreserveSig]
		int GetThreadContext(
			[In] IntPtr Context,
			[In] UInt32 ContextSize);
		[PreserveSig]
		int SetThreadContext(
			[In] IntPtr Context,
			[In] UInt32 ContextSize);
	}
}
