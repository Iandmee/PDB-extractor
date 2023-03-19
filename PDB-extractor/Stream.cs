using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdbExtractor
{
    struct Stream
    {
        public int[] pointers;
        public int size;
        public Stream(int size, int blockSize)
        {
            this.size = size;
            pointers = new int[size % blockSize == 0 ? size / blockSize : size / blockSize + 1];

        }
    }
}
