using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs;

[StructLayout(LayoutKind.Sequential)]
internal struct TSK_POOL_INFO
{
    internal StructureMagic tag;

    internal PoolTypeEnum ctype;

    internal uint block_size;

    internal ulong num_blocks;

    internal int num_vols;

    internal ulong img_offset;

    internal IntPtr ptr_vol_list;// TSK_POOL_VOLUME_INFO* vol_list;     // Linked list of volume info structs

    // Callbacks
    //void (* close) (const struct _TSK_POOL_INFO *);  ///< \internal
    internal IntPtr close;

    //uint8_t(*poolstat)(const struct _TSK_POOL_INFO *pool, FILE *hFile)
    internal IntPtr poolstat;

    //TSK_IMG_INFO* (* get_img_info) (const struct _TSK_POOL_INFO *pool, TSK_DADDR_T pvol_block)
    internal IntPtr ptr_get_img_info;

    //void* impl;  ///< \internal Implementation specific pointer
    internal IntPtr impl;

    /// <summary>
    /// First volume info
    /// </summary>
    internal TSK_POOL_VOLUME_INFO? FirstPoolVolumeInfo => TSK_POOL_VOLUME_INFO.FromIntPtr(this.ptr_vol_list);

    /// <summary>
    /// All volume infos
    /// </summary>
    internal IEnumerable<TSK_POOL_VOLUME_INFO> PoolVolumeInfos
    {
        get
        {
            var first = this.FirstPoolVolumeInfo;

            var cur = first;
            while (cur.HasValue)
            {
                yield return cur.Value;
                cur = cur.Value.Next;
            }
        }
    }
}