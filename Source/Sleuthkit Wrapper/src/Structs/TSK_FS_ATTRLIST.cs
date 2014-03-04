using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_FS_ATTRLIST
    {
        IntPtr head_ptr;

        public TSK_FS_ATTR head
        {
            get
            {
                return ((TSK_FS_ATTR)Marshal.PtrToStructure(head_ptr, typeof(TSK_FS_ATTR)));            
            }
        }
    }
}
