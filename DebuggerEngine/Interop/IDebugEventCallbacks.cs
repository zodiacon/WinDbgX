using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace DebuggerEngine.Interop
{
	[ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown), Guid("337be28b-5036-4d72-b6bf-c45fbb9f2eaa")]
	public interface IDebugEventCallbacks
	{
		[PreserveSig] int GetInterestMask(
			[Out] out DEBUG_EVENT mask);
		[PreserveSig] int Breakpoint(
			[In, MarshalAs(UnmanagedType.Interface)] IDebugBreakpoint Bp);
		[PreserveSig] int Exception(
			[In] EXCEPTION_RECORD64 Exception,
			[In] UInt32 FirstChance);
		[PreserveSig] int CreateThread(
			[In] UInt64 Handle,
			[In] UInt64 DataOffset,
			[In] UInt64 StartOffset);
		[PreserveSig] int ExitThread(
			[In] UInt32 ExitCode);
		[PreserveSig] int CreateProcess(
			[In] UInt64 ImageFileHandle,
			[In] UInt64 Handle,
			[In] UInt64 BaseOffset,
			[In] UInt32 ModuleSize,
			[In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
			[In, MarshalAs(UnmanagedType.LPStr)] string ImageName,
			[In] UInt32 CheckSum,
			[In] UInt32 TimeDateStamp,
			[In] UInt64 InitialThreadHandle,
			[In] UInt64 ThreadDataOffset,
			[In] UInt64 StartOffset);
		[PreserveSig] int ExitProcess(
			[In] UInt32 ExitCode);
		[PreserveSig] int LoadModule(
			[In] UInt64 imageFileHandle,
			[In] UInt64 baseOffset,
			[In] UInt32 moduleSize,
			[In, MarshalAs(UnmanagedType.LPStr)] string ModuleName,
			[In, MarshalAs(UnmanagedType.LPStr)] string imageName,
			[In] UInt32 checkSum,
			[In] UInt32 timeDateStamp);
		[PreserveSig] int UnloadModule(
			[In, MarshalAs(UnmanagedType.LPStr)] string imageBaseName,
			[In] UInt64 baseOffset);
		[PreserveSig] int SystemError(
			[In] UInt32 Error,
			[In] UInt32 Level);
		[PreserveSig] int SessionStatus(
			[In] DEBUG_SESSION flags);
		[PreserveSig] int ChangeDebuggeeState(
			[In] DEBUG_CDS flags,
			[In] UInt64 argument);
		[PreserveSig] int ChangeEngineState(
			[In] DEBUG_CES flags,
			[In] UInt64 argument);
		[PreserveSig] int ChangeSymbolState(
			[In] DEBUG_CSS flags,
			[In] UInt64 argument);
	}
}
