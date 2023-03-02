using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    /// <summary>
    /// struct TSK_FS_INFO
    /// </summary>
    [StructLayout(LayoutKind.Explicit,
    Size = 424
)]
    public struct TSK_FS_INFO
    {
        /// <summary>
        /// tag, can be used to validate that we have the right kind of struct.  a magic header for the struct, essentially.
        /// </summary>
        [FieldOffset(0)]
        private StructureMagic tag;

        /// <summary>
        /// pointer to imageinfo struct
        /// </summary>
        [FieldOffset(8)]
        private IntPtr img_info_ptr;

        /// <summary>
        /// TSK_OFF_T offset, Byte offset into img_info that fs starts
        /// </summary>
        [FieldOffset(16)]
        private long offset;

        /* meta data */

        /// <summary>
        /// number of metadata addresses
        /// </summary>
        [FieldOffset(24)]
        internal long inum_count;

        /// <summary>
        /// address of root directory
        /// </summary>
        [FieldOffset(32)]
        internal long root_inum;

        /// <summary>
        /// address of first metadata
        /// </summary>
        [FieldOffset(40)]
        internal long first_inum;

        /// <summary>
        /// address of last metadata
        /// </summary>
        [FieldOffset(48)]
        internal long last_inum;

        /* content */

        /// <summary>
        /// Number of blocks in fs
        /// </summary>
        [FieldOffset(56)]
        internal long block_count;

        /// <summary>
        /// Address of first block
        /// </summary>
        [FieldOffset(64)]
        internal long first_block;

        /// <summary>
        /// Address of last block as reported by file system (could be larger than last_block in image if end of image does not exist)
        /// </summary>
        [FieldOffset(72)]
        internal long last_block;

        /// <summary>
        /// Address of last block -- adjusted so that it is equal to the last block in the image or volume (if image is not complete)
        /// </summary>
        [FieldOffset(80)]
        internal long last_block_act;

        /// <summary>
        /// Size of each block (in bytes)
        /// </summary>
        [FieldOffset(88)]
        internal int block_size;

        /// <summary>
        /// Size of device block (typically always 512)
        /// </summary>
        [FieldOffset(92)]
        internal int dev_bsize;

        /// <summary>
        /// Number of bytes that preceed each block (currently only used for RAW CDs)
        /// </summary>
        [FieldOffset(96)]
        internal uint block_pre_size;

        /// <summary>
        /// Number of bytes that follow each block (currently only used for RAW CDs)
        /// </summary>
        [FieldOffset(100)]
        internal uint block_post_size;

        /// <summary>
        /// Address of journal inode
        /// </summary>
        [FieldOffset(104)]
        private ulong journ_inum;

        /// <summary>
        /// type of file system
        /// </summary>
        [FieldOffset(112)]
        private FileSystemType ftype;

        /// <summary>
        /// string "name" of data unit type
        /// </summary>
        [FieldOffset(120)]
        private IntPtr duname_ptr;

        /// <summary>
        /// flags
        /// </summary>
        [FieldOffset(128)]
        private FilesystemInfoFlag flags;

        /// <summary>
        /// File system id (as reported in boot sector)
        /// </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        [FieldOffset(132)]
        private IntPtr fs_id_ptr; //uint8_t[] fs_id;

        /// <summary>
        /// fs id used
        /// </summary>
        [FieldOffset(168)]
        private UIntPtr fs_id_used;
        
        [FieldOffset(176)]
        private Endianness endian;

        /// <summary>
        /// taken when r/w the list_inum_named list
        /// </summary>
        [FieldOffset(184)]
        private tsk_lock_t list_inum_named_lock;

        /// <summary>
        /// List of unallocated inodes that
        /// </summary>
        [FieldOffset(224)]
        private IntPtr list_inum_named_ptr; //TSK_LIST *list_inum_named;

        /// <summary>
        /// taken for the duration of orphan hunting (not just when updating orphan_dir)
        /// </summary>
        [FieldOffset(232)]
        private tsk_lock_t orphan_dir_lock;

        /// <summary>
        /// Files and dirs in the top level of the $OrphanFiles directory.  NULL if orphans have not been hunted for yet.
        /// </summary>
        [FieldOffset(272)]
        private IntPtr orphan_dir_ptr; //TSK_FS_DIR *orphan_dir;

        #region methods
        
        [FieldOffset(280)]
        private IntPtr block_walk_ptr;
        
        [FieldOffset(288)]
        private IntPtr block_getflags_ptr;
        
        [FieldOffset(296)]
        private IntPtr inode_Walk_ptr;
        
        [FieldOffset(304)]
        private IntPtr file_add_meta_ptr;
        
        [FieldOffset(312)]
        private IntPtr get_default_attr_type_ptr;
        
        [FieldOffset(320)]
        private IntPtr load_attrs_ptr;
        
        [FieldOffset(328)]
        private IntPtr decrypt_block_ptr;
        
        [FieldOffset(336)]
        private IntPtr istat_ptr;
        
        [FieldOffset(344)]
        private IntPtr dir_open_meta_ptr;
        
        [FieldOffset(352)]
        private IntPtr jopen_ptr;
        
        [FieldOffset(360)]
        private IntPtr jblk_walk_ptr;
        
        [FieldOffset(368)]
        private IntPtr jentry_walk_ptr;

        /*
        public fsstatDelegate fsstat;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public unsafe delegate byte fsstatDelegate(IntPtr fs, FILE* hFile);
        //*/
        
        [FieldOffset(376)]
        private IntPtr fsstat_ptr;
        
        [FieldOffset(384)]
        private IntPtr name_cmp_ptr;
        
        [FieldOffset(392)]
        private IntPtr fscheck_ptr;
        
        [FieldOffset(400)]
        private IntPtr close_ptr;
        
        [FieldOffset(408)]
        private IntPtr fread_owner_sid_ptr;
        
        [FieldOffset(416)]
        private IntPtr impl_ptr;

        #endregion methods

        /// <summary>
        /// Returns nothing at the moment
        /// </summary>
        // TODO: get the ID working.
        public string ID
        {
            get
            {
                string ret = null;
                //if (ptr_fs_id != IntPtr.Zero && fs_id_used!=UIntPtr.Zero)
                //{
                //    int btr = (int)fs_id_used.ToUInt32();
                //    byte[] buf = new byte[btr];
                //    Marshal.Copy(ptr_fs_id, buf, 0, btr);
                //    ret = Encoding.ASCII.GetString(buf, 0, btr);

                //}
                return ret;
            }
        }

        /// <summary>
        /// validates the tag contains the proper constant
        /// </summary>
        public bool AppearsValid
        {
            get
            {
                return this.tag == StructureMagic.FilesystemInfoTag;
            }
        }

        /// <summary>
        /// Image information
        /// </summary>
        public TSK_IMG_INFO ImageInfo
        {
            get
            {
                var ret = new TSK_IMG_INFO();
                if (img_info_ptr != IntPtr.Zero)
                {
                    ret = (TSK_IMG_INFO)Marshal.PtrToStructure(img_info_ptr, typeof(TSK_IMG_INFO));
                }
                return ret;
            }
        }

        /// <summary>
        /// the offset where the filesystem starts
        /// </summary>
        public long Offset
        {
            get
            {
                return this.offset;
            }
        }

        /// <summary>
        /// the type of filesystem
        /// </summary>
        public FileSystemType FilesystemType
        {
            get
            {
                return this.ftype;
            }
        }

        public Endianness Endian
        {
            get
            {
                return endian;
            }
        }
    }
}