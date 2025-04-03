using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace SleuthKit.Structs
{
    /// <summary>
    /// represents TSK_VS_INFO.  Data structure used to store state and basic information for open volume systems.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TSK_VS_INFO
    {
        /// <summary>
        /// Will be set to TSK_VS_INFO_TAG if structure is still allocated.
        /// </summary>
        private int tag;

        /// <summary>
        /// Pointer to disk image that VS is in
        /// </summary>
        private IntPtr ptr_imageInfo; //TSK_IMG_INFO *img_info;

        /// <summary>
        /// Type of volume system / media management
        /// </summary>
        private VolumeSystemType vstype;

        /// <summary>
        /// 1 if the partition table found was a backup
        /// </summary>
        private int is_backup;

        /// <summary>
        /// Byte offset where VS starts in disk image
        /// </summary>
        private long offset;

        /// <summary>
        /// Size of blocks in bytes
        /// </summary>
        private uint block_size;

        /// <summary>
        /// Endian ordering of data
        /// </summary>
        private Endianness endian;

        /// <summary>
        /// Linked list of partitions
        /// </summary>
        internal IntPtr ptr_first_volumeinfo;// TSK_VS_PART_INFO *part_list;    //

        /// <summary>
        /// number of partitions
        /// </summary>
        private long part_count;

        //void (*close) (TSK_VS_INFO *);  // \internal Progs should call tsk_vs_close().
        private IntPtr funcptr_close;

        /// <summary>
        /// The offset to the start of this volume system
        /// </summary>
        public long Offset => offset;

        /// <summary>
        /// The type of volume system (MBR, APM, GPT, etc)
        /// </summary>
        public VolumeSystemType Type => vstype;

        /// <summary>
        /// The endianness of this volume system (little, big, etc)
        /// </summary>
        public Endianness Endianness => this.endian;

        /// <summary>
        /// The number of partitions on this volume system
        /// </summary>
        public long PartitionCount => part_count;

        /// <summary>
        /// The number of blocks
        /// </summary>
        public uint BlockSize => block_size;

        /// <summary>
        /// First volume info
        /// </summary>
        internal TSK_VS_PART_INFO? FirstVolumeInfo => TSK_VS_PART_INFO.FromIntPtr(this.ptr_first_volumeinfo);

        /// <summary>
        /// All volume infos
        /// </summary>
        internal IEnumerable<TSK_VS_PART_INFO> VolumeInfos
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
}