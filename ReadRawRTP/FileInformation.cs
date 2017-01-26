using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadRawRTP
{
    public class FileInformation

    {
        public int fileInformationOneLine = 0;
        public int offset = 0;
        public int size = 0;

        public FileInformation(int offset, int size, int fileInformationOneLine)
        {
            this.fileInformationOneLine = fileInformationOneLine;
            this.size = size;
            this.offset = offset;
        }


    }
}
