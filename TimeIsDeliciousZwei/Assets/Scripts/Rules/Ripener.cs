using System;
using System.Collections.Generic;
using UniRx;
using static TIDZ.MeatDef;

namespace TIDZ
{
    // 熟成器クラス
    public class Ripener
    {
        public Guid ID { get; private set; } = Guid.NewGuid();

        // カード追加チェック
        public bool CanAdd(MeatCard card)
        {
            if(_agingMeats.Count == 0)
            {
                return true;
            }
            else if(_agingMeats[0].Type == card.Type)
            {
                return true;
            }

            return false;
        }

        public ReactiveCollection<MeatCard> _agingMeats = new ReactiveCollection<MeatCard>();

        // カードを追加する
        public void AddCard(MeatCard card)
        {
            if(CanAdd(card))
            {
                _agingMeats.Add(card);
            }
        }

        public IReactiveProperty<bool> Empty
        {
            get
            {
                return _agingMeats.ObserveCountChanged(true).Select(cnt => cnt == 0).ToReactiveProperty(true);
            }
        }

        // 熟成度(カード枚数)
        public int Maturity
        {
            get
            {
                return _agingMeats.Count;
            }
        }

        // 熟成中の肉の種類
        public MeatType AgingType
        {
            get
            {
                if(Empty.Value)
                {
                    return default(MeatType);
                }
                else
                {
                    return _agingMeats[0].Type;
                }
            }
        }

        // 熟成器の属性リスト
        public HashSet<ColorElement> Colors
        {
            get
            {
                var ret = new HashSet<ColorElement>();
                foreach(var col in _agingMeats )
                {
                    ret.Add(col.Color);
                }
                return ret;
            }
        }

        // 熟成期間
        private ReactiveProperty<int> _agingPeriod = new ReactiveProperty<int>(0);
        public IReactiveProperty<int> AgingPeriod
        {
            get { return _agingPeriod; }
        }
        public void AddDay()
        {
            _agingPeriod.Value++;
        }

        public void Reset()
        {
            _agingPeriod.Value = 0;
            _agingMeats.Clear();
        }
    }
}

