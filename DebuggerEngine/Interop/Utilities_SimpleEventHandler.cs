using System;
using System.Runtime.InteropServices;

#pragma warning disable 1591

namespace Microsoft.Mex.DotNetDbg
{
    public partial class DebugUtilities
    {
        public class SimpleEventHandler : DebugBaseEventCallbacks, IDisposable
        {
            public delegate void BREAKPOINT_HANDLER(IDebugBreakpoint bp);

            private BREAKPOINT_HANDLER _breakpointHandler;
            public DEBUG_STATUS ExecutionStatus;
            private bool _installed;
            private IntPtr _previousCallbacks; /* this could be either an interface or a conversion thunk which doesn't support the interface */
            public bool SessionIsActive;
            private DebugUtilities _utilities;

            private SimpleEventHandler()
            {
                throw new NotImplementedException("This constructor should never be called");
            }

            private SimpleEventHandler(DebugUtilities debugUtilities)
            {
                _installed = false;
                _utilities = debugUtilities;
                SessionIsActive = false;
                ExecutionStatus = 0;
                _previousCallbacks = IntPtr.Zero;
                _breakpointHandler = null;
            }

            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            private static bool FAILED(int hr)
            {
                return (hr < 0);
            }

            public override int GetInterestMask(out DEBUG_EVENT mask)
            {
                mask = DEBUG_EVENT.CHANGE_ENGINE_STATE | DEBUG_EVENT.SESSION_STATUS | DEBUG_EVENT.BREAKPOINT;
                return S_OK;
            }

            public override int ChangeEngineState(DEBUG_CES flags, UInt64 argument)
            {
                if ((flags & DEBUG_CES.EXECUTION_STATUS) != 0)
                {
                    ExecutionStatus = (((DEBUG_STATUS)argument) & DEBUG_STATUS.MASK);

                    if ((((DEBUG_CES_EXECUTION_STATUS)argument) & DEBUG_CES_EXECUTION_STATUS.INSIDE_WAIT) == 0)
                    {
                        _utilities.DebugClient.ExitDispatch(_utilities.DebugClient);
                    }
                }
                return S_OK;
            }

            public override int SessionStatus(DEBUG_SESSION flags)
            {
                if (flags == DEBUG_SESSION.ACTIVE)
                {
                    SessionIsActive = true;
                }

                return (int)DEBUG_STATUS.NO_CHANGE;
            }

            public override int Breakpoint(IDebugBreakpoint Bp)
            {
                if (_breakpointHandler != null)
                {
                    _breakpointHandler(Bp);
                }
                return (int)DEBUG_STATUS.GO;
            }

            public static int Install(DebugUtilities debugUtilities, out SimpleEventHandler eventCallbacks, BREAKPOINT_HANDLER breakpointHandler = null)
            {
                var ec = new SimpleEventHandler(debugUtilities);
                if (breakpointHandler != null)
                {
                    ec._breakpointHandler = breakpointHandler;
                }
                IDebugEventCallbacks idec = ec;
                IntPtr unknownPtr = Marshal.GetIUnknownForObject(idec);
                IntPtr idecPtr;
                Guid guid = typeof(IDebugEventCallbacks).GUID;
                int hr = Marshal.QueryInterface(unknownPtr, ref guid, out idecPtr);
                if (FAILED(hr))
                {
                    ec.Dispose();
                    eventCallbacks = null;
                    return hr;
                }

                debugUtilities.DebugClient.GetEventCallbacks(out ec._previousCallbacks); /* We will need to release this */
                hr = debugUtilities.DebugClient.SetEventCallbacks(idecPtr);
                if (FAILED(hr))
                {
                    ec.Dispose();
                    eventCallbacks = null;
                    return hr;
                }

                ec._installed = true;
                eventCallbacks = ec;
                return hr;
            }

            public BREAKPOINT_HANDLER GetBreakpointHandler()
            {
                return _breakpointHandler;
            }

            public void SetBreakpointHandler(BREAKPOINT_HANDLER handler)
            {
                _breakpointHandler = handler;
            }

            public void RemoveBreakpointHandler()
            {
                _breakpointHandler = null;
            }

            private void Dispose(bool disposing)
            {
                if (_installed && disposing)
                {
                    _installed = false;
                    IntPtr currentCallbacks = IntPtr.Zero;
                    _utilities.DebugClient.GetEventCallbacks(out currentCallbacks); /* We need to release this */
                    if (this == Marshal.GetObjectForIUnknown(currentCallbacks))
                    {
                        _utilities.DebugClient.SetEventCallbacks(_previousCallbacks);
                    }
                    if (_previousCallbacks != IntPtr.Zero)
                    {
                        Marshal.Release(_previousCallbacks);
                        _previousCallbacks = IntPtr.Zero;
                    }
                    if (currentCallbacks != IntPtr.Zero)
                    {
                        Marshal.Release(currentCallbacks);
                        currentCallbacks = IntPtr.Zero;
                    }
                    _utilities = null;
                }
            }

            ~SimpleEventHandler()
            {
                Dispose(false);
            }
        }
    }
}