
using System.Collections.Generic;
using System.Linq;
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
                return Mathf.Min(2, bacterias.Where(b=>b.IsStrong==false).Count());
            }
            else
            {
                return Mathf.Max(1, bacterias.Where(b => b.IsStrong == false).Count());
            }
        }

        // 菌が除去できるチェック
        public bool CanRemove(MeatCard card)
        {
            return Removable(card) > 0;
        }


        // 除去する
        public void Remove(MeatCard card)
        {
            /*
            var num = Removable(card);
            bacterias.RemoveRange(0, num);
            return num;
            */
            if (CanRemove(card))
            {
                foreach(var rem in bacterias.Where(b => b.IsStrong == false).Take(Removable(card)))
                {
                    bacterias.Remove(rem);
                }
            }
        }

        // アウトブレイクしたかどうか
        public bool OutBreak()
        {
            return bacterias.Count > 4;
        }

        public bool OutBreakInCurrRound { get; set; } = false;
        
    }
}


