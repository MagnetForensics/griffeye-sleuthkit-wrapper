using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.MemoryLeak
{
    public class MediaPath
    {
        private readonly String displayPath;

        private readonly bool hasVolumes;

        private readonly long volumeAddress;

        private readonly long fileAddress;

        public String InternalPath
        {
            get 
            {
                return hasVolumes
                    ? String.Format("{0}\\{1}", volumeAddress, fileAddress)
                    : fileAddress.ToString();
            }
        }

        public String DisplayPath
        {
            get 
            {
                return displayPath;
            }
        }

        public long VolumeAddress
        {
            get 
            {
                return volumeAddress;
            }
        }

        public long FileAddress
        {
            get
            {
                return fileAddress;
            }
        }
        
        public MediaPath(String displayPath, long fileAddress)
        {
            this.displayPath = displayPath;

            this.fileAddress = fileAddress;

            hasVolumes = false;
        }

        public MediaPath(String displayPath, long volumeAddress, long fileAddress)
        {
            this.displayPath = displayPath;

            this.volumeAddress = volumeAddress;
            this.fileAddress = fileAddress;

            hasVolumes = true;
        }
    }
}
