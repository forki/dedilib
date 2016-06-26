using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace DediLib.IO
{
    public class FileNameCleaner
    {
        readonly Regex _regexSearchInvalidFileNameChars = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars()))), RegexOptions.Compiled);
        readonly Regex _regexSearchInvalidPathChars = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidPathChars()))), RegexOptions.Compiled);
        readonly Regex _regexSearchAllInvalidChars = new Regex(string.Format("[{0}]", Regex.Escape(new string(Path.GetInvalidFileNameChars().Union(Path.GetInvalidPathChars()).Distinct().ToArray()))), RegexOptions.Compiled);

        public string ReplaceInvalidFileChars(string fileName, string replaceValue = "")
        {
            if (fileName == null) return null;
            return _regexSearchInvalidFileNameChars.Replace(fileName, replaceValue);
        }

        public string ReplaceInvalidPathChars(string fileName, string replaceValue = "")
        {
            if (fileName == null) return null;
            return _regexSearchInvalidPathChars.Replace(fileName, replaceValue);
        }

        public string ReplaceAllInvalidChars(string fileName, string replaceValue = "")
        {
            if (fileName == null) return null;
            return _regexSearchAllInvalidChars.Replace(fileName, replaceValue);
        }
    }
}
