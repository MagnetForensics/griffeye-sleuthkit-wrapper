using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
        Size = 456 + 16
    )]
    public struct ISO_INFO
    {
        [FieldOffset(0)]
        private TSK_FS_INFO fs_info;

        // Skipping som properties

        [FieldOffset(432 + 16)]
        private IntPtr pvd_ptr;

        internal TSK_FS_INFO tsk_fs_info => fs_info;

        public iso9660_pvd pvd => ((iso9660_pvd)Marshal.PtrToStructure(pvd_ptr, typeof(iso9660_pvd)));
    }
}