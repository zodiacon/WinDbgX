using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Mex.Framework;

namespace Microsoft.Mex.DotNetDbg
{
    [Serializable]
    public partial class DebugUtilities
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate int CreateClientDelegate(IntPtr oldClient, out IntPtr newClient);

        // We only ever need one of these...
        private static IntPtr _debugClientPtr = IntPtr.Zero;
        private static CreateClientDelegate _createClientDelegate;
        private static IDebugClient5 _staticClient5;

        private readonly bool _threadSafe;
        private Thread _myThread;
        public DEBUG_OUTCTL _outCtl = DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.NOT_LOGGED;
        private IDebugControl6 _debugControl6;
        private IDebugAdvanced3 _debugAdvanced;


        private IDebugClient5 _debugClient;


        private IDebugControl4 _debugControl;
        private IDebugDataSpaces4 _debugDataSpaces;
        private IDebugRegisters2 _debugRegisters;
        private IDebugSymbols3 _debugSymbols;
        private IDebugSymbols5 _debugSymbols5;
        private IDebugSystemObjects2 _debugSystemObjects;
        private bool _firstCommand;

        private static bool? _dmlCapable = null;
        private static bool? _preferDML = null;
        public bool IsFiltered = false;
        private bool _releaseClient = false;

        /// <summary>
        ///     Create a new DebugUtilities instance
        /// </summary>
        /// <param name="debugClient"></param>
        /// <param name="threadSafe">Make interfaces thread-neutral. Likely performance penalty involved.</param>
        public DebugUtilities(IDebugClient debugClient, bool threadSafe = false)
        {
            _myThread = Thread.CurrentThread;
            _threadSafe = threadSafe;
            if (_threadSafe == true)
            {
                InitThreadSafeStuff(debugClient);  // This has to be done before casting..
            }
            _debugClient = debugClient as IDebugClient5;
            if (_staticClient5 == null)
            {
                _staticClient5 = _debugClient;
            }
            if (_dmlCapable == null || _dmlCapable.Value == false)
            {
                _dmlCapable = IsDebuggerDMLCapable();
            }
            _preferDML = PreferDML();
          
        }


        /// <summary>
        ///     Create a new DebugUtilities instance
        /// </summary>
        /// <param name="debugClient"></param>
        public DebugUtilities(out IDebugClient debugClient)
        {
            _myThread = Thread.CurrentThread;
            int hr = _staticClient5.CreateClient(out debugClient);
            if (FAILED(hr))
            {
                OutputVerboseLine("DebugUtilities Failed creating a new debug client for execution: {0:x8}", hr);
                debugClient = null;
                return;
            }
            _releaseClient = true;
            _threadSafe = false;
           
            _debugClient = debugClient as IDebugClient5;
            if (_dmlCapable == null || _dmlCapable.Value == false)
            {
                _dmlCapable = IsDebuggerDMLCapable();
            }
            _preferDML = PreferDML();

        }

        ~DebugUtilities()
        {
            if (_releaseClient)
            {
                Marshal.ReleaseComObject(_debugClient);
                _debugClient = null;
            }
        }
        /// <summary>IDebugClient5</summary>
        public IDebugClient5 DebugClient
        {
            get
            {
                // If we are threadsafe AND aren't on the same thread anymore
                if (_threadSafe == true && _myThread != Thread.CurrentThread)
                {
                    InitNewDebugClientForThisThread();
                }

                return _debugClient;
            }
        }

        /// <summary>IDebugClient5</summary>
        public IDebugClient5 DebugClientForThreadInit
        {
            get
            {
                return _debugClient;
            }
        }


