using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace Microsoft.Mex.DotNetDbg
{
    /// <summary>
    ///     A class that makes receiving and filtering debuger output simple.
    /// </summary>
    public class SimpleOutputHandler : IDebugOutputCallbacksWide, IDisposable
    {
        private const int S_OK = 0;
        private readonly List<BufferLine> BufferedOutput;
        private readonly DbgStringBuilder DataBuffer;
        private readonly int InstallationHRESULT;

        // Timhe - If we get output on a thread that didnt invoke us, buffer it..
        private readonly int InstalledThreadId;
        private readonly DbgStringBuilder LineBuffer;
        private readonly bool PassThrough;
        private readonly bool PassThroughOnly;
        private readonly DebugUtilities PassthroughUtilities;
        private DebugUtilities ExecutionUtilities;
        private bool Installed;
        private int LineInputPos;
        private StreamWriter LogFile;
        private bool OutputActive = true;
        private OUTPUT_FILTER OutputFilter;
        private IntPtr PreviousCallbacks; /* this could be either an interface or a conversion thunk which doesn't support the interface */
        private DEBUG_OUTPUT PreviousMask;
        private bool ReEnter;
        private IntPtr ThisIDebugOutputCallbacksPtr;

        private SimpleOutputHandler()
        {
            throw new NotImplementedException("This constructor should never be called");
        }

        private SimpleOutputHandler(DebugUtilities executionUtilities, DebugUtilities passthroughUtilities, OUTPUT_FILTER outputFilter, bool passThrough, bool passThroughOnly)
        {
            ExecutionUtilities = executionUtilities;
            PassthroughUtilities = passthroughUtilities;
            passthroughUtilities.OutputMaskSave();
            Installed = false;
            PreviousCallbacks = IntPtr.Zero;
            if (passThroughOnly == false)
            {
                DataBuffer = new DbgStringBuilder(executionUtilities, 1024);
            }
            LineBuffer = new DbgStringBuilder(executionUtilities, 1024);
            OutputFilter = outputFilter;
            PassThrough = passThrough;
            PassThroughOnly = passThroughOnly;

            executionUtilities.DebugClient.GetOutputCallbacksWide(out PreviousCallbacks); /* We will need to release this */

            ThisIDebugOutputCallbacksPtr = Marshal.GetComInterfaceForObject(this, typeof(IDebugOutputCallbacksWide));
            InstallationHRESULT = executionUtilities.DebugClient.SetOutputCallbacksWide(ThisIDebugOutputCallbacksPtr);
            if (SUCCEEDED(InstallationHRESULT))
            {
                Installed = true;
                InstalledThreadId = Thread.CurrentThread.ManagedThreadId;
                BufferedOutput = new List<BufferLine>(1024);
            }
            else
            {
                Dispose();
            }
        }

        /// <summary>
        ///     Outputs a line of test.
        /// </summary>
        /// <param name="Mask">Flags describing the output.</param>
        /// <param name="Text">The text to output.</param>
        /// <returns>
        ///     HRESULT which is almost always S_OK since errors are ignored by the debugger engine unless they signal an RPC
        ///     error.
        /// </returns>
        public int Output(DEBUG_OUTPUT Mask, string Text)
        {
            if (OutputActive)
            {
                lock (this)
                {
                    OutputActive = false;
                    PreviousMask = Mask;

                    for (int i = 0; i < Text.Length; ++i)
                    {
                        char c = Text[i];
                        if (c == '\n')
                        {
                            LineBuffer.AppendLine();
                            ProcessLine(Mask, LineBuffer.ToString());
                            LineBuffer.Clear();
                            LineInputPos = 0;
                        }
                        else if (c == '\r')
                        {
                            LineInputPos = 0;
                        }
                        else
                        {
                            if (LineInputPos >= LineBuffer.Length)
                            {
                                LineBuffer.Append(c);
                            }
                            else
                            {
                                LineBuffer[LineInputPos] = c;
                            }
                            ++LineInputPos;
                        }
                    }

                    OutputActive = true;
                }
            }
            return S_OK;
        }

        /// <summary>
        ///     IDisposable.Dipose(). MAKE SURE THIS IS CALLED!!!
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static bool FAILED(int hr)
        {
            return (hr < 0);
        }

        private static bool SUCCEEDED(int hr)
        {
            return (hr >= 0);
        }

        private string FilterLine(string line)
        {
            if (OutputFilter != null)
            {
                string filteredLine = OutputFilter.FilterText(line);
                return !string.IsNullOrEmpty(filteredLine) ? filteredLine : "";
            }
            return line;
        }

        private void ProcessLine(DEBUG_OUTPUT Mask, string line)
        {

            if (OutputFilter != null)
            {
                if (!Mask.HasFlag(DEBUG_OUTPUT.STATUS))
                {
                    string filteredLine = OutputFilter.FilterText(line);
                    if (!string.IsNullOrEmpty(filteredLine))
                    {
                        line = filteredLine;
                    }
                    else
                    {
                        return;
                    }
                }
            }
            if (LogFile != null)
            {
                LogFile.Write(line);
            }
            if (PassThrough)
            {
                PassThroughLine(Mask, line);
            }
            if (PassThroughOnly == false)
            {
                DataBuffer.Append(line);
            }
        }

        private void ProcessLineNoFilter(DEBUG_OUTPUT Mask, string line)
        {
            if (line == null)
            {
                return;
            }
            if (LogFile != null)
            {
                LogFile.Write(line);
            }
            if (PassThrough)
            {
                PassThroughLine(Mask, line);
            }
            if (PassThroughOnly == false)
            {
                DataBuffer.Append(line);
            }
        }

        //FIX ME!!! This is a hack due to Win8:235420

        private void PassThroughLine(DEBUG_OUTPUT Mask, string line)
        {
            if (ReEnter == false)
            {
                ReEnter = true;
                DEBUG_OUTCTL outctl;


                if (PassthroughUtilities.IsFirstCommand)
                {
                    outctl = DEBUG_OUTCTL.ALL_CLIENTS;
                }
                else
                {
                    outctl = DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.NOT_LOGGED;
                }


                if (Mask.HasFlag(DEBUG_OUTPUT.ADDR_TRANSLATE) && (Mask.HasFlag(DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.ERROR | DEBUG_OUTPUT.WARNING | DEBUG_OUTPUT.VERBOSE) == false))
                {
                    Mask = Mask | DEBUG_OUTPUT.NORMAL;
                }
                try
                {
                    PreviousMask = Mask;
                    if (InstalledThreadId == Thread.CurrentThread.ManagedThreadId)
                    {
                        PassthroughUtilities.DebugClient.FlushCallbacks();
                        PassthroughUtilities.OutputMaskRestore();
                        PassthroughUtilities.ControlledOutputWide(outctl, Mask, line);
                        PassthroughUtilities.OutputMaskDisableAll();
                    }
                    else
                    {
                        BufferedOutput.Add(new BufferLine(outctl, Mask, line));
                    }
                }
                catch
                {
                    DebugUtilities.CoUninitialize();
                    if (InstalledThreadId == Thread.CurrentThread.ManagedThreadId)
                    {
                        PassthroughUtilities.OutputMaskRestore();
                        PassthroughUtilities.ControlledOutputWide(outctl, Mask, line);
                        PassthroughUtilities.OutputMaskDisableAll();
                    }
                    else
                    {
                        BufferedOutput.Add(new BufferLine(outctl, Mask, line));
                    }
                }

                ReEnter = false;
            }
        }

        /// <summary>
        ///     Process any remaining data such as a partial (non-terminated) line or output cached by a filter.
        /// </summary>
        public void Flush()
        {
            lock (this)
            {
                if (PreviousMask == DEBUG_OUTPUT.STATUS || PreviousMask == (int)0)
                {
                    PreviousMask = DEBUG_OUTPUT.NORMAL;
                }

                if (LineBuffer.Length != 0)
                {
                    LineBuffer.AppendLine(); /* Decided it is always best to end with a line break, so adding one here during a flush */
                    ProcessLine(PreviousMask, LineBuffer.ToString());
                    LineBuffer.Length = 0;
                    LineInputPos = 0;
                }

                if (OutputFilter != null)
                {
                    string flushedData = OutputFilter.Flush();
                    if (!string.IsNullOrEmpty(flushedData))
                    {
                        ProcessLineNoFilter(PreviousMask, flushedData);
                    }
                }

                if (LogFile != null)
                {
                    try
                    {
                        LogFile.Flush();
                    }
                    catch {}
                }
                PassthroughUtilities.OutputMaskRestore();
            }
        }

        /// <summary>
        ///     Creates a new SimpleOutputHandler instances and sets it as the active handler for the debugger.
        /// </summary>
        /// <param name="debugUtilities">A DebugUtilities associated with the debugger.</param>
        /// <param name="outputCallbacks">
        ///     An out parameter to receive the new SimpleOutputHandler. CALL Revert AND/OR Dipose TO
        ///     REMOVE!
        /// </param>
        /// <param name="filter">An optional filter object to process the output data.</param>
        /// <param name="passThrough">
        ///     Whether the output data should be passed to the previously installed output handler. Default
        ///     is to cache locally only.
        /// </param>
        /// <param name="passThroughOnly">
        ///     Disables local caching of output data. Save memory and is more effient when the primary
        ///     goes it to pass the output data to the previously installed output handled (normally WinDbg).
        /// </param>
        /// <returns>Last HRESULT of the install process.</returns>
        public static int Install(DebugUtilities debugUtilities, out SimpleOutputHandler outputCallbacks, OUTPUT_FILTER filter = null, bool passThrough = false, bool passThroughOnly = false)
        {
            var oc = new SimpleOutputHandler(debugUtilities, debugUtilities, filter, passThrough, passThroughOnly);
            outputCallbacks = SUCCEEDED(oc.InstallationHRESULT) ? oc : null;
            return oc.InstallationHRESULT;
        }

        /// <summary>
        ///     Creates a new SimpleOutputHandler instances and sets it as the active handler for the debugger.
        /// </summary>
        /// <param name="debugUtilities">A DebugUtilities associated with the debugger.</param>
        /// <param name="executionUtilities">
        ///     An utilities associated with the output handler. This interface should be used for all
        ///     actions where output should be redirected to the output handler.
        /// </param>
        /// <param name="outputCallbacks">
        ///     An out parameter to receive the new SimpleOutputHandler. CALL Revert AND/OR Dipose TO
        ///     REMOVE!
        /// </param>
        /// <param name="filter">An optional filter object to process the output data.</param>
        /// <param name="passThrough">
        ///     Whether the output data should be passed to the previously installed output handler. Default
        ///     is to cache locally only.
        /// </param>
        /// <param name="passThroughOnly">
        ///     Disables local caching of output data. Save memory and is more effient when the primary
        ///     goes it to pass the output data to the previously installed output handled (normally WinDbg).
        /// </param>
        /// <returns>Last HRESULT of the install process.</returns>
        public static int Install(DebugUtilities debugUtilities, out DebugUtilities executionUtilities, out SimpleOutputHandler outputCallbacks, OUTPUT_FILTER filter = null, bool passThrough = false, bool passThroughOnly = false)
        {
            IDebugClient executionClient;
            int hr = debugUtilities.DebugClient.CreateClient(out executionClient);
            if (FAILED(hr))
            {
                debugUtilities.OutputVerboseLine("SimpleOutputHandler.Install Failed creating a new debug client for execution: {0:x8}", hr);
                outputCallbacks = null;
                executionUtilities = null;
                return hr;
            }

            executionUtilities = new DebugUtilities(executionClient);
            var oc = new SimpleOutputHandler(executionUtilities, debugUtilities, filter, passThrough, passThroughOnly);
            outputCallbacks = SUCCEEDED(oc.InstallationHRESULT) ? oc : null;
            return oc.InstallationHRESULT;
        }

        /// <summary>
        ///     Finalizer if Dispose() is not called. This should never execute.
        /// </summary>
        ~SimpleOutputHandler()
        {
            try
            {
                Dispose(false);
            }
            catch {}
        }

        /// <summary>
        ///     Clears the internal text buffer and any partial (unterminated) output. DOES NOT CALL Flush(), though it could. Let
        ///     me know if you think that would be better.
        /// </summary>
        public void ClearText()
        {
            lock (this)
            {
                //Should this flush? Flushing would acquire the lock again which may deadlock?
                if (PassThroughOnly == false)
                {
                    DataBuffer.Length = 0;
                }
                LineBuffer.Length = 0;
                LineInputPos = 0;
            }
        }

        /// <summary>
        ///     Gets a copy of all stored output if PassThroughOnly is not active.
        /// </summary>
        /// <returns>Any stored output.</returns>
        public string GetText()
        {
            lock (this)
            {
                if (PassThroughOnly)
                {
                    return "PassThroughOnly specified. No local recording of data performed";
                }
                return (LineBuffer.Length != 0) ? DataBuffer + FilterLine(LineBuffer.ToString()) : DataBuffer.ToString();
            }
        }

        /// <summary>
        ///     Adds a note to the output. DOES NOT GET PASSED TO THE FILTER! DOES get passed to previous output handlers and the
        ///     local output buffer.
        /// </summary>
        /// <param name="note">Text to add to the output.</param>
        public void AddNoteLine(string note)
        {
            lock (this)
            {
                /* Ignoring partial string on purpose */
                ProcessLineNoFilter(DEBUG_OUTPUT.NORMAL, note);
            }
        }

        /// <summary>
        ///     Remove the output handler and restore the previously installed handler.
        ///     If we ever get into a situation where we are reverting but not the active output handler we will not overwrite the
        ///     current handler and will never restore the original handler!
        /// </summary>
        private void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                if (Installed)
                {
                    Flush();
                    Installed = false;
                }

                IntPtr currentCallbacks = IntPtr.Zero;

                if (ExecutionUtilities != null)
                {
                    ExecutionUtilities.DebugClient.GetOutputCallbacksWide(out currentCallbacks); /* Will need to release this */
                    if (currentCallbacks == ThisIDebugOutputCallbacksPtr)
                    {
                        ExecutionUtilities.DebugClient.SetOutputCallbacksWide(PreviousCallbacks);
                    }
                    if (currentCallbacks != IntPtr.Zero)
                    {
                        Marshal.Release(currentCallbacks);
                        currentCallbacks = IntPtr.Zero;
                    }

                    if (PreviousCallbacks != IntPtr.Zero)
                    {
                        Marshal.Release(PreviousCallbacks);
                        PreviousCallbacks = IntPtr.Zero;
                    }
                    if (ThisIDebugOutputCallbacksPtr != IntPtr.Zero)
                    {
                        Marshal.Release(ThisIDebugOutputCallbacksPtr);
                        ThisIDebugOutputCallbacksPtr = IntPtr.Zero;
                    }

                    ExecutionUtilities = null; /* No need to release, just a copy of what was sent in */
                }
                if (LogFile != null)
                {
                    LogFile.Dispose();
                    LogFile = null;
                }

                foreach (BufferLine line in BufferedOutput)
                {
                    line.OutputLine(PassthroughUtilities);
                }
                BufferedOutput.Clear();
            }
        }

        /// <summary>
        ///     Log all output to a file.
        /// </summary>
        /// <param name="filename">Path to the file where the output should be stored.</param>
        public void LogToFile(string filename)
        {
            LogFile = new StreamWriter(File.OpenWrite(filename));
            lock (this)
            {
                LogFile.Write(GetText());
            }
        }

        /// <summary>
        ///     Gets the current output filter.
        /// </summary>
        /// <returns>The active output filter.</returns>
        public OUTPUT_FILTER GetOutputFilter()
        {
            return OutputFilter;
        }

        /// <summary>
        ///     Sets the active output filter. There can only be one. If a filter is already installed Flush() will be called
        ///     first.
        /// </summary>
        /// <param name="filter">The output filter to make active.</param>
        public void SetOutputFilter(OUTPUT_FILTER filter)
        {
            if (OutputFilter != null)
            {
                ProcessLineNoFilter(PreviousMask, OutputFilter.Flush());
            }
            OutputFilter = filter;
        }

        /// <summary>
        ///     Remove any output filter that is installed. Flushes a
        /// </summary>
        public void RemoveOutputFilter()
        {
            if (OutputFilter != null)
            {
                ProcessLineNoFilter(PreviousMask, OutputFilter.Flush());
                OutputFilter = null;
            }
        }

        private struct BufferLine
        {
            private readonly string Data;
            private readonly DEBUG_OUTPUT Mask;
            private readonly DEBUG_OUTCTL OutputControl;

            public BufferLine(DEBUG_OUTCTL OutputControl, DEBUG_OUTPUT Mask, string Data)
            {
                this.OutputControl = OutputControl;
                this.Mask = Mask;
                this.Data = Data;
            }

            public int OutputLine(DebugUtilities d)
            {
                return d.ControlledOutputWide(OutputControl, Mask, Data);
            }
        }

        /// <summary>
        ///     An interface that can be used to filter/modify data that the output handler receives.
        /// </summary>
        public interface OUTPUT_FILTER
        {
            /// <summary>
            ///     Called for every line of output. Will always be a single line ending with terminator(s), except for the last line
            ///     which is not guaranteed to have a CR and/or LF.
            /// </summary>
            /// <param name="input">A single line of debugger output.</param>
            /// <returns>A string to pass to the rest of the output system, or null if there is nothing to process.</returns>
            string FilterText(string input);

            /// <summary>
            ///     Called when the output handle is being removed so that any cached final data can be logged.
            /// </summary>
            /// <returns>A string to pass to the rest of the output system, or null if there is nothing to process.</returns>
            string Flush();
        }
    }
}