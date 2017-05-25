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

            public RAFFileEntry(BinaryReader br)
            {
                this.PathHash = br.ReadUInt32();
                this.Offset = br.ReadUInt32();
                this.Length = br.ReadUInt32();
                this._pathListIndex = br.ReadInt32();
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

            public RAFFileEntry(string path, uint offset, uint length)
            {
                this.Path = path;
                this.Offset = offset;
                this.Length = length;
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