        /// <summary>IDebugControl4</summary>
        public IDebugControl4 DebugControl
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugControl ?? (_debugControl = _debugClient as IDebugControl4);
                }
                return DebugClient as IDebugControl4;
            }
        }

        /// <summary>IDebugControl6</summary>
        public IDebugControl6 DebugControl6
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugControl6 ?? (_debugControl6 = _debugClient as IDebugControl6);
                }
                return DebugClient as IDebugControl6;
            }
        }

        /// <summary>IDebugDataSpaces4</summary>
        public IDebugDataSpaces4 DebugDataSpaces
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugDataSpaces ?? (_debugDataSpaces = _debugClient as IDebugDataSpaces4);
                }
                return DebugClient as IDebugDataSpaces4;
            }
        }

        /// <summary>IDebugRegisters2</summary>
        public IDebugRegisters2 DebugRegisters
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugRegisters ?? (_debugRegisters = _debugClient as IDebugRegisters2);
                }
                return DebugClient as IDebugRegisters2;
            }
        }

        /// <summary>IDebugSymbols3</summary>
        public IDebugSymbols3 DebugSymbols
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugSymbols ?? (_debugSymbols = _debugClient as IDebugSymbols3);
                }
                return DebugClient as IDebugSymbols3;
            }
        }

        /// <summary>IDebugSymbols5</summary>
        public IDebugSymbols5 DebugSymbols5
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugSymbols5 ?? (_debugSymbols5 = _debugClient as IDebugSymbols5);
                }
                return DebugClient as IDebugSymbols5;
            }
        }

        /// <summary>IDebugSystemObjects2</summary>
        public IDebugSystemObjects2 DebugSystemObjects
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugSystemObjects ?? (_debugSystemObjects = _debugClient as IDebugSystemObjects2);
                }
                return DebugClient as IDebugSystemObjects2;
            }
        }

        /// <summary>IDebugAdvanced3</summary>
        public IDebugAdvanced3 DebugAdvanced
        {
            get
            {
                if (_threadSafe == false)
                {
                    return _debugAdvanced ?? (_debugAdvanced = _debugClient as IDebugAdvanced3);
                }
                return DebugClient as IDebugAdvanced3;
            }
        }

        public bool IsFirstCommand
        {
            get {return _firstCommand;}
            set
            {
                _firstCommand = value;

                _DML = DEBUG_OUTCTL.DML;
                _NODML = 0;

                if (value == false)
                {
                    _outCtl = DEBUG_OUTCTL.THIS_CLIENT | DEBUG_OUTCTL.NOT_LOGGED;
                }
                else
                {

                    _outCtl = DEBUG_OUTCTL.ALL_CLIENTS;
                    Cache.GetTypeId.ClearCache("DebugUtilities Created.");

                    if (Settings.Get("EnableAmbient") == true)
                    {
                        _outCtl = 0;
                        _NODML = DEBUG_OUTCTL.AMBIENT_TEXT;
                        _DML = DEBUG_OUTCTL.AMBIENT_DML;
                    }
                }
            }
        }

        public void InitNewDebugClientForThisThread()
        {
            // Reset our current thread and get a new IDebugClient for the current thread
            _myThread = Thread.CurrentThread;

            // Call CreateClient to get a thread-safe IDebugClient
            IntPtr newIDebugClientPtr;

            if (_createClientDelegate == null)
            {
               InitThreadSafeStuff(this.DebugClientForThreadInit);
            }

            if (_createClientDelegate == null)
            {
                throw new Exception("_createClientDelegate is null.  Did thread Safe initialization fail?");
            }

            int hr = _createClientDelegate(_debugClientPtr, out newIDebugClientPtr);
            if (FAILED(hr))
            {
                throw new Exception("Failed to _createClientDelegate");
            }
            IDebugClient newClient = null;
            if (newIDebugClientPtr != IntPtr.Zero)
            {
                newClient = (IDebugClient)Marshal.GetObjectForIUnknown(newIDebugClientPtr);
                Marshal.Release(newIDebugClientPtr);
            }

            _debugClient = newClient as IDebugClient5;
        }


        private void InitThreadSafeStuff(IDebugClient debugClient)
        {
            _myThread = Thread.CurrentThread;

            //
            // Save our DebugClientPtr address for later use (if it hasn't been done already)
            //
            if (_debugClientPtr == IntPtr.Zero ||
                _createClientDelegate == null)
            {
                IntPtr iUnknown = Marshal.GetIUnknownForObject(debugClient);

                Guid iDebugClientGuid = typeof(IDebugClient).GUID;

                int hr = Marshal.QueryInterface(iUnknown, ref iDebugClientGuid, out _debugClientPtr);
                if (hr != 0)
                {
                    throw new Exception();
                }
                try
                {
                    MemberInfo createClientMemberInfo = typeof(IDebugClient).GetMember("CreateClient", BindingFlags.Instance | BindingFlags.Public)[0];
                    int createClientComSlot = Marshal.GetComSlotForMethodInfo(createClientMemberInfo);
                    IntPtr iDebugClientVtbl = Marshal.ReadIntPtr(_debugClientPtr);
                    IntPtr createClientPtr = Marshal.ReadIntPtr(iDebugClientVtbl, IntPtr.Size*createClientComSlot);
                    _createClientDelegate = (CreateClientDelegate)Marshal.GetDelegateForFunctionPointer(createClientPtr, typeof(CreateClientDelegate));

                }
                finally
                {
                    // leaking this on purpose.  Trying to fix refcount bug.
                    //Marshal.Release(iUnknown);
                }
            }
        }
    }
}