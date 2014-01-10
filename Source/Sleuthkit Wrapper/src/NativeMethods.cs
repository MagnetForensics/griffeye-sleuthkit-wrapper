using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SleuthKit
{
    /// <summary>
    /// pinvoke
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>
        /// The framework should know to look for libtsk4.dll, libtsk4.so, libtsk3.dylib, depending on the OS.  It is that smart.
        /// </summary>
        private const string NativeLibrary = "libtsk4";

        #region pinvoke

        #region disk image functions

        #region open functions

        /// extern TSK_IMG_INFO *tsk_img_open(int,const TSK_TCHAR * const images[], TSK_IMG_TYPE_ENUM, unsigned int a_ssize);
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern DiskImageHandle tsk_img_open(int imageCount, IntPtr[] image, ImageType imageType, uint sectorSize);

        //extern TSK_IMG_INFO *tsk_img_open_sing(const TSK_TCHAR * a_image,TSK_IMG_TYPE_ENUM type, unsigned int a_ssize);
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        internal static extern DiskImageHandle tsk_img_open_sing(string image, uint imageType, uint sectorSize);

        //extern TSK_IMG_INFO *tsk_img_open_utf8(int num_img, const char *const images[], TSK_IMG_TYPE_ENUM type, unsigned int a_ssize);
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern DiskImageHandle tsk_img_open_utf8(int imageCount, byte[] image, ImageType imageType, uint sectorSize);

        //extern TSK_IMG_INFO *tsk_img_open_utf8_sing(const char *a_image, TSK_IMG_TYPE_ENUM type, unsigned int a_ssize);
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr tsk_img_open_utf8_sing(
            [MarshalAs(UnmanagedType.LPWStr)]//.CustomMarshaler, MarshalTypeRef = typeof(UTF8Marshaler))]
            string image,
            ImageType imageType, uint sectorSize);

        #endregion

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr tsk_img_read(DiskImageHandle img, long a_off, byte[] buf, int len);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tsk_img_close(IntPtr img);

        #endregion

        #region volume system functions

        /// <summary>
        /// Opens a volume system
        /// </summary>
        /// <remarks>
        /// TSK_VS_INFO* tsk_vs_open 	( 	TSK_IMG_INFO *  	img_info, TSK_DADDR_T  	offset, TSK_VS_TYPE_ENUM  	type ) 
        /// </remarks>
        /// <param name="image">handle to an existing disk image</param>
        /// <param name="offset">location to look for a volume system</param>
        /// <param name="type">the image type - can be set to Autodetect</param>
        /// <returns></returns>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern VolumeSystemHandle tsk_vs_open(DiskImageHandle image, long offset, VolumeSystemType type);

        /// <summary>
        /// Closes a volume system
        /// </summary>
        /// <param name="handle"></param>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tsk_vs_close(IntPtr handle);

        /// <summary>
        /// Reads one or more blocks of data with an address relative to the start of the volume system.
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="offset"></param>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UIntPtr tsk_vs_part_read(IntPtr handle, long offset, byte[] buffer, UIntPtr length);

        #endregion

        #region volume functions

        #endregion

        #region filesystem functions

        //TSK_FS_INFO* tsk_fs_open_img 	( 	TSK_IMG_INFO *  	a_img_info,TSK_OFF_T  	a_offset,TSK_FS_TYPE_ENUM  	a_ftype 
        //TskFilesystemInfo
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileSystemHandle tsk_fs_open_img(DiskImageHandle image, long offset, FileSystemType fstype);

        //TSK_FS_INFO* tsk_fs_open_vol 	( 	const TSK_VS_PART_INFO *  	a_part_info,TSK_FS_TYPE_ENUM  	a_ftype ) 	
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileSystemHandle tsk_fs_open_vol(IntPtr volinfo, FileSystemType fstype);

        //const char* tsk_fs_type_toname 	( 	TSK_FS_TYPE_ENUM  	ftype ) 	
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Auto)]
        internal static extern string tsk_fs_type_toname(FileSystemType ftype);


        #region block stuff

        //TSK_FS_BLOCK* tsk_fs_block_get 	( 	TSK_FS_INFO *  	a_fs, TSK_FS_BLOCK *  	a_fs_block, TSK_DADDR_T  	a_addr  ) 
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileSystemBlockHandle tsk_fs_block_get(FileSystemHandle fsinfo, IntPtr should_be_zero, long address);

        //[DllImport(SleuthkitwrapperLibrary, CallingConvention = CallingConvention.Cdecl)]
        //internal static extern FileSystemBlockInfo tsk_fs_block_get(FileSystemHandle fsinfo, ref FileSystemBlockInfo existingBlock, long address);

        //void tsk_fs_block_free 	( 	TSK_FS_BLOCK *  	a_fs_block ) 	
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tsk_fs_block_free(IntPtr fsblockptr);

        #endregion

        #region file stuff

        //TSK_FS_FILE* tsk_fs_file_open 	( 	TSK_FS_INFO *  	a_fs, TSK_FS_FILE *  	a_fs_file, const char *  	a_path  ) 	
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileHandle tsk_fs_file_open(FileSystemHandle fs, [In] IntPtr should_be_zero, [In] string utf8path);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileHandle tsk_fs_file_open_meta(FileSystemHandle fs, [In] IntPtr should_be_zero, [In] long metadata_address);

        internal static FileHandle tsk_fs_file_open_x(FileSystemHandle fs, IntPtr should_be_zero, string utf8path)
        {
            byte[] data = Encoding.Unicode.GetBytes(utf8path);
            var pream = Encoding.UTF8.GetPreamble();
            
            byte[] data2 = new byte[data.Length + 3];
            Buffer.BlockCopy(pream, 0, data2, 0, 3);
            Buffer.BlockCopy(data, 0, data2, 3, data.Length);
           // var ret = tsk_fs_file_open(fs, IntPtr.Zero, data);
            return null;// ret;
        }

        //extern ssize_t tsk_fs_file_read(TSK_FS_FILE *, TSK_OFF_T, char *, size_t, TSK_FS_FILE_READ_FLAG_ENUM);
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr tsk_fs_file_read(FileHandle ptr_fs_file, long offset, byte[] buf, int buf_len, FileReadFlag e);

        /// <summary>
        /// Process a file and call a callback function with the file contents.
        /// </summary>
        /// <param name="ptr_fs_file">File to process.</param>
        /// <param name="flag">Flags to use while processing file.</param>
        /// <param name="file_walk_cb">Callback action to call with content.</param>
        /// <param name="a_ptr">Pointer that will passed to callback.</param>
        /// <returns>1 on error and 0 on success.</returns>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte tsk_fs_file_walk(FileHandle ptr_fs_file, FileWalkFlag flag, FileContentWalkDelegate file_walk_cb, IntPtr a_ptr);

        /// <summary>
        /// Close an open file.   void tsk_fs_file_close 	( 	TSK_FS_FILE *  	a_fs_file ) 	
        /// </summary>
        /// <param name="ptr_fs_file"></param>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tsk_fs_file_close(IntPtr ptr_fs_file);

        #endregion

        #region dir stuff

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern DirectoryHandle tsk_fs_dir_open(FileSystemHandle file, string path);

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern DirectoryHandle tsk_fs_dir_open_meta(FileSystemHandle file, long metadata_address);

        /// <summary>
        /// Returns the number of files and subdirectories in a directory.
        /// </summary>
        /// <param name="fs_dir">Directory to get information about</param>
        /// <returns>Number of files and subdirectories (or 0 on error)</returns>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern UIntPtr tsk_fs_dir_getsize(DirectoryHandle fs_dir);

        /// <summary>
        /// Return a specific file or subdirectory from an open directory.
        /// </summary>
        /// <param name="fs_dir">Directory to analyze</param>
        /// <param name="idx"></param>
        /// <returns></returns>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileHandle tsk_fs_dir_get(DirectoryHandle fs_dir, UIntPtr idx);

        //extern void tsk_fs_dir_close(TSK_FS_DIR *);
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void tsk_fs_dir_close(IntPtr handle);

        //uint8_t tsk_fs_dir_walk 	( 	TSK_FS_INFO *  	a_fs, TSK_INUM_T  	a_addr, TSK_FS_DIR_WALK_FLAG_ENUM  	a_flags, TSK_FS_DIR_WALK_CB  	a_action, void *  	a_ptr ) 	
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte tsk_fs_dir_walk(FileSystemHandle fs, long directory_address, DirWalkFlags walk_flags, DirWalkDelegate callback, IntPtr a_ptr);

        #endregion

        #region meta

        /// <summary>
        /// 	Walk a range of metadata structures and call a callback for each structure that matches the flags supplied. 
        /// </summary>
        /// <returns></returns>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern byte tsk_fs_meta_walk(FileSystemHandle fs, long start_address, long end_address, MetadataFlags walk_flags, MetaWalkDelegate callback, IntPtr a_ptr);


        #endregion

        #region error stuff

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern uint tsk_error_get_errno();

        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr tsk_error_get_errstr();

        #endregion

        /// <summary>
        /// Close an open file system. 
        /// </summary>
        /// <param name="fsptr"></param>
        /// <returns></returns>
        [DllImport(NativeLibrary, CallingConvention = CallingConvention.Cdecl)]
        internal static extern FileSystemInfo tsk_fs_close(IntPtr fsptr);

        #endregion

        #endregion
    }

    /// <summary>
    /// safe handle to work with TSK Disk Images (TSK_IMG_INFO*)
    /// </summary>
    internal class DiskImageHandle : SafeHandle
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DiskImageHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// invalid if pointer is zero
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return base.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// closes handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.tsk_img_close(this.handle);
            base.SetHandleAsInvalid();
            return true;
        }

        /// <summary>
        /// converts this pointer into an ImageInfo struct
        /// </summary>
        /// <returns></returns>
        internal ImageInfo GetStruct()
        {
            return ((ImageInfo)Marshal.PtrToStructure(this.handle, typeof(ImageInfo)));
        }

        /// <summary>
        /// Opens a new handle to a VolumeSystem of the specified type, at the specified offset.
        /// </summary>
        /// <param name="vstype"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal VolumeSystemHandle OpenVolumeSystemHandle(VolumeSystemType vstype = VolumeSystemType.Autodetect, long offset = 0)
        {
            return NativeMethods.tsk_vs_open(this, offset, vstype);
        }

        /// <summary>
        /// Opens a new Filesystem handle of the specified filesystem type, at the specified offset.
        /// </summary>
        /// <param name="fstype"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        internal FileSystemHandle OpenFileSystemHandle(FileSystemType fstype = FileSystemType.Autodetect, long offset = 0)
        {
            return NativeMethods.tsk_fs_open_img(this, offset, fstype);
        }
    }

    /// <summary>
    /// safe handle to work with TSK Volume Systems (TSK_VS_INFO*)
    /// </summary>
    internal class VolumeSystemHandle : SafeHandle
    {
        /// <summary>
        /// ctor
        /// </summary>
        public VolumeSystemHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// invalid if pointer is zero
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return base.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// closes handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.tsk_vs_close(this.handle);
            base.SetHandleAsInvalid();
            return true;
        }

        /// <summary>
        /// Converts this handle into a new VolumeSystemInfo.
        /// </summary>
        /// <returns></returns>
        internal VolumeSystemInfo GetStruct()
        {
            if (this.IsInvalid)
                return new VolumeSystemInfo();
            else
                return ((VolumeSystemInfo)Marshal.PtrToStructure(this.handle, typeof(VolumeSystemInfo)));
        }
    }

    /// <summary>
    /// safe handle to work with TSK File Systems (TSK_FS_INFO*)
    /// </summary>
    internal class FileSystemHandle : SafeHandle
    {
        /// <summary>
        /// ctor
        /// </summary>
        public FileSystemHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// invalid if pointer is zero
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return base.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// closes handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.tsk_fs_close(this.handle);
            base.SetHandleAsInvalid();
            return true;
        }

        internal FileSystemInfo GetStruct()
        {
            if (this.IsInvalid)
                return new FileSystemInfo();
            else
                return ((FileSystemInfo)Marshal.PtrToStructure(this.handle, typeof(FileSystemInfo)));
        }
    }

    /// <summary>
    /// safe handle to work with TSK File Systems (TSK_FS_BLOCK*)
    /// </summary>
    internal class FileSystemBlockHandle : SafeHandle
    {
        /// <summary>
        /// ctor
        /// </summary>
        public FileSystemBlockHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// invalid if pointer is zero
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return base.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// closes handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.tsk_fs_block_free(this.handle);
            base.SetHandleAsInvalid();
            return true;
        }

        internal FileSystemBlockInfo GetStruct()
        {
            if (this.IsInvalid)
                return new FileSystemBlockInfo();
            else
                return ((FileSystemBlockInfo)Marshal.PtrToStructure(this.handle, typeof(FileSystemBlockInfo)));
        }
    }

    /// <summary>
    /// safe handle to work with TSK File (TSK_FS_FILE*)
    /// </summary>
    internal class FileHandle : SafeHandle
    {
        /// <summary>
        /// ctor
        /// </summary>
        public FileHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// invalid if pointer is zero
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return base.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// closes handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.tsk_fs_file_close(this.handle);
            base.SetHandleAsInvalid();
            return true;
        }

        internal FileStruct GetStruct()
        {
            if (this.IsInvalid)
                return new FileStruct();
            else
                return ((FileStruct)Marshal.PtrToStructure(this.handle, typeof(FileStruct)));
        }
    }

    /// <summary>
    /// safe handle to work with TSK Directory (TSK_FS_DIR*)
    /// </summary>
    internal class DirectoryHandle : SafeHandle
    {
        /// <summary>
        /// ctor
        /// </summary>
        public DirectoryHandle()
            : base(IntPtr.Zero, true)
        {
        }

        /// <summary>
        /// invalid if pointer is zero
        /// </summary>
        public override bool IsInvalid
        {
            get
            {
                return base.handle == IntPtr.Zero;
            }
        }

        /// <summary>
        /// closes handle
        /// </summary>
        /// <returns></returns>
        protected override bool ReleaseHandle()
        {
            NativeMethods.tsk_fs_dir_close(this.handle);
            base.SetHandleAsInvalid();
            return true;
        }

        internal DirectoryStruct GetStruct()
        {
            if (this.IsInvalid)
                return new DirectoryStruct();
            else
                return ((DirectoryStruct)Marshal.PtrToStructure(this.handle, typeof(DirectoryStruct)));
        }
    }
}