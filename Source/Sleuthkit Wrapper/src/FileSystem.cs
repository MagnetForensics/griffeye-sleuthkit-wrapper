using System;
using System.Text;

namespace SleuthKit
{
    /// <summary>
    /// Stores state information for an open file system.   Roughly equivalent to the TSK_FS_INFO structure.
    /// </summary>
    public class FileSystem : IDisposable
    {
        #region Fields
        private DiskImage _image;
        internal FileSystemHandle _handle;
        private FileSystemInfo _struct;
        private Volume _volume;
        #endregion

        #region Constructors

        /// <summary>
        /// ctor for when there is no volume system (like thumb drives, floppies, etc. things with no MBR.. they just start with the filesystem)
        /// </summary>
        /// <param name="diskImage"></param>
        /// <param name="type"></param>
        /// <param name="offset"></param>
        internal FileSystem(DiskImage diskImage, FileSystemType type, long offset)
        {
            this._image = diskImage;
            this._volume = null; //no luck on this, no volume system!
            this._handle = diskImage._handle.OpenFileSystemHandle(type, offset);
            this._struct = _handle.GetStruct();
        }

        /// <summary>
        /// ctor for filesystem that comes from within a volume
        /// </summary>
        /// <param name="volume"></param>
        /// <param name="fstype"></param>
        internal FileSystem(Volume volume, FileSystemType fstype)
        {
            this._image = volume.VolumeSystem.DiskImage;
            this._volume = volume;
            this._handle = NativeMethods.tsk_fs_open_vol(volume._ptr_volinfo, fstype);
            this._struct = _handle.GetStruct();
        }

        #endregion

        #region Properties

        /// <summary>
        /// 	Size of each block (in bytes) 
        /// </summary>
        public int BlockSize
        {
            get
            {
                return this._struct.block_size;
            }
        }

        /// <summary>
        /// Number of blocks in fs. 
        /// </summary>
        public long BlockCount
        {
            get
            {
                return this._struct.block_count;
            }
        }

        /// <summary>
        /// Address of first block. 
        /// </summary>
        public long FirstBlock
        {
            get
            {
                return this._struct.first_block;
            }
        }

        /// <summary>
        /// Address of last block as reported by file system (could be larger than last_block in image if end of image does not exist) 
        /// </summary>
        public long LastBlock
        {
            get
            {
                return this._struct.last_block;
            }
        }

        /// <summary>
        /// Address of last block -- adjusted so that it is equal to the last block in the image or volume (if image is not complete) 
        /// </summary>
        public long ActualLastBlock
        {
            get
            {
                return this._struct.last_block_act;
            }
        }

        /// <summary>
        /// the filesystem type
        /// </summary>
        public FileSystemType Type
        {
            get
            {
                return this._struct.FilesystemType;
            }
        }

