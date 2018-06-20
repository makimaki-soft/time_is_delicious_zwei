namespace TIDZ
{
    // 肉カードクラス
    public class MeatCard
    {
        public enum MeatType
        {
            Sirloin,        // サーロイン
            ShoulderRoast,  // 肩ロース
            RibRoast,       // リブロース
            Katabara,       // 肩バラ
            Nakabara,       // ナカバラ
            Harami,         // ハラミ
            Shintama,       // シンタマ
            Momo,           // モモ
            Sotomomo,       // ソトモモ
            Lump,           // ランプ
        }

        public enum ColorElement
        {
            Red,
            Blue,
            Yellow,
            Green,
            Purple
        }

        public MeatType Type { get; private set; }
        public ColorElement Color { get; private set; }

        public MeatCard(MeatType type, ColorElement color)
        {
            Type = type;
            Color = color;
        }
    }
}

