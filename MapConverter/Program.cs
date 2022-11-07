namespace FileConverter
{
    public class Program
    {
        public static unsafe void Main(string[] args)
        {
            Converter p = new(".\\", 4, 1448, 1448);
            p.Convert("converted.bin");
        }
    }
}