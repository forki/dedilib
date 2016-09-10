using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime;
using System.Runtime.Serialization;
using System.Text;

namespace DediLib.Collections
{
    public static class HashSetExtensions
    {
        [TargetedPatchingOptOut("")]
        public static HashSet<T> Clone<T>(this HashSet<T> original)
        {
            if (original.Count < 60) return new HashSet<T>(original, original.Comparer);

            var clone = (HashSet<T>)FormatterServices.GetUninitializedObject(typeof(HashSet<T>));
            Copy(Fields<T>.Comparer, original, clone);

            if (original.Count == 0)
            {
                Fields<T>.FreeList.SetValue(clone, -1);
            }
            else
            {
                Fields<T>.Count.SetValue(clone, original.Count);
                Clone(Fields<T>.Buckets, original, clone);
                Clone(Fields<T>.Slots, original, clone);
                Copy(Fields<T>.FreeList, original, clone);
                Copy(Fields<T>.LastIndex, original, clone);
                Copy(Fields<T>.Version, original, clone);
            }

            return clone;
        }

        static void Copy<T>(FieldInfo field, HashSet<T> source, HashSet<T> target)
        {
            field.SetValue(target, field.GetValue(source));
        }

        static void Clone<T>(FieldInfo field, HashSet<T> source, HashSet<T> target)
        {
            field.SetValue(target, ((Array)field.GetValue(source)).Clone());
        }

        static class Fields<T>
        {
            // ReSharper disable StaticFieldInGenericType
            public static readonly FieldInfo FreeList = GetFieldInfo("m_freeList");
            public static readonly FieldInfo Buckets = GetFieldInfo("m_buckets");
            public static readonly FieldInfo Slots = GetFieldInfo("m_slots");
            public static readonly FieldInfo Count = GetFieldInfo("m_count");
            public static readonly FieldInfo LastIndex = GetFieldInfo("m_lastIndex");
            public static readonly FieldInfo Version = GetFieldInfo("m_version");
            public static readonly FieldInfo Comparer = GetFieldInfo("m_comparer");
            // ReSharper restore StaticFieldInGenericType

            static FieldInfo GetFieldInfo(string name)
            {
                return typeof(HashSet<T>).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            }
        }

        /// <summary>
        /// Checks if a delimited part is in the HashSet
        /// </summary>
        /// <remarks>
        /// Example:<br/>
        /// <br/>
        /// var hashSet = new HashSet&lt;string&gt; { "test.it" };<br/>
        /// hashSet.ContainsSuffixFor("test", '.'); // false<br/>
        /// hashSet.ContainsSuffixFor("test.it", '.'); // true<br/>
        /// hashSet.ContainsSuffixFor("it", '.'); // false<br/>
        /// hashSet.ContainsSuffixFor("www.test.it", '.'); // true<br/>
        /// hashSet.ContainsSuffixFor("www.sub.test.it", '.'); // true<br/>
        /// hashSet.ContainsSuffixFor("wwwtest.it", '.'); // false<br/>
        /// </remarks>
        /// <param name="hashSet"></param>
        /// <param name="text"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        [TargetedPatchingOptOut("")]
        public static bool ContainsSuffixFor(this HashSet<string> hashSet, string text, char delimiter)
        {
            if (string.IsNullOrEmpty(text)) return false;

            var sb = new StringBuilder();
            var parts = text.Split(delimiter);
            for (var i = parts.Length - 1; i >= 0; i--)
            {
                if (sb.Length > 0) sb.Insert(0, delimiter);
                sb.Insert(0, parts[i]);
                if (hashSet.Contains(sb.ToString())) return true;
            }
            return false;
        }
    }
}
