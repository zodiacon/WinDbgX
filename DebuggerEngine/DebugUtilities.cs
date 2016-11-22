using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace DebuggerEngine {
    unsafe static class Utilities {
        private static readonly CopyMemoryDelegate_IntPtr CopyMemory_IntPtr = (CopyMemoryDelegate_IntPtr)CreateCopyMemory(true, false);
        private static readonly CopyMemoryDelegate_IntPtr CopyMemory_IntPtr_Aligned = (CopyMemoryDelegate_IntPtr)CreateCopyMemory(true, true);
        private static readonly CopyMemoryDelegate_VoidPtr CopyMemory_VoidPtr = (CopyMemoryDelegate_VoidPtr)CreateCopyMemory(false, false);
        private static readonly CopyMemoryDelegate_VoidPtr CopyMemory_VoidPtr_Aligned = (CopyMemoryDelegate_VoidPtr)CreateCopyMemory(false, true);

        public static void CopyMemory(IntPtr dest, IntPtr source, int size) {
            CopyMemory_IntPtr(dest, source, (uint)size);
        }

        public static void CopyMemory(IntPtr dest, IntPtr source, uint size) {
            CopyMemory_IntPtr(dest, source, size);
        }

        public static void CopyMemory(void* dest, void* source, int size) {
            CopyMemory_VoidPtr(dest, source, (uint)size);
        }

        public static void CopyMemory(void* dest, void* source, uint size) {
            CopyMemory_VoidPtr(dest, source, size);
        }

        private delegate void CopyMemoryDelegate_VoidPtr(void* dest, void* source, uint size);

        private delegate void CopyMemoryDelegate_IntPtr(IntPtr dest, IntPtr source, uint size);

        private static Delegate CreateCopyMemory(bool useIntPtr, bool aligned) {
            /* Have to specify an owning module or type; otherwise the function is "anonymous" and we will get a verification exception when using pointers */
            var m = Assembly.GetExecutingAssembly().ManifestModule;
            var pointerType = useIntPtr ? typeof(IntPtr) : typeof(void*);
            var delegateType = useIntPtr ? typeof(CopyMemoryDelegate_IntPtr) : typeof(CopyMemoryDelegate_VoidPtr);
            Type[] parameterTypes = { pointerType, pointerType, typeof(uint) };
            var dm = new DynamicMethod("CopyMemory", null, parameterTypes, m, true);
            var ilg = dm.GetILGenerator();

            ilg.Emit(OpCodes.Ldarg_0);
            ilg.Emit(OpCodes.Ldarg_1);
            ilg.Emit(OpCodes.Ldarg_2);
            if(aligned == false) {
                ilg.Emit(OpCodes.Unaligned, (byte)1);
            }
            ilg.Emit(OpCodes.Cpblk);
            ilg.Emit(OpCodes.Ret);

            return dm.CreateDelegate(delegateType);
        }

        public static ulong SignExtendAddress(uint address) {
            return (ulong)(int)address;
        }

        public static ulong SignExtendAddress(ulong address) {
            return (ulong)(int)address;
        }

    }
}

