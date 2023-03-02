using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
        Size = 17616
    )]
    public struct FATFS_INFO
    {
        [FieldOffset(0)]
        private TSK_FS_INFO fs_info;

        /// <summary>
        /// super block
        /// </summary>
        [FieldOffset(17024)]
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 512)]
        private byte[] boot_sector_buffer;

        internal TSK_FS_INFO tsk_fs_info
        {
            get
            {
                return fs_info;
            }
        }

        internal FATXXFS_SB sb
        {
            get
            {
                GCHandle handle = GCHandle.Alloc(boot_sector_buffer, GCHandleType.Pinned);
                FATXXFS_SB superblock = (FATXXFS_SB)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(FATXXFS_SB));
                handle.Free();
                return superblock;
            }
        }
    }
}