namespace UOClient
{
    internal static class Settings
    {
#if DEBUG
        public static string FilePath = "C:\\Program Files (x86)\\Electronic Arts\\Ultima Online Classic";
#else
        public static string FilePath = ".\\";
#endif

        public const bool UseEnhancedTextures = false;
    }
}
