using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SleuthKit.Structs
{
    /// <summary>
    /// represents TSK_FS_META
    /// </summary>
    [StructLayout(LayoutKind.Explicit,
    Size = 260
)]
    public struct TSK_FS_META
    {
        #region fields

        /// <summary>
        /// tag, can be used to validate that we have the right kind of struct.  a magic header for the struct, essentially.
        /// </summary>
        [FieldOffset(0)]
        private StructureMagic tag;

        /// <summary>
        /// Flags for this file for its allocation status etc.
        /// </summary>
        [FieldOffset(4)]
        internal MetadataFlags flags;

        /// <summary>
        /// Address of the meta data structure for this file
        /// </summary>
        [FieldOffset(8)]
        internal ulong addr;

        /// <summary>
        /// File type
        /// </summary>
        [FieldOffset(16)]
        internal MetadataType type;

        /// <summary>
        /// Unix-style permissions
        /// </summary>
        [FieldOffset(20)]
        internal MetadataMode mode;

        /// <summary>
        /// link count (number of file names pointing to this)
        /// </summary>
        [FieldOffset(24)]
        private long nlink;

        /// <summary>
        /// file size (in bytes) - yes this is a signed 64-bit integer, despite it being unsigned in sleuthkit (?).  It is easier this way.
        /// </summary>
        [FieldOffset(32)]
        private long size;

        [FieldOffset(40)]
        private long start_of_inode;

        /// <summary>
        /// user id
        /// </summary>
        [FieldOffset(48)]
        private uint uid;

        /// <summary>
        /// group id
        /// </summary>
        [FieldOffset(52)]
        private uint gid;

        #region times

        /// <summary>
        /// last file content modification time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        [FieldOffset(56)]
        private long mtime;

        /// <summary>
        /// nano-second resolution in addition to m_time
        /// </summary>
        [FieldOffset(64)]
        private long mtime_nano;

        /// <summary>
        /// last file content accessed time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        [FieldOffset(72)]
        private long atime;

        /// <summary>
        /// nano-second resolution in addition to a_time
        /// </summary>
        [FieldOffset(80)]
        private long atime_nano;

        /// <summary>
        ///  last file / metadata status change time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        [FieldOffset(88)]
        private long ctime;

        /// <summary>
        /// nano-second resolution in addition to c_time
        /// </summary>
        [FieldOffset(96)]
        private long ctime_nano;

        /// <summary>
        /// Created time (stored in number of seconds since Jan 1, 1970 UTC)
        /// </summary>
        [FieldOffset(104)]
        private long crtime;

        /// <summary>
        /// nano-second resolution in addition to cr_time
        /// </summary>
        [FieldOffset(112)]
        private long crtime_nano;

        #region filesystem specific times

        /* lots of stuff here, not mapped
        int special_time;

        uint special_nano;
        //*/

        #endregion filesystem specific times

        #endregion times

        /// <summary>
        /// Pointer to file system specific data that is used to store references to file content
        /// </summary>
        [FieldOffset(192)]
        private IntPtr content_ptr;

        /// <summary>
        /// size of content  buffer
        /// </summary>
        [FieldOffset(200)]
        private UIntPtr content_len;

        /// <summary>
        /// The content type
        /// </summary>
        [FieldOffset(208)]
        private FileSystemMetaContentType content_type;

        /// <summary>
        /// internal Optional callback used for any internal cleanup needed before freeing content_ptr
        /// </summary>
        [FieldOffset(212)]
        private IntPtr reset_content_ptr;

        /// <summary>
        /// Sequence number for file (NTFS only, is incremented when entry is reallocated)
        /// </summary>
        [FieldOffset(220)]
        private uint seq;

        /// <summary>
        /// Contains run data on the file content (specific locations where content is stored).
        /// Check attr_state to determine if data in here is valid because not all file systems
        /// load this data when a file is loaded.  It may not be loaded until needed by one
        /// of the APIs. Most file systems will have only one attribute, but NTFS will have several.
        /// </summary>
        [FieldOffset(232)]
        private IntPtr attr_ptr; //TSK_FS_ATTRLIST *attr;

        /// <summary>
        /// State of the data in the TSK_FS_META::attr structure
        /// </summary>
        [FieldOffset(240)]
        private MetadataAttributeFlags attr_state;

        /// <summary>
        /// Name of file stored in metadata (FAT and NTFS Only)
        /// </summary>
        [FieldOffset(248)]
        private IntPtr name2;  //TSK_FS_META_NAME_LIST* name2;   ///<

        //char* link;
        /// <summary>
        /// Name of target file if this is a symbolic link
        /// </summary>
        [FieldOffset(256)]
        private IntPtr link;

        #endregion fields

        #region properties

        /// <summary>
        /// validates the tag contains the proper constant
        /// </summary>
        public bool AppearsValid => this.tag == StructureMagic.FilesystemMetadataTag;

        /// <summary>
        /// Metadata Address
        /// </summary>
        public ulong Address => this.addr;

        /// <summary>
        /// Metadata flags
        /// </summary>
        public MetadataFlags MetadataFlags => this.flags;

        /// <summary>
        /// Metadata type
        /// </summary>
        public MetadataType MetadataType => this.type;

        public MetadataMode Mode => this.mode;

        public long LinkCount => this.nlink;

        /// <summary>
        /// File size, in bytes
        /// </summary>
        public long Size => this.size;

        /// <summary>
        /// Modified time in unix epoch ticks
        /// </summary>
        public long MTime => this.mtime;

        /// <summary>
        /// Access time in unix epoch ticks
        /// </summary>
        public long ATime => this.atime;

        /// <summary>
        /// Metadata change time in unix epoch ticks
        /// </summary>
        public long CTime => this.ctime;

        /// <summary>
        /// Kreayshawn time in unix epoch ticks
        /// </summary>
        public long CRTime => this.crtime;

        public uint Sequence => this.seq;

        internal IntPtr attrPtr => attr_ptr;

        public bool HasAttributeList => attr_ptr != IntPtr.Zero;

        public TSK_FS_ATTRLIST AttributeList => ((TSK_FS_ATTRLIST)Marshal.PtrToStructure(attr_ptr, typeof(TSK_FS_ATTRLIST)));

        public MetadataAttributeFlags AttributeState => this.attr_state;

        public IEnumerable<TSK_FS_META_NAME_LIST> NameList
        {
            get
            {
                if (name2 != IntPtr.Zero)
                {
                    TSK_FS_META_NAME_LIST entry = (TSK_FS_META_NAME_LIST)Marshal.PtrToStructure(name2, typeof(TSK_FS_META_NAME_LIST));

                    for (; ; )
                    {
                        yield return entry;

                        if (entry.HasNext)
                        {
                            entry = entry.Next;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        /*
        public TSK_FS_META_NAME_LIST? NameListHead
        {
            get
            {
                TSK_FS_META_NAME_LIST? ret = null;
                if (name2 != IntPtr.Zero)
                {
                    ret = (TSK_FS_META_NAME_LIST)Marshal.PtrToStructure(name2, typeof(TSK_FS_META_NAME_LIST));
                }
                return ret;
            }
        }
        //*/

        //public DateTime LastAccessed
        //{
        //    get
        //    {
        //        var val = this.atime.ToUInt64();
        //        return DateTime.MinValue;
        //    }
        //}

        #endregion properties
    }
}