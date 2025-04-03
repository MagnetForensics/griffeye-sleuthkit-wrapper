using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    /// <summary>
    /// Dummy struct as a placeholder for tsk_lock_t struct in TSK
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct tsk_lock_t
    {
        public IntPtr DebugInfo;
        public int LockCount;
        public int RecursionCount;
        public IntPtr OwningThread;
        public IntPtr LockSemaphore;
        public UIntPtr SpinCount;
    }
}