using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.LeagueFileManager
{
    public partial class RAF
    {
        public string FilePath { get; private set; }
        public int Version { get; private set; }
        public int ManagerIndex { get; private set; }
        public List<RAFFileEntry> Files { get; private set; } = new List<RAFFileEntry>();

        public RAF(string filePath)
        {
            this.FilePath = filePath;
            using (BinaryReader br = new BinaryReader(File.OpenRead(filePath), Encoding.ASCII))
            {
                this.Read(br);
            }
        }

        private void Read(BinaryReader br)
        {
            uint magic = br.ReadUInt32();
            if (magic != 0x18be0ef0)
            {
                throw new InvalidMagicNumberException(magic);
            }
            this.Version = br.ReadInt32();
            this.ManagerIndex = br.ReadInt32();
            uint fileListOffset = br.ReadUInt32();
            uint pathListOffset = br.ReadUInt32();

            // Reading file list
            br.BaseStream.Seek(fileListOffset, SeekOrigin.Begin);
            int fileCount = br.ReadInt32();
            for (int i = 0; i < fileCount; i++)
            {
                this.Files.Add(new RAFFileEntry(br));
            }
            // Reading path list
            br.BaseStream.Seek(pathListOffset, SeekOrigin.Begin);
            PathList pathList = new PathList(br);
            List<string> paths = pathList.paths;
            foreach (RAFFileEntry fileEntry in this.Files)
            {
                fileEntry.AssignPath(paths);
            }
        }

        private void Write(BinaryWriter bw)
        {
            bw.Write(0x18be0ef0);
            bw.Write(this.Version);
            bw.Write(this.ManagerIndex);
            // File list offset
            bw.Write(20);
            // Path list offset
            int pathListOffset = 24 + (this.Files.Count * 16);
            bw.Write(pathListOffset);
            this.Files.Sort();
        }

        private class PathList
        {
            private uint _size;
            private int _count;
            private uint _offset;
            public List<string> paths { get; private set; } = new List<string>();

            public PathList(BinaryReader br)
            {
                this._offset = (uint)br.BaseStream.Position;
                this._size = br.ReadUInt32();
                this._count = br.ReadInt32();
                for (int i = 0; i < _count; i++)
                {
                    uint pathOffset = br.ReadUInt32();
                    int pathLength = br.ReadInt32();
                    long currentOffset = br.BaseStream.Position;
                    br.BaseStream.Seek(this._offset + pathOffset, SeekOrigin.Begin);
                    this.paths.Add(Encoding.ASCII.GetString(br.ReadBytes(pathLength - 1)));
                    br.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                }
            }
        }

        public class InvalidMagicNumberException : Exception
        {
            public InvalidMagicNumberException(uint readMagic) : base(String.Format("Invalid magic number (\"{0}\"), expected: \"{1}\".", readMagic, 0xF00EBE18)) { }
        }
    }
}
