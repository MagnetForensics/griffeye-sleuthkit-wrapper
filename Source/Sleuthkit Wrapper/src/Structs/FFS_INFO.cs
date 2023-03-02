using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
    Size = 528
    )]
    public struct FFS_INFO
    {
        [FieldOffset(0)]
        TSK_FS_INFO fs_info;
        
        [FieldOffset(424)]
        IntPtr sb_ptr;

        internal TSK_FS_INFO tsk_fs_info
        {
            get
            {
                return fs_info;
            }
        }

        internal ffs_sb2 sb
        {
            get
            {
                return ((ffs_sb2)Marshal.PtrToStructure(sb_ptr, typeof(ffs_sb2)));
            }
        }
    }
}
