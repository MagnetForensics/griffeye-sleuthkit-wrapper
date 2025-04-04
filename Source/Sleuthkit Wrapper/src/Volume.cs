﻿using SleuthKit.Structs;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace SleuthKit
{
    /// <summary>
    /// TSK_VS_PART_INFO structure is used to describe each volume. This structure has fields for the volume starting offset (relative to the start of the disk image), length, and type.
    /// </summary>
    public class Volume
    {
        private VolumeSystem _system;
        private TSK_VS_PART_INFO _struct;
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
            var v = TSK_VS_PART_INFO.FromIntPtr(ptrToVolumeInfo);
            if (v.HasValue)
                this._struct = v.Value;
        }

        /// <summary>
        /// The volume system that this partition came from
        /// </summary>
        public VolumeSystem VolumeSystem => this._system;

        internal TSK_VS_PART_INFO VolumeInfo => _struct;

        /// <summary>
        /// Description
        /// </summary>
        public string Description => _struct.Description;

        /// <summary>
        /// Flags
        /// </summary>
        public VolumeFlags Flags => _struct.Flags;

        /// <summary>
        /// Is allocated? <c>(Flags &amp; VolumeFlags.Allocated) != 0</c>
        /// </summary>
        public bool IsAllocated => (Flags & VolumeFlags.Allocated) != 0;

        /// <summary>
        /// The offset in bytes of the start of the partition
        /// </summary>
        public ulong Offset => _struct.SectorOffset * this._system.BlockSize;

        /// <summary>
        /// The length in bytes of this partition
        /// </summary>
        public ulong Length => _struct.SectorLength * this._system.BlockSize;

        /// <summary>
        /// Sector offset of start of partition.
        /// </summary>
        public ulong SectorOffset => _struct.SectorOffset;

        /// <summary>
        /// Sector length of start of partition.
        /// </summary>
        public ulong SectorLength => _struct.SectorLength;

        /// <summary>
        /// Address of this partition
        /// </summary>
        public uint Address => _struct.Address;

        /// <summary>
        /// Opens the filesystem on this volume, if any
        /// </summary>
        /// <param name="fileSystemType"></param>

        public FileSystem OpenFileSystem(FileSystemType fileSystemType = FileSystemType.Autodetect)
        {
            FileSystem fs = new FileSystem(this, fileSystemType);

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
        /// Open pool on this volume, if any
        /// </summary>
        /// <param name="fileSystemType"></param>

        public Pool OpenPool(FileSystemType fileSystemType = FileSystemType.Autodetect)
        {
            var p = new Pool(this, fileSystemType);
            if (p._handle.IsInvalid)
            {
                p._handle.Close();
                p = null;
            }

            return p;
        }

        /// <summary>
        /// A human-friendly description of the volume
        /// </summary>

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