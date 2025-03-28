using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using SleuthKit.Structs;

namespace SleuthKit;

/// <summary>
/// Pools can organize non-consecutive blocks into volumes. They were added to TSK as part of the APFS support.
/// https://www.sleuthkit.org/sleuthkit/docs/api-docs/4.12.0/poolpage.html
/// </summary>
public class Pool : IDisposable
{
    public PoolHandle _handle;
    internal TSK_POOL_INFO _struct;

    /// <summary>
    /// ctor for filesystem that comes from within a volume
    /// </summary>
    /// <param name="volume"></param>
    /// <param name="fstype"></param>
    internal Pool(Volume volume, FileSystemType fstype)
    {
        this._handle = NativeMethods.tsk_pool_open_sing(volume._ptr_volinfo, fstype);
        this._struct = _handle.GetStruct();
    }

    private delegate IntPtr TestCallbackDelegate(PoolHandle pool, ulong block);

    /// <summary>
    /// Create new image info to use with a specific pool volume
    /// </summary>

    internal IntPtr GetImageInfo(ulong pool_block)
    {
        var delegateForFunctionPointer = Marshal.GetDelegateForFunctionPointer<TestCallbackDelegate>(_struct.ptr_get_img_info);
        var ptrImgInfo = delegateForFunctionPointer.Invoke(_handle, pool_block);

        return ptrImgInfo;
    }

    /// <summary>
    /// Block size
    /// </summary>
    public uint BlockSize => _struct.block_size;

    /// <summary>
    /// Pool type
    /// </summary>
    public PoolTypeEnum Type => _struct.ctype;

    /// <summary>
    /// Number of volumes in pool
    /// </summary>
    public int VolumeCount => _struct.num_vols;

    /// <summary>
    /// Image offset
    /// </summary>
    public ulong ImageOffset => _struct.img_offset;

    /// <summary>
    /// Get volume info
    /// </summary>

    public IEnumerable<PoolVolumeInfo> GetVolumeInfos()
    {
        return _struct.PoolVolumeInfos.Select(pvi => new PoolVolumeInfo()
        {
            Block = pvi.Block,
            Description = pvi.Description
        });
    }

    /// <summary>
    /// Opens the filesystem on this pool, if any
    /// </summary>
    /// <param name="poolVolumeInfo"></param>
    /// <param name="fileSystemType"></param>

    public FileSystem OpenFileSystem(PoolVolumeInfo poolVolumeInfo, FileSystemType fileSystemType = FileSystemType.Autodetect)
    {
        FileSystem fs = new FileSystem(this, poolVolumeInfo, fileSystemType);

        if (fs._handle.IsInvalid)
        {
            fs._handle.Close();

            uint errorCode = NativeMethods.tsk_error_get_errno();
            IntPtr ptrToMessage = NativeMethods.tsk_error_get_errstr();
            String errorMessage = Marshal.PtrToStringAnsi(ptrToMessage);
            String ioExceptionMessage = String.Format("{0} (0x{1,8:X8})", errorMessage, errorCode);
            throw new IOException(ioExceptionMessage);
        }
        else
        {
            return fs;
        }
    }

    /// <summary>
    /// Releases resources
    /// </summary>
    public void Dispose()
    {
        _handle.Dispose();
    }
}