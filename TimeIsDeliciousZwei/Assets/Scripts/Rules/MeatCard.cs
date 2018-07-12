using System;
using static TIDZ.MeatDef;

namespace TIDZ
{
    // 肉カードクラス
    public class MeatCard
    {
        public MeatType Type { get; private set; }
        public ColorElement Color { get; private set; }
        public Guid ID { get; private set; } = Guid.NewGuid();

        public MeatCard(MeatType type, ColorElement color)
        {
            Type = type;
            Color = color;
        }
    }
}

