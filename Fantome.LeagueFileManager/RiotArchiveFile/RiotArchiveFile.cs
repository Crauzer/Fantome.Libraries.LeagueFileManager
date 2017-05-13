using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fantome.LeagueFileManager
{
    public class RiotArchiveFile
    {
        public int Version { get; private set; }
        public int ManagerIndex { get; private set; }

        private void Read(BinaryReader br)
        {
            uint magic = br.ReadUInt32();
            if (magic != 0xF00EBE18)
            {
                throw new InvalidMagicNumberException(magic);
            }
            this.Version = br.ReadInt32();
            this.ManagerIndex = br.ReadInt32();
        }

        public class InvalidMagicNumberException : Exception
        {
            public InvalidMagicNumberException(uint readMagic) : base(String.Format("Invalid magic number (\"{0}\"), expected: \"{1}\".", readMagic, 0xF00EBE18)) { }
        }
    }
}
