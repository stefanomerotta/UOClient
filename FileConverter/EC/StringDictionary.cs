using MYPReader;
using System.Text;

namespace FileConverter.EC
{
    internal class StringDictionary
    {
        public readonly string[] Entries;

        public StringDictionary(string filepath)
        {
            using MythicPackage package = new(filepath);

            MythicPackage.MythicPackageEnumerator enumerator = package.GetEnumerator();
            enumerator.MoveNext();

            byte[] bytes = package.UnpackFile(in enumerator.Current);

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
