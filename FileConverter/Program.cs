namespace FileConverter
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            string uoPath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic";

            //TerrainConverter p = new(uoPath, 4, 1448, 1448);
            //p.Convert("terrain.bin");

            //StaticsConverter s = new(uoPath, 4, 1448, 1448);
            //s.Convert("statics.bin");

            StaticsDataConverter t = new(".\\");
            t.Convert("tiledata.bin", "ecTextures.bin", "ccTextures.bin");
        }
    }
}