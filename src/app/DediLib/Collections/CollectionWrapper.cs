using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;

namespace DediLib.Collections
{
    public class CollectionWrapper<TFrom, TTo> : ICollection<TTo>
    {
        private readonly ICollection<TFrom> _underlyingCollection;
        private readonly Func<TFrom, TTo> _translateFromTo;
        private readonly Func<TTo, TFrom> _translateToFrom;

        [TargetedPatchingOptOut("")]
        public CollectionWrapper(ICollection<TFrom> underlyingCollection, Func<TFrom, TTo> translateFromTo, Func<TTo, TFrom> translateToFrom)
        {
            if (underlyingCollection == null) throw new ArgumentNullException(nameof(underlyingCollection));
            if (translateFromTo == null) throw new ArgumentNullException(nameof(translateFromTo));
            if (translateToFrom == null) throw new ArgumentNullException(nameof(translateToFrom));

            _underlyingCollection = underlyingCollection;
            _translateFromTo = translateFromTo;
            _translateToFrom = translateToFrom;
        }

        [TargetedPatchingOptOut("")]
        public IEnumerator<TTo> GetEnumerator()
        {
            return _underlyingCollection.Select(x => _translateFromTo(x)).GetEnumerator();
        }

        [TargetedPatchingOptOut("")]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [TargetedPatchingOptOut("")]
        public void Add(TTo item)
        {
            _underlyingCollection.Add(_translateToFrom(item));
        }

        [TargetedPatchingOptOut("")]
        public void Clear()
        {
            _underlyingCollection.Clear();
        }

        [TargetedPatchingOptOut("")]
        public bool Contains(TTo item)
        {
            return _underlyingCollection.Contains(_translateToFrom(item));
        }

        [TargetedPatchingOptOut("")]
        public void CopyTo(TTo[] array, int arrayIndex)
        {
            var i = arrayIndex;
            foreach (var item in _underlyingCollection)
            {
                array[i++] = _translateFromTo(item);
            }
        }

        [TargetedPatchingOptOut("")]
        public bool Remove(TTo item)
        {
            return _underlyingCollection.Remove(_translateToFrom(item));
        }

        public int Count => _underlyingCollection.Count;

        public bool IsReadOnly => _underlyingCollection.IsReadOnly;
    }
}
