using System;
using System.Collections.Generic;
using System.IO;

namespace Fantome.Libraries.LeagueFileManager.RiotArchive
{
    public class RAFFileEntry : IComparable<RAFFileEntry>
    {
        private string _path;
        public string Path
        {
            get
            {
                return _path;
            }
            set
            {
                _path = value;
                PathHash = GetPathHash();
            }
        }
        public uint Offset { get; private set; }
        public uint Length { get; private set; }
        public uint PathHash { get; private set; }
        public int PathListIndex { get; set; }
        private RAF _raf;

        public RAFFileEntry(RAF raf, BinaryReader br)
        {
            this._raf = raf;
            this.PathHash = br.ReadUInt32();
            this.Offset = br.ReadUInt32();
            this.Length = br.ReadUInt32();
            this.PathListIndex = br.ReadInt32();
        }

        public RAFFileEntry(RAF raf, string path, uint offset, uint length)
        {
            this._raf = raf;
            this.Path = path;
            this.Offset = offset;
            this.Length = length;
        }

        public byte[] GetContent(bool decompress)
        {
            this._raf.InitDataStream();
            byte[] data;
            if (decompress)
            {
                this._raf._dataStream.Seek((int)this.Offset + 2, SeekOrigin.Begin);
                data = new byte[this.Length - 6];
                this._raf._dataStream.Read(data, 0, (int)this.Length - 6);
                return RAF.GetDecompressedData(data);
            }
            else
            {
                this._raf._dataStream.Seek((int)this.Offset, SeekOrigin.Begin);
                data = new byte[this.Length];
                this._raf._dataStream.Read(data, 0, (int)this.Length);
                return data;
            }
        }

        private uint GetPathHash()
        {
            uint hash = 0;
            uint temp = 0;
            string path = this.Path.ToLower(new System.Globalization.CultureInfo("en-US", false));
            foreach (char chr in path)
            {
                hash = (hash << 4) + chr;
                temp = hash & 0xf0000000;
                if (temp != 0)
                {
                    hash = hash ^ (temp >> 24);
                    hash = hash ^ temp;
                }
            }
            return hash;
        }

        public void Write(BinaryWriter bw)
        {
            bw.Write(this.PathHash);
            bw.Write(this.Offset);
            bw.Write(this.Length);
            bw.Write(this.PathListIndex);
        }

        public int CompareTo(RAFFileEntry other)
        {
            if (this.PathHash > other.PathHash)
            {
                return 1;
            }
            else if (this.PathHash < other.PathHash)
            {
                return -1;
            }
            else
            {
                // Hash collision
                return String.Compare(this.Path, other.Path, true);
            }
        }
    }
}
