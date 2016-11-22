using System;

#pragma warning disable 1591

namespace Microsoft.Mex.DotNetDbg
{
    public abstract class DebugBaseEventCallbacks : IDebugEventCallbacks
    {
        /* The base implementation does not implement GetInterestMask, that must be done by the child class */
        public abstract int GetInterestMask(out DEBUG_EVENT mask);

        public virtual int Breakpoint(IDebugBreakpoint Bp)
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
            UInt64 imageFileHandle,
            UInt64 baseOffset,
            UInt32 moduleSize,
            string ModuleName,
            string imageName,
            UInt32 checkSum,
            UInt32 timeDateStamp)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int UnloadModule(string imageBaseName, UInt64 baseOffset)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int SystemError(UInt32 Error, UInt32 Level)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int SessionStatus(DEBUG_SESSION flags)
        {
            return (int)DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ChangeDebuggeeState(DEBUG_CDS flags, UInt64 argument)
        {
            return 0;
        }

        public virtual int ChangeEngineState(DEBUG_CES flags, UInt64 argument)
        {
            return 0;
        }

        public virtual int ChangeSymbolState(DEBUG_CSS flags, UInt64 argument)
        {
            return 0;
        }
    }
}