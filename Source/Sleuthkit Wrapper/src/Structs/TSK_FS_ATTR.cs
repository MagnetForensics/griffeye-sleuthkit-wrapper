using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
#if Bit32
    Size = 104
#elif Bit64
    Size = 144
#endif
    )]
    public struct TSK_FS_ATTR
    {
        [FieldOffset(0)]
        IntPtr next_ptr;

#if Bit32
        [FieldOffset(4)]
#elif Bit64
        [FieldOffset(8)]
#endif
        IntPtr fs_file_ptr;

#if Bit32
        [FieldOffset(8)]
#elif Bit64
        [FieldOffset(16)]
#endif
        AttributeFlags flags;

#if Bit32
        [FieldOffset(12)]
#elif Bit64
        [FieldOffset(24)]
#endif
        IntPtr name_ptr;

#if Bit32
        [FieldOffset(16)]
#elif Bit64
        [FieldOffset(32)]
#endif
        int name_size;

#if Bit32
        [FieldOffset(20)]
#elif Bit64
        [FieldOffset(40)]
#endif
        AttributeType type;

#if Bit32
        [FieldOffset(24)]
#elif Bit64
        [FieldOffset(44)]
#endif
        ushort id;

#if Bit32
        [FieldOffset(32)]
#elif Bit64
        [FieldOffset(48)]
#endif
        long size;

#if Bit32
        [FieldOffset(40)]
#elif Bit64
        [FieldOffset(56)]
#endif
        IntPtr nrd_run_ptr;

#if Bit32
        [FieldOffset(44)]
#elif Bit64
        [FieldOffset(64)]
#endif
        IntPtr nrd_run_end_ptr;

#if Bit32
        [FieldOffset(48)]
#elif Bit64
        [FieldOffset(72)]
#endif
        uint nrd_skiplen;

#if Bit32
        [FieldOffset(56)]
#elif Bit64
        [FieldOffset(80)]
#endif
        long nrd_allocsize;

#if Bit32
        [FieldOffset(64)]
#elif Bit64
        [FieldOffset(88)]
#endif
        long nrg_initsize;

#if Bit32
        [FieldOffset(72)]
#elif Bit64
        [FieldOffset(96)]
#endif
        uint nrd_compsize;

#if Bit32
        [FieldOffset(80)]
#elif Bit64
        [FieldOffset(104)]
#endif
        IntPtr rd_buf_ptr;

#if Bit32
        [FieldOffset(84)]
#elif Bit64
        [FieldOffset(112)]
#endif
        int rd_buf_size;

#if Bit32
        [FieldOffset(88)]
#elif Bit64
        [FieldOffset(120)]
#endif
        long rd_offset;

        //Some more attributes here, but not wrapped

        public bool hasNext
        {
            get
            {
                return next_ptr != IntPtr.Zero;
            }
        }

        public TSK_FS_ATTR next
        {
            get
            {
                return ((TSK_FS_ATTR)Marshal.PtrToStructure(next_ptr, typeof(TSK_FS_ATTR)));
            }
        }

        public AttributeFlags AttributeFlags
        {
            get
            {
                return flags;
            }
        }

        public AttributeType AttributeType
        {
            get 
            {
                return type;
            }
        }

        public ushort Id
        {
            get
            {
                return id;
            }
        }

        public String rdBufString
        {
            get
            {
                return Marshal.PtrToStringUni(rd_buf_ptr, (int)(size / 2));
            }
        }
    }
}
