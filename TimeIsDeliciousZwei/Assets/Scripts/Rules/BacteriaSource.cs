using System;
using System.Collections.Generic;
using static TIDZ.MeatDef;

namespace TIDZ
{
    public class BacteriaSource
    {
        private List<Bacteria> _source;

        public BacteriaSource()
        {
            _source = new List<Bacteria>();

            foreach (ColorElement color in Enum.GetValues(typeof(ColorElement)))
            {
                for(int i=0; i<12; i++)
                {
                    if(i<4)
                    {
                        _source.Add(new Bacteria(color, true));
                    }
                    else
                    {
                        _source.Add(new Bacteria(color, false));
                    }
                }
            }
            Shuffle();
        }

        private void Shuffle()
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

        // 菌トークンを取り出す
        public Bacteria Draw()
        {
            var ret = _source[0];
            _source.Remove(ret);
            return ret;
        }

        // 色を指定して菌トークンを取り出す
        public Bacteria DrawByColor(ColorElement color)
        {
            var ret = _source.Find(b => b.Color == color && b.IsStrong == false);
            _source.Remove(ret);
            return ret;
        }

        // 菌トークンを戻す
        public void Return(Bacteria bacteria)
        {
            _source.Add(bacteria);
            Shuffle();
        }
    }
}


