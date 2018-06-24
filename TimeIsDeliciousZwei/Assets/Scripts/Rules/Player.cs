using System.Collections.Generic;
using UniRx;

namespace TIDZ
{
    public class Player
    {
        public static int i = 0;

        // Index
        public int Index;

        // 熟成器
        public List<Ripener> Ripeners { get; private set; }

        // 手札
        public ReactiveCollection<MeatCard> Hand { get; private set; }

        // 得点
        public int Point { get; private set; }

        public Player()
        {
            Ripeners = new List<Ripener>();
            
            Ripeners.Add(new Ripener());
            Ripeners.Add(new Ripener());

            Hand = new ReactiveCollection<MeatCard>();

            Point = 0;
            Index = Player.i++;
        }

        public void AddHand(MeatCard card)
        {
            Hand.Add(card);
        }

        
    }
}
