using System;
using System.IO;
using System.Text;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Microsoft.Mex.DotNetDbg
{
	public unsafe partial class DebugUtilities
	{
		/// <summary>
		/// A class which provides functionality required to implement a debugger.
		/// </summary>
		public class SimpleDebugger : IDisposable
		{
			private static bool FAILED(int hr)
			{
				return (hr < 0);
			}

			/// <summary>
			/// Process ID of the process that is being debugged.
			/// </summary>
			public readonly uint ProcessID;

			/// <summary>
			/// Indicates whether the target is being passively debugged.
			/// </summary>
			public readonly bool DebuggerIsPassive;

			/// <summary>
			/// Holds an instance of a SimpleOutputHandler which receives output.
			/// </summary>
			public SimpleOutputHandler OutputHandler { get { return _OutputHandler; } }
			private SimpleOutputHandler _OutputHandler;

			/// <summary>
			/// A SimpleEventHandler instance for processing breakpoints, etc.
			/// </summary>
			public SimpleEventHandler EventHandler { get { return _EventHandler; } }
			private SimpleEventHandler _EventHandler;

			/// <summary>
			/// An IDebugClient interface associated with the connection.
			/// </summary>
			public DebugUtilities OriginatingDebugUtilities { get { return _OriginatingDebugUtilities; } }
			private DebugUtilities _OriginatingDebugUtilities;

			/// <summary>
			/// Creates a new SimpleDebugger object with the required parameters.
			/// </summary>
			/// <param name="originatingClient">An IDebugClient interface associated with the connection</param>
			/// <param name="processId">The ID of the process being debugged</param>
			/// <param name="connectionIsPassive">True if the debugger is passively attached to the target</param>
			/// <param name="outputHandler">A SimpleOutputHandler which is receiving the debugger output</param>
			/// <param name="eventHandler">A SimpleEventHandler to handle the debugger events</param>
			public SimpleDebugger(DebugUtilities originatingClient, uint processId, bool connectionIsPassive, SimpleOutputHandler outputHandler, SimpleEventHandler eventHandler)
			{
				_OriginatingDebugUtilities = originatingClient;
				ProcessID = processId;
				DebuggerIsPassive = connectionIsPassive;
				_OutputHandler = outputHandler;
				_EventHandler = eventHandler;
			}

			private bool Detached = false;
			/// <summary>
			/// Detaches from the target process
			/// </summary>
			public void Detach(bool detaching)
			{
				if (Detached == false && detaching == true)
				{
					if (OutputHandler != null) { OutputHandler.Dispose(); _OutputHandler = null; }
					if (EventHandler != null) { EventHandler.Dispose(); _EventHandler = null; }
					if (OriginatingDebugUtilities != null)
					{
						OriginatingDebugUtilities.DebugClient.DetachProcesses();
						OriginatingDebugUtilities.DebugClient.EndSession(DEBUG_END.ACTIVE_DETACH);
						DebugUtilities.ReleaseComObjectSafely(OriginatingDebugUtilities);
						_OriginatingDebugUtilities = null;
					}

                    Detached = true;
				}
                
			}

			/// <summary>
			/// IDisposable interface to detach from the target process
			/// </summary>
			public void Dispose()
			{
				Detach(true);
				GC.SuppressFinalize(this);
			}

			/// <summary>
			/// Finalizer to clean up the object if Dispose is not called.
			/// </summary>
			~SimpleDebugger()
			{
                Detach(false);
			}
		}

		/// <summary>
		/// A raw version of DebugCreate. More flexible in that any interface can be retrieved, but you have to handle converting the IntPtr to a RCW yourself.
		/// Look at Marshal.GetTypedObjectForIUnknown() or Marshal.GetObjectForIUnknown().
		/// YOU MUST CALL Marshal.Release() ON Interface OR YOU WILL LEAK COM OBJECTS!!!
		/// </summary>
		/// <param name="InterfaceId">Interface GUID you would like DebugCreate to return</param>
		/// <param name="Interface">Interface pointer returned by DebugCreate</param>
		/// <returns>HRESULT of the DebugCreate call</returns>
		[DllImport("dbgeng.dll", PreserveSig = true, EntryPoint = "DebugCreate")]
		public static extern int DebugCreate([In, MarshalAs(UnmanagedType.LPStruct)] Guid InterfaceId, [Out] out IntPtr Interface);

		/// <summary>
		/// An easier to use version of DebugCreate which is restricted to the IDebugClient interface.
		/// </summary>
		/// <param name="InterfaceId">Must be typeof(IDebugClient).GUID</param>
		/// <param name="Interface">The IDebugClient interface object</param>
		/// <returns>HRESULT of the DebugCreate call</returns>
		[DllImport("dbgeng.dll", PreserveSig = true, EntryPoint = "DebugCreate")]
		public static extern int DebugCreate_IDebugClient([In, MarshalAs(UnmanagedType.LPStruct)] Guid InterfaceId, [Out, MarshalAs(UnmanagedType.Interface)] out IDebugClient Interface);

		private static int ConnectDebuggerDumpHelper(DebugUtilities debugUtilities, string dumpFile, out SimpleDebugger debuggerInformation)
		{
			int hr;

			SimpleOutputHandler debuggerOutputCallbacks = null;
			SimpleEventHandler debuggerEventCallbacks = null;

			debuggerInformation = null;

			hr = SimpleOutputHandler.Install(debugUtilities, out debuggerOutputCallbacks);
			if (hr != S_OK)
			{
				goto Error;
			}

			hr = SimpleEventHandler.Install(debugUtilities, out debuggerEventCallbacks);
			if (hr != S_OK)
			{
				goto ErrorWithDetach;
			}

			hr = debugUtilities.DebugClient.OpenDumpFileWide(dumpFile, 0);
			if (FAILED(hr))
			{
				goto ErrorWithDetach;
			}

			while (debuggerEventCallbacks.SessionIsActive == false)
			{
				hr = debugUtilities.DebugControl.WaitForEvent(DEBUG_WAIT.DEFAULT, 50);
				if (FAILED(hr))
				{
					goto ErrorWithDetach;
				}
			}

			debuggerInformation = new SimpleDebugger(debugUtilities, 0, true, debuggerOutputCallbacks, debuggerEventCallbacks);

			goto Exit;

		ErrorWithDetach:
			debugUtilities.DebugClient.DetachProcesses();
			debugUtilities.DebugClient.EndSession(DEBUG_END.ACTIVE_DETACH);
		Error:
			if (debuggerEventCallbacks != null) debuggerEventCallbacks.Dispose();
			if (debuggerOutputCallbacks != null) debuggerOutputCallbacks.Dispose();
		Exit:
			return hr;
		}

		private static int ConnectDebuggerLiveHelper(DebugUtilities debugUtilities, uint processID, bool passive, out SimpleDebugger debuggerInformation)
		{
			int hr;

			IDebugControl debugControl = null;
			IDebugSystemObjects debugSystemObjects = null;

			SimpleOutputHandler debuggerOutputCallbacks = null;
			SimpleEventHandler debuggerEventCallbacks = null;

			debuggerInformation = null;

			hr = SimpleOutputHandler.Install(debugUtilities, out debuggerOutputCallbacks);
			if (hr != S_OK)
			{
				goto Error;
			}

			hr = SimpleEventHandler.Install(debugUtilities, out debuggerEventCallbacks);
			if (hr != S_OK)
			{
				goto ErrorWithDetach;
			}

			DEBUG_ATTACH attachFlags = passive ? DEBUG_ATTACH.NONINVASIVE | DEBUG_ATTACH.NONINVASIVE_NO_SUSPEND : DEBUG_ATTACH.INVASIVE_RESUME_PROCESS;

			hr = debugUtilities.DebugClient.AttachProcess(0, processID, attachFlags);
			if (hr != S_OK)
			{
				goto ErrorWithDetach;
			}

			while (debuggerEventCallbacks.SessionIsActive == false)
			{
				hr = debugControl.WaitForEvent(DEBUG_WAIT.DEFAULT, 50);
				if (FAILED(hr))
				{
					goto ErrorWithDetach;
				}
			}

			bool foundMatchingProcess = false;
			uint numProcesses;
			debugSystemObjects.GetNumberProcesses(out numProcesses);
			uint[] systemProcessIDs = new uint[numProcesses];
			uint[] engineProcessIDs = new uint[numProcesses];
			hr = debugSystemObjects.GetProcessIdsByIndex(0, numProcesses, engineProcessIDs, systemProcessIDs);
			for (uint i = 0; i < numProcesses; ++i)
			{
				if (systemProcessIDs[i] == processID)
				{
					foundMatchingProcess = true;
					hr = debugSystemObjects.SetCurrentProcessId(engineProcessIDs[i]);
					if (FAILED(hr))
					{
						debuggerOutputCallbacks.AddNoteLine(String.Format(CultureInfo.InvariantCulture, "ERROR! Failed to set the active process! hr={0:x8}", hr));
						goto ErrorWithDetach;
					}
					break;
				}
			}
			if (foundMatchingProcess == false)
			{
				hr = E_FAIL;
				debuggerOutputCallbacks.AddNoteLine(String.Format(CultureInfo.InvariantCulture, "ERROR! The debugger engine could not find the requested process ID ({0})!", processID));
				goto ErrorWithDetach;
			}

			debuggerInformation = new SimpleDebugger(debugUtilities, processID, passive, debuggerOutputCallbacks, debuggerEventCallbacks);

			goto Exit;

		ErrorWithDetach:
			debugUtilities.DebugClient.DetachProcesses();
			debugUtilities.DebugClient.EndSession(DEBUG_END.ACTIVE_DETACH);
		Error:
			if (debuggerEventCallbacks != null) debuggerEventCallbacks.Dispose();
			if (debuggerOutputCallbacks != null) debuggerOutputCallbacks.Dispose();
		Exit:
			return hr;
		}

		/// <summary>
		/// Create a debugging connection to a process
		/// </summary>
		/// <param name="processID">The ID of the process to attach to</param>
		/// <param name="passive">Whether the debugger connection should be passive</param>
		/// <param name="debuggerInformation">An SimpleDebugger instance which contains connection information. Call Dispose() on this object.</param>
		/// <returns>HRESULT of the creation process</returns>
		public static int ConnectDebugger(uint processID, bool passive, out SimpleDebugger debuggerInformation)
		{
			IDebugClient debugClient = null;
			int hr = DebugCreate_IDebugClient(typeof(IDebugClient).GUID, out debugClient);
			if (FAILED(hr))
			{
				debuggerInformation = null;
				return hr;
			}
			else
			{
				DebugUtilities debugUtilities = new DebugUtilities(debugClient);
				return ConnectDebuggerLiveHelper(debugUtilities, processID, passive, out debuggerInformation);
			}
		}

		/// <summary>
		/// Create a debugging connection to a process
		/// </summary>
		/// <param name="processName">The name of the process to attach to</param>
		/// <param name="passive">Whether the debugger connection should be passive</param>
		/// <param name="debuggerInformation">An SimpleDebugger instance which contains connection information. Call Dispose() on this object.</param>
		/// <returns>HRESULT of the creation process</returns>
		public static int ConnectDebugger(string processName, bool passive, out SimpleDebugger debuggerInformation)
		{
			IDebugClient debugClient = null;
			int hr = DebugCreate_IDebugClient(typeof(IDebugClient).GUID, out debugClient);
			if (FAILED(hr))
			{
				debuggerInformation = null;
				return hr;
			}

			uint processID = 0;
			hr = debugClient.GetRunningProcessSystemIdByExecutableName(0, processName, 0, out processID);
			if (FAILED(hr))
			{
				goto Error;
			}

			DebugUtilities debugUtilities = new DebugUtilities(debugClient);
			hr = ConnectDebuggerLiveHelper(debugUtilities, processID, passive, out debuggerInformation);
			if (FAILED(hr))
			{
				goto Error;
			}

			goto Exit;

		Error:
			if (debugClient != null) ReleaseComObjectSafely(debugClient);
			debuggerInformation = null;
		Exit:
			return hr;
		}

		/// <summary>
		/// Opens a dump file for debugging.
		/// </summary>
		/// <param name="dumpFile">Path to a dump file to process.</param>
		/// <param name="debuggerInformation">An SimpleDebugger instance which contains connection information. Call Dispose() on this object.</param>
		/// <returns>HRESULT</returns>
		public static int OpenDumpFile(string dumpFile, out SimpleDebugger debuggerInformation)
		{
			IDebugClient debugClient = null;
			int hr = DebugCreate_IDebugClient(typeof(IDebugClient).GUID, out debugClient);
			if (FAILED(hr))
			{
				debuggerInformation = null;
				return hr;
			}

			DebugUtilities debugUtilities = new DebugUtilities(debugClient);
			hr = ConnectDebuggerDumpHelper(debugUtilities, dumpFile, out debuggerInformation);
			if (FAILED(hr))
			{
				goto Error;
			}

			goto Exit;

		Error:
			if (debugClient != null) ReleaseComObjectSafely(debugClient);
			debuggerInformation = null;
		Exit:
			return hr;
		}
	}
}
