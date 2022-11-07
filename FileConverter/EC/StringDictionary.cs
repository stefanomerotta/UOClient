using Mythic.Package;
using System.Text;

namespace FileConverter.EC
{
    internal class StringDictionary
    {
        public readonly string[] Entries;

        public StringDictionary(string filepath)
        {
            MythicPackage package = new(filepath);
            byte[] bytes = package.Blocks[0].Files[0].Unpack();

            Span<byte> s = bytes.AsSpan(16);
            List<string> entries = new() { "INVALID" };

            for (int i = 0; i < s.Length;)
            {
                ushort size = BitConverter.ToUInt16(s.Slice(i, 2));
                i += 2;

                entries.Add(Encoding.UTF8.GetString(s.Slice(i, size)));
                i += size;
            }

            Entries = entries.ToArray();
        }

        public string Get(int index)
        {
            if (index < 1 || index >= Entries.Length)
                return "OUT OF BOUNDS";

            return Entries[index];
        }
    }
}
