namespace FileConverter
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            //MapConverter p = new(".\\", 4, 1448, 1448);
            //p.Convert("converted.bin");

            StaticsConverter t = new(".\\");
            t.Convert("tiledata.bin", "ecTextures.bin", "ccTextures.bin");
        }
    }
}