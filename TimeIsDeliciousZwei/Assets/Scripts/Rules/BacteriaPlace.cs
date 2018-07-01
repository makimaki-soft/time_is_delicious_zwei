using static TIDZ.MeatDef;

namespace TIDZ
{
    public class BacteriaPlace
    {
        // 属性色
        public ColorElement Color { get; private set; }

        // 左側隣接菌トークン置き場
        public BacteriaPlace LeftSide { get; private set; }

        // 右側隣接菌トークン置き場
        public BacteriaPlace RightSide { get; private set; }

        // 菌トークン数
        public int NumberOfBacterias { get; private set; }

        // そのカードで除去できる個数
        public int Removable(MeatCard card)
        {
            return 0;
        }

        // 除去する
        public int Remove(MeatCard card)
        {
            return 0;
        }

        // アウトブレイクしたかどうか
        public bool OutBreak()
        {
            return false;
        }
    }
}


