using System.Collections.Generic;

namespace TIDZ
{
    // 熟成器クラス
    public class Ripener
    {
        // カード追加チェック
        public bool CanAdd(MeatCard card)
        {
            return false;
        }

        // カードを追加する
        public void AddCard(MeatCard card)
        {

        }

        // 熟成度(カード枚数)
        public int Maturity { get; private set; }

        // 熟成中の肉の種類
        public MeatCard.MeatType AgingType { get; private set; }

        // 熟成器の属性リスト
        public HashSet<MeatCard.ColorElement> Colors { get; private set; }

        // 熟成期間
        public int AgingPeriod { get; private set; }
    }
}

