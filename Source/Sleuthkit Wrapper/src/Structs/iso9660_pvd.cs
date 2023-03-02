using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
        Size = 2056
    )]
    public struct iso9660_pvd
    {
        [FieldOffset(40)]
        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 32)]
        String vol_id;

        public String VolumeName
        {
            get
            {
                return vol_id;
            }
        }
    }
}
