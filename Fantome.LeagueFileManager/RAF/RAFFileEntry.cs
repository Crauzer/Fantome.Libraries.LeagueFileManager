using System;
using System.Collections.Generic;
using System.IO;

namespace Fantome.LeagueFileManager
{
    public partial class RAF
    {
        public class RAFFileEntry : IComparable<RAFFileEntry>
        {
            public string Path { get; private set; }
            public uint Offset { get; private set; }
            public uint Length { get; private set; }
            public uint PathHash { get; private set; }
            private int _pathListIndex;
            private RAF _raf;

            public RAFFileEntry(RAF raf, BinaryReader br)
            {
                this._raf = raf;
                this.PathHash = br.ReadUInt32();
                this.Offset = br.ReadUInt32();
                this.Length = br.ReadUInt32();
                this._pathListIndex = br.ReadInt32();
            }

            public RAFFileEntry(RAF raf, string path, uint offset, uint length)
            {
                this._raf = raf;
                this.Path = path;
                this.Offset = offset;
                this.Length = length;
                this.PathHash = this.GetPathHash();
            }

            public byte[] GetContent(bool compressed)
            {
                this._raf.InitDataStream();
                byte[] data;
                if (compressed)
                {
                    this._raf._dataStream.Seek((int)this.Offset + 2, SeekOrigin.Begin);
                    data = new byte[this.Length - 6];
                    this._raf._dataStream.Read(data, 0, (int)this.Length - 6);
                    return GetDecompressedData(data);
                }
                else
                {
                    this._raf._dataStream.Seek((int)this.Offset, SeekOrigin.Begin);
                    data = new byte[this.Length];
                    this._raf._dataStream.Read(data, 0, (int)this.Length);
                    return data;
                }
            }

            public uint GetPathHash()
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

            public void AssignPath(List<string> paths)
            {
                this.Path = paths[this._pathListIndex];
            }

            public void AssignPathListIndex(int pathListIndex)
            {
                this._pathListIndex = pathListIndex;
            }

            public void Write(BinaryWriter bw)
            {
                bw.Write(this.PathHash);
                bw.Write(this.Offset);
                bw.Write(this.Length);
                bw.Write(this._pathListIndex);
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
}
