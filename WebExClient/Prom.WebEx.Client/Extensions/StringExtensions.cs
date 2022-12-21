namespace Prom.WebEx.Client.Extensions
{
    internal static class StringExtensions
    {
        private static string numberPattern = " ({0})";
        public static string NextAvailableFilename(this string path)
        {
            // Short-cut if already available
            if (!File.Exists(path))
                return path;

            // If path has extension then insert the number pattern just before the extension and return next filename
            if (Path.HasExtension(path))
                return GetNextFilename(path.Insert(path.LastIndexOf(Path.GetExtension(path)), numberPattern), f => File.Exists(f));

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern, f => File.Exists(f));
        }

        public static string NextAvailableDirectory(this string path)
        {
            // Short-cut if already available
            if (!Directory.Exists(path))
                return path;

            // Otherwise just append the pattern to the path and return next filename
            return GetNextFilename(path + numberPattern, d => Directory.Exists(d));
        }

        private static string GetNextFilename(string pattern, Predicate<string> existsPredicate)
        {
            string tmp = string.Format(pattern, 1);

            if (!existsPredicate(tmp))
                return tmp; // short-circuit if no matches

            int min = 1, max = 2; // min is inclusive, max is exclusive/untested

            while (existsPredicate(string.Format(pattern, max)))
            {
                min = max;
                max *= 2;
            }

            while (max != min + 1)
            {
                int pivot = (max + min) / 2;
                if (existsPredicate(string.Format(pattern, pivot)))
                    min = pivot;
                else
                    max = pivot;
            }

            return string.Format(pattern, max);
        }
    }
}
