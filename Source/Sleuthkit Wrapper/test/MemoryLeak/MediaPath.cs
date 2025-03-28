using System;

namespace Test.MemoryLeak
{
    public class MediaPath
    {
        private readonly String displayPath;

        private readonly bool hasVolumes;

        private readonly uint volumeAddress;

        private readonly ulong fileAddress;

        public String InternalPath => hasVolumes
                    ? String.Format("{0}\\{1}", volumeAddress, fileAddress)
                    : fileAddress.ToString();

        public String DisplayPath => displayPath;

        public uint VolumeAddress => volumeAddress;

        public ulong FileAddress => fileAddress;

        public MediaPath(String displayPath, ulong fileAddress)
        {
            this.displayPath = displayPath;

            this.fileAddress = fileAddress;

            hasVolumes = false;
        }

        public MediaPath(String displayPath, uint volumeAddress, ulong fileAddress)
        {
            this.displayPath = displayPath;

            this.volumeAddress = volumeAddress;
            this.fileAddress = fileAddress;

            hasVolumes = true;
        }
    }
}