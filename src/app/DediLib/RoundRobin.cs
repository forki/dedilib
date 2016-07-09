using System;

namespace DediLib
{
    public class RoundRobin<T> where T : new()
    {
        private readonly T[] _items;
        private int _currentRound;

        private readonly Action<T> _onSelected;
        public Action<T> OnSelected => _onSelected;

        private readonly int _rounds;
        public int Rounds => _rounds;

        public T Current => _items[_currentRound];

        public T Last
        {
            get
            {
                var lastRound = _currentRound - 1;
                if (lastRound < 0) lastRound = _rounds - 1;
                return _items[lastRound];
            }
        }

        public RoundRobin(int rounds = 2, Action<T> onSelected = null)
        {
            if (rounds < 2) throw new ArgumentOutOfRangeException(nameof(rounds), "rounds must be at least 2");

            _onSelected = onSelected ?? (t => { });

            _items = new T[rounds];
            for (var i = 0; i < rounds; i++) _items[i] = new T();
            _rounds = rounds;
        }

        private readonly object _cycleLock = new object();
        public void Cycle()
        {
            lock (_cycleLock)
            {
                var currentRound = _currentRound;
                var nextRound = (currentRound + 1) % _rounds;
                _onSelected(_items[nextRound]);
                _currentRound = nextRound;
            }
        }

        public void Cycle(Action<T> onSelected)
        {
            if (onSelected == null)
            {
                Cycle();
                return;
            }
            lock (_cycleLock)
            {
                var currentRound = _currentRound;
                var nextRound = (currentRound + 1) % _rounds;
                onSelected(_items[nextRound]);
                _currentRound = nextRound;
            }
        }
    }
}