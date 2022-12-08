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

            using TerrainConverter p = new(uoCCPath, outPath, 4, 1448, 1448, newChunkSize);
            p.Convert("terrain.bin");

            using StaticsConverter s = new(uoCCPath, outPath, 4, 1448, 1448, newChunkSize);
            s.Convert("statics.bin");

            using StaticsDataConverter t = new(uoECPath, outPath);
            t.Convert("tiledata.bin", "ecTextures.bin", "ccTextures.bin");

            //AnimationsConverter c = new(uoCCPath);
            //c.Convert(uoCCPath, "animations{0}.bin");
        }
    }
}