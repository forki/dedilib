using System.Collections.Concurrent;
using System.Linq;
using System.Security.Cryptography;

namespace DediLib.Crypto
{
    public static class SHA1Pool
    {
        private static readonly ConcurrentStack<SHA1Managed> Stack =
            new ConcurrentStack<SHA1Managed>(Enumerable.Range(0, 16).Select(x => new SHA1Managed()));

        public static SHA1Managed Aquire()
        {
            SHA1Managed sha1;
            return Stack.TryPop(out sha1) ? sha1 : new SHA1Managed();
        }

        public static void Release(SHA1Managed sha1)
        {
            Stack.Push(sha1);
        }
    }
}
