using System.Collections.Generic;
using UniRx;

namespace TIDZ
{
    // 共通肉カード置き場
    public class CommonResource
    {
        private List<MeatCard> _internal;
        private ReactiveCollection<MeatCard> _source;
        public ReactiveCollection<MeatCard> Cards
        {
            get { return _source; }
        }

        // 入手できるカード
        public IReadOnlyList<MeatCard> Availables
        {
            get { return _internal.AsReadOnly(); }
        }

        public CommonResource(int numResources)
        {
            _internal = new List<MeatCard>(numResources);
            _source = new ReactiveCollection<MeatCard>(_internal);
        }

        public void AddCard(MeatCard card)
        {
            if(_internal.Count < _internal.Capacity)
            {
                _source.Add(card);
            }
        }

        // 入手する
        public MeatCard GetCard(MeatCard card)
        {
            if(_source.IndexOf(card) != -1)
            {
                _source.Remove(card);
                return card;
            }

            return null;
        }
    }
}

