using SleuthKit.Structs;
using System;
using System.Runtime.InteropServices;

namespace SleuthKit
{
    /// <summary>
    /// Called when processing a filesystem
    /// </summary>
    /// <param name="tskFilesystem"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate FilterReturnCode ProcessVolumeDelegate(ref TSK_VS_PART_INFO tskFilesystem);

    /// <summary>
    /// Called when processing a filesystem
    /// </summary>
    /// <param name="tskFilesystem"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate FilterReturnCode ProcessFilesystemDelegate(ref TSK_FS_INFO tskFilesystem);

    /// <summary>
    /// Called when processing a file
    /// </summary>
    /// <param name="tskFile"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate ReturnCode ProcessFileDelegate(ref TSK_FS_FILE tskFile, string path);

    //typedef TSK_WALK_RET_ENUM(*TSK_FS_DIR_WALK_CB) (TSK_FS_FILE *a_fs_file, const char *a_path, void *a_ptr);
    /// <summary>
    /// Callback when walking over directories.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="utf8_path">A pointer to a UTF8 encoded string of the directory. Convert the string to UTF16 by using <see cref="IntPtrExtensions.Utf8ToUtf16(IntPtr)"/></param>
    /// <param name="some_ptr"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate WalkReturnEnum DirWalkDelegate(ref TSK_FS_FILE file, IntPtr utf8_path, IntPtr some_ptr);

    /// <summary>
    /// Callback when walking over directories.
    /// </summary>
    /// <param name="filePtr"></param>
    /// <param name="utf8_path">A pointer to a UTF8 encoded string of the directory. Convert the string to UTF16 by using <see cref="IntPtrExtensions.Utf8ToUtf16(IntPtr)"/></param>
    /// <param name="some_ptr"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate WalkReturnEnum DirWalkPtrDelegate(IntPtr filePtr, IntPtr utf8_path, IntPtr some_ptr);

    /// <summary>
    /// Called for each metdata entry during a metadata walk
    /// typedef TSK_WALK_RET_ENUM(* TSK_FS_META_WALK_CB)(TSK_FS_FILE *a_fs_file, void *a_ptr)
    /// </summary>
    /// <param name="file"></param>
    /// <param name="some_ptr"></param>
    /// <returns></returns>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate WalkReturnEnum MetaWalkDelegate(ref TSK_FS_FILE file, IntPtr some_ptr);

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
    public delegate WalkReturnEnum FileContentWalkDelegate(ref TSK_FS_FILE file, long offset, long address, IntPtr buffer, int length, FileSystemBlockFlags flags, IntPtr dataPtr);
}