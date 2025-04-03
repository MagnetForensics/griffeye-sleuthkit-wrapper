using System;

namespace SleuthKit
{
    public class VolumeInformation
    {
        public uint Address { get; set; }
        public String Description { get; set; }
        public VolumeFlags Flags { get; set; }
        public bool IsAllocated { get; set; }
        public ulong Length { get; set; }
        public ulong Offset { get; set; }
        public ulong SectorLength { get; set; }
        public ulong SectorOffset { get; set; }
    }
}