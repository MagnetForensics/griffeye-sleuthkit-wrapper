﻿using System;
using System.Text;

namespace SleuthKit
{
    /// <summary>
    /// TSK_VS_PART_INFO structure is used to describe each volume. This structure has fields for the volume starting offset (relative to the start of the disk image), length, and type.
    /// </summary>
    public class Volume
    {
        private VolumeSystem _system;
        private VolumeInfo _struct;
        internal IntPtr _ptr_volinfo;

        /// <summary>
        /// ctor, for internal use only
        /// </summary>
        /// <param name="volumeSystem"></param>
        /// <param name="ptrToVolumeInfo"></param>
        internal Volume(VolumeSystem volumeSystem, IntPtr ptrToVolumeInfo)
        {
            this._system = volumeSystem;

            this._ptr_volinfo = ptrToVolumeInfo;
            var v = VolumeInfo.FromIntPtr(ptrToVolumeInfo);
            if (v.HasValue)
                this._struct = v.Value;
        }

        /// <summary>
        /// The volume system that this partition came from
        /// </summary>
        public VolumeSystem VolumeSystem
        {
            get
            {
                return this._system;
            }
        }

        internal VolumeInfo VolumeInfo
        {
            get
            {
                return _struct;
            }
        }

        /// <summary>
        /// Description
        /// </summary>
        public string Description
        {
            get
            {
                return _struct.Description;
            }
        }

        /// <summary>
        /// Flags
        /// </summary>
        public VolumeFlags Flags
        {
            get
            {
                return _struct.Flags;
            }
        }

        /// <summary>
        /// Is allocated? <c>(Flags &amp; VolumeFlags.Allocated) != 0</c>
        /// </summary>
        public bool IsAllocated
        {
            get
            {
                return (Flags & VolumeFlags.Allocated) != 0;
            }
        }

        /// <summary>
        /// The offset in bytes of the start of the partition
        /// </summary>
        public long Offset
        {
            get
            {
                return _struct.SectorOffset * this._system.BlockSize;
            }

        }

        /// <summary>
        /// The length in bytes of this partition
        /// </summary>
        public long Length
        {
            get
            {
                return _struct.SectorLength * this._system.BlockSize;
            }
        }

        /// <summary>
        /// Sector offset of start of partition. 
        /// </summary>
        public long SectorOffset
        {
            get
            {
                return _struct.SectorOffset;
            }
        }

        /// <summary>
        /// Sector length of start of partition. 
        /// </summary>
        public long SectorLength
        {
            get
            {
                return _struct.SectorLength;
            }
        }

        /// <summary>
        /// Address of this partition
        /// </summary>
        public long Address
        {
            get 
            {
                return _struct.Address;
            }
        }

        /// <summary>
        /// Opens the filesystem on this volume, if any
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public FileSystem OpenFileSystem(FileSystemType type = FileSystemType.Autodetect)
        {
            return new FileSystem(this,type);
        }

        /// <summary>
        /// A human-friendly description of the volume
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var buf = new StringBuilder();
            buf.AppendFormat("offset={0},len={1},status={2}", this.Offset, this.Length, this.Flags);
            string desc = this.Description;
            if (desc != null)
                buf.AppendFormat(",desc={0}", desc);
            return buf.ToString();
        }
    }
}