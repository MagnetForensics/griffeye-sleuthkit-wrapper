using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    [StructLayout(LayoutKind.Explicit,
    Size = 576
)]
    public struct EXT2FS_INFO
    {
        /// <summary>
        /// super class
        /// </summary>
        [FieldOffset(0)]
        TSK_FS_INFO fs_info;

        /// <summary>
        /// super block
        /// </summary>
        [FieldOffset(424)]
        IntPtr fs_ptr;

        /// <summary>
        /// lock protects grp_buf, grp_num, bmap_buf, bmap_grp_num, imap_buf, imap_grp_num
        /// </summary>
        [FieldOffset(432)]
        tsk_lock_t s_lock;
        
        [FieldOffset(472)]
        IntPtr v_grp_buf_ptr;
        
        [FieldOffset(480)]
        IntPtr ext4_grp_buf_ptr;

        /// <summary>
        /// cached group descriptor r/w shared - lock
        /// </summary>
        [FieldOffset(488)]
        IntPtr grp_buf_ptr;

        /// <summary>
        /// cached group number r/w shared - lock
        /// </summary>
        [FieldOffset(496)]
        ulong grp_num;

        /// <summary>
        /// cached block allocation bitmap r/w shared - lock
        /// </summary>
        [FieldOffset(504)]
        IntPtr bmap_buf_ptr;

        /// <summary>
        /// cached block bitmap nr r/w shared - lock
        /// </summary>
        [FieldOffset(512)]
        ulong bmap_grp_num;

        /// <summary>
        /// cached inode allocation bitmap r/w shared - lock
        /// </summary>
        [FieldOffset(520)]
        IntPtr imap_buf_ptr;

        /// <summary>
        /// cached inode bitmap nr r/w shared - lock
        /// </summary>
        [FieldOffset(528)]
        ulong imap_grp_num;

        /// <summary>
        /// offset to first group desc
        /// </summary>
        [FieldOffset(536)]
        long groups_offset;

        /// <summary>
        /// nr of descriptor group blocks
        /// </summary>
        [FieldOffset(544)]
        ulong groups_count;

        /// <summary>
        /// v1 or v2 of dentry
        /// </summary>
        [FieldOffset(552)]
        byte deentry_type;

        /// <summary>
        /// size of each inode
        /// </summary>
        [FieldOffset(554)]
        short inode_size;
        
        [FieldOffset(560)]
        ulong first_data_block;
        
        [FieldOffset(568)]
        IntPtr jinfo_ptr;
        
        internal TSK_FS_INFO tsk_fs_info
        {
            get
            {
                return fs_info;
            }
        }

        internal ext2fs_sb fs
        {
            get
            {
                return ((ext2fs_sb)Marshal.PtrToStructure(fs_ptr, typeof(ext2fs_sb)));
            }
        }
    }
}
