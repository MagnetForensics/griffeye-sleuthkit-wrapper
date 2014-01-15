using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test.MemoryLeak
{
    public class MediaInfo
    {
        public String DirectoryName { get; set; }
        public String DisplayPath { get; set; }
        public bool Exists { get; set; }
        public String Extension { get; set; }
        public long Length { get; set; }
        public String Name { get; set; }

        public DateTime CreationTimeUtc { get; set; }
        public DateTime LastWriteTimeUtc { get; set; }
        public DateTime LastAccessTimeUtc { get; set; }

        public MediaInfo()
        {
            DirectoryName = String.Empty;
            DisplayPath = String.Empty;
            Extension = String.Empty;
            Name = String.Empty;

            CreationTimeUtc = DateTime.UtcNow;
            LastWriteTimeUtc = DateTime.UtcNow;
            LastAccessTimeUtc = DateTime.UtcNow;
        }
    }
}
