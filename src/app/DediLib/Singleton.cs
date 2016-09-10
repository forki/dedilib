using System.Runtime;

namespace DediLib
{
    public class Singleton<T> where T : class, new()
    {
        private static volatile T _instance;
        public static T Instance
        {
            get
            {
                if (_instance != null) return _instance;
                return _instance = new T();
            }
        }

        private Singleton()
        {
        }

        [TargetedPatchingOptOut("")]
        public static void SetInstance(T instance)
        {
            _instance = instance;
        }
    }
}
