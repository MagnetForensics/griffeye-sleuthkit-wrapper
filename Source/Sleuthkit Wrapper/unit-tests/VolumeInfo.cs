using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SleuthkitSharp_UnitTests
{
    class VolumeInfo
    {
        public long Length { get; set; }

        public long Start { get; set; }

        public bool Allocated { get; set; }
    }
}
