using System;
using System.Runtime.InteropServices;

namespace SleuthKit.Structs
{
    /// <summary>
    /// TSK_VS_PART_INFO
    /// </summary>
    public struct TSK_VS_PART_INFO
    {
        /// <summary>
        /// Tag of TSK_VS_PART_INFO
        /// </summary>
        private int tag;

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
        private ulong start;

        /// <summary>
        /// Number of sectors in partition
        /// </summary>
        private ulong len;

        /// <summary>
        ///  UTF-8 description of partition (volume system type-specific)
        /// </summary>
        private IntPtr ptr_utf8desc;// char* desc;

        /// <summary>
        /// Table address that describes this partition
        /// </summary>
        private sbyte table_num;

        /// <summary>
        /// Entry in the table that describes this partition
        /// </summary>
        private short slot_num;

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
        public ulong SectorOffset => this.start;

        /// <summary>
        /// Size in bytes of this volume
        /// </summary>
        public ulong SectorLength => this.len;

        /// <summary>
        /// Table address that describes this partition.
        /// </summary>
        public int TableNumber => table_num;

        /// <summary>
        /// Table address that describes this partition.
        /// </summary>
        public int SlotNumber => slot_num;

        /// <summary>
        /// Address of this partition
        /// </summary>
        public uint Address => addr;

        /// <summary>
        /// Flags.. what more can you say?
        /// </summary>
        public VolumeFlags Flags => this.flags;

        internal static TSK_VS_PART_INFO? FromIntPtr(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero)
                return null;
            else
                return (TSK_VS_PART_INFO)Marshal.PtrToStructure(ptr, typeof(TSK_VS_PART_INFO));
        }

        /// <summary>
        /// Next volume, if any
        /// </summary>
        public TSK_VS_PART_INFO? Next => FromIntPtr(this.ptr_next_part);

        /// <summary>
        /// Previous volume, if any
        /// </summary>
        public TSK_VS_PART_INFO? Previous => FromIntPtr(this.ptr_prev_part);

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
}