        /// <summary>
        /// The image this filesystem comes from
        /// </summary>
        public DiskImage DiskImage
        {
            get
            {
                return this._image;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Opens a file by path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public File OpenFile(string path, Directory parent = null)
        {
            File file = null;
            var fh = NativeMethods.tsk_fs_file_open(this._handle, IntPtr.Zero, path);
            if (!fh.IsInvalid)
            {
                file = new File(this, fh, parent, null);
            }
            else
            {
                fh.Close();
            }
            return file;
        }

        /// <summary>
        /// Opens a file by metadata address
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public File OpenFile(long address, Directory parent = null)
        {
            File file = null;
            var fh = NativeMethods.tsk_fs_file_open_meta(this._handle, IntPtr.Zero, address);
            if (!fh.IsInvalid)
            {
                file = new File(this, fh, parent, null);
            }
            else
            {
                fh.Close();
            }
            return file;
        }

        /// <summary>
        /// Opens a directory by path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Directory OpenDirectory(string path, Directory parent = null)
        {
            Directory dir = null;
            var dh = NativeMethods.tsk_fs_dir_open(this._handle, path);
            if (!dh.IsInvalid)
            {
                dir = new Directory(this, dh, parent);
            }
            else
            {
                dh.Close();
            }
            return dir;
        }

        /// <summary>
        /// Opens a directory by metadata address
        /// </summary>
        /// <param name="address"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public Directory OpenDirectory(long address, Directory parent = null)
        {
            Directory dir = null;
            var dh = NativeMethods.tsk_fs_dir_open_meta(this._handle, address);
            if (!dh.IsInvalid)
            {
                dir = new Directory(this, dh, parent);
            }
            return dir;
        }

        /// <summary>
        /// Opens the root directory
        /// </summary>
        /// <returns></returns>
        public Directory OpenRootDirectory()
        {
            Directory dir = null;
            var dh = NativeMethods.tsk_fs_dir_open_meta(this._handle, _struct.root_inum);
            if (!dh.IsInvalid)
            {
                dir = new Directory(this, dh) { isRoot = true };
            }
            return dir;
        }

        /// <summary>
        /// Opens a block
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        public FileSystemBlock OpenBlock(long block)
        {
            var fsbh = NativeMethods.tsk_fs_block_get(this._handle, IntPtr.Zero, block);
            var ret = new FileSystemBlock(this, fsbh, block);
            return ret;
        }

        /// <summary>
        /// Walks metadata structures of the file system
        /// </summary>
        /// <param name="mwd">Meta Walk Delegate</param>
        /// <param name="flags">Meta data Flags</param>
        /// <returns>returns a byte value</returns>
        public bool WalkMetadata(MetaWalkDelegate mwd, MetadataFlags flags = MetadataFlags.Allocated)
        {
            var ret = NativeMethods.tsk_fs_meta_walk(this._handle, this._struct.first_inum, this._struct.last_inum, flags, mwd, IntPtr.Zero);
            return ret == 0;
        }

        /// <summary>
        /// Walks the file names in a directory and obtain the details of the files via a callback.
        /// </summary>
        /// <param name="callback">
        /// Callback function that is called for each file name.
        /// </param>
        /// <param name="flags">
        /// Flags used during analysis.
        /// </param>
        /// <returns>
        /// Whether the operation is successful or not.
        /// </returns>
        public bool WalkDirectories(DirWalkDelegate callback, DirWalkFlags flags = DirWalkFlags.Recurse)
        {
            var ret = NativeMethods.tsk_fs_dir_walk(this._handle, this._struct.root_inum, flags, callback, IntPtr.Zero);
            return ret == 0;
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        public void Dispose()
        {
            _handle.Dispose();
        }

        /// <summary>
        /// diagnostic string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.AppendFormat("type={0},offset={1},bsize={2},bcount={3},first={4},last={5},last_act={6}", _struct.FilesystemType, _struct.Offset, BlockSize, BlockCount, FirstBlock, LastBlock, ActualLastBlock);
            return buf.ToString();
        }

        #endregion
    }

    /// <summary>
    /// Generic data strcture to hold block data with metadata
    /// </summary>
    public class FileSystemBlock : IDisposable
    {
        #region Fields        
        private FileSystem _fs;
        private FileSystemBlockHandle _handle;
        private long _blockAddress;
        private FileSystemBlockInfo _struct;
        #endregion

        internal FileSystemBlock(FileSystem fileSystem, FileSystemBlockHandle fsbh, long block)
        {
            this._fs = fileSystem;
            this._handle = fsbh;
            this._blockAddress = block;
            this._struct = fsbh.GetStruct();
        }

        /// <summary>
        /// The filesystem
        /// </summary>
        public FileSystem FileSystem
        {
            get { return _fs; }
        }

        /// <summary>
        /// The address of this block
        /// </summary>
        public long BlockAddress
        {
            get
            {
                return this._blockAddress;
            }
        }

        /// <summary>
        /// The flags
        /// </summary>
        public FileSystemBlockFlags Flags
        {
            get
            {
                return this._struct.flags;
            }
        }

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            this._handle.Dispose();
        }
    }
}
