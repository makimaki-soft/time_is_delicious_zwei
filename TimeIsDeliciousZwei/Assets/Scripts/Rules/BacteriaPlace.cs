using System.Collections.Generic;
using UnityEngine;
using static TIDZ.MeatDef;

namespace TIDZ
{
    public class BacteriaPlace
    {
        // 属性色
        public ColorElement Color { get; private set; }

        // 左側隣接菌トークン置き場
        public BacteriaPlace LeftSide { get; set; }

        // 右側隣接菌トークン置き場
        public BacteriaPlace RightSide { get; set; }

        // 菌トークン数
        public int NumberOfBacterias { get; private set; }

        private List<Bacteria> bacterias = new List<Bacteria>();

        public BacteriaPlace(ColorElement color)
        {
            Color = color;
        }

        public void AddToken(Bacteria token)
        {
            bacterias.Add(token);
        }

        // そのカードで除去できる個数
        public int Removable(MeatCard card)
        {
            if(card.Color == Color)
            {
                return Mathf.Max(2, bacterias.Count);
            }
            else
            {
                return Mathf.Max(1, bacterias.Count);
            }
        }

        // 除去する
        public int Remove(MeatCard card)
        {
            var num = Removable(card);
            bacterias.RemoveRange(0, num);
            return num;
        }

        // アウトブレイクしたかどうか
        public bool OutBreak()
        {
            return bacterias.Count > 5;
        }

        public bool OutBreakInCurrRound { get; set; } = false;
        
    }
}


