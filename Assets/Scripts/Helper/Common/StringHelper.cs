namespace yxy
{
    public static class StringHelper
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        public static string GetDirectoryPath(this string assetPath)
        {
            if (assetPath.IsNullOrEmpty()) return assetPath;

            assetPath = assetPath.Replace("\\", "/");
            int lastIndex = assetPath.LastIndexOf('/');
            return assetPath[..(lastIndex >= 0 ? lastIndex : 0)];
        }
    }
}

