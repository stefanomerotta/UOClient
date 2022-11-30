namespace FileConverter
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            string uoCCPath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic";
            string uoECPath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Enhanced";

            TerrainConverter p = new(uoCCPath, 4, 1448, 1448);
            p.Convert("terrain.bin");

            StaticsConverter s = new(uoCCPath, 4, 1448, 1448);
            s.Convert("statics.bin");

            StaticsDataConverter t = new(uoECPath);
            t.Convert("tiledata.bin", "ecTextures.bin", "ccTextures.bin");

            //AnimationsConverter c = new(uoCCPath);
            //c.Convert(uoCCPath, "animations.bin");
        }
    }
}