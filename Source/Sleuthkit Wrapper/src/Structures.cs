using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace SleuthKit
{
    /// <summary>
    /// represents TSK_FS_FILE.  Generic structure used to refer to files in the file system.  A file will typically have a name and metadata.  This structure holds that type of information.
    /// When deleted files are being processed, this structure may have the name defined but not metadata because it no longer exists. 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FileStruct
    {
        public static FileStruct FromStream(Stream stream)
        {
            var ret = new FileStruct();
            var br = new BinaryReader(stream);
            ret.tag = (StructureMagic) br.ReadUInt32();
            return ret;
        }

        /// <summary>
        /// tag, can be used to validate that we have the right kind of struct.  a magic header for the struct, essentially.
        /// </summary>
        StructureMagic tag;

        /// <summary>
        /// pointer to filename struct - or null if file was opened using metadata address
        /// </summary>
        IntPtr ptr_filename;

        /// <summary>
        /// Pointer to metadata of file (or NULL if name has invalid metadata address)
        /// </summary>
        IntPtr ptr_meta;

        /// <summary>
        /// Pointer to file system that the file is located in.
        /// </summary>
        IntPtr ptr_fsinfo;

        /// <summary>
        /// filename
        /// </summary>
        public NameStruct? Name
        {
            get
            {
                NameStruct? ret = null;
                if (ptr_filename != IntPtr.Zero)
                {
                    ret = (NameStruct)Marshal.PtrToStructure(ptr_filename, typeof(NameStruct));
                }
                return ret;
            }
        }

        /// <summary>
        /// metadata
        /// </summary>
        public FileMetadata? Metadata
        {
            get
            {
                FileMetadata? ret = null;
                if (ptr_fsinfo != IntPtr.Zero)
                {
                    ret = (FileMetadata)Marshal.PtrToStructure(ptr_meta, typeof(FileMetadata));
                }
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
                return tag == StructureMagic.FilesystemFileTag;
            }
        }
    }

    /// <summary>
    /// TSK_FS_NAME Generic structure to store the file name information that is stored in a directory. Most file systems seperate the file name from the metadata, but some do not (such as FAT). This structure contains the name and address of the metadata.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct NameStruct
    {
        /// <summary>
        /// tag, can be used to validate that we have the right kind of struct.  a magic header for the struct, essentially.
        /// </summary>
        StructureMagic tag;

        /// <summary>
        /// The name of the file (in UTF-8)
        /// </summary>
        IntPtr ptr_name;

        /// <summary>
        /// The number of bytes allocated to name
        /// </summary>
        UIntPtr name_size;

        /// <summary>
        /// The short name of the file or null (in UTF-8)
        /// </summary>
        IntPtr ptr_short_name;

        /// <summary>
        /// The number of bytes allocated to shrt_name
        /// </summary>
        UIntPtr short_name_size;

        /// <summary>
        /// Address of the metadata structure that the name points to. 
        /// </summary>
        ulong meta_addr;

        /// <summary>
        /// Sequence number for metadata structure (NTFS only) 
        /// </summary>
        uint meta_seq;

        /// <summary>
        /// Metadata address of parent directory (equal to meta_addr if this entry is for root directory). 
        /// </summary>
        ulong par_addr;

        /// <summary>
        /// File type information (directory, file, etc.)
        /// </summary>
        FilesystemNameType type;

        /// <summary>
        /// Flags that describe allocation status etc. 
        /// </summary>
        NameFlags flags;

        /// <summary>
        /// validates the tag contains the proper constant
        /// </summary>
        public bool AppearsValid
        {
            get
            {
                return this.tag == StructureMagic.FilesystemNameTag;
            }
        }

        /// <summary>
        /// The filename
        /// </summary>
        public string LongName
        {
            get
            {
                string str = null;
                var ns = (int)name_size.ToUInt32();
                if (ns > 0)
                {
                    var local = new byte[ns];
                    Marshal.Copy(ptr_name, local, 0, local.Length);
                    if (local[ns - 1] == 0)
                    {
                        ns--;//trim it
                    }
                    str = Encoding.UTF8.GetString(local, 0, ns);
                }
                return str;
            }
        }

        /// <summary>
        /// The short name, if any
        /// </summary>
        public string ShortName
        {
            get
            {
                string str = null;
                uint sns = short_name_size.ToUInt32();
                if (sns > 0)
                {
                    var local = new byte[short_name_size.ToUInt32()];
                    Marshal.Copy(ptr_short_name, local, 0, local.Length);
                    str = Encoding.UTF8.GetString(local);
                }
                return str;
            }
        }

        /// <summary>
        /// returns the long name or short name, depending on whats available.
        /// </summary>
        /// <returns></returns>
        public string GetName()
        {
            string name = null;

            //try long name
            if (ptr_name != IntPtr.Zero)
            {
                name = this.LongName;
            }
            //long name was null try shortname
            if (name == null && ptr_short_name != IntPtr.Zero)
            {
                name = this.ShortName;
            }

            //go back to the base impl, which sucks but its better than nothing 
            if (name == null)
            {
                name = base.ToString();
            }

            if (name.IndexOf((char)0) > -1)
            {
                name = name.Replace("\0", "");
            }

            return name;
        }

        /// <summary>
        /// Prints out the name, long name first, if it is null then it tries short name.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var ln = this.LongName;
            string n = null;
            if (ln == null)
            {
                n = this.ShortName;
            }
            else
            {
                n = ln;
            }
            return n;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct TSK_FS_META_NAME_LIST
    {
        /// <summary>
        /// Pointer to next name (or NULL)
        /// </summary>
        IntPtr next;

        /// <summary>
        /// Name in UTF-8 (does not include parent directory name)
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
        byte[] name;

        /// <summary>
        /// Inode address of parent directory (NTFS only)
        /// </summary>
        ulong par_inode;

        /// <summary>
        /// Sequence number of parent directory (NTFS only)
        /// </summary>
        uint par_seq;
    };

    /// <summary>
    /// represents TSK_FS_META
    /// </summary>
    [StructLayout(LayoutKind.Sequential), DebuggerDisplay("TSK_FS_META inode={addr}")]
    public struct FileMetadata
    {
        #region fields
        /// <summary>
        /// tag, can be used to validate that we have the right kind of struct.  a magic header for the struct, essentially.
        /// </summary>
        StructureMagic tag;

        /// <summary>
        /// Flags for this file for its allocation status etc.
        /// </summary>
        internal MetadataFlags flags;

        /// <summary>
        /// Address of the meta data structure for this file
        /// </summary>
        internal long addr;

        /// <summary>
        /// File type
        /// </summary>
        internal MetadataType type;

        /// <summary>
        /// Unix-style permissions
        /// </summary>
        internal MetadataMode mode;

        /// <summary>
        /// link count (number of file names pointing to this)
        /// </summary>
        long nlink_count;

        /// <summary>
        /// file size (in bytes) - yes this is a signed 64-bit integer, despite it being unsigned in sleuthkit (?).  It is easier this way.
        /// </summary>
        long size;

        /// <summary>
        /// user id
        /// </summary>
        uint uid;

        /// <summary>
        /// group id
        /// </summary>
        uint gid;

        #region times

        /// <summary>
        /// last file content modification time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        long mtime;

        /// <summary>
        /// nano-second resolution in addition to m_time
        /// </summary>
        long mtime_nano;

        /// <summary>
        /// last file content accessed time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        long atime;

        /// <summary>
        /// nano-second resolution in addition to a_time
        /// </summary>
        long atime_nano;

        /// <summary>
        ///  last file / metadata status change time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        long ctime;

        /// <summary>
        /// nano-second resolution in addition to c_time
        /// </summary>
        long ctime_nano;

        /// <summary>
        /// Created time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        long crtime;

        /// <summary>
        /// nano-second resolution in addition to cr_time
        /// </summary>
        long crtime_nano;

        #region filesystem specific times

        int special_time;

        uint special_nano;

        #endregion

        #endregion

        /// <summary>
        /// Pointer to file system specific data that is used to store references to file content
        /// </summary>
        IntPtr content_ptr;

        /// <summary>
        /// size of content  buffer
        /// </summary>
        UIntPtr content_len;

        /// <summary>
        /// The content type
        /// </summary>
        FileSystemMetaContentType content_type;   

        /// <summary>
        /// Sequence number for file (NTFS only, is incremented when entry is reallocated) 
        /// </summary>
        uint seq;

        /// <summary>
        /// Contains run data on the file content (specific locations where content is stored).  
        /// Check attr_state to determine if data in here is valid because not all file systems 
        /// load this data when a file is loaded.  It may not be loaded until needed by one
        /// of the APIs. Most file systems will have only one attribute, but NTFS will have several. 
        /// </summary>
        IntPtr attrlist; //TSK_FS_ATTRLIST *attr;

        /// <summary>
        /// State of the data in the TSK_FS_META::attr structure
        /// </summary>
        MetadataAttributeFlags attr_state;

        /// <summary>
        /// Name of file stored in metadata (FAT and NTFS Only)
        /// </summary>
        IntPtr name2;  //TSK_FS_META_NAME_LIST* name2;   ///< 

        //char* link;             
        /// <summary>
        /// Name of target file if this is a symbolic link
        /// </summary>
        IntPtr link;

        #endregion

        #region properties

        /// <summary>
        /// validates the tag contains the proper constant
        /// </summary>
        public bool AppearsValid
        {
            get
            {
                return this.tag == StructureMagic.FilesystemMetadataTag;
            }
        }

        /// <summary>
        /// Metadata Address
        /// </summary>
        public long Address
        {
            get
            {
                return this.addr;
            }
        }

        /// <summary>
        /// Metadata flags
        /// </summary>
        public MetadataFlags MetadataFlags
        {
            get
            {
                return this.flags;
            }
        }

        /// <summary>
        /// Metadata type
        /// </summary>
        public MetadataType MetadataType
        {
            get
            {
                return this.type;
            }
        }

        /// <summary>
        /// File size, in bytes
        /// </summary>
        public long Size
        {
            get
            {
                return this.size;
            }
        }

        /// <summary>
        /// Modified time in unix epoch ticks
        /// </summary>
        public long MTime
        {
            get
            {
                return (long)this.mtime;
            }
        }

        /// <summary>
        /// Access time in unix epoch ticks
        /// </summary>
        public long ATime
        {
            get
            {
                return (long)this.atime;
            }
        }

        /// <summary>
        /// Metadata change time in unix epoch ticks
        /// </summary>
        public long CTime
        {
            get
            {
                return (long)this.ctime;
            }
        }


        /// <summary>
        /// Kreayshawn time in unix epoch ticks
        /// </summary>
        public long CRTime
        {
            get
            {
                return (long)this.crtime;
            }
        }

        public uint Seq
        {
            get
            {
                return this.seq;
            }
        }

        //public DateTime LastAccessed
        //{
        //    get
        //    {
        //        var val = this.atime.ToUInt64();
        //        return DateTime.MinValue;
        //    }
        //}

        #endregion
    }

    /// <summary>
    /// Dummy struct as a placeholder for tsk_lock_t struct in TSK 
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct PlaceholderStruct
    {
        /// <summary>
        /// The placeholder1.
        /// </summary>
        internal int placeholder1;
        
        /// <summary>
        /// The placeholder2
        /// </summary>
        internal int placeholder2;

        /// <summary>
        /// The placeholder3
        /// </summary>
        internal int placeholder3;

        /// <summary>
        /// The placeholder4
        /// </summary>
        internal int placeholder4;

        /// <summary>
        /// The placeholder5
        /// </summary>
        internal int placeholder5;

        /// <summary>
        /// The placeholder6
        /// </summary>
        internal int placeholder6;
    }


    /// <summary>
    /// represents TSK_IMG_INFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct ImageInfo
    {
        /// <summary>
        /// Set to TSK_IMG_INFO_TAG when struct is alloc
        /// </summary>
        internal uint tag; 

        /// <summary>
        /// Type of disk image format
        /// </summary>
        internal ImageType itype;

        /// <summary>
        /// Total size of image in bytes
        /// </summary>
        internal long size;

        /// <summary>
        /// sector size of device in bytes (typically 512)
        /// </summary>
        internal uint sector_size;

        /// <summary>
        /// page size of NAND page in bytes (defaults to 2048)
        /// </summary>
        internal uint page_size;     

        /// <summary>
        /// spare or OOB size of NAND in bytes (defaults to 64)
        /// </summary>
        internal uint spare_size;     

        /// <summary>
        /// Place holder for lock of cache and associated values
        /// </summary>
        internal PlaceholderStruct cacheLock; 

        //#define TSK_IMG_INFO_CACHE_NUM  4
        //#define TSK_IMG_INFO_CACHE_LEN  65536
        IntPtr cache;//        char[][] cache[4][65536];     ///< read cache

        #region function pointers

        IntPtr cache_off;//TSK_OFF_T cache_off[TSK_IMG_INFO_CACHE_NUM];    ///< starting byte offset of corresponding cache entry
        IntPtr cache_age; //int cache_age[TSK_IMG_INFO_CACHE_NUM];  ///< "Age" of corresponding cache entry, higher means more recently used
        IntPtr cache_len; //size_t cache_len[TSK_IMG_INFO_CACHE_NUM];       ///< Length of cache entry used (0 if never used)

        IntPtr read;//ssize_t(*read) (TSK_IMG_INFO * img, TSK_OFF_T off, char *buf, size_t len);     ///< \internal External progs should call tsk_img_read() 
        IntPtr close;//void (*close) (TSK_IMG_INFO *); ///< \internal Progs should call tsk_img_close()
        IntPtr imgstat;//void (*imgstat) (TSK_IMG_INFO *, FILE *);       ///< Pointer to file type specific function 

        #endregion

        /// <summary>
        /// Checks that size and sector_size are non-zero.
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return size != 0 && sector_size != 0;
            }
        }
    };

    /// <summary>
    /// represents TSK_VS_INFO.  Data structure used to store state and basic information for open volume systems.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VolumeSystemInfo
    {
        /// <summary>
        /// Will be set to TSK_VS_INFO_TAG if structure is still allocated.
        /// </summary>
        int tag;    
        
        /// <summary>
        /// Pointer to disk image that VS is in
        /// </summary>
        IntPtr ptr_imageInfo; //TSK_IMG_INFO *img_info; 

        /// <summary>
        /// Type of volume system / media management
        /// </summary>
        VolumeSystemType vstype;

        /// <summary>
        /// Byte offset where VS starts in disk image
        /// </summary>
        long offset;

        /// <summary>
        /// Size of blocks in bytes
        /// </summary>
        int block_size;

        /// <summary>
        /// Endian ordering of data
        /// </summary>
        Endianness endian;

        /// <summary>
        /// Linked list of partitions
        /// </summary>
        internal IntPtr ptr_first_volumeinfo;// TSK_VS_PART_INFO *part_list;    // 

        /// <summary>
        /// number of partitions 
        /// </summary>
        int part_count;

        //void (*close) (TSK_VS_INFO *);  // \internal Progs should call tsk_vs_close().
        IntPtr funcptr_close;

        /// <summary>
        /// The offset to the start of this volume system
        /// </summary>
        public long Offset
        {
            get
            {
                return offset;
            }
        }

        /// <summary>
        /// The type of volume system (MBR, APM, GPT, etc)
        /// </summary>
        public VolumeSystemType Type
        {
            get
            {
                return vstype;
            }
        }

        /// <summary>
        /// The endianness of this volume system (little, big, etc)
        /// </summary>
        public Endianness Endianness
        {
            get
            {
                return this.endian;
            }
        }

        /// <summary>
        /// The number of partitions on this volume system
        /// </summary>
        public int PartitionCount
        {
            get
            {
                return part_count;
            }
        }

        /// <summary>
        /// The number of blocks
        /// </summary>
        public int BlockSize
        {
            get
            {
                return block_size;
            }
        }

        /// <summary>
        /// First volume info
        /// </summary>
        internal VolumeInfo? FirstVolumeInfo
        {
            get
            {
                return VolumeInfo.FromIntPtr(this.ptr_first_volumeinfo);
            }
        }

        /// <summary>
        /// All volume infos
        /// </summary>
        internal IEnumerable<VolumeInfo> VolumeInfos
        {
            get
            {
                var first = this.FirstVolumeInfo;

                var cur = first;
                while (cur.HasValue)
                {
                    yield return cur.Value;
                    cur = cur.Value.Next;
                }
            }
        }
    }

    /// <summary>
    /// TSK_VS_PART_INFO
    /// </summary>
    public struct VolumeInfo
    {
        /// <summary>
        /// Tag of TSK_VS_PART_INFO
        /// </summary>
        int tag; 

        /// <summary>
        /// Pointer to previous partition (or NULL)
        /// </summary>
        internal IntPtr ptr_prev_part;// TSK_VS_PART_INFO* prev; 

        /// <summary>
        /// Pointer to next partition (or NULL)
        /// </summary>
        internal IntPtr ptr_next_part;// TSK_VS_PART_INFO* next; 

        /// <summary>
        /// Pointer to parent volume system handle
        /// </summary>
        private IntPtr ptr_vs_info; //TSK_VS_INFO* vs;        

        /// <summary>
        /// Sector offset of start of partition
        /// </summary>
        private long start;

        /// <summary>
        /// Number of sectors in partition
        /// </summary>
        private long len;

        /// <summary>
        ///  UTF-8 description of partition (volume system type-specific)
        /// </summary>
        private IntPtr ptr_utf8desc;// char* desc;

        /// <summary>
        /// Table address that describes this partition
        /// </summary>
        private byte table_num;

        /// <summary>
        /// Entry in the table that describes this partition
        /// </summary>
        private byte slot_num;

        /// <summary>
        /// Address of this partition
        /// </summary>
        private uint addr;

        /// <summary>
        /// Flags for partition
        /// </summary>
        private VolumeFlags flags;


        /// <summary>
        /// Sector offset of start of partition. 
        /// </summary>
        public long SectorOffset
        {
            get
            {
                return this.start;
            }
        }

        /// <summary>
        /// Size in bytes of this volume
        /// </summary>
        public long SectorLength
        {
            get
            {
                return this.len;
            }
        }

        /// <summary>
        /// Table address that describes this partition. 
        /// </summary>
        public int TableNumber
        {
            get
            {
                return table_num;
            }
        }

        /// <summary>
        /// Table address that describes this partition. 
        /// </summary>
        public int SlotNumber
        {
            get
            {
                return slot_num;
            }
        }

        /// <summary>
        /// Address of this partition
        /// </summary>
        public uint Address
        {
            get
            {
                return addr;
            }
        }

        /// <summary>
        /// Flags.. what more can you say?
        /// </summary>
        public VolumeFlags Flags
        {
            get
            {
                return this.flags;
            }
        }

        internal static VolumeInfo? FromIntPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            else
                return (VolumeInfo)Marshal.PtrToStructure(ptr, typeof(VolumeInfo));
        }

        /// <summary>
        /// Next volume, if any
        /// </summary>
        public VolumeInfo? Next
        {
            get
            {
                return FromIntPtr(this.ptr_next_part);
            }
        }

        /// <summary>
        /// Previous volume, if any
        /// </summary>
        public VolumeInfo? Previous
        {
            get
            {
                return FromIntPtr(this.ptr_prev_part);
            }
        }

        /// <summary>
        /// Volume description
        /// </summary>
        public string Description
        {
            get
            {
                string ret = null;

                if (ptr_utf8desc != IntPtr.Zero)
                {
                    UTF8Marshaler mars = new UTF8Marshaler();
                    ret = (string)mars.MarshalNativeToManaged(ptr_utf8desc);
                }
                return ret;
            }
        }
    };

    /// <summary>
    /// struct TSK_FS_INFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FileSystemInfo
    {
        /// <summary>
        /// tag, can be used to validate that we have the right kind of struct.  a magic header for the struct, essentially.
        /// </summary>
        StructureMagic tag;

        /// <summary>
        /// pointer to imageinfo struct
        /// </summary>
        IntPtr ptr_imageinfo;

        /// <summary>
        /// TSK_OFF_T offset, Byte offset into img_info that fs starts 
        /// </summary>
        long offset;

        /* meta data */

        /// <summary>
        /// number of metadata addresses
        /// </summary>
        internal long inum_count;

        /// <summary>
        /// address of root directory
        /// </summary>
        internal long root_inum;

        /// <summary>
        /// address of first metadata
        /// </summary>
        internal long first_inum;

        /// <summary>
        /// address of last metadata
        /// </summary>
        internal long last_inum;

        /* content */

        /// <summary>
        /// Number of blocks in fs
        /// </summary>
        internal long block_count;

        /// <summary>
        /// Address of first block
        /// </summary>
        internal long first_block;

        /// <summary>
        /// Address of last block as reported by file system (could be larger than last_block in image if end of image does not exist)
        /// </summary>
        /// 
        internal long last_block;

        /// <summary>
        /// Address of last block -- adjusted so that it is equal to the last block in the image or volume (if image is not complete)
        /// </summary>
        internal long last_block_act;

        /// <summary>
        /// Size of each block (in bytes)
        /// </summary>
        internal int block_size;

        /// <summary>
        /// Size of device block (typically always 512)
        /// </summary>
        internal int dev_bsize;

        /// <summary>
        /// Address of journal inode
        /// </summary>
        ulong journ_inum;

        /// <summary>
        /// type of file system 
        /// </summary>
        FileSystemType fstype;

        /// <summary>
        /// string "name" of data unit type 
        /// </summary>
        IntPtr ptr_DataUnitName;

        /// <summary>
        /// flags
        /// </summary>
        FilesystemInfoFlag flags;

        /// <summary>
        /// File system id (as reported in boot sector)
        /// </summary>
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        IntPtr ptr_fs_id; //uint8_t[] fs_id;

        /// <summary>
        /// fs id used
        /// </summary>
        UIntPtr fs_id_used;

        Endianness endian;

        /// <summary>
        /// taken when r/w the list_inum_named list
        /// </summary>
        PlaceholderStruct list_inum_named_lock; 

        /// <summary>
        /// List of unallocated inodes that
        /// </summary>
        IntPtr list; //TSK_LIST *list_inum_named; 

        /// <summary>
        /// taken for the duration of orphan hunting (not just when updating orphan_dir)
        /// </summary>
        PlaceholderStruct orphan_dir_lock; 

        /// <summary>
        /// Files and dirs in the top level of the $OrphanFiles directory.  NULL if orphans have not been hunted for yet. 
        /// </summary>
        IntPtr orphans; //TSK_FS_DIR *orphan_dir; 


        #region methods

        IntPtr funcptr_block_walk;
        IntPtr funcptr_block_getflags;
        IntPtr funcptr_inode_Walk;
        IntPtr funcptr_file_add_meta;
        IntPtr funcptr_get_default_attr_type;
        IntPtr funcptr_load_attrs;

        IntPtr funcptr_istat;
        IntPtr funcptr_dir_open_meta;
        IntPtr funcptr_jopen;
        IntPtr funcptr_jblk_walk;
        IntPtr funcptr_jentry_walk;
        IntPtr funcptr_fsstat;
        IntPtr funcptr_name_cmp;
        IntPtr funcptr_fscheck;
        IntPtr funcptr_close;
        IntPtr funcptr_fread_owner_sid;

        #endregion

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
        public ImageInfo ImageInfo
        {
            get
            {
                var ret = new ImageInfo();
                if (ptr_imageinfo != IntPtr.Zero)
                {
                    ret = (ImageInfo)Marshal.PtrToStructure(ptr_imageinfo, typeof(ImageInfo));
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
                return this.fstype;
            }
        }
    }

    /// <summary>
    /// struct TSK_FS_BLOCK
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct FileSystemBlockInfo
    {
        /// <summary>
        /// internal Will be set to TSK_FS_BLOCK_TAG if structure is valid / allocated 
        /// </summary>
        internal StructureMagic tag;

        /// <summary>
        ///Pointer to file system that block is from
        /// </summary>
        internal IntPtr ptr_fs_info; //TSK_FS_INFO *fs_info;   

        /// <summary>
        ///  Buffer with block data (of size TSK_FS_INFO::block_size)
        /// </summary>
        internal IntPtr ptr_block_data; //char *buf;  

        /// <summary>
        /// Address of block
        /// </summary>
        internal long addr; //TSK_DADDR_T addr;       

        /// <summary>
        /// Flags for block (alloc or unalloc)
        /// </summary>
        internal FileSystemBlockFlags flags;

        /// <summary>
        /// validates the tag contains the proper constant
        /// </summary>
        public bool AppearsValid
        {
            get
            {
                return tag == StructureMagic.FilesystemBlockTag;
            }
        }

    }

    /// <summary>
    /// TSK_FS_DIR
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    struct DirectoryStruct
    {
        internal StructureMagic tag;                // \internal Will be set to TSK_FS_DIR_TAG if structure is still allocated, 0 if not
        internal IntPtr fs_dir;// TSK_FS_FILE* fs_file;   // Pointer to the file structure for the directory.
        internal IntPtr ptr_list_names;// TSK_FS_NAME* names;     // Pointer to list of names in directory. 
        internal int names_used;// size_t names_used;      // Number of name structures in queue being used
        internal int names_allocated;// size_t names_alloc;     // Number of name structures that were allocated

        /// <summary>
        /// Metadata address of this directory 
        /// </summary>
        internal long address;// TSK_INUM_T addr;    // Metadata address of this directory 

        //not really needed
        IntPtr ptr_fsinfo;// TSK_FS_INFO* fs_info;   // Pointer to file system the directory is located in

        /// <summary>
        /// Verifies tag
        /// </summary>
        public bool AppearsValid
        {
            get
            {
                return tag == StructureMagic.FilesystemDirectoryTag;
            }
        }
    }

    /// <summary>
    /// Called when processing a filesystem
    /// </summary>
    /// <param name="tskFilesystem"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate FilterReturnCode ProcessVolumeDelegate(ref VolumeInfo tskFilesystem);

    /// <summary>
    /// Called when processing a filesystem
    /// </summary>
    /// <param name="tskFilesystem"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate FilterReturnCode ProcessFilesystemDelegate(ref FileSystemInfo tskFilesystem);

    /// <summary>
    /// Called when processing a file
    /// </summary>
    /// <param name="tskFile"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ReturnCode ProcessFileDelegate(ref FileStruct tskFile, string path);

    //typedef TSK_WALK_RET_ENUM(*TSK_FS_DIR_WALK_CB) (TSK_FS_FILE *a_fs_file, const char *a_path, void *a_ptr);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate WalkReturnEnum DirWalkDelegate(ref FileStruct file, string path, IntPtr some_ptr);

    /// <summary>
    /// Called for each metdata entry during a metadata walk
    /// typedef TSK_WALK_RET_ENUM(* TSK_FS_META_WALK_CB)(TSK_FS_FILE *a_fs_file, void *a_ptr)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="some_ptr"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate WalkReturnEnum MetaWalkDelegate(ref FileStruct file, IntPtr some_ptr);

    /// <summary>
    /// Callback function that is called for file content during file walk. (TSK_FS_FILE_WALK_CB)
    /// </summary>
    /// <param name="file">The file.</param>
    /// <param name="offset">The offset.</param>
    /// <param name="address">The address.</param>
    /// <param name="buffer">The data buffer.</param>
    /// <param name="length">The length.</param>
    /// <param name="flags">The flags.</param>
    /// <param name="dataPtr">Pointer to data that is passed to the callback function each time.</param>
    /// <returns>Value to control the file walk.</returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate WalkReturnEnum FileContentWalkDelegate(ref FileStruct file, long offset, long address, IntPtr buffer, int length, FileSystemBlockFlags flags, IntPtr dataPtr);

}

