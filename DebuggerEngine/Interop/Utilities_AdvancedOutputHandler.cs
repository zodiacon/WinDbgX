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
    public class AdvancedOutputHandler : IDebugOutputCallbacks2, IDisposable
    {
        private const int S_OK = 0;
        private readonly List<CallbackData> _bufferedOutput;
        private readonly int _installationHresult;

        // Timhe - If we get output on a thread that didnt invoke us, buffer it..
        private readonly int _installedThreadId;
        private readonly DEBUG_OUTCBI _interestMask;
        private readonly Dictionary<DEBUG_OUTPUT, CallbackData> _lineBuffers = new Dictionary<DEBUG_OUTPUT, CallbackData>();
        private readonly DebugUtilities _passthroughUtilities;
        private DebugUtilities _executionUtilities;
        private bool _installed;
        private volatile bool _outputActive = true;
        private OUTPUT_FILTER _outputFilter;
        private IntPtr _previousCallbacks; /* this could be either an interface or a conversion thunk which doesn't support the interface */
        private volatile bool _reEnter;
        private IntPtr _thisIDebugOutputCallbacksPtr;

        private AdvancedOutputHandler(DebugUtilities executionUtilities, DebugUtilities passthroughUtilities, OUTPUT_FILTER outputFilter, bool wantsDml)
        {
            if (_installed == true)
            {
                Revert();
                throw new Exception("Can not install an Output Handler more than once.");
            }

            _executionUtilities = executionUtilities;
            _passthroughUtilities = passthroughUtilities;
            _previousCallbacks = IntPtr.Zero;

            passthroughUtilities.OutputMaskSave();
            passthroughUtilities.OutputMaskDisableAll();
            passthroughUtilities.DebugClient.FlushCallbacks();
            executionUtilities.DebugClient.FlushCallbacks();
            _outputFilter = outputFilter;

            _interestMask = wantsDml ? DEBUG_OUTCBI.DML : DEBUG_OUTCBI.TEXT;

            executionUtilities.DebugClient.GetOutputCallbacks(out _previousCallbacks); /* We will need to release this */

            _thisIDebugOutputCallbacksPtr = Marshal.GetComInterfaceForObject(this, typeof(IDebugOutputCallbacks2));
            _installationHresult = executionUtilities.DebugClient.SetOutputCallbacks(_thisIDebugOutputCallbacksPtr);
            if (SUCCEEDED(_installationHresult))
            {
                _installed = true;
                _installedThreadId = Thread.CurrentThread.ManagedThreadId;
                _bufferedOutput = new List<CallbackData>(128);
            }
            else
            {
                Dispose();
            }
        }

        /// <summary>
        ///     Outputs a line of text.
        /// </summary>
        /// <param name="Mask">Flags describing the output.</param>
        /// <param name="Text">The text to output.</param>
        /// <returns>
        ///     HRESULT which is almost always S_OK since errors are ignored by the debugger engine unless they signal an RPC
        ///     error.
        /// </returns>
        public int Output(DEBUG_OUTPUT Mask, string Text)
        {
            return Output2(DEBUG_OUTCB.TEXT, 0, (ulong)Mask, Text);
        }

        /// <summary>
        ///     Implements IDebugOutputCallbacks2::GetInterestMask
        /// </summary>
        public int GetInterestMask(out DEBUG_OUTCBI Mask)
        {
            Mask = _interestMask;
            return S_OK;
        }

        /// <summary>
        ///     Implements IDebugOutputCallbacks2::Output2
        /// </summary>
        public int Output2(DEBUG_OUTCB Which, DEBUG_OUTCBF Flags, ulong Arg, string Text)
        {
            var mask = (DEBUG_OUTPUT)Arg;

            if (string.IsNullOrEmpty(Text))
            {
                return S_OK;
            }

            _executionUtilities.OutputMaskRestore();
            var textIsDml = Which == DEBUG_OUTCB.DML;

            if (_outputActive)
            {
                lock (this)
                {
                    _outputActive = false;

                    var sb = new DbgStringBuilder(_passthroughUtilities, Text.Length);
                    sb.Append(Text);
                    var callbackData = new CallbackData(mask, sb, textIsDml, Flags);

                    // The advanced filter gets 1st crack at the data before it goes to the line by line filter.
                    callbackData = _outputFilter.AdvancedFilterText(callbackData);
                    if (callbackData == null)
                    {
                        _outputActive = true;
                        return S_OK;

                    }
                    // It can force data to be output now, skipping the line-by-line filtering.
                    if (callbackData.ForceOutputNow)
                    {
                        PassThroughLine(callbackData);
                        _outputActive = true;
                        return S_OK;
                    }

                    Text = callbackData.Data.ToString();
                    // Was there a partial line for this mask? if so, lets finish it and send it to the filter!
                    CallbackData lineBuffer;
                    _lineBuffers.TryGetValue(callbackData.Mask, out lineBuffer);

                    if (lineBuffer == null)
                    {
                        lineBuffer = new CallbackData(callbackData.Mask, new DbgStringBuilder(_passthroughUtilities, Text.Length), callbackData.IsDML, callbackData.DMLFlags);
                        _lineBuffers[mask] = lineBuffer;
                    }

                    for (var i = 0; i < Text.Length; ++i)
                    {
                        var c = Text[i];
                        if (c == '\n')
                        {
                            lineBuffer.Data.Append(c);
                            ProcessLine(lineBuffer.Copy(_passthroughUtilities));
                            lineBuffer.Clear();
                        }
                        else if (c == '\r')
                        {
                            if ((i + 1) < Text.Length && Text[i + 1] != '\n')
                            {
                                lineBuffer.Clear();
                            }
                        }
                        else
                        {
                            lineBuffer.Data.Append(c);
                        }
                    }

                    //if (Which == DEBUG_OUTCB.EXPLICIT_FLUSH)
                    //{
                    //    _passthroughUtilities.DebugClient.FlushCallbacks();
                    //}
                    _outputActive = true;
                }
            }

            return S_OK;
        }

        /// <summary>
        ///     IDisposable.Dipose(). MAKE SURE THIS IS CALLED!!!
        /// </summary>
        public void Dispose()
        {
            Revert();
            GC.SuppressFinalize(this);
        }

        private static bool FAILED(int hr)
        {
            return hr < 0;
        }

        private static bool SUCCEEDED(int hr)
        {
            return hr >= 0;
        }

        private CallbackData FilterLine(CallbackData cbd)
        {
            if (_outputFilter != null)
            {
                return _outputFilter.FilterText(cbd);
            }
            return cbd;
        }

        private void ProcessLine(CallbackData cbd)
        {
            if (_outputFilter != null)
            {
                var filteredLine = _outputFilter.FilterText(cbd);
                if (string.IsNullOrEmpty(filteredLine.Data?.ToString()))
                {
                    return;
                }
                cbd = filteredLine;
            }

            PassThroughLine(cbd);
        }

        //FIX ME!!! This is a hack due to Win8:235420

        private void PassThroughLine(CallbackData cbd)
        {
            if (string.IsNullOrEmpty(cbd.Data?.ToString()))
            {
                return;
            }
            if (_reEnter == false)
            {
                _reEnter = true;

                if (cbd.Mask.HasFlag(DEBUG_OUTPUT.ADDR_TRANSLATE) && cbd.Mask.HasFlag(DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.ERROR | DEBUG_OUTPUT.WARNING | DEBUG_OUTPUT.VERBOSE) == false)
                {
                    cbd.Mask = cbd.Mask | DEBUG_OUTPUT.NORMAL;
                }
                try
                {
                    if (_installedThreadId == Thread.CurrentThread.ManagedThreadId)
                    {
                        foreach (var bufferLine in _bufferedOutput)
                        {
                            bufferLine.Data.Insert(0, "BUFFERED ");
                            bufferLine.OutputLineMaskDisabled(_passthroughUtilities);
                        }
                        _bufferedOutput.Clear();
                        cbd.OutputLineMaskDisabled(_passthroughUtilities);
                    }
                    else
                    {
                        _bufferedOutput.Add(cbd);
                    }
                }
                catch
                {
                    DebugUtilities.CoUninitialize();
                    if (_installedThreadId == Thread.CurrentThread.ManagedThreadId)
                    {
                        cbd.OutputLineMaskDisabled(_passthroughUtilities);
                    }
                    else
                    {
                        _bufferedOutput.Add(cbd);
                    }
                }

                _reEnter = false;
            }
        }

        /// <summary>
        ///     Process any remaining data such as a partial (non-terminated) line or output cached by a filter.
        /// </summary>
        public void Flush()
        {
            lock (this)
            {
                try
                {
                    foreach (var lineBuffer in _lineBuffers)
                    {
                        if (lineBuffer.Value.Data.Length != 0)
                        {
                            if (!lineBuffer.Value.Data.ToString().EndsWith("\n"))
                            {
                                lineBuffer.Value.Data.AppendLine(); /* Decided it is always best to end with a line break, so adding one here during a flush */
                            }
                            ProcessLine(lineBuffer.Value);
                            lineBuffer.Value.Clear();
                        }
                    }
                    var flushedData = _outputFilter?.Flush();
                    flushedData?.OutputLineMaskDisabled(_passthroughUtilities);
                }
                finally
                {
                    _passthroughUtilities.OutputMaskRestore();
                }
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
        /// <param name="wantsDml">Whether DML should be accepted.</param>
        /// <param name="InterestMask"></param>
        /// <returns>DebugUtilities that will get filtered</returns>
        public static DebugUtilities Install(DebugUtilities debugUtilities, out AdvancedOutputHandler outputCallbacks, OUTPUT_FILTER filter = null, bool wantsDml = true, DEBUG_OUTPUT InterestMask = DEBUG_OUTPUT.NORMAL | DEBUG_OUTPUT.WARNING | DEBUG_OUTPUT.ERROR)
        {
            IDebugClient executionClient;
            DebugUtilities executionUtilities;
            var hr = debugUtilities.DebugClient.CreateClient(out executionClient);
            if (FAILED(hr))
            {
                debugUtilities.OutputVerboseLine("SimpleOutputHandler.Install Failed creating a new debug client for execution: {0:x8}", hr);
                outputCallbacks = null;
                executionUtilities = null;
                return null;
            }

            executionUtilities = new DebugUtilities(executionClient) {IsFiltered = true};
            executionUtilities.DebugClient.SetOutputMask(InterestMask);
            executionUtilities.OutputMaskSave();
            var oc = new AdvancedOutputHandler(executionUtilities, debugUtilities, filter, wantsDml);
            
            outputCallbacks = SUCCEEDED(oc._installationHresult) ? oc : null;
            filter.OnInstall(debugUtilities, executionUtilities);
            if (outputCallbacks != null)
            {
                return executionUtilities;
            }
            return null;
        }

        /// <summary>
        ///     Finalizer if Dispose() is not called. This should never execute.
        /// </summary>
        ~AdvancedOutputHandler()
        {
            try
            {
                Revert();
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
                _lineBuffers.Clear();
                _bufferedOutput.Clear();
            }
        }


        /// <summary>
        ///     Gets the current output filter.
        /// </summary>
        /// <returns>The active output filter.</returns>
        public OUTPUT_FILTER GetOutputFilter()
        {
            return _outputFilter;
        }

        /// <summary>
        ///     Sets the active output filter. There can only be one. If a filter is already installed Flush() will be called
        ///     first.
        /// </summary>
        /// <param name="filter">The output filter to make active.</param>
        public void SetOutputFilter(OUTPUT_FILTER filter)
        {
            var flushed = _outputFilter?.Flush();
            flushed?.OutputLineMaskDisabled(_passthroughUtilities);
            _outputFilter = filter;
        }

        /// <summary>
        ///     Remove any output filter that is installed. Flushes a
        /// </summary>
        public void RemoveOutputFilter()
        {
            if (_outputFilter != null)
            {
                var flushed = _outputFilter.Flush();
                flushed?.OutputLineMaskDisabled(_passthroughUtilities);
                _outputFilter = null;
            }
        }

        /// <summary>
        ///     Remove the output handler and restore the previously installed handler.
        ///     If we ever get into a situation where we are reverting but not the active output handler we will not overwrite the
        ///     current handler and will never restore the original handler!
        /// </summary>
        public void Revert()
        {
            if (_installed)
            {
                Flush();
                _installed = false;
                this._outputFilter.OnUnInstall(_passthroughUtilities, _executionUtilities);
                _passthroughUtilities.DebugClient.FlushCallbacks();
                /* Previous version did an intelligent check to see if we needed to call this, but that was breaking stuff for IDebugOutputCallbacks2 */
                _executionUtilities.DebugClient.FlushCallbacks();
                _executionUtilities.DebugClient.SetOutputCallbacks(_previousCallbacks);
                _executionUtilities.DebugClient.FlushCallbacks();
                foreach (var line in _bufferedOutput)
                {
                    line.OutputLineMaskDisabled(_passthroughUtilities);
                }           
                _passthroughUtilities.OutputMaskRestore();
                _bufferedOutput.Clear();
            }
            if (_previousCallbacks != IntPtr.Zero)
            {
                Marshal.Release(_previousCallbacks);
                _previousCallbacks = IntPtr.Zero;
            }
            if (_thisIDebugOutputCallbacksPtr != IntPtr.Zero)
            {
                Marshal.Release(_thisIDebugOutputCallbacksPtr);
                _thisIDebugOutputCallbacksPtr = IntPtr.Zero;
            }
            _executionUtilities = null; /* No need to release, just a copy of what was sent in */
        }

        public class CallbackData
        {
            public int CurrentPosition;
            public DbgStringBuilder Data {get; set;}
            public DEBUG_OUTCBF DMLFlags {get; set;}
            public bool ForceOutputNow;
            public bool IsDML {get; set;}
            public DEBUG_OUTPUT Mask {get; set;}

            public CallbackData(DEBUG_OUTPUT Mask, DbgStringBuilder Data, bool isDML = true, DEBUG_OUTCBF dmlFlags = 0)
            {
                this.Mask = Mask;
                this.Data = Data;
                IsDML = isDML;
                DMLFlags = dmlFlags;
            }

            public void Clear()
            {
                Data.Clear();
                CurrentPosition = 0;
                ForceOutputNow = false;
            }

            public CallbackData Copy(DebugUtilities d)
            {
                var sb = new DbgStringBuilder(d, Data.Length);
                sb.Append(Data);
                return new CallbackData(Mask, sb, IsDML, DMLFlags);
            }

            public int OutputLine(DebugUtilities d)
            {
                if (string.IsNullOrEmpty(Data.ToString()))
                {
                    return S_OK;
                }
                DEBUG_OUTCTL outctl;
                if (d.IsFirstCommand)
                {
                    outctl = DEBUG_OUTCTL.ALL_CLIENTS;
                }
                else
                {
                    outctl = DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.NOT_LOGGED;
                }

                if (IsDML)
                {
                    outctl |= DEBUG_OUTCTL.DML;
                }

                return d.ControlledOutputWide(outctl, Mask, Data.ToString());
            }

            public int OutputLineMaskDisabled(DebugUtilities d)
            {
                if (string.IsNullOrEmpty(Data.ToString()))
                {
                    return S_OK;
                }
                DEBUG_OUTCTL outctl;
                if (d.IsFirstCommand)
                {
                    outctl = DEBUG_OUTCTL.ALL_CLIENTS;
                }
                else
                {
                    outctl = DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.NOT_LOGGED;
                }

                if (IsDML)
                {
                    outctl |= DEBUG_OUTCTL.DML;
                }

                d.DebugClient.FlushCallbacks();
                d.OutputMaskRestore();
                var hr = d.ControlledOutputWide(outctl, Mask, Data.ToString());
                d.OutputMaskDisableAll();
                d.DebugClient.FlushCallbacks();

                return hr;
            }
        }

        /// <summary>
        ///     An interface that can be used to filter/modify data that the output handler receives.
        /// </summary>
        public interface OUTPUT_FILTER
        {

            void OnInstall(DebugUtilities InstallUtilities, DebugUtilities FilterUtilities);
            void OnUnInstall(DebugUtilities InstallUtilities, DebugUtilities FilterUtilities);          

            /// <summary>
            ///     Called for every line of output. Will always be a single line ending with terminator(s), except for the last line
            ///     which is not guaranteed to have a CR and/or LF.
            /// </summary>
            /// <param name="inputData"></param>
            /// <returns>A string to pass to the rest of the output system, or null if there is nothing to process.</returns>
            CallbackData FilterText(CallbackData inputData);

            /// <summary>
            ///     Called for every output. If data doesnt end with CR and/or LF, it will queued up and passed in again when a
            ///     complete line is reached, unless ForceOutputNow is set.
            /// </summary>
            /// <param name="inputData">Input Data.  This should be modified and returned</param>
            /// <returns>A CallBackData structure representing the new data.</returns>
            CallbackData AdvancedFilterText(CallbackData inputData);

            /// <summary>
            ///     Called when the output handle is being removed so that any cached final data can be logged.
            /// </summary>
            /// <returns>A string to pass to the rest of the output system, or null if there is nothing to process.</returns>
            CallbackData Flush();
        }
    }
}