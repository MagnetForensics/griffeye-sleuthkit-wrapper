using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit, 
#if Bit32        
        Size = 312
#elif Bit64
        Size = 456
#endif
    )]
    public struct ISO_INFO
    {
        [FieldOffset(0)]
        TSK_FS_INFO fs_info;

        // Skipping som properties

#if Bit32
        [FieldOffset(296)]
#elif Bit64
        [FieldOffset(432)]
#endif
        IntPtr pvd_ptr;

        internal TSK_FS_INFO tsk_fs_info
        {
            get
            {
                return fs_info;
            }
        }

        public iso9660_pvd pvd
        {
            get
            {
                return ((iso9660_pvd)Marshal.PtrToStructure(pvd_ptr, typeof(iso9660_pvd)));                
            }
        }
    }
}
