using System;
using System.Runtime;

namespace DediLib
{
    public static class PlatformHelpers
    {
        /// <summary>
        /// Returns "true", if current platform is Linux
        /// </summary>
        /// <returns>"true", if current platform is Linux</returns>
        [TargetedPatchingOptOut("")]
        public static bool IsLinux()
        {
            var p = (int)Environment.OSVersion.Platform;
            return (p == 4) || (p == 6) || (p == 128);
        }
    }
}
