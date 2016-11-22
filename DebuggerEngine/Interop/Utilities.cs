using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DebuggerEngine.Interop {
    public static class Utilities {
        public static ulong SignExtendAddress(uint address) {
            return (ulong)(int)address;
        }

        public static ulong SignExtendAddress(ulong address) {
            return (ulong)(int)address;
        }
    }


    /// <summary>
    ///     A collection of utilities to make writing a debugger or debugger extension easier.
    /// </summary>
    //    public unsafe partial class DebugUtilities
    //    {
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        public delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam);

    //        /// <summary>
    //        ///     Delegate for the WindbgExtensionAPI Ioctl function
    //        /// </summary>
    //        /// <param name="IoctlType">Type of IOCTL to invoke</param>
    //        /// <param name="lpvData">Pointer to data</param>
    //        /// <param name="cbSizeOfContext">Size of the data passed</param>
    //        /// <returns>Depends on the specific IOCTL invoked</returns>
    //        public delegate uint IoctlDelegate(IG IoctlType, IntPtr lpvData, uint cbSizeOfContext);

    //        public const uint WM_SETTEXT = 0x000c;
    //        public const uint WM_GETTEXT = 0x000d;
    //        public const uint WM_GETTEXTLENGTH = 0x000e;
    //        private static bool? _isWindbg;
    //        // Prevent output if we are outputting on a different thread.
    //        public static int CurrentlyExecutingThreadId = -1;
    //        // Match /r as long as it is not in front of a \n or the end of the string.
    //        private static readonly Regex RemoveCarriageReturnRegex = new Regex(@"\r([^$\n])", RegexOptions.Compiled | RegexOptions.Multiline);
    //        private static readonly char[] CharactersThatRequireQuoting = {' ', '\t'};
    //        public static volatile bool BreakStatus;
    //        private static int shouldBreakCount;
    //        private static readonly object FindWindbgWindowLock = new object();
    //        private static string _findWindbgWindowClassName;
    //        private static bool _findWindbgWindowParentNot;
    //        private static string _findWindbgWindowParentCaption;
    //        private static HashSet<IntPtr> _findWindbgWindowFoundWindows;
    //        private static IntPtr _windbgCommandOutputWindowHandle;
    //        private static IntPtr _windbgCommandInputWindowHandle;
    //        private static volatile string _lastStatusText = string.Empty;
    //        private static volatile string _appendStatusText = string.Empty;
    //        private static volatile bool _refreshStatusText;
    //        private static readonly Stopwatch StatusBarStopWatch = new Stopwatch();
    //        private static readonly CopyMemoryDelegate_IntPtr CopyMemory_IntPtr = (CopyMemoryDelegate_IntPtr)CreateCopyMemory(true, false);
    //        private static readonly CopyMemoryDelegate_IntPtr CopyMemory_IntPtr_Aligned = (CopyMemoryDelegate_IntPtr)CreateCopyMemory(true, true);
    //        private static readonly CopyMemoryDelegate_VoidPtr CopyMemory_VoidPtr = (CopyMemoryDelegate_VoidPtr)CreateCopyMemory(false, false);
    //        private static readonly CopyMemoryDelegate_VoidPtr CopyMemory_VoidPtr_Aligned = (CopyMemoryDelegate_VoidPtr)CreateCopyMemory(false, true);
    //        private readonly Stack<ulong> _processStack = new Stack<ulong>();
    //        private readonly Stack<ulong> _threadStack = new Stack<ulong>();

    //        public static bool IsWindbg
    //        {
    //            get
    //            {
    //                if (null == _isWindbg)
    //                {
    //                    _isWindbg = Process.GetCurrentProcess().ProcessName.ToLower().StartsWith("windbg");               
    //                }
    //                return _isWindbg.Value;
    //            }
    //        }

    //        // Publically documented API - http://msdn.microsoft.com/en-us/library/windows/desktop/ms644943(v=vs.85).aspx
    //        [DllImport("user32")]
    //        private static extern int PeekMessage(
    //            MSG* lpMsg,
    //            IntPtr hwnd,
    //            uint wMsgFilterMin,
    //            uint wMsgFilterMax,
    //            uint wRemoveMsg
    //            );

    //        // Publically documented API -  http://msdn.microsoft.com/en-us/library/windows/desktop/ms644936(v=vs.85).aspx
    //        [DllImport("user32")]
    //        private static extern int GetMessage(
    //            MSG* lpMsg,
    //            IntPtr hwnd,
    //            uint wMsgFilterMin,
    //            uint wMsgFilterMax
    //            );

    //        // Publically documented API - http://msdn.microsoft.com/en-us/library/windows/desktop/ms644955(v=vs.85).aspx
    //        [DllImport("user32")]
    //        private static extern int TranslateMessage(
    //            MSG* lpMsg
    //            );

    //        // Publically documented API - http://msdn.microsoft.com/en-us/library/windows/desktop/ms644934(v=vs.85).aspx
    //        [DllImport("user32")]
    //        private static extern uint DispatchMessage(
    //            MSG* lpMsg
    //            );

    //        // Publically documented API - http://msdn.microsoft.com/en-us/library/windows/desktop/ms688715(v=vs.85).aspx
    //        [DllImport("ole32.dll")]
    //        public static extern void CoUninitialize();

    //        /// <summary>
    //        ///     Wrapper for the native Win32 WriteFile
    //        ///     http://msdn.microsoft.com/en-us/library/aa365747.aspx
    //        /// </summary>
    //        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        public static extern bool WriteFile(IntPtr hFile, void* lpBuffer, uint nNumberOfBytesToWrite, uint* lpNumberOfBytesWritten, void* lpOverlapped_PassAsNull = null);

    //        /// <summary>
    //        ///     Wrapper for the native Win32 CloseHandle
    //        ///     http://msdn.microsoft.com/en-us/library/ms724211.aspx
    //        /// </summary>
    //        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        public static extern bool CloseHandle(IntPtr hObject);

    //        /// <summary>
    //        ///     Wrapper for the native Win32 CreateFile
    //        ///     http://msdn.microsoft.com/en-us/library/aa363858.aspx
    //        /// </summary>
    //        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    //        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPWStr)] string lpFileName, EFileAccess dwDesiredAccess, EFileShare dwShareMode, void* lpSecurityAttributes_PassAsNull, ECreationDisposition dwCreationDisposition, EFileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

    //        /// <summary>
    //        ///     Wrapper for the native Win32 SetFilePointerEx
    //        ///     http://msdn.microsoft.com/en-us/library/aa365542.aspx
    //        /// </summary>
    //        [DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        public static extern bool SetFilePointerEx(IntPtr hFile, ulong liDistanceToMove, ulong* lpNewFilePointer, SPF_MOVE_METHOD dwMoveMethod);

    //        /// <summary>
    //        ///     Wrapper for the Win32 GetModuleHandle function. Returns a handle for a module if it is loaded in memory.
    //        /// </summary>
    //        /// <param name="lpModuleName">Name of the module</param>
    //        /// <returns>Handle to the module</returns>
    //        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, PreserveSig = true, EntryPoint = "GetModuleHandleW")]
    //        public static extern IntPtr GetModuleHandle([In, Optional, MarshalAs(UnmanagedType.LPWStr)] string lpModuleName);

    //        /// <summary>
    //        ///     Wrapper for the Win32 GetModuleFileName. Takes a handle to a module and returns the filename associated with it.
    //        ///     http://msdn.microsoft.com/en-us/library/windows/desktop/ms683197(v=vs.85).aspx
    //        /// </summary>
    //        /// <param name="hModule">Handle to a module</param>
    //        /// <param name="lpFilename">A pre-allocated StringBuilder instance to receive the name</param>
    //        /// <param name="nSize">Size, in characters, of lpFilename</param>
    //        /// <returns>
    //        ///     If the function fails the return value is 0. If the function succeeds the return value is the length of the
    //        ///     string that is copied to lpFilename. If lpFilename is too small the output is truncated, the function returns
    //        ///     nSize, and the Win32 last error is set to ERROR_INSUFFICIENT_BUFFER.
    //        /// </returns>
    //        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, PreserveSig = true, SetLastError = true, EntryPoint = "GetModuleFileNameW")]
    //        public static extern uint GetModuleFileName([In, Optional] IntPtr hModule, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpFilename, [In] uint nSize);

    //        /// <summary>
    //        ///     Returns the directory that a module resides in, if it is loaded in the process.
    //        /// </summary>
    //        /// <param name="moduleName">Name of the module. E.g. ntdll.dll</param>
    //        /// <param name="directory">The directory that contains the module</param>
    //        /// <returns>True if successful, false otherwise.</returns>
    //        public bool GetDirectoryForModule(string moduleName, out string directory)
    //        {
    //            var hModule = GetModuleHandle(moduleName);
    //            if (hModule == IntPtr.Zero)
    //            {
    //                goto Error;
    //            }

    //            var modulePath = new StringBuilder(4096);
    //            for (;;)
    //            {
    //                var bufferSize = (uint)modulePath.Capacity;
    //                var bytesCopied = GetModuleFileName(hModule, modulePath, bufferSize);
    //                if (bytesCopied == 0)
    //                {
    //                    goto Error;
    //                }
    //                if (bytesCopied < bufferSize)
    //                {
    //                    break;
    //                }
    //                if ((bytesCopied == bufferSize) && (Marshal.GetLastWin32Error() == ERROR_INSUFFICIENT_BUFFER))
    //                {
    //                    /* Grow the buffer and try again */
    //                    modulePath.Capacity = (modulePath.Capacity << 2);
    //                }
    //                else
    //                {
    //                    /* Not sure what happened here */
    //                    goto Error;
    //                }
    //            }

    //            directory = Path.GetDirectoryName(modulePath.ToString());
    //            return true;

    //            Error:
    //            directory = null;
    //            return false;
    //        }

    //        /// <summary>
    //        ///     Returns true if a HRESULT indicates failure.
    //        /// </summary>
    //        /// <param name="hr">HRESULT</param>
    //        /// <returns>True if hr indicates failure</returns>
    //        public static bool FAILED(int hr)
    //        {
    //            return (hr < 0);
    //        }

    //        /// <summary>
    //        ///     Returns true if a HRESULT indicates success.
    //        /// </summary>
    //        /// <param name="hr">HRESULT</param>
    //        /// <returns>True if hr indicates success</returns>
    //        public static bool SUCCEEDED(int hr)
    //        {
    //            return (hr >= 0);
    //        }

    //        /// <summary>
    //        ///     Wait for the debugger to enter a specific state.
    //        /// </summary>
    //        /// <param name="status">The status to wait for</param>
    //        /// <param name="timeout">How long to wait. Can specify INFINITE.</param>
    //        /// <param name="endingStatus">The final status, may not be what was requested if a timeout occurred.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int WaitForStatus(DEBUG_STATUS status, uint timeout, out DEBUG_STATUS endingStatus)
    //        {
    //            var hr = DebugControl.GetExecutionStatus(out endingStatus);
    //            if (FAILED(hr))
    //            {
    //                return hr;
    //            }

    //            if (timeout == INFINITE)
    //            {
    //                while (endingStatus != status)
    //                {
    //                    hr = DebugControl.WaitForEvent(DEBUG_WAIT.DEFAULT, INFINITE);
    //                    DebugControl.GetExecutionStatus(out endingStatus); /* Technically don't need to call this, we should get an event */
    //                    if (FAILED(hr))
    //                    {
    //                        return hr;
    //                    }
    //                }
    //            }
    //            else
    //            {
    //                var loopTime = DateTime.Now;
    //                var stopTime = loopTime.AddMilliseconds(timeout);
    //                while ((endingStatus != status) && ((loopTime = DateTime.Now) < stopTime))
    //                {
    //                    var waitTime = (uint)((stopTime - loopTime).TotalMilliseconds);
    //                    if (waitTime == 0)
    //                    {
    //                        waitTime = 1;
    //                    }
    //                    hr = DebugControl.WaitForEvent(DEBUG_WAIT.DEFAULT, waitTime);
    //                    DebugControl.GetExecutionStatus(out endingStatus); /* Technically don't need to call this, we should get an event */
    //                    if (FAILED(hr))
    //                    {
    //                        return hr;
    //                    }
    //                }
    //            }

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Breaks into the target process.
    //        /// </summary>
    //        /// <param name="timeout">How long to wait. Can specify INFINITE.</param>
    //        /// <param name="endingStatus">The final status, may not be what was requested if a timeout occurred.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int BreakExecution(uint timeout, out DEBUG_STATUS endingStatus)
    //        {
    //            var hr = DebugControl.GetExecutionStatus(out endingStatus);
    //            if (endingStatus == DEBUG_STATUS.BREAK)
    //            {
    //                goto Exit;
    //            }

    //            hr = DebugControl.SetInterrupt(DEBUG_INTERRUPT.ACTIVE);
    //            if (FAILED(hr))
    //            {
    //                DebugControl.GetExecutionStatus(out endingStatus);
    //                goto Exit;
    //            }

    //            hr = WaitForStatus(DEBUG_STATUS.BREAK, timeout, out endingStatus);

    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Resumes execution of the target process if it isn't currently running.
    //        /// </summary>
    //        /// <param name="timeout">How long to wait. Can specify INFINITE.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int ResumeExecution(uint timeout)
    //        {
    //            if (GetExecutionStatus() == DEBUG_STATUS.GO)
    //            {
    //                return S_OK;
    //            }

    //            var hr = DebugControl.SetExecutionStatus(DEBUG_STATUS.GO);
    //            if (FAILED(hr))
    //            {
    //                return hr;
    //            }
    //            DEBUG_STATUS newStatus;
    //            hr = WaitForStatus(DEBUG_STATUS.GO, timeout, out newStatus);
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine
    //        /// </summary>
    //        /// <param name="command">The command to execute</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int RunCommandInternal(string command, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false)
    //        {
    //            DEBUG_STATUS executionStatus;

    //            OutputVerboseLine("RunCommand: {0}", command);

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommand() called with a null or empty command!");
    //                return E_FAIL;
    //            }

    //            if (ShouldBreak())
    //            {
    //                OutputVerboseLine("ERROR! Not running command because ShouldBreak() returned true! '{0}'", command);
    //                return E_FAIL;
    //            }

    //            if (connectionIsPassive == false)
    //            {
    //                BreakExecution(breakExecutionTimeout, out executionStatus);
    //            }

    //            CurrentlyExecutingThreadId = Thread.CurrentThread.ManagedThreadId;


    //            int hr;
    //            try
    //            {
    //                hr = DebugControl.ExecuteWide(_outCtl | _NODML, command, DEBUG_EXECUTE.NOT_LOGGED | DEBUG_EXECUTE.NO_REPEAT);
    //            }
    //            finally
    //            {

    //            }
    //            CurrentlyExecutingThreadId = -1;

    //            if ((connectionIsPassive == false) && (resumeExecution == true))
    //            {
    //                if (SUCCEEDED(DebugControl.SetExecutionStatus(DEBUG_STATUS.GO)))
    //                {
    //                    WaitForStatus(DEBUG_STATUS.GO, breakExecutionTimeout, out executionStatus);
    //                }
    //            }

    //            try
    //            {
    //                DebugClient.FlushCallbacks();
    //            }
    //            catch
    //            {
    //                CoUninitialize();
    //                try
    //                {
    //                    DebugClient.FlushCallbacks();
    //                }
    //                catch
    //                {
    //                    // If it fails now, nothing we can do..  Just eat it :(
    //                }
    //            }

    //            return hr;
    //        }

    //        public int RunCommand(string command)
    //        {
    //#if DEBUG
    //            var sw = new Stopwatch();
    //            sw.Start();
    //#endif
    //            var hr = RunCommandWithFilter2(command, new AllOutputFilter2());
    //#if DEBUG
    //            sw.Stop();
    //            OutputDebugLine("RunCommand: '{0}' completed in {1} with hr:0x{2}", command, sw.Elapsed.ToString(), hr.ToString("x"));
    //#endif
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine.
    //        /// </summary>
    //        /// <param name="commandFormat">The command to execute.</param>
    //        /// <param name="parameters">Parameters for the command string.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandFormat(string commandFormat, params object[] parameters)
    //        {
    //            return RunCommand(FormatString(commandFormat, parameters));
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="command">The command to execute</param>
    //        /// <param name="outputFilter">A filter interface to use on the output.</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int RunCommandWithFilter(string command, SimpleOutputHandler.OUTPUT_FILTER outputFilter, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false)
    //        {
    //            SimpleOutputHandler outputHandler;

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommandWithFilter() called with a null or empty command!");
    //                return E_FAIL;
    //            }

    //            DebugUtilities executionUtilities;
    //            var hr = SimpleOutputHandler.Install(this, out executionUtilities, out outputHandler, outputFilter, true, true);
    //            if (FAILED(hr))
    //            {
    //                return hr;
    //            }

    //            using (outputHandler)
    //            {
    //                hr = executionUtilities.RunCommandInternal(command, resumeExecution, breakExecutionTimeout, connectionIsPassive);
    //                outputHandler.Dispose();
    //            }
    //            outputHandler = null;

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="command">The command to execute</param>
    //        /// <param name="outputFilter">A filter interface to use on the output.</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int RunCommandWithFilter2(string command, SimpleOutputHandler2.OUTPUT_FILTER outputFilter, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false)
    //        {
    //            SimpleOutputHandler2 outputHandler;

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommandWithFilter() called with a null or empty command!");
    //                return E_FAIL;
    //            }

    //            DebugUtilities executionUtilities;
    //            var hr = SimpleOutputHandler2.Install(this, out executionUtilities, out outputHandler, outputFilter, true, true, true);
    //            if (FAILED(hr))
    //            {
    //                return hr;
    //            }

    //            using (outputHandler)
    //            {
    //                hr = executionUtilities.RunCommandInternal(command, resumeExecution, breakExecutionTimeout, connectionIsPassive);
    //                outputHandler.Revert();
    //            }
    //            outputHandler = null;

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="command">The command to execute</param>
    //        /// <param name="outputFilter">A filter interface to use on the output.</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <returns>HRESULT of the operation</returns>
    //        public int RunCommandWithFilter3(string command, AdvancedOutputHandler.OUTPUT_FILTER outputFilter, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false)
    //        {
    //            AdvancedOutputHandler outputHandler;

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommandWithFilter() called with a null or empty command!");
    //                return E_FAIL;
    //            }

    //            DebugUtilities executionUtilities = null;
    //            executionUtilities = AdvancedOutputHandler.Install(this, out outputHandler, outputFilter);
    //            int hr = S_OK;
    //            using (outputHandler)
    //            {
    //                hr = executionUtilities.RunCommandInternal(command, resumeExecution, breakExecutionTimeout, connectionIsPassive);
    //                outputHandler.Flush();
    //                outputHandler.Revert();
    //            }
    //            outputHandler = null;

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and returns the text.
    //        /// </summary>
    //        /// <param name="command"></param>
    //        /// <param name="parameters"></param>
    //        /// <returns>Command output</returns>
    //        public List<string> RunCommandSaveOutputAndSplit(string command, params object[] parameters)
    //        {
    //            string commandOutput = null;
    //            var retVal = new List<string>();

    //            var hr = RunCommandSaveOutput(out commandOutput, command, parameters);

    //            if (!SUCCEEDED(hr))
    //            {
    //                if (commandOutput != null)
    //                {
    //                    ThrowExceptionHere(commandOutput, hr);
    //                }
    //                else
    //                {
    //                    ThrowExceptionHere(hr);
    //                }
    //            }

    //            else
    //            {
    //                var splits = commandOutput.Split(new[] {"\n"}, StringSplitOptions.None);
    //                foreach (var l in splits)
    //                {
    //                    retVal.Add(l);
    //                }
    //            }

    //            return retVal;
    //        }


    //        /// <summary>
    //        ///     Executes a command in the debugger engine and returns the text.
    //        /// </summary>
    //        /// <param name="commandFormat"></param>
    //        /// <param name="parameters"></param>
    //        /// <returns>Command output</returns>
    //        public string RunCommandSaveOutput(string commandFormat, params object[] parameters)
    //        {
    //            string commandOutput;

    //            var hr = RunCommandSaveOutput(out commandOutput, commandFormat, parameters);

    //            if (!SUCCEEDED(hr))
    //            {
    //                ThrowExceptionHere("Error running " + string.Format(commandFormat, parameters), hr);
    //            }

    //            return commandOutput;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and returns the text.
    //        /// </summary>
    //        /// <param name="commandFormat"></param>
    //        /// <param name="parameters"></param>
    //        /// <returns>Command output</returns>
    //        public string RunCommandSaveOutputWithErrors(string commandFormat, params object[] parameters)
    //        {
    //            string commandOutput;

    //            RunCommandSaveOutput(out commandOutput, commandFormat, parameters);

    //            return commandOutput;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="command">The command to execute.</param>
    //        /// <param name="commandOutput">A string variable to receive the stored output. NOTE: Always newline terminated.</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <param name="outputFilter">A filter interface to filter the output data.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandSaveOutput(string command, out string commandOutput, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false, SimpleOutputHandler.OUTPUT_FILTER outputFilter = null)
    //        {
    //            SimpleOutputHandler outputHandler;

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommandSaveOutput() called with a null or empty command!");
    //                commandOutput = "";
    //                return E_FAIL;
    //            }

    //            OutputVerboseLine("RunCommandSaveOutput: {0}", command);

    //            DebugUtilities executionUtilities;
    //            var hr = SimpleOutputHandler.Install(this, out executionUtilities, out outputHandler, outputFilter, false, false);
    //            if (FAILED(hr))
    //            {
    //                commandOutput = string.Format(CultureInfo.InvariantCulture, "ERROR! Failed installing the output handler: {0:x8}", hr);
    //                return hr;
    //            }

    //            using (outputHandler)
    //            {
    //                hr = executionUtilities.RunCommandInternal(command, resumeExecution, breakExecutionTimeout, connectionIsPassive);
    //                if (FAILED(hr))
    //                {
    //                    outputHandler.AddNoteLine(string.Format(CultureInfo.InvariantCulture, "WARNING! RunCommand returned an error: {0:x8}", hr));
    //                }
    //                commandOutput = outputHandler.GetText();
    //                //OutputDebugLine(commandOutput);
    //            }
    //            outputHandler = null;

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="command">The command to execute.</param>
    //        /// <param name="commandOutput">A string variable to receive the stored output. NOTE: Always newline terminated.</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <param name="outputFilter">A filter interface to filter the output data.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandSaveOutput2(string command, out string commandOutput, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false, SimpleOutputHandler2.OUTPUT_FILTER outputFilter = null)
    //        {
    //            SimpleOutputHandler2 outputHandler;

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommandSaveOutput2() called with a null or empty command!");
    //                commandOutput = "";
    //                return E_FAIL;
    //            }

    //            OutputVerboseLine("RunCommandSaveOutput2: {0}", command);

    //            DebugUtilities executionUtilities;
    //            var hr = SimpleOutputHandler2.Install(this, out executionUtilities, out outputHandler, outputFilter, false, false);
    //            if (FAILED(hr))
    //            {
    //                commandOutput = string.Format(CultureInfo.InvariantCulture, "ERROR! Failed installing the output handler: {0:x8}", hr);
    //                return hr;
    //            }

    //            using (outputHandler)
    //            {
    //                hr = executionUtilities.RunCommandInternal(command, resumeExecution, breakExecutionTimeout, connectionIsPassive);
    //                if (FAILED(hr))
    //                {
    //                    outputHandler.AddNoteLine(string.Format(CultureInfo.InvariantCulture, "WARNING! RunCommand returned an error: {0:x8}", hr), false);
    //                }
    //                commandOutput = outputHandler.GetText();
    //            }
    //            // OutputDebugLine(commandOutput);
    //            outputHandler = null;

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="commandOutput">A string variable to receive the stored output. NOTE: Always newline terminated.</param>
    //        /// <param name="commandFormat">The command to execute.</param>
    //        /// <param name="parameters">Parameters for the command string.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandSaveOutput(out string commandOutput, string commandFormat, params object[] parameters)
    //        {
    //            return RunCommandSaveOutput(FormatString(commandFormat, parameters), out commandOutput);
    //        }


    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="command">The command to execute.</param>
    //        /// <param name="commandOutput">A string variable to receive the stored output.</param>
    //        /// <param name="resumeExecution">True if execution should resume after the command completes. Only valid for live targets.</param>
    //        /// <param name="breakExecutionTimeout">How long the debugger should wait to break into the target if it is running.</param>
    //        /// <param name="connectionIsPassive">This must be set to True if the process is being debugged passively.</param>
    //        /// <param name="outputFilter">A filter interface to filter the output data.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandSaveOutputDML(string command, out string commandOutput, bool resumeExecution = false, uint breakExecutionTimeout = INFINITE, bool connectionIsPassive = false, SimpleOutputHandler2.OUTPUT_FILTER outputFilter = null)
    //        {
    //            SimpleOutputHandler2 outputHandler;

    //            if (string.IsNullOrEmpty(command))
    //            {
    //                OutputVerboseLine("ERROR! RunCommandSaveOutput2() called with a null or empty command!");
    //                commandOutput = "";
    //                return E_FAIL;
    //            }

    //            OutputVerboseLine("RunCommandSaveOutput2: {0}", command);

    //            DebugUtilities executionUtilities;
    //            var hr = SimpleOutputHandler2.Install(this, out executionUtilities, out outputHandler, outputFilter, true, false, false);
    //            if (FAILED(hr))
    //            {
    //                commandOutput = string.Format(CultureInfo.InvariantCulture, "ERROR! Failed installing the output handler: {0:x8}", hr);
    //                return hr;
    //            }

    //            using (outputHandler)
    //            {
    //                hr = executionUtilities.RunCommandInternal(command, resumeExecution, breakExecutionTimeout, connectionIsPassive);
    //                if (FAILED(hr))
    //                {
    //                    outputHandler.AddNoteLine(string.Format(CultureInfo.InvariantCulture, "WARNING! RunCommand returned an error: {0:x8}", hr), false);
    //                }
    //                commandOutput = outputHandler.GetText();
    //                outputHandler.Revert();
    //            }

    //            outputHandler = null;

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="commandOutput">A string variable to receive the stored output.</param>
    //        /// <param name="commandFormat">The command to execute.</param>
    //        /// <param name="parameters">Parameters for the command string.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandSaveOutputDML(out string commandOutput, string commandFormat, params object[] parameters)
    //        {
    //            return RunCommandSaveOutputDML(FormatString(commandFormat, parameters), out commandOutput);
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and returns the output including DML and errors.
    //        /// </summary>
    //        /// <param name="commandFormat">The command to execute.</param>
    //        /// <param name="parameters">Parameters for the command string.</param>
    //        /// <returns>The command output.</returns>
    //        public string RunCommandSaveOutputDMLWithErrors(string commandFormat, params object[] parameters)
    //        {
    //            string commandOutput;

    //            var hr = RunCommandSaveOutputDML(FormatString(commandFormat, parameters), out commandOutput);

    //            if (!SUCCEEDED(hr) &&
    //                !commandOutput.Contains("Syntax error")) // Ignore simple typo errors
    //            {
    //                ThrowExceptionHere(hr);
    //            }

    //            return commandOutput;
    //        }

    //        /// <summary>
    //        ///     Executes a command in the debugger engine and filters the output. The filtered output is passed up to higher levels
    //        ///     and is not returned to the caller.
    //        /// </summary>
    //        /// <param name="commandFormat">The command to execute.</param>
    //        /// <param name="parameters">Optional parameters for the command format string.</param>
    //        /// <returns>HRESULT of the operation.</returns>
    //        public int RunCommandSilent(string commandFormat, params object[] parameters)
    //        {
    //            OutputDebugLine("Running command (silent): " + FormatString(commandFormat, parameters));

    //            return RunCommandWithFilter3(FormatString(commandFormat, parameters), new IgnoreOutputFilterAdvanced());
    //        }


    //        /// <summary>
    //        ///     Get the current execution status of the target process
    //        /// </summary>
    //        /// <returns>HRESULT</returns>
    //        public DEBUG_STATUS GetExecutionStatus()
    //        {
    //            DEBUG_STATUS status;
    //            DebugControl.GetExecutionStatus(out status);
    //            return status;
    //        }

    //        /// <summary>
    //        ///     Checks for a specific exception caused by releasing a COM object received from native code that was actually a
    //        ///     managed object.
    //        /// </summary>
    //        /// <param name="comObject">COM object to release</param>
    //        public static void ReleaseComObjectSafely(object comObject)
    //        {
    //            try
    //            {
    //                Marshal.ReleaseComObject(comObject);
    //            }
    //            catch (InvalidCastException)
    //            {
    //                /* If we got a managed object we can't release it, just ignore the exception */
    //            }
    //            catch (ArgumentException) {}
    //        }

    //        /// <summary>
    //        ///     Creates a new breakpoint with command
    //        ///     Equivalent to: > bp breakpointExpression "command"
    //        /// </summary>
    //        /// <param name="breakpointExpression">The expression of where to set the breakpoint</param>
    //        /// <param name="command">The command to be executed when breakpoint is hit</param>
    //        /// <param name="id">A UInt32 to receive the breakpoint ID of the new breakpoint</param>
    //        /// <returns>HRESULT</returns>
    //        public int CreateBreakpoint(string breakpointExpression, string command, out uint id)
    //        {
    //            IDebugBreakpoint2 debugBreakpoint = null;
    //            int hr;
    //            if (FAILED(hr = DebugControl.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, DEBUG_ANY_ID, out debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.SetOffsetExpressionWide(breakpointExpression)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.SetCommandWide(command)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.GetId(out id)))
    //            {
    //                goto ExitWithRelease;
    //            }

    //            goto Exit;
    //            ExitWithRelease:
    //            id = 0;
    //            ReleaseComObjectSafely(debugBreakpoint);
    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Creates a new breakpoint
    //        /// </summary>
    //        /// <param name="breakpointExpression">The expression of where to set the breakpoint</param>
    //        /// <param name="id">A UInt32 to receive the breakpoint ID of the new breakpoint</param>
    //        /// <returns>HRESULT</returns>
    //        public int CreateBreakpoint(string breakpointExpression, out uint id)
    //        {
    //            IDebugBreakpoint2 debugBreakpoint = null;
    //            int hr;
    //            if (FAILED(hr = DebugControl.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, DEBUG_ANY_ID, out debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.SetOffsetExpressionWide(breakpointExpression)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.GetId(out id)))
    //            {
    //                goto ExitWithRelease;
    //            }

    //            goto Exit;
    //            ExitWithRelease:
    //            id = 0;
    //            ReleaseComObjectSafely(debugBreakpoint);
    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Creates a new data breakpoint
    //        /// </summary>
    //        /// <param name="size">The pointer size</param>
    //        /// <param name="offset">The pointer to the memory</param>
    //        /// <param name="access">The acces type on which to break</param>
    //        /// <param name="id">The id of the breakpoint</param>
    //        /// <returns>HRESULT</returns>
    //        public int CreateDataBreakpoint(uint size, ulong offset, DEBUG_BREAKPOINT_ACCESS_TYPE access, out uint id)
    //        {
    //            IDebugBreakpoint2 debugBreakpoint = null;
    //            int hr;
    //            if (FAILED(hr = DebugControl.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.DATA, DEBUG_ANY_ID, out debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.SetOffset(offset)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.SetDataParameters(size, access)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.GetId(out id)))
    //            {
    //                goto ExitWithRelease;
    //            }

    //            goto Exit;
    //            ExitWithRelease:
    //            id = 0;
    //            ReleaseComObjectSafely(debugBreakpoint);
    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Removes a breakpoint.
    //        /// </summary>
    //        /// <param name="id">The id of the breakpoint</param>
    //        /// <returns>HRESULT</returns>
    //        public int RemoveBreakpoint(uint id)
    //        {
    //            IDebugBreakpoint2 debugBreakpoint = null;
    //            int hr;
    //            if (FAILED(hr = DebugControl.GetBreakpointById2(id, out debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = DebugControl.RemoveBreakpoint2(debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }

    //            goto Exit;
    //            ExitWithRelease:
    //            id = 0;
    //            ReleaseComObjectSafely(debugBreakpoint);
    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Disables a breakpoint.
    //        /// </summary>
    //        /// <param name="id">The id of the breakpoint</param>
    //        /// <returns>HRESULT</returns>
    //        public int DisableBreakpoint(uint id)
    //        {
    //            IDebugBreakpoint2 debugBreakpoint = null;
    //            int hr;
    //            if (FAILED(hr = DebugControl.GetBreakpointById2(id, out debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.RemoveFlags(DEBUG_BREAKPOINT_FLAG.ENABLED)))
    //            {
    //                goto ExitWithRelease;
    //            }

    //            goto Exit;
    //            ExitWithRelease:
    //            id = 0;
    //            ReleaseComObjectSafely(debugBreakpoint);
    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        /// Get the ids of all breakpoints.
    //        /// </summary>
    //        /// <returns></returns>
    //        public List<uint> GetBreakpointIds()
    //        {
    //            List<uint> result = new List<uint>();

    //            // get the number of breakpoints
    //            uint bpNum;
    //            if (SUCCEEDED(DebugControl.GetNumberBreakpoints(out bpNum)))
    //            {
    //                for (uint i = 0; i < bpNum; ++i)
    //                {
    //                    // get the bp
    //                    IDebugBreakpoint bp;
    //                    if (SUCCEEDED(DebugControl.GetBreakpointByIndex(i, out bp)))
    //                    {
    //                        // get the id
    //                        uint id;
    //                        if (SUCCEEDED(bp.GetId(out id)))
    //                        {
    //                            result.Add(id);
    //                        }
    //                    }
    //                }
    //            }

    //            return result;
    //        }
    //        /// <summary>
    //        /// Get the offset addresses of the currently enabled breakpoints.
    //        /// </summary>
    //        /// <returns></returns>
    //        public List<ulong> GetActiveBreakpointOffsets()
    //        {
    //            List<ulong> result = new List<ulong>();

    //            // get the number of breakpoints
    //            uint bpNum;
    //            if (SUCCEEDED(DebugControl.GetNumberBreakpoints(out bpNum)))
    //            {
    //                for (uint i = 0; i < bpNum; ++i)
    //                {
    //                    // get the bp
    //                    IDebugBreakpoint bp;
    //                    if (SUCCEEDED(DebugControl.GetBreakpointByIndex(i, out bp)))
    //                    {
    //                        DEBUG_BREAKPOINT_FLAG flags;
    //                        if (SUCCEEDED(bp.GetFlags(out flags)))
    //                        {
    //                            // is it active?
    //                            if ((flags & DEBUG_BREAKPOINT_FLAG.ENABLED) == DEBUG_BREAKPOINT_FLAG.ENABLED)
    //                            {
    //                                // get the offset
    //                                ulong offset;
    //                                if (SUCCEEDED(bp.GetOffset(out offset)))
    //                                {
    //                                    result.Add(offset);
    //                                }
    //                            }
    //                        }
    //                    }
    //                }
    //            }

    //            return result;
    //        }

    //        /// <summary>
    //        ///     Enables an existing breakpoint.
    //        /// </summary>
    //        /// <param name="id">The id of the breakpoint</param>
    //        /// <returns>HRESULT</returns>
    //        public int EnableBreakpoint(uint id)
    //        {
    //            IDebugBreakpoint2 debugBreakpoint = null;
    //            int hr;
    //            if (FAILED(hr = DebugControl.GetBreakpointById2(id, out debugBreakpoint)))
    //            {
    //                goto ExitWithRelease;
    //            }
    //            if (FAILED(hr = debugBreakpoint.AddFlags(DEBUG_BREAKPOINT_FLAG.ENABLED)))
    //            {
    //                goto ExitWithRelease;
    //            }

    //            goto Exit;
    //            ExitWithRelease:
    //            id = 0;
    //            ReleaseComObjectSafely(debugBreakpoint);
    //            Exit:
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Converts an address to a 8 or 16 character hex string depending on target architecture.
    //        /// </summary>
    //        /// <param name="address">Address to convert to a string</param>
    //        /// <param name="ActualCPU">P2S based on actual CPU type rather than current CPU type</param>
    //        /// <returns>8 or 16 character hex representation of address</returns>
    //        public string P2S(ulong address, bool ActualCPU = false)
    //        {
    //            if (ActualCPU)
    //            {
    //                if (Wow64Exts.IsActualProcessor64Bit(this))
    //                {
    //                    return address.ToString("x16", CultureInfo.InvariantCulture);
    //                }
    //                return ((uint)address).ToString("x8", CultureInfo.InvariantCulture);
    //            }
    //            if (DebugControl.IsPointer64Bit() == S_OK)
    //            {
    //                return address.ToString("x16", CultureInfo.InvariantCulture);
    //            }
    //            return ((uint)address).ToString("x8", CultureInfo.InvariantCulture);
    //        }

    //        /// <summary>
    //        ///     Converts an address to a 8 or 16 character hex string depending on target architecture.
    //        ///     This version uses uppercase alpha characters.
    //        /// </summary>
    //        /// <param name="address">Address to convert to a string</param>
    //        /// <returns>8 or 16 character hex representation of address</returns>
    //        public string P2SUC(ulong address, bool ActualCPU = false)
    //        {
    //            if (ActualCPU)
    //            {
    //                if (Wow64Exts.IsActualProcessor64Bit(this))
    //                {
    //                    return address.ToString("X16", CultureInfo.InvariantCulture);
    //                }
    //                return ((uint)address).ToString("X8", CultureInfo.InvariantCulture);
    //            }
    //            if (DebugControl.IsPointer64Bit() == S_OK)
    //            {
    //                return address.ToString("X16", CultureInfo.InvariantCulture);
    //            }
    //            return ((uint)address).ToString("X8", CultureInfo.InvariantCulture);
    //        }

    //        public string RemoveCarriageReturn(string sz, string replace = "\\r\n")
    //        {
    //            if (string.IsNullOrWhiteSpace(sz))
    //            {
    //                return string.Empty;
    //            }

    //            // Make sure whatever character we matched after \r is kept
    //            return RemoveCarriageReturnRegex.Replace(sz, replace + "$1");
    //        }

    //        /// <summary>
    //        ///     Reads a null-terminated ANSI or Multi-byte string from the target.
    //        /// </summary>
    //        /// <param name="address">Address of the string</param>
    //        /// <param name="maxSize">Maximum number of bytes to read</param>
    //        /// <param name="output">The string</param>
    //        /// <returns>Last HRESULT received while retrieving the string</returns>
    //        public int ReadAnsiString(ulong address, uint maxSize, out string output)
    //        {
    //            var sb = new StringBuilder((int)maxSize + 1);
    //            uint bytesRead;
    //            var hr = DebugDataSpaces.ReadMultiByteStringVirtual(address, maxSize, sb, maxSize, &bytesRead);
    //            if (SUCCEEDED(hr))
    //            {
    //                if (bytesRead > maxSize)
    //                {
    //                    sb.Length = (int)maxSize;
    //                }
    //                output = sb.ToString();
    //            }
    //            else
    //            {
    //                output = null;
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Reads a null-terminated Unicode string from the target.
    //        /// </summary>
    //        /// <param name="address">Address of the Unicode string</param>
    //        /// <param name="maxSize">Maximum number of bytes to read</param>
    //        /// <param name="output">The string</param>
    //        /// <returns>Last HRESULT received while retrieving the string</returns>
    //        public int ReadUnicodeString(ulong address, uint maxSize, out string output)
    //        {
    //            var sb = new StringBuilder((int)maxSize + 1);
    //            uint bytesRead;
    //            var hr = DebugDataSpaces.ReadUnicodeStringVirtualWide(address, (maxSize*2), sb, maxSize, &bytesRead);
    //            if (SUCCEEDED(hr))
    //            {
    //                if ((bytesRead/2) > maxSize)
    //                {
    //                    sb.Length = (int)maxSize;
    //                }
    //                output = sb.ToString();
    //            }
    //            else if (ERROR_INVALID_PARAMETER == hr)
    //            {
    //                sb.Length = (int)maxSize;
    //                output = sb.ToString();
    //            }
    //            else
    //            {
    //                output = null;
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        /// </summary>
    //        /// <param name="address"></param>
    //        /// <param name="length">Number of BYTES to read (1 unicode char = 2 bytes)</param>
    //        /// <returns></returns>
    //        public string ReadUnicodeStringFixedLength(ulong address, int lengthInBytes)
    //        {
    //            var bytes = ReadBytes(address, lengthInBytes);
    //            return Encoding.Unicode.GetString(bytes);
    //        }

    //        /// <summary>
    //        ///     Converts a UNICODE_STRING structure on the target to a string
    //        /// </summary>
    //        /// <param name="address">Address of the UNICODE_STRING structure</param>
    //        /// <param name="output">String containing the string.</param>
    //        /// <param name="options"></param>
    //        /// <returns>HRESULT of the last helper function called</returns>
    //        public int ReadUNICODE_STRING(ulong address, out string output, ReadUNICODE_STRINGOptions options = ReadUNICODE_STRINGOptions.Escaped)
    //        {
    //            var result = IsPointer64Bit() ? ReadUNICODE_STRING64(address, out output) : ReadUNICODE_STRING32(address, out output);

    //            if (output == null || output.StartsWith("\0"))
    //            {
    //                output = string.Empty;
    //                return result;
    //            }

    //            switch (options)
    //            {
    //                case ReadUNICODE_STRINGOptions.Escaped:
    //                {
    //                    output = output.Replace("\0", "\\0");
    //                    break;
    //                }
    //                case ReadUNICODE_STRINGOptions.Truncated:
    //                {
    //                    var firstnull = output.IndexOf('\0');
    //                    if (firstnull > 0)
    //                    {
    //                        var trunccount = output.Length - firstnull;
    //                        if (trunccount > "\\0  <TRUNCATED x CHARS>".Length)
    //                        {
    //                            output = output.Substring(0, firstnull) + string.Format("\\0  <TRUNCATED {0} CHARS>", trunccount);
    //                        }
    //                        else
    //                        {
    //                            output = output.Replace("\0", "\\0");
    //                        }
    //                    }
    //                    break;
    //                }
    //            }
    //            return result;
    //        }

    //        /// <summary>
    //        ///     Converts a UNICODE_STRING structure on the target to a string.
    //        ///     This version forces 32-bit pointers, useful when dealing with the WOW64 environment.
    //        /// </summary>
    //        /// <param name="address">Address of the UNICODE_STRING structure</param>
    //        /// <param name="output">String containing the string</param>
    //        /// <returns>HRESULT of the last helper function called</returns>
    //        public int ReadUNICODE_STRING32(ulong address, out string output)
    //        {
    //            /*
    //				USHORT Length; - does not include null terminator, if there is one
    //				USHORT MaximumLength;
    //				PTR Buffer
    //			*/

    //            ushort lengthInBytes;
    //            var buffer = IntPtr.Zero;

    //            var hr = ReadVirtual16(address, out lengthInBytes);
    //            if (hr != S_OK)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            if (lengthInBytes == 0)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            var lengthInChars = (lengthInBytes >> 1);

    //            uint bufferAddress32;
    //            hr = ReadVirtual32(address + 4, out bufferAddress32);
    //            if (hr != S_OK)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            var bufferAddress = SignExtendAddress(bufferAddress32);

    //            buffer = Marshal.AllocHGlobal(lengthInBytes + 2);

    //            hr = DebugDataSpaces.ReadVirtual(bufferAddress, buffer, lengthInBytes, null);
    //            if (hr != S_OK)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            output = Marshal.PtrToStringUni(buffer, lengthInChars);

    //            Exit:
    //            if (buffer != IntPtr.Zero)
    //            {
    //                Marshal.FreeHGlobal(buffer);
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Converts a UNICODE_STRING structure on the target to a string.
    //        ///     This version forces 64-bit pointers, useful when you want to read data in the WOW64 environment itself not in the
    //        ///     wrapped process.
    //        /// </summary>
    //        /// <param name="address">Address of the UNICODE_STRING structure</param>
    //        /// <param name="output">String containing the string</param>
    //        /// <returns>HRESULT of the last helper function called</returns>
    //        public int ReadUNICODE_STRING64(ulong address, out string output)
    //        {
    //            /*
    //				USHORT Length; - does not include null terminator, if there is one
    //				USHORT MaximumLength;
    //				PTR Buffer
    //			*/

    //            ushort lengthInBytes;
    //            var buffer = IntPtr.Zero;

    //            var hr = ReadVirtual16(address, out lengthInBytes);
    //            if (hr != S_OK)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            if (lengthInBytes == 0)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            var lengthInChars = (lengthInBytes >> 1);

    //            ulong bufferAddress;
    //            hr = ReadVirtual64(address + 8, out bufferAddress);
    //            if (hr != S_OK)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            buffer = Marshal.AllocHGlobal(lengthInBytes + 2);

    //            hr = DebugDataSpaces.ReadVirtual(bufferAddress, buffer, lengthInBytes, null);
    //            if (hr != S_OK)
    //            {
    //                output = null;
    //                goto Exit;
    //            }

    //            output = Marshal.PtrToStringUni(buffer, lengthInChars);

    //            Exit:
    //            if (buffer != IntPtr.Zero)
    //            {
    //                Marshal.FreeHGlobal(buffer);
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Aligns an address on a page boundary
    //        /// </summary>
    //        /// <param name="PageSize">Page size of the target system</param>
    //        /// <param name="Addr">Address to align (down)</param>
    //        /// <returns>Page address that contains Addr</returns>
    //        public ulong DBG_PAGE_ALIGN(uint PageSize, ulong Addr)
    //        {
    //            return (Addr & ~(((ulong)PageSize) - 1));
    //        }

    //        /// <summary>
    //        ///     Get the address of the next page
    //        /// </summary>
    //        /// <param name="PageSize">Page size of the target system</param>
    //        /// <param name="Addr">Address to align up</param>
    //        /// <returns>Next page boundary past Addr</returns>
    //        public ulong PAGE_ROUND_UP(uint PageSize, ulong Addr)
    //        {
    //            return DBG_PAGE_ALIGN(PageSize, Addr + (((ulong)PageSize) - 1));
    //        }

    //        /// <summary>
    //        ///     Returns the size of a pointer on the target
    //        /// </summary>
    //        /// <returns>Size of a pointer on the target machine</returns>
    //        public uint PointerSize()
    //        {
    //            if (DebugControl.IsPointer64Bit() == S_OK)
    //            {
    //                return 8;
    //            }
    //            return 4;
    //        }

    //        /// <summary>
    //        ///     Used to determine whether the debug target has a 64-bit pointer size
    //        /// </summary>
    //        /// <returns>True if 64-bit, otherwise false</returns>
    //        public bool IsPointer64Bit()
    //        {
    //            return (DebugControl.IsPointer64Bit() == S_OK);
    //        }

    //        /// <summary>
    //        ///     Splits a string into an argument list, leaving quotes and escaped characters as found.
    //        /// </summary>
    //        /// <param name="commandLine">A string containing all of the arguments.</param>
    //        /// <returns>An array containing the arguments in commandLine.</returns>
    //        public static string[] SegmentCommandLine(string commandLine)
    //        {
    //            var quoteChar = (char)0;
    //            var inquote = false;
    //            var numargs = 0;

    //            var argumentIndexes = new int[commandLine.Length + 1];
    //            var destinationBuffer = new char[commandLine.Length + 1];
    //            var dstPos = 0;

    //            var commandLineCopy = Marshal.StringToHGlobalUni(commandLine);
    //            var psrc = (char*)commandLineCopy;

    //            /* loop on each argument */
    //            for (;;)
    //            {
    //                if ((*psrc) != 0)
    //                {
    //                    while ((*psrc) == ' ' || (*psrc) == '\t')
    //                    {
    //                        ++psrc;
    //                    }
    //                }

    //                if ((*psrc) == '\0')
    //                {
    //                    break; /* end of args */
    //                }

    //                /* scan an argument */
    //                argumentIndexes[numargs++] = dstPos;

    //                /* loop through scanning one argument */
    //                var literal = false;

    //                if ((*psrc) == '@')
    //                {
    //                    literal = true;
    //                    ++psrc;                 
    //                    if (((*psrc) != '"') && ((*psrc) != '\''))
    //                    {
    //                        literal = false;
    //                        --psrc;
    //                    }
    //                }

    //                for (;;)
    //                {
    //                    /* Rules: 2N backslashes + " ==> N backslashes and begin/end quote
    //							  2N+1 backslashes + " ==> N backslashes + literal "
    //							  N backslashes ==> N backslashes */
    //                    var numslash = 0;

    //                    if (!literal)
    //                    {
    //                        while ((*psrc) == '\\')
    //                        {
    //                            /* count number of backslashes for use below */
    //                            ++psrc;
    //                            ++numslash;
    //                        }
    //                    }

    //                    if (((*psrc) == '"') || ((*psrc) == '\''))
    //                    {
    //                        /* if 2N backslashes before, start/end quote, otherwise copy literally */
    //                        if (numslash%2 == 0 || literal)
    //                        {
    //                            if (inquote == false)
    //                            {
    //                                inquote = true;
    //                                quoteChar = *psrc;
    //                            }
    //                            else if ((*psrc) == quoteChar)
    //                            {
    //                                inquote = false;
    //                                literal = false;
    //                                quoteChar = (char)0;
    //                            }
    //                        }
    //                    }

    //                    /* copy slashes */
    //                    while ((numslash--) != 0)
    //                    {
    //                        destinationBuffer[dstPos++] = '\\';
    //                    }

    //                    /* if at end of arg, break loop */
    //                    if ((*psrc) == '\0' || (!inquote && ((*psrc) == ' ' || (*psrc) == '\t')))
    //                    {
    //                        break;
    //                    }

    //                    /* copy character into argument */
    //                    destinationBuffer[dstPos++] = (*psrc);
    //                    ++psrc;
    //                }

    //                /* null-terminate the argument */
    //                destinationBuffer[dstPos++] = '\0';
    //            }

    //            var args = new string[numargs];
    //            fixed (char* destinationBufferPtr = &destinationBuffer[0])
    //            {
    //                for (var i = 0; i < args.Length; ++i)
    //                {
    //                    args[i] = Marshal.PtrToStringUni((IntPtr)(destinationBufferPtr + argumentIndexes[i]));
    //                }
    //            }

    //            if (commandLineCopy != IntPtr.Zero)
    //            {
    //                Marshal.FreeHGlobal(commandLineCopy);
    //                commandLineCopy = IntPtr.Zero;
    //            }

    //            return args;
    //        }

    //        /// <summary>
    //        ///     Finds the next instance of a character that isn't escaped with a backslash and isn't contained within a quote
    //        ///     block.
    //        ///     NOTE: Two slashes followed by the search character would NOT count as a hit since the first slash would cancel out
    //        ///     the second. I have debated whether it is worth counting \\C as an escaped, escaped C so that may become an option
    //        ///     in the future.
    //        ///     NOTE: The algorithm does not scan backwards before start looking for escapes or quotes.
    //        /// </summary>
    //        /// <param name="input">String to search</param>
    //        /// <param name="character">Character to search for</param>
    //        /// <param name="start">Offset to start searching at</param>
    //        /// <returns>Offset of the first non-quoted non-escaped character, or -1 if not found.</returns>
    //        public int NextNonEscapedNonQuotedCharacter(string input, char character, int start = 0)
    //        {
    //            if ((start < 0) || (input == null) || (start >= input.Length))
    //            {
    //                return -1;
    //            }

    //            do
    //            {
    //                var c = input[start];
    //                if (c == '\\')
    //                {
    //                    /* Don't even bother processing the escaped character */
    //                    ++start;
    //                }
    //                if (c == '"')
    //                {
    //                    while (++start < input.Length)
    //                    {
    //                        c = input[start];
    //                        if (c == '\\')
    //                        {
    //                            /* Don't even bother processing the escaped character */
    //                            ++start;
    //                        }
    //                        if (c == '"')
    //                        {
    //                            break;
    //                        }
    //                    }
    //                }
    //                else if (c == character)
    //                {
    //                    return start;
    //                }
    //            } while (++start < input.Length);
    //            return -1;
    //        }

    //        /// <summary>
    //        ///     Finds the next non-escaped instance of a character in a string.
    //        /// </summary>
    //        /// <param name="input">String to search.</param>
    //        /// <param name="character">Character to look for.</param>
    //        /// <param name="start">Index of the first character to start at, which IS included in the search.</param>
    //        /// <returns>Index of the character which will be greater-than-or-equal to start, or -1 if the character is not found.</returns>
    //        public int NextNonEscapedCharacter(string input, char character, int start = 0)
    //        {
    //            var index = input.IndexOf(character, start);
    //            if (index <= 0)
    //            {
    //                return index;
    //            }
    //            do
    //            {
    //                var previousChar = input[index - 1];
    //                if (previousChar != '\\')
    //                {
    //                    return index;
    //                }
    //                start = index + 1;
    //                if (start >= input.Length)
    //                {
    //                    return -1;
    //                }
    //                index = input.IndexOf(character, start);
    //            } while (index > 0);
    //            return index;
    //        }

    //        /// <summary>
    //        ///     Finds the previous non-escaped instance of a character in a string.
    //        /// </summary>
    //        /// <param name="input">String to search.</param>
    //        /// <param name="character">Character to look for.</param>
    //        /// <param name="start">Index of the first character to start at, which IS included in the search.</param>
    //        /// <returns>Index of the character which will be less-than-or-equal to start, or -1 if the character is not found.</returns>
    //        public int PreviousNonEscapedCharacter(string input, char character, int start)
    //        {
    //            if (start >= input.Length)
    //            {
    //                start = input.Length - 1;
    //            }

    //            var index = input.IndexOf(character, start);
    //            index = input.LastIndexOf(character, start);
    //            if (index <= 0)
    //            {
    //                return index;
    //            }
    //            do
    //            {
    //                var previousChar = input[index - 1];
    //                if (previousChar != '\\')
    //                {
    //                    return index;
    //                }
    //                start = index + 1;
    //                if (start >= input.Length)
    //                {
    //                    return -1;
    //                }
    //                index = input.LastIndexOf(character, start);
    //            } while (index > 0);
    //            return index;
    //        }

    //        /// <summary>
    //        ///     Splits a string into parts using a designated separator character. If the character is escaped with a backslash or
    //        ///     is contained within double quotes it is not treated as a separator.
    //        /// </summary>
    //        /// <param name="input">The string to split</param>
    //        /// <param name="separator">The charater to separate on</param>
    //        /// <returns>An array of strings</returns>
    //        public string[] SplitStringIgnoreEscapedAndQuoted(string input, char separator)
    //        {
    //            var lfIndex = NextNonEscapedNonQuotedCharacter(input, separator, 0);
    //            if (lfIndex == -1)
    //            {
    //                return new[] {input};
    //            }

    //            var stringStart = lfIndex + 1;
    //            var strings = new List<string> {input.Substring(0, lfIndex)};
    //            while (stringStart < input.Length)
    //            {
    //                Console.Write("Start character: {0}={1}", stringStart, input[stringStart]);

    //                lfIndex = NextNonEscapedNonQuotedCharacter(input, separator, stringStart);

    //                Console.WriteLine(" : {0}", lfIndex);

    //                if (lfIndex == -1)
    //                {
    //                    break;
    //                }
    //                strings.Add(input.Substring(stringStart, lfIndex - stringStart));
    //                stringStart = lfIndex + 1;
    //            }
    //            if (stringStart <= input.Length)
    //            {
    //                strings.Add(input.Substring(stringStart));
    //            }
    //            return strings.ToArray();
    //        }

    //        /// <summary>
    //        ///     Splits a string into parts using a designated separator character. If the character is escaped with a backslash it
    //        ///     is not treated as a separator.
    //        /// </summary>
    //        /// <param name="input">The string to split</param>
    //        /// <param name="separator">The charater to separate on</param>
    //        /// <returns>An array of strings</returns>
    //        public string[] SplitStringIgnoreEscaped(string input, char separator)
    //        {
    //            var lfIndex = NextNonEscapedCharacter(input, separator, 0);
    //            if (lfIndex == -1)
    //            {
    //                return new[] {input};
    //            }

    //            var stringStart = lfIndex + 1;
    //            var strings = new List<string> {input.Substring(0, lfIndex)};
    //            while (stringStart < input.Length)
    //            {
    //                Console.Write("Start character: {0}={1}", stringStart, input[stringStart]);

    //                lfIndex = NextNonEscapedCharacter(input, separator, stringStart);

    //                Console.WriteLine(" : {0}", lfIndex);

    //                if (lfIndex == -1)
    //                {
    //                    break;
    //                }
    //                strings.Add(input.Substring(stringStart, lfIndex - stringStart));
    //                stringStart = lfIndex + 1;
    //            }
    //            if (stringStart <= input.Length)
    //            {
    //                strings.Add(input.Substring(stringStart));
    //            }
    //            return strings.ToArray();
    //        }

    //        /// <summary>
    //        ///     Searches a string for a non-escaped separator character and splits the string into two parts if it is found.
    //        /// </summary>
    //        /// <param name="input">The string to split.</param>
    //        /// <param name="separator">The charater to separate on.</param>
    //        /// <param name="remainder">Null if the separator is not found, otherwise the END of the input string past the separator</param>
    //        /// <returns>Everything before the first separator, or the entire string if no separator is found.</returns>
    //        public string SplitStringIgnoreEscaped_FirstOnly(string input, char separator, out string remainder)
    //        {
    //            var lfIndex = NextNonEscapedCharacter(input, separator, 0);
    //            if (lfIndex == -1)
    //            {
    //                remainder = null;
    //                return input;
    //            }
    //            remainder = input.Substring(lfIndex + 1);
    //            return input.Substring(0, lfIndex);
    //        }

    //        /// <summary>
    //        ///     Searches a string for a non-escaped separator character, from the end, and splits the string into two parts if it
    //        ///     is found.
    //        /// </summary>
    //        /// <param name="input">The string to split.</param>
    //        /// <param name="separator">The charater to separate on.</param>
    //        /// <param name="remainder">
    //        ///     Null if the separator is not found, otherwise the BEGINNING of the input string before the
    //        ///     separator
    //        /// </param>
    //        /// <returns>Everything after the first separator, or the entire string if no separator is found.</returns>
    //        public string SplitStringIgnoreEscaped_LastOnly(string input, char separator, out string remainder)
    //        {
    //            var lfIndex = PreviousNonEscapedCharacter(input, separator, input.Length - 1);
    //            if (lfIndex == -1)
    //            {
    //                remainder = null;
    //                return input;
    //            }
    //            remainder = input.Substring(0, lfIndex);
    //            return input.Substring(lfIndex + 1);
    //        }

    //        /// <summary>
    //        ///     Returns a version of the input string that has all instances of a specific character escaped
    //        /// </summary>
    //        /// <param name="character">The character to escape</param>
    //        /// <param name="input">An input string to modify</param>
    //        /// <returns>The input string with all instances of character escaped (i.e. with a preceeding backslash)</returns>
    //        public string EscapeCharacter(char character, string input)
    //        {
    //            var charAsString = character.ToString(CultureInfo.InvariantCulture);
    //            return input.Replace(charAsString, "\\" + charAsString);
    //        }

    //        /// <summary>
    //        ///     Returns a version of the input string with one level of escaping removed for a specific character
    //        /// </summary>
    //        /// <param name="character">The character to unescape</param>
    //        /// <param name="input">An input string to modify</param>
    //        /// <returns>The input string with one level of escaping removed (i.e. the preceeding backslash removed)</returns>
    //        public string UnEscapeCharacter(char character, string input)
    //        {
    //            var charAsString = character.ToString(CultureInfo.InvariantCulture);
    //            return input.Replace("\\" + charAsString, charAsString);
    //        }

    //        /// <summary>
    //        ///     If a string begins and ends with a double or single quote, and the beginning quote is the matching pair for the
    //        ///     ending quote, remove the quotes then unescape all instanes of that quote type in the string.
    //        /// </summary>
    //        /// <param name="input">An input string to modify</param>
    //        /// <returns>Modified string</returns>
    //        public string RemoveContainingQuotesAndUnEscapeIfPossible(string input)
    //        {
    //            if (string.IsNullOrEmpty(input))
    //            {
    //                return string.Empty;
    //            }

    //            if (input.StartsWith("@'") || input.StartsWith("@\""))
    //            {
    //                return (RemoveContainingQuotesWithoutUnEscaping(input.Substring(1, input.Length - 1)));
    //            }

    //            char quoteCharacter;
    //            if ((input[0] == '"') && (input[input.Length - 1] == '"'))
    //            {
    //                quoteCharacter = '"';
    //            }
    //            else if ((input[0] == '\'') && (input[input.Length - 1] == '\''))
    //            {
    //                quoteCharacter = '\'';
    //            }
    //            else
    //            {
    //                return input;
    //            }

    //            var nextQuotePosition = NextNonEscapedCharacter(input, quoteCharacter, 1);
    //            if (nextQuotePosition != (input.Length - 1))
    //            {
    //                return input;
    //            }

    //            return UnEscapeCharacter(quoteCharacter, input.Substring(1, input.Length - 2));
    //        }

    //        /// <summary>
    //        ///     Removes quotes from a string only if the first and last characters are quotes.
    //        ///     Does not check for multiple sets of quotes
    //        /// </summary>
    //        /// <param name="input">Input string</param>
    //        /// <returns>The original string with the beginning and ending quote removed, if applicable</returns>
    //        public string RemoveContainingQuotesWithoutUnEscaping(string input)
    //        {
    //            if (input.Length <= 1)
    //            {
    //                return input;
    //            }

    //            var firstChar = input[0];
    //            var lastChar = input[input.Length - 1];

    //            if ((firstChar != lastChar) || ((firstChar != '"') && (firstChar != '\'')))
    //            {
    //                return input;
    //            }
    //            return input.Substring(1, input.Length - 2);
    //        }

    //        /// <summary>
    //        ///     Adds quotes to the beginning and end of a string if it contains a space or tab
    //        /// </summary>
    //        /// <param name="input">Input string to quote</param>
    //        /// <returns>The original string if no quoting is not necessary, or the string with quotes around it</returns>
    //        public string QuoteStringIfNecessary(string input)
    //        {
    //            if (input.IndexOfAny(CharactersThatRequireQuoting) == -1)
    //            {
    //                return input;
    //            }
    //            return '"' + input + '"';
    //        }

    //        /// <summary>
    //        ///     Combines an array of strings into a single string, putting quotes around any input string that contains a space or
    //        ///     tab.
    //        /// </summary>
    //        /// <param name="arguments">A array of strings</param>
    //        /// <returns>A single string with a space separating the possibly-quoted input strings.</returns>
    //        public string QuoteArgumentsAndCombine(string[] arguments)
    //        {
    //            if ((arguments == null) || (arguments.Length == 0))
    //            {
    //                return "";
    //            }

    //            var sb = new StringBuilder(1024);
    //            sb.Append(QuoteStringIfNecessary(arguments[0]));
    //            for (var i = 1; i < arguments.Length; ++i)
    //            {
    //                sb.Append(' ');
    //                sb.Append(QuoteStringIfNecessary(arguments[i]));
    //            }

    //            return sb.ToString();
    //        }

    //        /// <summary>
    //        ///     Combines an array of strings into a single string. Does not quote arguments that may need it. USE WITH CAUTION!
    //        /// </summary>
    //        /// <param name="arguments">A array of strings</param>
    //        /// <returns>A single string containing all of the arguments separated by spaces.</returns>
    //        public static string CombineArgumentsWithoutQuoting(string[] arguments)
    //        {
    //            if ((arguments == null) || (arguments.Length == 0))
    //            {
    //                return "";
    //            }

    //            var sb = new StringBuilder(1024);
    //            sb.Append(arguments[0]);
    //            for (var i = 1; i < arguments.Length; ++i)
    //            {
    //                sb.Append(' ');
    //                sb.Append(arguments[i]);
    //            }

    //            return sb.ToString();
    //        }

    //        /// <summary>
    //        ///     Evaluates the input expression and sets value to the result
    //        /// </summary>
    //        /// <param name="input">An expression to evaluate</param>
    //        /// <param name="value">Value of the result</param>
    //        /// <returns>HRESULT</returns>
    //        public int ExpressionToUInt32(string input, out uint value)
    //        {
    //            var convertedValue = new DEBUG_VALUE();


    //            var hr = DebugControl.EvaluateWide(input, DEBUG_VALUE_TYPE.INT32, out convertedValue, null);
    //            value = SUCCEEDED(hr) ? convertedValue.I32 : 0U;
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Evaluates the input expression and sets value to the result
    //        /// </summary>
    //        /// <param name="input">An expression to evaluate</param>
    //        /// <param name="value">Value of the result</param>
    //        /// <returns>HRESULT</returns>
    //        public int ExpressionToInt32(string input, out int value)
    //        {
    //            var convertedValue = new DEBUG_VALUE();

    //            var hr = DebugControl.EvaluateWide(input, DEBUG_VALUE_TYPE.INT32, out convertedValue, null);
    //            value = SUCCEEDED(hr) ? (int)convertedValue.I32 : 0;
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Evaluates the input expression and sets value to the result
    //        /// </summary>
    //        /// <param name="input">An expression to evaluate</param>
    //        /// <param name="value">Value of the result</param>
    //        /// <returns>HRESULT</returns>
    //        public int ExpressionToUInt64(string input, out ulong value)
    //        {
    //            var convertedValue = new DEBUG_VALUE();

    //            var hr = DebugControl.EvaluateWide(input, DEBUG_VALUE_TYPE.INT64, out convertedValue, null);
    //            value = SUCCEEDED(hr) ? convertedValue.I64 : 0UL;
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Evaluates the input expression and sets value to the result
    //        /// </summary>
    //        /// <param name="input">An expression to evaluate</param>
    //        /// <param name="value">Value of the result</param>
    //        /// <returns>HRESULT</returns>
    //        public int ExpressionToInt64(string input, out long value)
    //        {
    //            var convertedValue = new DEBUG_VALUE();

    //            var hr = DebugControl.EvaluateWide(input, DEBUG_VALUE_TYPE.INT64, out convertedValue, null);
    //            value = SUCCEEDED(hr) ? (long)convertedValue.I64 : 0L;
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Evaluates the input expression and sets value to the result, sign extending if necessary on a 32-bit system
    //        /// </summary>
    //        /// <param name="input">An expression to evaluate</param>
    //        /// <param name="value">Value of the result</param>
    //        /// <returns>HRESULT</returns>
    //        public int ExpressionToPointer(string input, out ulong value)
    //        {
    //            var convertedValue = new DEBUG_VALUE();

    //            var hr = DebugControl.EvaluateWide(input, DEBUG_VALUE_TYPE.INT64, out convertedValue, null);
    //            if (SUCCEEDED(hr))
    //            {
    //                value = IsPointer64Bit() ? convertedValue.I64 : SignExtendAddress(convertedValue.I64);
    //            }
    //            else
    //            {
    //#if INTERNAL
    //                SetStatusBar("ExpressionToPointer: Error Resolving expression: " + input);
    //#endif
    //                value = 0UL;
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Resets the internal Break status
    //        /// </summary>
    //        /// <returns>HRESULT</returns>
    //        public int ResetBreak()
    //        {
    //            if (MexFrameworkClass.BypassReset == true)
    //            {
    //                MexFrameworkClass.BypassReset = false;
    //                return S_OK;
    //            }
    //                DebugControl.GetInterrupt();
    //                BreakStatus = false;
    //                return S_OK;
    //        }

    //        /// <summary>
    //        ///     Resets the internal Break status
    //        /// </summary>
    //        /// <returns>HRESULT</returns>
    //        public static int ResetBreak(IDebugClient debugClient)
    //        {
    //            if (MexFrameworkClass.BypassReset == true)
    //            {
    //                MexFrameworkClass.BypassReset = false;
    //                return S_OK;
    //            }

    //                ((IDebugControl)debugClient).GetInterrupt();
    //                BreakStatus = false;
    //                return S_OK;
    //        }

    //        public static bool ShouldBreak(DebugUtilities d, bool slow = false, bool retrigger = false, bool threadSafe = false)
    //        {
    //            if (threadSafe || d == null || d._myThread != Thread.CurrentThread)
    //            {
    //                return BreakStatus;
    //            }
    //            if (BreakStatus == true)
    //            {
    //                if (retrigger)
    //                {
    //                    d.TriggerBreak();
    //                }

    //                return BreakStatus;
    //            }

    //            if (slow || shouldBreakCount > 50)
    //            {
    //                BreakStatus = (d.DebugControl.GetInterrupt() == S_OK);
    //                shouldBreakCount = 0;
    //            }
    //            else
    //            {
    //                // Now set in SetInternalBreakStatus for WinDbg
    //                if (!BreakStatus && !MexFrameworkClass.RichEditInitialized)
    //                {
    //                    BreakStatus = (d.DebugControl.GetInterrupt() == S_OK);
    //                }
    //                else
    //                {
    //                    shouldBreakCount = shouldBreakCount++;
    //                }
    //            }
    //            return BreakStatus;
    //        }

    //        /// <summary>
    //        ///     Detects if Ctrl+C or the Stop button has been pressed.
    //        /// </summary>
    //        /// <returns>True if the a break has been triggered, otherwise false.</returns>
    //        public bool ShouldBreak(bool slow = false, bool retrigger = false, bool threadSafe = false)
    //        {
    //           return ShouldBreak(this, slow, retrigger, threadSafe );
    //        }

    //        /// <summary>
    //        ///     Triggers a break using DEBUG_INTERRUPT_PASSIVE
    //        /// </summary>
    //        public void TriggerBreak()
    //        {
    //                DebugControl.SetInterrupt(DEBUG_INTERRUPT.PASSIVE);
    //                BreakStatus = true;
    //        }

    //        /// <summary>
    //        ///     Triggers a break using DEBUG_INTERRUPT_PASSIVE
    //        /// </summary>
    //        public static void SetInternalBreakStatus()
    //        {
    //                BreakStatus = true;
    //        }

    //        /// <summary>
    //        ///     Sign-extends a 32-bit address to 64-bits
    //        /// </summary>
    //        /// <param name="address">Address to extend</param>
    //        /// <returns>Sign extended address</returns>
    //        public ulong SignExtendAddress(uint address)
    //        {
    //            return (ulong)(int)address;
    //        }

    //        /// <summary>
    //        ///     Sign-extends a 32-bit address to 64-bits
    //        /// </summary>
    //        /// <param name="address">Address to extend</param>
    //        /// <returns>Sign extended address</returns>
    //        public ulong SignExtendAddress(ulong address)
    //        {
    //            return (ulong)(int)address;
    //        }

    //        /// <summary>
    //        ///     Changes to a new thread, pushing the previous thread onto a stack
    //        /// </summary>
    //        /// <param name="newThread">The thread to change to</param>
    //        /// <returns>HRESULT</returns>
    //        public int PushThread(ulong newThread)
    //        {
    //            if (DumpInfo.IsKernelMode)
    //            {
    //                ulong oldThread;
    //                var hr = DebugSystemObjects.GetImplicitThreadDataOffset(out oldThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed getting the current thread: {0:x8}", hr);
    //                }
    //                else
    //                {
    //                    _threadStack.Push(oldThread);
    //                }
    //                hr = DebugSystemObjects.SetImplicitThreadDataOffset(newThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed changing to the requested thread ({0}): {1:x8}", P2S(newThread), hr);
    //                }
    //                return hr;
    //            }
    //            else
    //            {
    //                uint oldThread;
    //                var hr = DebugSystemObjects.GetCurrentThreadId(out oldThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed getting the current thread: {0:x8}", hr);
    //                }
    //                else
    //                {
    //                    _threadStack.Push(oldThread);
    //                }
    //                hr = DebugSystemObjects.SetCurrentThreadId((uint)newThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed changing to the requested thread ({0}): {1:x8}", newThread, hr);
    //                }
    //                return hr;
    //            }
    //        }

    //        /// <summary>
    //        ///     Pushes the current thread onto the thread stack without changing threads.
    //        ///     Useful when about to change to lots of threads.
    //        /// </summary>
    //        /// <returns>HRESULT</returns>
    //        public int PushThread()
    //        {
    //            if (DumpInfo.IsKernelMode)
    //            {
    //                ulong oldThread;
    //                var hr = DebugSystemObjects.GetImplicitThreadDataOffset(out oldThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed getting the current thread: {0:x8}", hr);
    //                }
    //                else
    //                {
    //                    _threadStack.Push(oldThread);
    //                }
    //                return hr;
    //            }
    //            else
    //            {
    //                uint oldThread;
    //                var hr = DebugSystemObjects.GetCurrentThreadId(out oldThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed getting the current thread: {0:x8}", hr);
    //                }
    //                else
    //                {
    //                    _threadStack.Push(oldThread);
    //                }
    //                return hr;
    //            }
    //        }

    //        /// <summary>
    //        ///     Change to the last thread pushed onto the stack by a call to PushThread
    //        /// </summary>
    //        /// <returns>HRESULT</returns>
    //        public int PopThread()
    //        {
    //            return PopThread(null);
    //        }

    //        /// <summary>
    //        ///     Change to the last thread pushed onto the stack by a call to PushThread
    //        /// </summary>
    //        /// <param name="newThread">The thread that was just made current, or 0 if a failure occurred</param>
    //        /// <returns>HRESULT</returns>
    //        public int PopThread(ulong* newThread)
    //        {
    //            ulong threadToChangeTo;
    //            try
    //            {
    //                threadToChangeTo = _threadStack.Pop();
    //            }
    //            catch (Exception e)
    //            {
    //                if (newThread != null)
    //                {
    //                    *newThread = 0;
    //                }
    //                OutputVerboseLine("DotNetDbg: PopThread failed retrieving a thread off the stack: {0}", e.Message);
    //                return E_FAIL;
    //            }

    //            var hr = ChangeToThread(threadToChangeTo);
    //            if (newThread != null)
    //            {
    //                *newThread = FAILED(hr) ? 0 : threadToChangeTo;
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Changes to a new thread
    //        /// </summary>
    //        /// <param name="newThread">The thread to change to</param>
    //        /// <returns>HRESULT</returns>
    //        public int ChangeToThread(ulong newThread)
    //        {
    //            if (DumpInfo.IsKernelMode)
    //            {
    //                var hr = DebugSystemObjects.SetImplicitThreadDataOffset(newThread);
    //                if (FAILED(hr) && (hr != unchecked((int)0xd0000147)))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed changing to the requested thread ({0}): {1:x8}", P2S(newThread), hr);
    //                }
    //                return hr;
    //            }
    //            else
    //            {
    //                var hr = DebugSystemObjects.SetCurrentThreadId((uint)newThread);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed changing to the requested thread ({0}): {1:x8}", newThread, hr);
    //                }
    //                return hr;
    //            }
    //        }

    //        /// <summary>
    //        ///     Changes to a new process, pushing the previous process onto a stack
    //        /// </summary>
    //        /// <param name="newProcess">The process to change to</param>
    //        /// <param name="reloadSymbols">Whether to reload symbols after changing processes</param>
    //        /// <param name="quiet">Suppresses any output message if true</param>
    //        /// <returns>HRESULT</returns>
    //        public int PushProcess(ulong newProcess, bool reloadSymbols = true, bool quiet = true)
    //        {
    //            if (DumpInfo.IsKernelMode)
    //            {
    //                ulong oldProcess;
    //                var hr = DebugSystemObjects.GetImplicitProcessDataOffset(out oldProcess);
    //                if (FAILED(hr))
    //                {
    //                    OutputVerboseLine("DotNetDbg: Failed getting the current process: {0:x8}", hr);
    //                }
    //                else
    //                {
    //                    _processStack.Push(oldProcess);
    //                }
    //                return ChangeToProcess(newProcess, reloadSymbols, quiet);
    //            }
    //            OutputVerboseLine("DotNetDbg: PushProcess is not a valid call for a usermode dump");
    //            return E_FAIL;
    //        }

    //        /// <summary>
    //        ///     Pushes the current process onto the process stack without changing processes.
    //        ///     Useful when about to change to lots of processes.
    //        /// </summary>
    //        /// <param name="currentProcess">The process that was pushed to the stack, or 0 if a failure occurs.</param>
    //        /// <returns>HRESULT</returns>
    //        public int PushProcess(ulong* currentProcess = null)
    //        {
    //            if (DumpInfo.IsKernelMode)
    //            {
    //                ulong oldProcess;
    //                var hr = DebugSystemObjects.GetImplicitProcessDataOffset(out oldProcess);
    //                if (FAILED(hr))
    //                {
    //                    if (currentProcess != null)
    //                    {
    //                        *currentProcess = 0;
    //                    }
    //                    OutputVerboseLine("DotNetDbg: Failed getting the current process: {0:x8}", hr);
    //                }
    //                else
    //                {
    //                    if (currentProcess != null)
    //                    {
    //                        *currentProcess = oldProcess;
    //                    }
    //                    _processStack.Push(oldProcess);
    //                }
    //                return hr;
    //            }
    //            OutputVerboseLine("DotNetDbg: PushProcess is not a valid call for a usermode dump");
    //            return E_FAIL;
    //        }

    //        /// <summary>
    //        ///     Change to the last process pushed onto the stack by a call to PushProcess
    //        /// </summary>
    //        /// <param name="newProcess">The process that was just made current, or 0 if a failure occurred</param>
    //        /// <param name="reloadSymbols">Whether to reload symbols after changing processes</param>
    //        /// <param name="quiet">Suppresses output</param>
    //        /// <returns>HRESULT</returns>
    //        public int PopProcess(ulong* newProcess = null, bool reloadSymbols = true, bool quiet = true)
    //        {
    //            if (DumpInfo.IsKernelMode == false)
    //            {
    //                OutputVerboseLine("DotNetDbg: PopProcess is not a valid call for a usermode dump");
    //                return E_FAIL;
    //            }

    //            ulong processToChangeTo;
    //            try
    //            {
    //                processToChangeTo = _processStack.Pop();
    //            }
    //            catch (Exception e)
    //            {
    //                if (newProcess != null)
    //                {
    //                    *newProcess = 0;
    //                }
    //                OutputVerboseLine("DotNetDbg: PopProcess failed retrieving a process off the stack: {0}", e.Message);
    //                return E_FAIL;
    //            }

    //            var hr = ChangeToProcess(processToChangeTo, reloadSymbols, quiet);
    //            if (newProcess != null)
    //            {
    //                *newProcess = FAILED(hr) ? 0 : processToChangeTo;
    //            }
    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Changes to a new process
    //        /// </summary>
    //        /// <param name="newProcess">The process to change to</param>
    //        /// <param name="reloadSymbols">Whether to reload symbols after changing processes</param>
    //        /// <param name="quiet">Suppresses output</param>
    //        /// <returns>HRESULT</returns>
    //        public int ChangeToProcess(ulong newProcess, bool reloadSymbols = true, bool quiet = true)
    //        {
    //            //SimpleOutputHandler outputHandler = null;
    //            AdvancedOutputHandler advancedHandler = null;
    //            DebugUtilities quietUtils = this;
    //            try
    //            {
    //                if (quiet)
    //                {
    //                    quietUtils = AdvancedOutputHandler.Install(this, out advancedHandler, new IgnoreOutputFilterAdvanced() ,InterestMask: DEBUG_OUTPUT.NONE);

    //                    //SimpleOutputHandler.Install(this, out outputHandler, new IgnoreOutputFilter(), true, true);
    //                }

    //                quietUtils.OutputLine("This is a test");

    //                if (DumpInfo.IsKernelMode)
    //                {
    //                    var hr = quietUtils.DebugSystemObjects.SetImplicitProcessDataOffset(newProcess);
    //                    if (FAILED(hr))
    //                    {
    //                        quietUtils.OutputVerboseLine("DotNetDbg: Failed changing to the requested process ({0}): {1:x8}", quietUtils.P2S(newProcess), hr);
    //                    }
    //                    else if (reloadSymbols)
    //                    {
    //                        quietUtils.ReloadSymbols("/user", quiet && (advancedHandler == null));
    //                    }
    //                    return hr;
    //                }
    //                else
    //                {
    //                    quietUtils.OutputVerboseLine("DotNetDbg: ChangeToProcess is not a valid call for a usermode dump");
    //                    return E_FAIL;
    //                }
    //            }
    //            finally
    //            {
    //                if (advancedHandler != null)
    //                {
    //                    advancedHandler.Dispose();
    //                    advancedHandler = null;
    //                }
    //            }
    //        }

    //        /// <summary>
    //        ///     Reloads symbols. See help for IDebugSymbols for module syntax but it exactly mirrors .reload. E.G. specify "/f
    //        ///     module.dll" to force a reload for a module
    //        /// </summary>
    //        /// <param name="module">
    //        ///     Module to reload. Doesn't necessarily have to be a module, can specify /user, etc, just like
    //        ///     .reload
    //        /// </param>
    //        /// <param name="quiet">Whether to silence the output</param>
    //        /// <returns>HRESULT</returns>
    //        public int ReloadSymbols(string module, bool quiet = true)
    //        {
    //            if (quiet)
    //            {
    //                AdvancedOutputHandler outputHandler;
    //                DebugUtilities executionUtilities;
    //                executionUtilities = AdvancedOutputHandler.Install(this,  out outputHandler, new IgnoreOutputFilterAdvanced(), InterestMask: DEBUG_OUTPUT.NONE);
    //                int hr = S_OK;
    //                if ((outputHandler != null))
    //                {
    //                    using (outputHandler)
    //                    {
    //                        hr = executionUtilities.DebugSymbols.ReloadWide(module);
    //                        outputHandler.Dispose();
    //                        return hr;
    //                    }
    //                }
    //                return DebugSymbols.ReloadWide(module);
    //            }
    //            return DebugSymbols.ReloadWide(module);
    //        }

    //        /// <summary>
    //        ///     Tries to determine if the debug connection is live.
    //        ///     If an error occurs TRUE is returned by default.
    //        /// </summary>
    //        public bool DebuggingLive()
    //        {
    //            DEBUG_CLASS debugClass;
    //            DEBUG_CLASS_QUALIFIER debugClassQualifier;

    //            var hr = DebugControl.GetDebuggeeType(out debugClass, out debugClassQualifier);
    //            if (FAILED(hr))
    //            {
    //                OutputVerboseLine("WARNING! DebugUtilities.DebuggingLive returning TRUE because IDebugControl::GetDebuggeeType failed: {0:x8}", hr);
    //                return true;
    //            }

    //            return (debugClassQualifier < DEBUG_CLASS_QUALIFIER.KERNEL_SMALL_DUMP);
    //        }

    //        /// <summary>
    //        ///     Tries to determine if full memory is available.
    //        ///     If an error occurs TRUE is returned by default, functions should perform error checking on all reads.
    //        /// </summary>
    //        public bool FullMemoryAvailable()
    //        {
    //            DEBUG_CLASS debugClass;
    //            DEBUG_CLASS_QUALIFIER debugClassQualifier;

    //            var hr = DebugControl.GetDebuggeeType(out debugClass, out debugClassQualifier);
    //            if (FAILED(hr))
    //            {
    //                OutputVerboseLine("WARNING! DebugUtilities.FullMemoryAvailable returning TRUE because IDebugControl::GetDebuggeeType failed: {0:x8}", hr);
    //                return true;
    //            }

    //            DEBUG_FORMAT debugFormat;
    //            hr = DebugControl.GetDumpFormatFlags(out debugFormat);
    //            if (FAILED(hr))
    //            {
    //                OutputVerboseLine("WARNING! DebugUtilities.FullMemoryAvailable returning TRUE because IDebugControl2::GetDumpFormatFlags failed: {0:x8}", hr);
    //                return true;
    //            }

    //            return ((debugFormat & (DEBUG_FORMAT.USER_SMALL_FULL_MEMORY | DEBUG_FORMAT.USER_SMALL_FULL_MEMORY_INFO)) != 0);
    //        }

    //        /// <summary>
    //        ///     Gets the comment for the dump
    //        /// </summary>
    //        public string GetComment(bool bVerbose)
    //        {
    //            var szOut = string.Empty;
    //            var buffer = IntPtr.Zero;
    //            buffer = Marshal.AllocHGlobal(1024*1024);
    //            DEBUG_READ_USER_MINIDUMP_STREAM strm;

    //            strm.StreamType = MINIDUMP_STREAM_TYPE.CommentStreamW;
    //            strm.Flags = 0;
    //            strm.Offset = 0;
    //            strm.Buffer = buffer;
    //            strm.BufferSize = 1024*1024;
    //            strm.BufferUsed = 0;
    //            try
    //            {
    //                var bCommentFound = false;
    //                var hr = DebugAdvanced.Request(DEBUG_REQUEST.READ_USER_MINIDUMP_STREAM, &strm, sizeof(DEBUG_READ_USER_MINIDUMP_STREAM), null, 0, null);
    //                if (SUCCEEDED(hr))
    //                {
    //                    szOut += "Comment: '";
    //                    szOut += Marshal.PtrToStringUni(buffer, (int)strm.BufferUsed/2);
    //                    if (!bVerbose)
    //                    {
    //                        var szIndex = szOut.IndexOf('\0');
    //                        if (szIndex > 0)
    //                        {
    //                            szOut = szOut.Remove(szIndex);
    //                        }
    //                    }
    //                    szOut = szOut.TrimEnd('\0');
    //                    szOut += "'";
    //                    bCommentFound = true;
    //                }

    //                strm.StreamType = MINIDUMP_STREAM_TYPE.CommentStreamA;
    //                hr = DebugAdvanced.Request(DEBUG_REQUEST.READ_USER_MINIDUMP_STREAM, &strm, sizeof(DEBUG_READ_USER_MINIDUMP_STREAM), null, 0, null);
    //                if (SUCCEEDED(hr))
    //                {
    //                    if (bCommentFound)
    //                    {
    //                        szOut += "\n";
    //                    }
    //                    szOut += "Comment: '";
    //                    szOut += Marshal.PtrToStringAnsi(buffer, (int)strm.BufferUsed);
    //                    if (!bVerbose)
    //                    {
    //                        var szIndex = szOut.IndexOf('\0');
    //                        if (szIndex > 0)
    //                        {
    //                            szOut = szOut.Remove(szIndex);
    //                        }
    //                    }
    //                    szOut = szOut.TrimEnd('\0');
    //                    szOut += "'";
    //                }
    //                if (bVerbose)
    //                {
    //                    szOut = szOut.Replace('\0', '.');
    //                }
    //                return szOut;
    //            }
    //            finally
    //            {
    //                if (buffer != IntPtr.Zero)
    //                {
    //                    Marshal.FreeHGlobal(buffer);
    //                    buffer = IntPtr.Zero;
    //                }
    //            }
    //        }

    //        /// <summary>
    //        ///     Gets the extension path in use by the debugger.
    //        ///     If an error occurs the return value is an empty string.
    //        /// </summary>
    //        public string GetExtensionPath()
    //        {
    //            var buffer = IntPtr.Zero;
    //            try
    //            {
    //                int outSize;
    //                var hr = DebugAdvanced.Request(DEBUG_REQUEST.GET_EXTENSION_SEARCH_PATH_WIDE, null, 0, null, 0, &outSize);
    //                if (FAILED(hr))
    //                {
    //                    return string.Empty;
    //                }

    //                outSize += 16;
    //                buffer = Marshal.AllocHGlobal(outSize);

    //                hr = DebugAdvanced.Request(DEBUG_REQUEST.GET_EXTENSION_SEARCH_PATH_WIDE, null, 0, buffer.ToPointer(), outSize, &outSize);
    //                if (FAILED(hr))
    //                {
    //                    return string.Empty;
    //                }

    //                return Marshal.PtrToStringUni(buffer);
    //            }
    //            finally
    //            {
    //                if (buffer != IntPtr.Zero)
    //                {
    //                    Marshal.FreeHGlobal(buffer);
    //                    buffer = IntPtr.Zero;
    //                }
    //            }
    //        }

    //        /// <summary>
    //        ///     Gets a stack trace using the new, inline aware API when available.
    //        /// </summary>
    //        public int GetStackTrace(out DEBUG_STACK_FRAME_EX[] stack, uint MaxFrames = 1024)
    //        {
    //            int hr;
    //            uint numFrames;
    //            stack = null;
    //            var allframes = false;

    //            if (MaxFrames == 0)
    //            {
    //                MaxFrames = 1024;
    //                allframes = true;
    //            }
    //            if (DebugControl6 != null)
    //            {
    //                var tempStackEx = new DEBUG_STACK_FRAME_EX[MaxFrames]; // Will there ever be more then 1k stack frames.. Lets hope not.

    //                hr = DebugControl6.GetStackTraceEx(0, 0, 0, tempStackEx, tempStackEx.Length, &numFrames);
    //                if (SUCCEEDED(hr))
    //                {
    //                    if (allframes == true && numFrames == MaxFrames)
    //                    {
    //                        MaxFrames = 4096;
    //                        tempStackEx = new DEBUG_STACK_FRAME_EX[MaxFrames];
    //                        hr = DebugControl6.GetStackTraceEx(0, 0, 0, tempStackEx, tempStackEx.Length, &numFrames);
    //                    }

    //                    stack = new DEBUG_STACK_FRAME_EX[numFrames];
    //                    if (numFrames > 0)
    //                    {
    //                        Array.Copy(tempStackEx, stack, numFrames);
    //                    }
    //                    return hr;
    //                }
    //            }

    //            var tempStack = new DEBUG_STACK_FRAME[MaxFrames]; // Will there ever be more then 4k stack frames.. Lets hope not.

    //            hr = DebugControl.GetStackTrace(0, 0, 0, tempStack, tempStack.Length, &numFrames);
    //            if (SUCCEEDED(hr))
    //            {
    //                stack = new DEBUG_STACK_FRAME_EX[numFrames];
    //                for (var i = 0; i < numFrames; i++)
    //                {
    //                    stack[i] = new DEBUG_STACK_FRAME_EX(tempStack[i]);
    //                }
    //            }

    //            return hr;
    //        }

    //        /// <summary>
    //        ///     Installs an ignore output filter. THIS MUST BE WRAPPED IN A USING CALL!!!
    //        /// </summary>
    //        public SimpleOutputHandler InstallIgnoreFilter_WRAP_WITH_USING()
    //        {
    //            SimpleOutputHandler outputHandler;
    //            SimpleOutputHandler.Install(this, out outputHandler, new IgnoreOutputFilter(), true, true);
    //            return outputHandler;
    //        }

    //        /// <summary>
    //        ///     Installs an ignore output filter. THIS MUST BE WRAPPED IN A USING CALL!!!
    //        /// </summary>
    //        public SimpleOutputHandler2 InstallIgnoreFilter2_WRAP_WITH_USING(out DebugUtilities executionUtilities)
    //        {
    //            SimpleOutputHandler2 outputHandler;
    //            SimpleOutputHandler2.Install(this, out executionUtilities, out outputHandler, new IgnoreOutputFilter2(), true, true, true);
    //            return outputHandler;
    //        }

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms679351(v=vs.85).aspx
    //        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, CharSet = CharSet.Unicode)]
    //        private static extern uint FormatMessage(FORMAT_MESSAGE dwFlags, IntPtr lpSource, uint dwMessageId, uint dwLanguageId, IntPtr* lpBuffer, int nSize, IntPtr Arguments_ShouldBeNull);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms684175(v=vs.85).aspx
    //        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, CharSet = CharSet.Unicode)]
    //        private static extern IntPtr LoadLibrary([In, MarshalAs(UnmanagedType.LPWStr)] string lpFileName);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms683152(v=vs.85).aspx
    //        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, CharSet = CharSet.Unicode)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        private static extern bool FreeLibrary(IntPtr hModule);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/aa366730(v=vs.85).aspx
    //        [DllImport("Kernel32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, CharSet = CharSet.Unicode)]
    //        private static extern IntPtr LocalFree(IntPtr hMem);

    //        /// <summary>
    //        ///     Converts an NT_STATUS code to a string
    //        /// </summary>
    //        public string NtStatusToString(uint ntStatus)
    //        {
    //            var hNtdll = LoadLibrary("ntdll.dll");
    //            var lpMessageBuffer = IntPtr.Zero;

    //            FormatMessage(FORMAT_MESSAGE.ALLOCATE_BUFFER | FORMAT_MESSAGE.FROM_SYSTEM | FORMAT_MESSAGE.FROM_HMODULE, hNtdll, ntStatus, 0, &lpMessageBuffer, 0, IntPtr.Zero);

    //            var retValue = (lpMessageBuffer == IntPtr.Zero) ? string.Empty : Marshal.PtrToStringUni(lpMessageBuffer);

    //            if (lpMessageBuffer != IntPtr.Zero)
    //            {
    //                LocalFree(lpMessageBuffer);
    //            }

    //            return retValue;
    //        }

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633495(v=vs.85).aspx
    //        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = false)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        public static extern bool EnumThreadWindows([In] uint dwThreadId, [In, MarshalAs(UnmanagedType.FunctionPtr)] EnumThreadWndProc lpfn, IntPtr lParam);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633494(v=vs.85).aspx
    //        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = false)]
    //        [return: MarshalAs(UnmanagedType.Bool)]
    //        public static extern bool EnumChildWindows([In] IntPtr hWndParent, [In, MarshalAs(UnmanagedType.FunctionPtr)] EnumThreadWndProc lpfn, IntPtr lParam);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633582(v=vs.85).aspx
    //        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, CharSet = CharSet.Unicode)]
    //        public static extern int GetClassName([In] IntPtr hWnd, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpClassName, [In] int nMaxCount);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633510(v=vs.85).aspx
    //        [DllImport("user32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, CharSet = CharSet.Unicode)]
    //        public static extern IntPtr GetParent([In] IntPtr hWnd);


    //#if INTERNAL
    //        private static bool FindWindbgProc(IntPtr hWnd, IntPtr lParam)
    //        {
    //            var sb = new StringBuilder(1024);
    //            var continueOn = string.IsNullOrEmpty(_findWindbgWindowClassName); //Sometimes someone may not want to specify a class name
    //            if (!continueOn)
    //            {
    //                var retval = GetClassName(hWnd, sb, sb.Capacity);
    //                continueOn = ((retval != 0) && (sb.ToString() == _findWindbgWindowClassName));
    //            }
    //            if (continueOn)
    //            {
    //                if (string.IsNullOrEmpty(_findWindbgWindowParentCaption))
    //                {
    //                    _findWindbgWindowFoundWindows.Add(hWnd);
    //                    return true;
    //                }

    //                var hwndParent = GetParent(hWnd);
    //                GetWindowText(hwndParent, sb, sb.Capacity);

    //                var parentNameMatch = sb.ToString().StartsWith(_findWindbgWindowParentCaption, StringComparison.OrdinalIgnoreCase);
    //                if (parentNameMatch != _findWindbgWindowParentNot)
    //                {
    //                    _findWindbgWindowFoundWindows.Add(hWnd);
    //                }

    //                return true;
    //            }
    //            EnumChildWindows(hWnd, FindWindbgProc, IntPtr.Zero);
    //            return true;
    //        }

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633584(v=vs.85).aspx
    //        [DllImport("User32.dll", CallingConvention = CallingConvention.Winapi, PreserveSig = true, SetLastError = true, EntryPoint = "GetWindowLong")]
    //        public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633520(v=vs.85).aspx
    //        [DllImport("user32", SetLastError = true, CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode)]
    //        public static extern int GetWindowText(
    //            IntPtr hwnd,
    //            StringBuilder lpString,
    //            int nMaxCount
    //            );

    //        public static IntPtr[] FindWindbgWindow(string className)
    //        {
    //            return FindWindbgWindow(className, null);
    //        }

    //        public static IntPtr[] FindWindbgWindow(string className, string parentWindowCaption, bool parentNot = false)
    //        {
    //            lock (FindWindbgWindowLock)
    //            {
    //                _findWindbgWindowClassName = className;
    //                _findWindbgWindowParentCaption = parentWindowCaption;
    //                _findWindbgWindowFoundWindows = new HashSet<IntPtr>();
    //                _findWindbgWindowParentNot = parentNot;

    //                var currentProcess = Process.GetCurrentProcess();
    //                for (var i = 0; i < currentProcess.Threads.Count; ++i)
    //                {
    //                    using (var thread = currentProcess.Threads[i])
    //                    {
    //                        var id = (uint)thread.Id;

    //                        EnumThreadWindows(id, FindWindbgProc, IntPtr.Zero);
    //                    }
    //                }

    //                var foundArray = new IntPtr[_findWindbgWindowFoundWindows.Count];

    //                _findWindbgWindowFoundWindows.CopyTo(foundArray);

    //                return foundArray;
    //            }
    //        }
    //#endif
    //        public ulong KeTickCount()
    //        {
    //            ulong keTickCount = 0;

    //            if (SUCCEEDED(ReadGlobalAsUInt64("nt", "KeTickCount", out keTickCount)))
    //            {
    //                OutputDebugLine("KeTickCount from nt!KeTickCount = 0x{0:x}", keTickCount);
    //                return keTickCount;
    //            }
    //            ulong sharedUserData = 0;

    //            DebugDataSpaces.ReadDebuggerData(RDD_DEBUG_DATA.SharedUserData, ((IntPtr)(&sharedUserData)), sizeof(ulong), null);

    //            OutputVerboseLine("Shared User Date Pointer = {0:x}", sharedUserData);
    //            GetFieldValue("nt", "_KUSER_SHARED_DATA", "TickCountQuad", sharedUserData, out keTickCount);
    //            OutputDebugLine("KeTickCount from nt!_KUSER_SHARED_DATA TickCountQuad {0:x} = 0x{1:x}", sharedUserData, keTickCount);
    //            return keTickCount;
    //        }
    //#if INTERNAL
    //        private static IntPtr FindWindbgRicheditCommandWindow(bool leftmostStart) /* rightmost if false */
    //        {
    //            var windows = FindWindbgWindow("RICHEDIT50W", "Command");
    //            if (windows.Length != 0)
    //            {
    //                if (windows.Length == 1)
    //                {
    //                    return windows[0];
    //                }
    //                /* Both the command output and the command input are richedit windows with the same parent, need to go based on coords */
    //                RECT desiredRect;
    //                var desiredIndex = 0;
    //                GetWindowRect(windows[0], &desiredRect);

    //                for (var i = 1; i < windows.Length; ++i)
    //                {
    //                    RECT windowRect;
    //                    GetWindowRect(windows[i], &windowRect);
    //                    if (leftmostStart ? (windowRect.left < desiredRect.left) : (windowRect.left > desiredRect.left))
    //                    {
    //                        desiredRect = windowRect;
    //                        desiredIndex = i;
    //                    }
    //                }
    //                return windows[desiredIndex];
    //            }
    //            return IntPtr.Zero;
    //        }

    //        public static List<IntPtr> FindWindbgSourceWindows()
    //        {
    //            var windows = new List<IntPtr>();

    //            var arr = FindWindbgWindow("RICHEDIT50W", "Command", true);

    //            windows.AddRange(arr);

    //            return windows;
    //        }

    //        public static IntPtr FindWindbgCommandOutputWindow()
    //        {
    //            if (_windbgCommandOutputWindowHandle == IntPtr.Zero)
    //            {
    //                _windbgCommandOutputWindowHandle = FindWindbgRicheditCommandWindow(true);
    //            }
    //            return _windbgCommandOutputWindowHandle;
    //        }

    //        public static IntPtr FindWindbgCommandInputWindow()
    //        {
    //            if (_windbgCommandInputWindowHandle == IntPtr.Zero)
    //            {
    //                _windbgCommandInputWindowHandle = FindWindbgRicheditCommandWindow(false);
    //            }
    //            return _windbgCommandInputWindowHandle;
    //        }

    //        private static IntPtr WindbgStatusBarWindowHandle;
    //        public static IntPtr FindWindbgStatusBarWindow()
    //        {
    //            if (WindbgStatusBarWindowHandle == IntPtr.Zero)
    //            {
    //                IntPtr[] windows = FindWindbgWindow("msctls_statusbar32");
    //                if (windows.Length != 0) WindbgStatusBarWindowHandle = windows[0];
    //            }
    //            return WindbgStatusBarWindowHandle;
    //        }
    //#endif
    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms644950(v=vs.85).aspx
    //        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage", CallingConvention = CallingConvention.Winapi)]
    //        private static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms644950(v=vs.85).aspx
    //        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage", CallingConvention = CallingConvention.Winapi)]
    //        private static extern int SendMessage(IntPtr hWnd, uint msg, uint wParam, uint lParam);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms644950(v=vs.85).aspx
    //        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage", CallingConvention = CallingConvention.Winapi)]
    //        private static extern int SendMessageAnsi(IntPtr hWnd, uint msg, IntPtr wParam, [In, MarshalAs(UnmanagedType.LPStr)] string lParam);

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms644950(v=vs.85).aspx
    //        [DllImport("User32.dll", SetLastError = true, EntryPoint = "SendMessage", CallingConvention = CallingConvention.Winapi)]
    //        private static extern int SendMessageAnsi(IntPtr hWnd, uint msg, IntPtr wParam, [Out, MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam);


    //        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessageTimeout", CallingConvention = CallingConvention.Winapi)]
    //        public static extern int SendMessageAnsiTimeout(
    //              IntPtr hwnd,
    //              uint Msg,
    //              IntPtr wParam,
    //              [In, MarshalAs(UnmanagedType.LPStr)] string lParam,
    //              uint fuFlags,
    //              uint uTimeout,
    //              out int lpdwResult
    //         );

    //        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SendMessageTimeout", CallingConvention = CallingConvention.Winapi)]
    //        public static extern int SendMessageAnsiTimeout(
    //             IntPtr hwnd,
    //             uint Msg,
    //             IntPtr wParam,
    //             [In, MarshalAs(UnmanagedType.LPStr)] StringBuilder lParam,
    //             uint fuFlags,
    //             uint uTimeout,
    //             out int lpdwResult
    //        );

    //        // - Publically Documented API: http://msdn.microsoft.com/en-us/library/windows/desktop/ms633519(v=vs.85).aspx
    //        [DllImport("User32.dll", SetLastError = true, CallingConvention = CallingConvention.Winapi)]
    //        private static extern int GetWindowRect(IntPtr hWnd, RECT* lpRect);

    //        public static int SetWindowTextAnsi(IntPtr hWnd, string text)
    //        {
    //            if ((hWnd != IntPtr.Zero) && (!MexFrameworkClass.SuppressMessages))
    //            {
    //                int result = 0;
    //                return SendMessageAnsiTimeout(hWnd, WM_SETTEXT, IntPtr.Zero, text, 0x2 /*SMTO_ABORTIFHUNG*/, 1, out result);
    //            }
    //            return 0;
    //        }
    //#if INTERNAL
    //        public void RefreshStatusBar()
    //        {
    //            SetStatusBarImpl(_lastStatusText);
    //        }

    //        private void SetStatusBarImpl(string text)
    //        {
    //            if (System.Diagnostics.Debugger.IsAttached)
    //            {
    //                return;
    //            }
    //            var textNew = text;
    //            lock (_appendStatusText)
    //            {
    //                if (!string.IsNullOrEmpty(_appendStatusText))
    //                {
    //                    textNew = " [" + _appendStatusText + "]" + textNew;
    //                }
    //            }
    //            _refreshStatusText = false;
    //            if (textNew.Contains("\n"))
    //            {
    //                textNew = textNew.Replace("\n", string.Empty);
    //            }
    //            if (!string.IsNullOrEmpty(textNew))
    //            {
    //                IntPtr hStatusBar = FindWindbgStatusBarWindow();
    //                if (hStatusBar == IntPtr.Zero)
    //                {
    //                    return;
    //                }
    //                int result = 0;
    //                SendMessageAnsiTimeout(hStatusBar, WM_SETTEXT, IntPtr.Zero, textNew, 0x2 /*SMTO_ABORTIFHUNG*/, 1 , out result);
    //            }
    //        }

    //        public void SetStatusBar(string text, bool force = false, bool includeLastText = false)
    //        {
    //            if (!IsWindbg)
    //            {
    //                return;
    //            }
    //            if (ShouldBreak())
    //            {
    //                try
    //                {
    //                    //ControlledOutputWideImpl(DEBUG_OUTCTL.AMBIENT_TEXT, DEBUG_OUTPUT.STATUS, "***BREAK***"); // Can Crash here on unload..
    //                    DebugClient.FlushCallbacks();
    //                }
    //                catch
    //                {
    //                    return;
    //                }        
    //                return;
    //            }

    //            force = (force || _refreshStatusText);

    //            StatusBarStopWatch.Stop();
    //            if (StatusBarStopWatch.ElapsedMilliseconds > 100 || force)
    //            {
    //                if (text != _lastStatusText || force)
    //                {
    //                    var textNew = text;
    //                    if (includeLastText)
    //                    {
    //                        if (_lastStatusText.Length + textNew.Length < 80)
    //                        {
    //                            textNew = text + " [" + _lastStatusText + "]";
    //                        }
    //                    }

    //                    _lastStatusText = text;
    //                    SetStatusBarImpl(textNew);
    //                    StatusBarStopWatch.Reset();
    //                }
    //            }
    //            else
    //            {
    //                _lastStatusText = text;
    //            }
    //            StatusBarStopWatch.Start();
    //        }

    //        public void AppendStatusBar(string text)
    //        {
    //            _refreshStatusText = true;
    //            if (text == null)
    //            {
    //                text = string.Empty;
    //            } 
    //            lock (_appendStatusText)
    //            {
    //                _appendStatusText = text;
    //            }
    //            _refreshStatusText = true;
    //        }

    //        public static void SendWindowEnter(IntPtr hWnd)
    //        {
    //            /*
    //				WM_KEYDOWN = 0x100
    //				WM_KEYUP   = 0x101
    //				VK_RETURN  = 0x00D
    //			*/
    //            SendMessage(hWnd, 0x100U, 0x0DU, 0U);
    //            SendMessage(hWnd, 0x101U, 0x0DU, 0xc0000001U);
    //        }

    //        /// <summary>
    //        ///     Runs a message loop until the current thread's queue is empty.
    //        ///     This is useful, for instance, if you need to wait for a bunch
    //        ///     of debugger output to be reflected in the command output window.
    //        /// </summary>
    //        public static void DrainMessageQueue(IntPtr hwnd)
    //        {
    //            var m = new MSG();
    //            while (PeekMessage(&m, hwnd, 0, 0, PM_REMOVE) != 0)
    //            {
    //                TranslateMessage(&m);
    //                DispatchMessage(&m);
    //            }
    //        }

    //        public void RunCommandWithWindbgHistory(string commandFormat, params object[] parameters)
    //        {
    //            RunCommandWithWindbgHistory(FormatString(commandFormat, parameters));
    //        }


    //        //CL - updated this for two reasons:
    //        //1.  The commands would not execute reliably without a delay and a delay was much less code than modifying the framework to execute delegates after it returned control to WinDbg in the testing that I was doing
    //        //2.  There was no ability to do something if the command did not get executed in the window assuming the window was found
    //        public void RunCommandWithWindbgHistory(string command, int delayInMilliseconds = 0, Action failureAction = null)
    //        {
    //            if (string.IsNullOrEmpty(command))
    //            {
    //                return;
    //            }

    //            var hWindbgCommandInput = FindWindbgCommandInputWindow();
    //            if (hWindbgCommandInput != IntPtr.Zero)
    //            {
    //                Action sendCommand = delegate
    //                                     {
    //                                         var textLengthWithoutNull = SendMessage(hWindbgCommandInput, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero);
    //                                         var sb = (textLengthWithoutNull > 0) ? new StringBuilder(textLengthWithoutNull + 1) : new StringBuilder();

    //                                         SendMessageAnsi(hWindbgCommandInput, WM_GETTEXT, (IntPtr)sb.Capacity, sb);

    //                                         if (SetWindowTextAnsi(hWindbgCommandInput, command) == 1)
    //                                         {
    //                                             SendWindowEnter(hWindbgCommandInput);

    //                                             SetWindowTextAnsi(hWindbgCommandInput, sb.ToString());
    //                                         }
    //                                         else if (failureAction != null)
    //                                         {
    //                                             failureAction();
    //                                         }
    //                                     };
    //                var t = new Task(sendCommand);
    //                if (delayInMilliseconds == 0)
    //                {
    //                    t.Start();
    //                }
    //                else
    //                {
    //                    var t2 = new Task(() => Thread.Sleep(delayInMilliseconds));
    //                    t2.ContinueWith(x => t.Start());
    //                    t2.Start();
    //                }
    //            }
    //            else
    //            {
    //                RunCommandInternal(command);
    //            }
    //        }
    //#endif

    //        public string RollupBytes(ulong bytes, string units = "", string _returnIfZero = "", bool fixedWidth = false)
    //        {
    //            return RollupBytes(this, bytes, units, _returnIfZero, fixedWidth);
    //        }

    //        /// <summary>Takes a byte count and converts to the appropriate size suffix, rounded to 2 digits passed the decimal</summary>
    //        /// <param name="d">An instance of DebugUtilities</param>
    //        /// <param name="bytes">A count of bytes to convert</param>
    //        /// <param name="units">Return size in these units.  Options: B, KB, MB, GB, TB, PB, EB, ZB, or YB</param>
    //        /// <param name="_returnIfZero"></param>
    //        /// <param name="fixedWidth"></param>
    //        /// <returns>A string</returns>
    //        public static string RollupBytes(DebugUtilities d, ulong bytes, string units = "", string _returnIfZero = "", bool fixedWidth = false)
    //        {
    //            string[] sizeSuffixes = {"B", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB"};

    //            // -1 == nothing
    //            // 0 to sizeSuffixes.Length == a size suffix override has been specified
    //            var suffixOverride = -1;

    //            if (bytes == 0)
    //            {
    //                return _returnIfZero;
    //            }

    //            // Try to find a match here
    //            if (!string.IsNullOrEmpty(units))
    //            {
    //                for (var i = 0; i < sizeSuffixes.Length; i++)
    //                {
    //                    var ss = sizeSuffixes[i];
    //                    if (units.Equals(ss, StringComparison.OrdinalIgnoreCase))
    //                    {
    //                        suffixOverride = i;
    //                        break;
    //                    }
    //                }
    //            }

    //            var index = 0; // doing this outside the for loop so we can use it after we get out of the loop
    //            double val = bytes;

    //            if (suffixOverride == -1)
    //            {
    //                // Auto units
    //                for (; index < sizeSuffixes.Length; index++)
    //                {
    //                    if (val < 1024)
    //                    {
    //                        break;
    //                    }

    //                    val /= 1024;
    //                }
    //            }
    //            else
    //            {
    //                // Fixed units
    //                for (; index < suffixOverride; index++)
    //                {
    //                    val /= 1024;
    //                }
    //            }

    //            if (fixedWidth == false)
    //            {
    //                return string.Format("{0:#.##} {1}", Math.Round(val, 2), sizeSuffixes[index]);
    //            }
    //            return string.Format("{0:0.00} {1}", Math.Round(val, 2), sizeSuffixes[index]);
    //        }

    //        public static string RollupBytes(DebugUtilities d, uint bytes)
    //        {
    //            return RollupBytes(d, (ulong)bytes);
    //        }

    //        /// <summary>
    //        ///     This command runs the "|" (pipe) command to checks to see if the process name passed in is contained in the pipe
    //        ///     output
    //        /// </summary>
    //        /// <param name="processName"></param>
    //        /// <returns></returns>
    //        public bool CurrentProcessNameIs(string processName)
    //        {
    //            if (DumpInfo.IsUserMode)
    //            {
    //                string pipeOutput;
    //                RunCommandSaveOutput(out pipeOutput, "|");

    //                if (pipeOutput.ToLowerInvariant().Contains(processName.ToLowerInvariant()))
    //                {
    //                    return true;
    //                }
    //            }
    //            else // Kernel mode
    //            {
    //                ulong implicitProcess;
    //                DebugSystemObjects.GetImplicitProcessDataOffset(out implicitProcess);

    //                var ProcessName = KernelProcessGetName(implicitProcess);
    //                if (processName.ToLowerInvariant() == ProcessName.ToLowerInvariant())
    //                {
    //                    return true;
    //                }
    //            }

    //            return false;
    //        }

    //        /// <summary>
    //        ///     Gets the process name in a kernel dump.
    //        /// </summary>
    //        /// <param name="ProcessBase"></param>
    //        /// <returns></returns>
    //        public string KernelProcessGetName(ulong ProcessBase)
    //        {
    //            if (ProcessBase == 0)
    //            {
    //                return string.Empty;
    //            }

    //            if (IsPointer64Bit() == false)
    //            {
    //                ProcessBase = SIGN_EXTEND(ProcessBase);
    //            }

    //            // All processes should end in 0.. Lets make sure.
    //            ProcessBase &= 0xfffffffffffffff8;

    //            string ShortName, LongName, Name;
    //            uint ProcessNameOffset;
    //            var hr = GetFieldOffset("nt!_EPROCESS", "ImageFileName", out ProcessNameOffset);
    //            if (FAILED(hr))
    //            {
    //                OutputErrorLine("GetProcessList: Failed getting of the offset of nt!_EPROCESS.ImageFileName: {0:x8}", hr);
    //                ShortName = string.Empty;
    //            }
    //            else
    //            {
    //                hr = ReadAnsiString(ProcessBase + ProcessNameOffset, 16, out ShortName);
    //                if (FAILED(hr) || string.IsNullOrEmpty(ShortName))
    //                {
    //                    ShortName = string.Empty;
    //                }

    //                uint Pid;
    //                ReadUInt32FromStructure("nt", "_EPROCESS", "UniqueProcessId", ProcessBase, out Pid);
    //                if ((string.IsNullOrEmpty(ShortName)) && Pid == 4)
    //                {
    //                    ShortName = "System Process";
    //                }
    //            }

    //            var wow64 = new Wow64Exts(this);
    //            wow64.SaveEffectiveProcessorAndSetEffectiveProcessorToActualProcessor();

    //            using (InstallIgnoreFilter_WRAP_WITH_USING())
    //            {
    //                // This causes a VirtualToOffset: 8b16bdac not properly sign extended error in 32 bit.
    //                ulong imageFileName;
    //                if (FAILED(ReadPointerFromStructure_Silent("nt", "_EPROCESS", "SeAuditProcessCreationInfo.ImageFileName", ProcessBase, out imageFileName)))
    //                {
    //                    LongName = ShortName;
    //                    wow64.RestoreOldEffectiveProcessor();
    //                }


    //                else if (FAILED(ReadUNICODE_STRING(imageFileName, out LongName)))
    //                {
    //                    LongName = ShortName;
    //                }
    //            }
    //            wow64.RestoreOldEffectiveProcessor();
    //            if (string.IsNullOrEmpty(LongName) || LongName.Length > 255 || !(LongName.Contains("\\")))
    //            {
    //                LongName = ShortName;
    //            }

    //            try
    //            {
    //                var index = LongName.LastIndexOf('\\');
    //                Name = LongName.Substring(index + 1);
    //            }
    //            catch (Exception)
    //            {
    //                Name = ShortName;
    //            }
    //            return Name;
    //        }

    //        /// <summary>
    //        ///     Determines if the provided module is loaded in the current process.
    //        /// </summary>
    //        /// <param name="moduleName"></param>
    //        /// <returns></returns>
    //        public bool CurrentProcessContainsModule(string moduleName)
    //        {
    //            //We need the dllName not to have the .dll part in the name so if it is there, we need to trim it off.
    //            if (moduleName.Contains("."))
    //            {
    //                moduleName = moduleName.Substring(0, moduleName.IndexOf(".", StringComparison.Ordinal));
    //            }

    //            var modInfo = new ModuleInfo(this, moduleName);

    //            if (modInfo.IsValid)
    //            {
    //                return true;
    //            }
    //            return false;
    //        }


    //        public bool GetSessionIds(out List<uint> sessionIds)
    //        {
    //            sessionIds = new List<uint>();

    //            ulong sessionIdBitmapAddress;
    //            if (OSInfo.Win10OrNewer)
    //            {
    //                // Starting with Win10, the Mi Globals live in MiState
    //                dynamic MiState = new DynamicStruct(this, "nt!MiState");
    //                sessionIdBitmapAddress = (ulong)(MiState.Vs.SessionIdBitmap.GetAddress());

    //                if (sessionIdBitmapAddress == 0)
    //                {
    //                    OutputErrorLine("Warning: Invalid/missing SessionIdBitmap, session list will be empty.");
    //                    return false;
    //                }
    //            }
    //            else
    //            {
    //                if (FAILED(ReadGlobalAsPointer("nt", "MiSessionIdBitmap", out sessionIdBitmapAddress)))
    //                {
    //                    OutputErrorLine("Warning: Could not read nt!MiSessionIdBitmap.  This probably means the session list will be empty.");
    //                    return false;
    //                }
    //            }
    //            OutputVerboseLine("Read global nt!MiSessionIdBitmap as {0:p}", sessionIdBitmapAddress);

    //            uint sessionLimit;
    //            ulong sessionIdBitmapBufferAddress;
    //            ulong sessionIdBitmap;
    //            const uint pointerSize = 64;

    //            if (OSInfo.Win10OrNewer)
    //            {
    //                ReadPointerFromStructure("nt", "_RTL_BITMAP", "Buffer", sessionIdBitmapAddress, out sessionIdBitmapBufferAddress);
    //                ReadUInt32FromStructure("nt", "_RTL_BITMAP", "SizeOfBitMap", sessionIdBitmapAddress, out sessionLimit);
    //            }
    //            else
    //            {
    //                ReadPointerFromStructure("nt", "MiSessionIdBitmap", "Buffer", sessionIdBitmapAddress, out sessionIdBitmapBufferAddress);
    //                ReadUInt32FromStructure("nt", "MiSessionIdBitmap", "SizeOfBitMap", sessionIdBitmapAddress, out sessionLimit);
    //            }
    //            ReadPointer(sessionIdBitmapBufferAddress, out sessionIdBitmap);
    //            ReadVirtual32(sessionIdBitmapAddress, out sessionLimit);

    //            uint sessionId = 0;

    //            for (int i = 0; i < sessionLimit / pointerSize; i++)
    //            {
    //                ulong tempSessionIdBitmap;
    //                ReadPointer(sessionIdBitmapBufferAddress, out tempSessionIdBitmap);
    //                OutputVerboseLine("nt!MiSessionIdBitmap is {0:p}", tempSessionIdBitmap);

    //                for (int j = 0; j < pointerSize; j++)
    //                {
    //                    if (tempSessionIdBitmap % 2 == 1)
    //                    {
    //                        sessionIds.Add(sessionId);
    //                    }
    //                    tempSessionIdBitmap /= 2;
    //                    sessionId++;
    //                }
    //                sessionIdBitmapBufferAddress += pointerSize / 8;
    //            }
    //            return true;
    //        }

    //        public static ulong SIGN_EXTEND(uint addr)
    //        {
    //            return (ulong)(int)addr;
    //        }

    //        public static ulong SIGN_EXTEND(ulong addr)
    //        {
    //            return (ulong)(int)addr;
    //        }

    //        // Publically Documented Api http://msdn.microsoft.com/en-us/library/windows/hardware/ff563610(v=vs.85).aspx
    //        [DllImport("Kernel32.dll", EntryPoint = "RtlZeroMemory", SetLastError = false)]
    //        private static extern void ZeroMemory(IntPtr dest, IntPtr size);

    //        public static void ZeroMemory(IntPtr dest, int size)
    //        {
    //            ZeroMemory(dest, new IntPtr(size));
    //        }

    //        private static Delegate CreateCopyMemory(bool useIntPtr, bool aligned)
    //        {
    //            /* Have to specify an owning module or type; otherwise the function is "anonymous" and we will get a verification exception when using pointers */
    //            var m = Assembly.GetExecutingAssembly().ManifestModule;
    //            var pointerType = useIntPtr ? typeof(IntPtr) : typeof(void*);
    //            var delegateType = useIntPtr ? typeof(CopyMemoryDelegate_IntPtr) : typeof(CopyMemoryDelegate_VoidPtr);
    //            Type[] parameterTypes = {pointerType, pointerType, typeof(uint)};
    //            var dm = new DynamicMethod("CopyMemory", null, parameterTypes, m, true);
    //            var ilg = dm.GetILGenerator();

    //            ilg.Emit(OpCodes.Ldarg_0);
    //            ilg.Emit(OpCodes.Ldarg_1);
    //            ilg.Emit(OpCodes.Ldarg_2);
    //            if (aligned == false)
    //            {
    //                ilg.Emit(OpCodes.Unaligned, (byte)1);
    //            }
    //            ilg.Emit(OpCodes.Cpblk);
    //            ilg.Emit(OpCodes.Ret);

    //            return dm.CreateDelegate(delegateType);
    //        }

    //        public static void CopyMemory(IntPtr dest, IntPtr source, int size)
    //        {
    //            CopyMemory_IntPtr(dest, source, (uint)size);
    //        }

    //        public static void CopyMemory(IntPtr dest, IntPtr source, uint size)
    //        {
    //            CopyMemory_IntPtr(dest, source, size);
    //        }

    //        public static void CopyMemory(void* dest, void* source, int size)
    //        {
    //            CopyMemory_VoidPtr(dest, source, (uint)size);
    //        }

    //        public static void CopyMemory(void* dest, void* source, uint size)
    //        {
    //            CopyMemory_VoidPtr(dest, source, size);
    //        }

    //        public void IterateOverMemoryInBlocks(out bool canceled, ulong regionStart, ulong endAddress, ulong blockSize, Action<ulong, ulong> callback)
    //        {
    //            uint pageSize;
    //            MexFrameworkClass.SymCache.GetPageSize(this, out pageSize);
    //            var pageMask = ~(pageSize - 1UL);
    //            var pointerMask = ~(PointerSize() - 1UL);
    //            var needToSignExtend = !IsPointer64Bit();

    //            var displayedQueryVirtualWarning = false;

    //            if (needToSignExtend)
    //            {
    //                regionStart = SIGN_EXTEND(regionStart);
    //                endAddress = SIGN_EXTEND(endAddress);
    //            }

    //            OutputVerboseLine("IterateOverMemoryInBlocks: Initial range {0:p} - {1:p}", regionStart, endAddress);

    //            while (regionStart < endAddress)
    //            {
    //                if (ShouldBreak())
    //                {
    //                    OutputLine("IterateOverMemoryInBlocks: Canceled");
    //                    canceled = true;
    //                    return;
    //                }

    //                ulong startAddr;
    //                MEMORY_BASIC_INFORMATION64 memoryInfo;
    //                if (SUCCEEDED(QueryVirtual(regionStart, out memoryInfo)))
    //                {
    //                    startAddr = Math.Max(regionStart, memoryInfo.BaseAddress);
    //                    if (needToSignExtend)
    //                    {
    //                        startAddr = SIGN_EXTEND(startAddr);
    //                    }

    //                    regionStart = memoryInfo.BaseAddress + memoryInfo.RegionSize;
    //                    if (needToSignExtend)
    //                    {
    //                        regionStart = SIGN_EXTEND(regionStart);
    //                    }

    //                    OutputVerboseLine("IterateOverMemoryInBlocks QV: Start={0:p} End={1:p}", memoryInfo.BaseAddress, regionStart);

    //                    memoryInfo.RegionSize = 0; /* Not sure why this is done but it was copied from the debugger */

    //                    if (((memoryInfo.Protect & PAGE.GUARD) != 0) || ((memoryInfo.Protect & PAGE.NOACCESS) != 0) || ((memoryInfo.State & MEM.FREE) != 0) || ((memoryInfo.State & MEM.RESERVE) != 0))
    //                    {
    //                        OutputVerboseLine("IterateOverMemoryInBlocks: Skipping range because of memory protection");
    //                        continue;
    //                    }
    //                }
    //                else
    //                {
    //                    // Slightly slower than QueryVirtual, but much faster than the brute force method, and works in more cases (like IDNA)
    //                    uint size;
    //                    if (SUCCEEDED(DebugDataSpaces.GetValidRegionVirtual(regionStart, (uint)blockSize, out startAddr, out size)))
    //                    {
    //                        OutputVerboseLine("IterateOverMemoryInBlocks GV: Start={0:p} Size={1:x}", startAddr, size);
    //                        if (size == 0)
    //                        {
    //                            ulong nextValid = 0;
    //                            if (FAILED(DebugDataSpaces.GetNextDifferentlyValidOffsetVirtual(regionStart, out nextValid)))
    //                            {
    //                                OutputVerboseLine("*IterateOverMemoryInBlocks GetNextDifferentlyValidOffsetVirtual failed: Start={0:p} Size={1:x}", startAddr, size);
    //                            }
    //                            if (regionStart >= nextValid || nextValid >= endAddress)
    //                            {
    //                                regionStart = endAddress;
    //                                break;
    //                            }
    //                            DebugDataSpaces.GetValidRegionVirtual(nextValid, (uint)blockSize, out startAddr, out size);
    //                            OutputVerboseLine("*IterateOverMemoryInBlocks GV: Start={0:p} Size={1:x}", startAddr, size);
    //                        }
    //                        if (startAddr == 0 || size == 0)
    //                        {
    //                            startAddr = regionStart & pointerMask;
    //                            regionStart = (regionStart + pageSize) & pageMask;
    //                        }
    //                        else
    //                        {
    //                            regionStart = startAddr + size;
    //                        }
    //                        if (needToSignExtend)
    //                        {
    //                            regionStart = SIGN_EXTEND(regionStart);
    //                            startAddr = SIGN_EXTEND(startAddr);
    //                        }
    //                        OutputVerboseLine("IterateOverMemoryInBlocks GV: Start={0:p} End={1:p}", startAddr, regionStart);
    //                    }
    //                    else
    //                    {
    //                        if (displayedQueryVirtualWarning == false)
    //                        {
    //                            displayedQueryVirtualWarning = true;
    //                            OutputWarningLine("IterateOverMemoryInBlocks: QueryVirtual failed, falling back to scanning every address regardless of page status.");
    //                            OutputWarningLine("IterateOverMemoryInBlocks: This could take a VERY long time.");
    //                        }

    //                        startAddr = regionStart & pointerMask;
    //                        if (needToSignExtend)
    //                        {
    //                            startAddr = SIGN_EXTEND(startAddr);
    //                        }

    //                        regionStart = (regionStart + pageSize) & pageMask;
    //                        if (needToSignExtend)
    //                        {
    //                            regionStart = SIGN_EXTEND(regionStart);
    //                        }
    //                    }
    //                }

    //                var regionEnd = Math.Min(endAddress, regionStart);
    //                while (startAddr < regionEnd)
    //                {
    //                    if (ShouldBreak())
    //                    {
    //                        OutputLine("IterateOverMemoryInBlocks: Canceled");
    //                        canceled = true;
    //                        return;
    //                    }

    //                    var bytesToProcess = regionEnd - startAddr;
    //                    if (needToSignExtend)
    //                    {
    //                        SIGN_EXTEND(bytesToProcess);
    //                    }
    //                    bytesToProcess = Math.Min(bytesToProcess, blockSize);

    //                    callback(startAddr, startAddr + bytesToProcess);

    //                    startAddr += bytesToProcess;
    //                    if (needToSignExtend)
    //                    {
    //                        startAddr = SIGN_EXTEND(startAddr);
    //                    }
    //                }
    //            }

    //            canceled = false;
    //        }

    //        [StructLayout(LayoutKind.Sequential)]
    //        private struct POINT
    //        {
    //            public readonly int x;
    //            public readonly int y;
    //        }

    //        [StructLayout(LayoutKind.Sequential)]
    //        private struct MSG
    //        {
    //            public readonly IntPtr hwnd;
    //            public readonly uint message;
    //            public readonly IntPtr wParam;
    //            public readonly IntPtr lParam;
    //            public readonly uint time;
    //            public readonly POINT pt;
    //        }

    //        //[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory", SetLastError = false)]
    //        //private static extern void CopyMemory(IntPtr dest, IntPtr source, IntPtr size);

    //        //public static void CopyMemory(IntPtr dest, IntPtr source, int size)
    //        //{
    //        //    CopyMemory(dest, source, new IntPtr(size));
    //        //}

    //        private delegate void CopyMemoryDelegate_VoidPtr(void* dest, void* source, uint size);

    //        private delegate void CopyMemoryDelegate_IntPtr(IntPtr dest, IntPtr source, uint size);
    //#pragma warning disable 1591

    //        public string hexMatch => @"[abcdef\d]+";

    //        public string hexMatch8plus => @"[`abcdef\d]{8,}";

    //        public const uint INFINITE = 0xffffffff;
    //        public const int S_OK = 0;
    //        private const int S_FALSE = 1;
    //        public const int E_FAIL = unchecked((int)0x80004005);
    //        public const int ERROR_INVALID_PARAMETER = unchecked((int)0x80070057);
    //        public const uint ERROR_INSUFFICIENT_BUFFER = 122;
    //        public const uint DEBUG_ANY_ID = 0xffffffff;
    //        public const uint IMAGE_DOS_SIGNATURE = 0x5A4D;
    //        public const uint IMAGE_NT_SIGNATURE = 0x4550;

    //        private const uint PM_NOREMOVE = 0;
    //        private const uint PM_REMOVE = 1;
    //        private const uint PM_NOYIELD = 2;

    //#pragma warning restore 1591
    //    }
}