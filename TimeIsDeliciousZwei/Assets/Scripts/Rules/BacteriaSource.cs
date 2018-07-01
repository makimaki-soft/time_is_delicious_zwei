using static TIDZ.MeatDef;

namespace TIDZ
{
    public class BacteriaSource
    {
        // 菌トークンを取り出す
        public Bacteria Draw()
        {
            return new Bacteria(ColorElement.Green);
        }

        // 色を指定して菌トークンを取り出す
        public Bacteria DrawByColor(ColorElement color)
        {
            return new Bacteria(color);
        }

        // 菌トークンを戻す
        public void Return(Bacteria bacteria)
        {

        }
    }
}


