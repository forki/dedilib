using System;
using System.Collections.Generic;
using System.IO;

namespace DediLib.Configuration
{
    public static class LoadConfigHelpers
    {
        public static string TrimComment(this string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int pos = text.IndexOf('#');
            if (pos < 0) return text.Trim();
            return text.Substring(0, pos).Trim();
        }

        public static string TrimComment(this string text, char commentChar)
        {
            if (string.IsNullOrEmpty(text)) return text;
            int pos = text.IndexOf(commentChar);
            if (pos < 0) return text.Trim();
            return text.Substring(0, pos).Trim();
        }

        public static HashSet<string> ReadNonEmptyLinesToHashSet(string fileName, Func<string, string> lineFunc = null)
        {
            using (var reader = File.OpenText(fileName))
            {
                return ReadNonEmptyLinesToHashSet(reader, lineFunc);
            }
        }

        public static HashSet<string> ReadNonEmptyLinesToHashSet(StreamReader reader,
            Func<string, string> lineFunc = null)
        {
            return ReadNonEmptyLinesToHashSet(reader, false, lineFunc);
        }

        public static HashSet<string> ReadNonEmptyLinesToHashSet(StreamReader reader, 
            bool ignoreCase, Func<string, string> lineFunc = null)
        {
            var res = ignoreCase ? 
                new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) : 
                new HashSet<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line;
                if (lineFunc != null) trimmed = lineFunc(trimmed);
                if (trimmed == "") continue;
                res.Add(trimmed);
            }
            return res;
        }

        public static List<string> ReadNonEmptyLinesToList(string fileName, Func<string, string> lineFunc = null)
        {
            using (var reader = File.OpenText(fileName))
            {
                return ReadNonEmptyLinesToList(reader, lineFunc);
            }
        }

        public static List<string> ReadNonEmptyLinesToList(StreamReader reader, Func<string, string> lineFunc = null)
        {
            var res = new List<string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line;
                if (lineFunc != null) trimmed = lineFunc(trimmed);
                if (trimmed == "") continue;
                res.Add(trimmed);
            }
            return res;
        }

        public static Dictionary<string, string> ReadNonEmptyLinesToDictionary(string fileName,
            Func<string, string> lineFunc = null)
        {
            return ReadNonEmptyLinesToDictionary(fileName, false, lineFunc);
        }

        public static Dictionary<string, string> ReadNonEmptyLinesToDictionary(string fileName,
            bool ignoreCase, Func<string, string> lineFunc = null)
        {
            using (var reader = File.OpenText(fileName))
            {
                return ReadNonEmptyLinesToDictionary(reader, ignoreCase, lineFunc);
            }
        }

        public static Dictionary<string, string> ReadNonEmptyLinesToDictionary(StreamReader reader,
            Func<string, string> lineFunc = null)
        {
            return ReadNonEmptyLinesToDictionary(reader, false, lineFunc);
        }

        public static Dictionary<string, string> ReadNonEmptyLinesToDictionary(StreamReader reader, 
            bool ignoreCase, Func<string, string> lineFunc = null)
        {
            var res = ignoreCase ? 
                new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) : 
                new Dictionary<string, string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                var trimmed = line;
                if (lineFunc != null) trimmed = lineFunc(trimmed);
                if (trimmed == "") continue;

                int pos = trimmed.IndexOfAny(new[] { ' ', '\t' });
                if (pos < 0)
                {
                    res[trimmed] = null;
                    continue;
                }

                res[trimmed.Substring(0, pos).TrimEnd()] = trimmed.Substring(pos + 1).TrimStart();
            }
            return res;
        }
    }
}
