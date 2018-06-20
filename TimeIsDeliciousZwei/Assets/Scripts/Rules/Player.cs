using System.Collections.Generic;

namespace TIDZ
{
    public class Player
    {
        // Index
        public int Index;

        // 熟成器
        public List<Ripener> Ripeners { get; private set; }

        // 手札
        public List<MeatCard> Hand { get; private set; }

        // 得点
        public int Point { get; private set; }
    }
}
