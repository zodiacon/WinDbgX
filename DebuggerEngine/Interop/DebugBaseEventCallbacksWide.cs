using System;

#pragma warning disable 1591

namespace Microsoft.Mex.DotNetDbg
{
    public abstract class DebugBaseEventCallbacksWide : IDebugEventCallbacksWide
    {
        /* The base implementation does not implement GetInterestMask, that must be done by the child class */
        public abstract int GetInterestMask(out DEBUG_EVENT Mask);

        public virtual int Breakpoint(IDebugBreakpoint2 Bp)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int Exception(EXCEPTION_RECORD64 Exception, UInt32 FirstChance)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int CreateThread(UInt64 Handle, UInt64 DataOffset, UInt64 StartOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ExitThread(UInt32 ExitCode)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int CreateProcess(
            UInt64 ImageFileHandle,
            UInt64 Handle,
            UInt64 BaseOffset,
            UInt32 ModuleSize,
            string ModuleName,
            string ImageName,
            UInt32 CheckSum,
            UInt32 TimeDateStamp,
            UInt64 InitialThreadHandle,
            UInt64 ThreadDataOffset,
            UInt64 StartOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ExitProcess(UInt32 ExitCode)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int LoadModule(
            UInt64 ImageFileHandle,
            UInt64 BaseOffset,
            UInt32 ModuleSize,
            string ModuleName,
            string ImageName,
            UInt32 CheckSum,
            UInt32 TimeDateStamp)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int UnloadModule(string ImageBaseName, UInt64 BaseOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int SystemError(UInt32 Error, UInt32 Level)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int SessionStatus(DEBUG_SESSION Status)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ChangeDebuggeeState(DEBUG_CDS Flags, UInt64 Argument)
        {
            return 0;
        }

        public virtual int ChangeEngineState(DEBUG_CES Flags, UInt64 Argument)
        {
            return 0;
        }

        public virtual int ChangeSymbolState(DEBUG_CSS Flags, UInt64 Argument)
        {
            return 0;
        }
    }
}