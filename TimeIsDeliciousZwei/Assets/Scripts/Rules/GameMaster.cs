using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TIDZ
{
    public class GameMaster : ITimeIsDeliciousZwei
    {
        public int NumberOfPlayers { get; set; }
        
        // 一人当たりのアクション回数
        public int ActionCount
        {
            get { return 2; }
        }
        // ラウンドごとの腐敗トークンの追加個数
        public int RotTokenPerRound
        {
            get { return 5; }
        }
        public int InitialHand
        {
            get { return 8; }
        }
        public int NumberOfCommonResources
        {
            get { return 4; }
        }

        public int NumberOfRipeners
        {
            get { return 2; }
        }

        private Deck<MeatCard> _playingDeck;
        public Deck<MeatCard> PlayingDeck
        {
            get { return _playingDeck; }
        }

        private List<Player> _players;
        public List<Player> Players
        {
            get { return _players; }
        }

        private CommonResource _commonRes;
        public CommonResource CommonResources
        {
            get { return _commonRes; }
        }

        public void Initialize()
        {
            var cards = new List<MeatCard>();
            foreach (MeatDef.MeatType type in Enum.GetValues(typeof(MeatDef.MeatType)))
            {
                foreach (MeatDef.ColorElement color in Enum.GetValues(typeof(MeatDef.ColorElement)))
                {
                    for(int i=0; i<3; i++)
                    {
                        cards.Add(new MeatCard(type, color));
                    }
                }
            }

            _playingDeck = new Deck<MeatCard>(cards);
            _playingDeck.Shuffle();

            _players = new List<Player>();
            for(int index=0; index<NumberOfPlayers; index++)
            {
                _players.Add(new Player());
            }

            _commonRes = new CommonResource(NumberOfCommonResources);
        }

        public void Prepare()
        {
            // 各プレイヤーに８枚ずつ肉カードを配る
            for (int n = 0; n < InitialHand; n++)
            {
                foreach (var player in _players)
                {
                    player.AddHand(_playingDeck.Open());
                }
            }

            // 共通リソース置き場に肉を配置
            for (int n = 0; n < NumberOfCommonResources; n++)
            {
                _commonRes.AddCard(_playingDeck.Open());
            }
        }
    }
}


