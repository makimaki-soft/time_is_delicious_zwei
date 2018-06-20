using System.Collections.Generic;

namespace TIDZ
{
    // 共通肉カード置き場
    public class Market
    {
        // 入手できるカード
        public IReadOnlyList<MeatCard> Availables { get; private set; }

        // 入手する
        public MeatCard GetCard(MeatCard card)
        {
            return card;
        }
    }
}

