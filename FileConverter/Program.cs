using FileConverter.EC;

namespace FileConverter
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            string uoCCPath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic";
            string uoECPath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Enhanced";
            string outPath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic";

            int newChunkSize = 32;

            StringDictionary dictionary = new(Path.Combine(uoECPath, "string_dictionary.uop"));

            using StaticsConverter s = new(uoCCPath, outPath, 4, 1448, 1448, newChunkSize);
            s.Convert("statics.bin");

            using StaticsDataConverter t = new(uoECPath, outPath, dictionary);
            t.Convert("staticdata.bin", "ecTextures.bin", "ccTextures.bin");

            using TerrainConverter p = new(uoCCPath, outPath, 4, 1448, 1448, newChunkSize);
            p.Convert("terrain.bin");

            using TerrainsDataConverter tdc = new(uoECPath, outPath);
            tdc.Convert("terraindata.bin", "terraintextures.bin");

            //AnimationsConverter c = new(uoCCPath);
            //c.Convert(uoCCPath, "animations{0}.bin");
        }
    }
}