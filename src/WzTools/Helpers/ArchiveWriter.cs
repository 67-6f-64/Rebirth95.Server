﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using WzTools.Package;

namespace WzTools.Helpers
{
    public class ArchiveWriter : BinaryWriter
    {
        private Dictionary<string, int> _stringPool = new Dictionary<string, int>();

        /// <summary>
        /// Encryption to use. Defaults to NopWzEncryption
        /// </summary>
        public IWzEncryption Encryption { get; set; } = new NopWzEncryption();

        public ArchiveWriter(Stream output) : base(output)
        {
        }

        public void Write(string value, byte existingId, byte newId)
        {
            if (_stringPool.TryGetValue(value, out var offset))
            {
                Write((byte)existingId);
                this.WriteCompressedInt(offset);
            }
            else
            {
                Write((byte)newId);
                var bytes = EncodeString(value, out var unicode);
                var actualLength = bytes.Length;
                if (unicode)
                {
                    actualLength /= 2;
                    if (actualLength >= 127)
                    {
                        Write((sbyte) 127);
                        Write((int) actualLength);
                    }
                    else
                    {
                        Write((sbyte) actualLength);
                    }
                }
                else
                {
                    if (actualLength >= 127)
                    {
                        Write((sbyte)-128);
                        Write((int)actualLength);
                    }
                    else
                    {
                        Write((sbyte)-actualLength);
                    }
                }

                Write(bytes);
            }
        }

        public byte[] EncodeString(string value, out bool unicode)
        {
            unicode = ArchiveReader.IsLegalUnicode(value);
            

            byte[] bytes;

            var encoding = unicode ? Encoding.Unicode : Encoding.ASCII;

            bytes = encoding.GetBytes(value);

            Encryption.Encrypt(bytes);

            bytes = bytes.ApplyStringXor(unicode);

            return bytes;
        }
    }
}
