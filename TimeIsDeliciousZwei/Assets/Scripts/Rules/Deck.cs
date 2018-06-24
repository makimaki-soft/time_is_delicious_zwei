using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;

namespace TIDZ
{
    public class Deck<T>
    {
        private ReactiveCollection<T> _source;
        public IObservable<CollectionRemoveEvent<T>> ObserveOpen
        {
            get { return _source.ObserveRemove(); }
        }

        public Deck(IList<T> source)
        {
            _source = new ReactiveCollection<T>(source);
        }

        public void Shuffle()
        {
            var rng = new Random();
            int n = _source.Count;

            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var tmp = _source[k];
                _source[k] = _source[n];
                _source[n] = tmp;
            }
        }

        public T Open()
        {
            if( _source.Count < 1 )
            {
                return default(T);
            }
            var top = _source[0];
            _source.Remove(top);
            return top;
        }
    }
}
