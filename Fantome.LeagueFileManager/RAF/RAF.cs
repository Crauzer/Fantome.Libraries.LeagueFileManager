using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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
            foreach (RAFFileEntry fileEntry in this.Files)
            {
                fileEntry.AssignPath(pathList.Paths);
            }
        }

        public void Save()
        {
            this.Save(this.FilePath);
        }

        public void Save(string filePath)
        {
            using (BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create)))
            {
                this.Write(bw);
            }
        }

        private void Write(BinaryWriter bw)
        {
            // Preparing file entries before writing
            this.Files.Sort();
            for (int i = 0; i < this.Files.Count; i++)
            {
                this.Files[i].AssignPathListIndex(i);
            }

            bw.Write(0x18be0ef0);
            bw.Write(this.Version);
            bw.Write(this.ManagerIndex);
            // File list offset
            bw.Write(20);
            // Path list offset
            int pathListOffset = 24 + (this.Files.Count * 16);
            bw.Write(pathListOffset);
            bw.Write(this.Files.Count);
            foreach (RAFFileEntry fileEntry in this.Files)
            {
                fileEntry.Write(bw);
            }
            PathList pathList = new PathList(this.Files);
            pathList.Write(bw);
        }

        private class PathList
        {
            private uint _size;
            public List<string> Paths { get; private set; } = new List<string>();

            public PathList(BinaryReader br)
            {
                long offset = br.BaseStream.Position;
                this._size = br.ReadUInt32();
                int count = br.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    uint pathOffset = br.ReadUInt32();
                    int pathLength = br.ReadInt32();
                    long currentOffset = br.BaseStream.Position;
                    br.BaseStream.Seek(offset + pathOffset, SeekOrigin.Begin);
                    this.Paths.Add(Encoding.ASCII.GetString(br.ReadBytes(pathLength - 1)));
                    br.BaseStream.Seek(currentOffset, SeekOrigin.Begin);
                }
            }

            public PathList(List<RAFFileEntry> files)
            {
                this._size = 0;
                foreach (RAFFileEntry fileEntry in files)
                {
                    Paths.Add(fileEntry.Path);
                    this._size += (uint)fileEntry.Path.Length + 1;
                }
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this._size);
                bw.Write(this.Paths.Count);
                // 8 bytes for each path entry
                int currentOffset = 8 + this.Paths.Count * 8;
                foreach (string path in this.Paths)
                {
                    bw.Write(currentOffset);
                    bw.Write(path.Length + 1);
                    currentOffset += path.Length + 1;
                }
                foreach (string path in this.Paths)
                {
                    bw.Write(Encoding.ASCII.GetBytes(path));
                    bw.Write((byte)0);
                }
            }
        }

        public class InvalidMagicNumberException : Exception
        {
            public InvalidMagicNumberException(uint readMagic) : base(String.Format("Invalid magic number (\"{0}\"), expected: \"{1}\".", readMagic, 0xF00EBE18)) { }
        }
    }
}
