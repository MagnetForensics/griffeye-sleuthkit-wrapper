using SleuthKit.Structs;
using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace SleuthKit
{
    /// <summary>
    /// Stores state information for an open file system.   Roughly equivalent to the TSK_FS_INFO structure.
    /// </summary>
    public class FileSystem : IDisposable
    {
        #region Fields

        private readonly DiskImage _image;
        internal FileSystemHandle _handle;
        private TSK_FS_INFO _struct;
        private readonly PoolVolumeInfo _poolVolumeInfo;
        private static readonly SemaphoreSlim poolLock = new SemaphoreSlim(1);

        private byte[] _bitmap;

        private byte[] Bitmap => _bitmap ??= GetBitmap();

        #endregion Fields

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
            this._handle = NativeMethods.tsk_fs_open_vol(volume._ptr_volinfo, fstype);
            this._struct = _handle.GetStruct();
        }

        internal FileSystem(Pool pool, PoolVolumeInfo poolVolumeInfo, FileSystemType fstype)
        {
            try
            {
                this._poolVolumeInfo = poolVolumeInfo;
                this._image = null;

                WaitForLockWhenWithinPool();
                var tskImgInfo = pool.GetImageInfo(poolVolumeInfo.Block);
                this._handle = NativeMethods.tsk_fs_open_img(tskImgInfo, 0, fstype);
                this._struct = _handle.GetStruct();
            }
            finally
            {
                ReleaseLockWhenWithinPool();
            }
        }

        #endregion Constructors

        #region Properties

        public String Label
        {
            get
            {
                switch (_struct.FilesystemType)
                {
                    case FileSystemType.Apfs:
                        return _poolVolumeInfo.Description;

                    case FileSystemType.ext2:
                    case FileSystemType.ext3:
                    case FileSystemType.ext4:
                        EXT2FS_INFO ext2Struct = _handle.GetStructExt2();
                        return ext2Struct.fs.VolumeName;

                    case FileSystemType.FAT12:
                    case FileSystemType.FAT16:
                        FATFS_INFO fatStruct16 = _handle.GetStructFat();
                        return fatStruct16.sb.VolumeName16;

                    case FileSystemType.FAT32:
                        FATFS_INFO fatStruct32 = _handle.GetStructFat();
                        return fatStruct32.sb.VolumeName32;

                    case FileSystemType.HFS: //OS X filesystem, stored in some metadata entry
                        HFS_ENTRY hfsRootEntry = new HFS_ENTRY();
                        IntPtr entryPtr = Marshal.AllocHGlobal(Marshal.SizeOf(hfsRootEntry));
                        Marshal.StructureToPtr(hfsRootEntry, entryPtr, false);
                        IntPtr hfsPtr = _handle.DangerousGetHandle();

                        byte result = NativeMethods.hfs_cat_file_lookup(hfsPtr, 2, entryPtr, 0);

                        if (result == 0)
                        {
                            hfsRootEntry = ((HFS_ENTRY)Marshal.PtrToStructure(entryPtr, typeof(HFS_ENTRY)));
                            Endianness endian = _handle.GetStruct().Endian;
                            return hfsRootEntry.Thread.Name.GetString(endian);
                        }
                        else
                        {
                            return _struct.Offset.ToString();
                        }

                    case FileSystemType.ISO9660:
                        ISO_INFO isoStruct = _handle.GetStructIso();
                        return isoStruct.pvd.VolumeName;

                    case FileSystemType.NTFS: //Stored in some metadata entry
                        File mftVol = OpenFile(0x03);

                        if (mftVol.FileStruct.Metadata.HasValue)
                        {
                            AttributeHandle attrHandle = NativeMethods.tsk_fs_attrlist_get(
                                mftVol.FileStruct.Metadata.Value.attrPtr, AttributeType.NtfsVName);

                            return attrHandle.GetStruct().rdBufString ?? _struct.Offset.ToString();
                        }
                        else
                        {
                            return _struct.Offset.ToString();
                        }

                    //TODO: Get sample image and test
                    case FileSystemType.UFS2:
                        FFS_INFO ffsStruct = _handle.GetStructFfs();
                        return ffsStruct.sb.VolumeName;

                    case FileSystemType.Raw:
                    case FileSystemType.Swap:
                    case FileSystemType.UFS1:
                    case FileSystemType.UFS1b:
                    case FileSystemType.Yaffs2:
                    default:
                        return _struct.Offset.ToString();
                }
            }
        }

        /// <summary>
        /// 	Size of each block (in bytes)
        /// </summary>
        public uint BlockSize => this._struct.block_size;

        /// <summary>
        /// Number of blocks in fs.
        /// </summary>
        public ulong BlockCount => this._struct.block_count;

        /// <summary>
        /// Address of first block.
        /// </summary>
        public ulong FirstBlock => this._struct.first_block;

        /// <summary>
        /// Address of last block as reported by file system (could be larger than last_block in image if end of image does not exist)
        /// </summary>
        public ulong LastBlock => this._struct.last_block;

        /// <summary>
        /// Address of last block -- adjusted so that it is equal to the last block in the image or volume (if image is not complete)
        /// </summary>
        public ulong ActualLastBlock => this._struct.last_block_act;

        /// <summary>
        /// the filesystem type
        /// </summary>
        public FileSystemType Type => this._struct.FilesystemType;

        /// <summary>
        /// The image this filesystem comes from
        /// </summary>
        public DiskImage DiskImage => this._image;

        /// <summary>
        /// If this file system is within a pool
        /// </summary>
        public bool WithinPool => this._poolVolumeInfo != null;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Opens a file by path
        /// </summary>
        /// <param name="path"></param>

        public File OpenFile(string path, Directory parent = null)
        {
            WaitForLockWhenWithinPool();
            File file = null;

            FileHandle fh;
            var utf8String = new Utf8String(path); // path here is utf-16
            fh = NativeMethods.tsk_fs_file_open(this._handle, IntPtr.Zero, utf8String.IntPtr);

            if (!fh.IsInvalid)
            {
                file = new File(this, fh, parent, null, utf8String);
            }
            else
            {
                utf8String.Dispose();
                fh.Close();
            }

            ReleaseLockWhenWithinPool();

            return file;
        }

        /// <summary>
        /// Opens a file by metadata address
        /// </summary>
        public File OpenFile(ulong address, Directory parent = null, String name = null)
        {
            WaitForLockWhenWithinPool();
            File file = null;
            var fh = NativeMethods.tsk_fs_file_open_meta(this._handle, IntPtr.Zero, address);
            if (!fh.IsInvalid)
            {
                file = new File(this, fh, parent, name, null);
            }
            else
            {
                fh.Close();
            }

            ReleaseLockWhenWithinPool();
            return file;
        }

        public void WaitForLockWhenWithinPool()
        {
            if (WithinPool)
            {
                poolLock.Wait();
            }
        }

        public void ReleaseLockWhenWithinPool()
        {
            if (WithinPool)
            {
                poolLock.Release();
            }
        }

        /// <summary>
        /// Opens a directory by path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="parent"></param>

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
        public Directory OpenDirectory(ulong address, Directory parent = null)
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

        public bool WalkDirectories(DirWalkPtrDelegate callback, DirWalkFlags flags = DirWalkFlags.Recurse)
        {
            var ret = NativeMethods.tsk_fs_dir_walk_ptr(this._handle, this._struct.root_inum, flags, callback, IntPtr.Zero);
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

        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.AppendFormat("type={0},offset={1},bsize={2},bcount={3},first={4},last={5},last_act={6}", _struct.FilesystemType, _struct.Offset, BlockSize, BlockCount, FirstBlock, LastBlock, ActualLastBlock);
            return buf.ToString();
        }

        /// <summary>
        /// Gets the FileAllocationFlags for the specified file.
        /// </summary>
        /// <param name="file">The file to check</param>
        public FileAllocationFlags GetFileAllocationFlags(TSK_FS_FILE file)
        {
            FileAllocationFlags flags = FileAllocationFlags.None;

            if ((file.Metadata.HasValue && file.Metadata.Value.MetadataFlags.HasFlag(MetadataFlags.Unallocated)) ||
                (file.Name.HasValue && file.Name.Value.Flags == NameFlags.Unallocated))
            {
                flags |= FileAllocationFlags.Deleted;

                if (Type == FileSystemType.NTFS)
                {
                    if (IsOverwrittenNTFS(Bitmap, file))
                    {
                        flags |= FileAllocationFlags.Overwritten;
                    }
                }
            }

            return flags;
        }

        private byte[] GetBitmap()
        {
            const long NTFS_BITMAP_ADDRESS = 6;

            //TODO: Add support for more filesystems.
            //      exFAT, HFS, HFS+, and ext3 uses bitmaps and it should be possible to read them.
            //      ext4 uses extents, which should be possible to convert to a bitmap, or use as
            //      is with another IsFree method.

            switch (Type)
            {
                case FileSystemType.NTFS:
                    using (Stream bitmapStream = new SleuthKit.FileStream(OpenFile(NTFS_BITMAP_ADDRESS)))
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        bitmapStream.CopyTo(memoryStream);
                        return memoryStream.ToArray();
                    }

                default:
                    return [];
            }
        }

        private static bool IsOverwrittenNTFS(byte[] bitmap, TSK_FS_FILE file)
        {
            if (bitmap.Any() && file.Metadata.HasValue && file.Metadata.Value.HasAttributeList)
            {
                foreach (TSK_FS_ATTR attr in file.Metadata.Value.AttributeList.List)
                {
                    if (attr.AttributeType == AttributeType.NtfsData &&
                        attr.Name.Equals(String.Empty) &&
                        attr.AttributeFlags.HasFlag(AttributeFlags.NonResident))
                    {
                        //This is the stream we are searching for
                        foreach (TSK_FS_ATTR_RUN run in attr.NonResidentBlocks)
                        {
                            if (run.Flags == AttributeRunFlags.None && !IsFree(bitmap, run.Address, run.Length))
                            {
                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsFree(byte[] bitmap, ulong startCluster, ulong length)
        {
            int index = (int)startCluster / 8;
            int bitmask = 1 << ((int)startCluster % 8);

            for (ulong i = 0; i < length; i++)
            {
                if ((bitmap[index] & bitmask) != 0)
                {
                    return false;
                }

                bitmask = bitmask << 1;
                if (bitmask == 256)
                {
                    index++;
                    bitmask = 1;
                }
            }

            return true;
        }

        #endregion Methods
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
        private TSK_FS_BLOCK _struct;

        #endregion Fields

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
        public FileSystem FileSystem => _fs;

        /// <summary>
        /// The address of this block
        /// </summary>
        public long BlockAddress => this._blockAddress;

        /// <summary>
        /// The flags
        /// </summary>
        public FileSystemBlockFlags Flags => this._struct.flags;

        /// <summary>
        /// Disposes resources
        /// </summary>
        public void Dispose()
        {
            this._handle.Dispose();
        }
    }
}