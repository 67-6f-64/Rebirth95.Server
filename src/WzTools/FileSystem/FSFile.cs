using System.IO;
using System.IO.MemoryMappedFiles;
using WzTools.Helpers;

namespace WzTools.FileSystem
{
    public class FSFile : NameSpaceFile
    {
        public string RealPath { get; set; }
        

        public override ArchiveReader GetReader()
        {
            var mmf = MemoryMappedFile.CreateFromFile(
                RealPath,
                FileMode.Open
            );
            return new ArchiveReader(mmf.CreateViewStream(), 0);
        }
    }
}
