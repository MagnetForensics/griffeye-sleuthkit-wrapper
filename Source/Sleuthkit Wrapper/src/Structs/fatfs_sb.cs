using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit, Size = 512)]
    public struct fatfs_sb
    {
        [FieldOffset(40)] //really 43, but some shortcoming of marshlling in C# made this ugly hack nessecary
        fatfs_vol_lab vol_lab_f16;

        [FieldOffset(68)] //really 71, see above
        fatfs_vol_lab vol_lab_f32;

        internal String VolumeName16
        {
            get
            {
                return vol_lab_f16.Name;
            }
        }

        internal String VolumeName32
        {
            get
            {
                return vol_lab_f32.Name;
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct fatfs_vol_lab
    {
        byte padding1; //ugly hack
        byte padding2; //ugly hack
        byte padding3; //ugly hack

        [MarshalAsAttribute(UnmanagedType.ByValTStr, SizeConst = 11)]
        String vol_lab;

        public String Name
        {
            get 
            {
                return vol_lab;
            }
        }
    }
}
