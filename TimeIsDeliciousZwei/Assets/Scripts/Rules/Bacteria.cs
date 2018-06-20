namespace TIDZ
{
    // 菌トークンクラス
    public class Bacteria
    {
        public enum ColorElement
        {
            Red,
            Blue,
            Yellow,
            Green,
            Purple
        }

        public ColorElement Color { get; private set; }

        public Bacteria(ColorElement color)
        {
            Color = color;
        }
    }
}
