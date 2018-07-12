using static TIDZ.MeatDef;

namespace TIDZ
{
    // 菌トークンクラス
    public class Bacteria
    {
        public ColorElement Color { get; private set; }

        public Bacteria(ColorElement color)
        {
            Color = color;
        }
    }
}
