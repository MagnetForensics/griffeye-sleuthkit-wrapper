using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs;

[StructLayout(LayoutKind.Sequential)]
internal struct TSK_POOL_VOLUME_INFO
{
    /// <summary>
    /// Set to TSK_POOL_VOLUME_INFO_TAG when struct is alloc
    /// </summary>
    internal StructureMagic tag;

    /// <summary>
    /// Index of Volume
    /// </summary>
    internal uint index;

    /// <summary>
    /// UTF-8 description of partition (volume system type-specific)
    /// </summary>
    private IntPtr ptr_utf8desc;// char* desc;
    
    /// <summary>
    /// UTF-8 password hint for encrypted volumes
    /// </summary>
    private IntPtr ptr_utf8password_hint;// char* password_hint;

    /// <summary>
    /// Starting block (for pool volumes only)
    /// </summary>
    private ulong block;

    /// <summary>
    /// Number of blocks in the volume
    /// </summary>
    private ulong num_blocks;

    /// <summary>
    /// Pointer to next partition (or NULL)
    /// </summary>
    internal IntPtr ptr_next_vol;// TSK_POOL_VOLUME_INFO* next; 

    /// <summary>
    /// Pointer to previous partition (or NULL)
    /// </summary>
    internal IntPtr ptr_prev_vol;// TSK_POOL_VOLUME_INFO* prev; 
    
    /// <summary>
    /// Pool volume flags
    /// </summary>
    internal PoolVolumeFlags flags;
    
    internal static TSK_POOL_VOLUME_INFO? FromIntPtr(IntPtr ptr)
    {
        if (ptr == IntPtr.Zero)
            return null;
        else
            return (TSK_POOL_VOLUME_INFO)Marshal.PtrToStructure(ptr, typeof(TSK_POOL_VOLUME_INFO));
    }

    /// <summary>
    /// Starting block (for pool volumes only)
    /// </summary>
    public ulong Block
    {
        get
        {
            return block;
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

    /// <summary>
    /// Pool volume flags
    /// </summary>
    public PoolVolumeFlags Flags
    {
        get
        {
            return flags;
        }
    }

    /// <summary>
    /// Next volume, if any
    /// </summary>
    internal TSK_POOL_VOLUME_INFO? Next
    {
        get
        {
            return FromIntPtr(this.ptr_next_vol);
        }
    }

    /// <summary>
    /// Previous volume, if any
    /// </summary>
    internal TSK_POOL_VOLUME_INFO? Previous
    {
        get
        {
            return FromIntPtr(this.ptr_prev_vol);
        }
    }
    
    /// <summary>
    /// Password hint
    /// </summary>
    public string PasswordHint
    {
        get
        {
            string ret = null;

            if (ptr_utf8password_hint != IntPtr.Zero)
            {
                UTF8Marshaler mars = new UTF8Marshaler();
                ret = (string)mars.MarshalNativeToManaged(ptr_utf8password_hint);
            }
            return ret;
        }
    }
}