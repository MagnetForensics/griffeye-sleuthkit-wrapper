using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_FS_META_NAME_LIST
    {
        /// <summary>
        /// Pointer to next name (or NULL)
        /// </summary>
        private IntPtr next;

        /// <summary>
        /// Name in UTF-8 (does not include parent directory name)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        private byte[] name;

        /// <summary>
        /// Inode address of parent directory (NTFS only)
        /// </summary>
        private ulong par_inode;

        /// <summary>
        /// Sequence number of parent directory (NTFS only)
        /// </summary>
        private uint par_seq;

        public bool HasNext => next != IntPtr.Zero;

        public TSK_FS_META_NAME_LIST Next
        {
            get
            {
                if (next != IntPtr.Zero)
                {
                    return (TSK_FS_META_NAME_LIST)Marshal.PtrToStructure(next, typeof(TSK_FS_META_NAME_LIST));
                }
                else
                {
                    throw new NullReferenceException();
                }
            }
        }

        public ulong ParentAddress => par_inode;

        public uint ParentSequence => par_seq;

        public String Name => Encoding.UTF8.GetString(name).Trim('\0');
    };
}