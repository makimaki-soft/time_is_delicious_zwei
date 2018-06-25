using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using System.Linq;
using TIDZ;

public class PhaseManager : MonoBehaviour
{
    public enum Phase
    {
        GamePreparation,    // ゲームの準備中
        InitialPreparation, // 最初の仕込みフェイズ
        PlayerAction,       // アクションフェイズ
        Putrefy,            // 腐敗フェイズ
        CashOut,            // 売却フェイズ
        FoodPreparation,    // 仕込みフェイズ
        Aging,              // 日数経過フェイズ
        Ending,             // ゲーム終了
        Unknown = 99        // 不明なフェイズ
    }

    [SerializeField]
    private MainScenePresenter _presenter;

    void Start()
    {
        Observable.FromCoroutine(PhaseControl).Subscribe().AddTo(gameObject);
    }

    IEnumerator PhaseControl()
    {
        /* 定数 */
        int NumberOfPlayers = 4;            // プレイヤー数
        int CommonResourcesCapacity = 4;    // 共通リソースにおけるカードの枚数
        int InitialHand = 8;                // 初期手札枚数
        int NumberOfRipeners = 2;           // 一人あたりの熟成器の数

        /* コンポーネントModelの準備 */

        // 1. 山札
        var cards = new List<MeatCard>();
        foreach (MeatDef.MeatType type in Enum.GetValues(typeof(MeatDef.MeatType)))
        {
            foreach (MeatDef.ColorElement color in Enum.GetValues(typeof(MeatDef.ColorElement)))
            {
                for (int i = 0; i < 3; i++)
                {
                    cards.Add(new MeatCard(type, color));
                }
            }
        }

        var deckModel = new Deck<MeatCard>(cards);
        deckModel.Shuffle();

        // 2. プレイヤー
        var players = new List<Player>();
        for (int index = 0; index < NumberOfPlayers; index++)
        {
            players.Add(new Player());
        }

        // 3. 共通リソース
        var resource = new CommonResource(CommonResourcesCapacity);

        /* ゲームの準備 */

        // 各プレイヤーに８枚ずつ肉カードを配る
        for (int n = 0; n < InitialHand; n++)
        {
            foreach (var playerModel in players)
            {
                var cardModel = deckModel.Open();
                yield return _presenter.OpenCard(cardModel).ToYieldInstruction();
                playerModel.AddHand(cardModel);
                yield return _presenter.AddHand(playerModel.Index, cardModel).ToYieldInstruction();
            }
        }

        // 共通リソース置き場に肉を配置
        for (int n = 0; n < CommonResourcesCapacity; n++)
        {
            var cardModel = deckModel.Open();
            yield return _presenter.OpenCard(cardModel).ToYieldInstruction();
            resource.AddCard(cardModel);
            yield return _presenter.AddCommonResource(cardModel).ToYieldInstruction();
        }

        /* ゲーム開始 */
        // スタートプレイヤーから時計回りに、１枚ずつ手札から肉カードを熟成器に配置する。全員の熟成器が空でなくなるまで繰り返す
        while (players.SelectMany(player=>player.Ripeners).Select(r=>r.Empty.Value).Where(b=>b==true).Count() != 0 )
        {
            for (int index = 0; index < NumberOfPlayers; index++)
            {
                while (true)
                {
                    // TODO : PhaseManagerとPresenter間の情報のやり取りを整理する
                    //        ViewとModelの対応、引数にModelを渡すか、IDなどにするか？
                    var cardRipenerSelect = _presenter.Preparation(players[index]).ToYieldInstruction();
                    yield return cardRipenerSelect;
                    var selection = cardRipenerSelect.Result;
                    if (selection.Item2 != null)
                    {
                        selection.Item2.AddCard(selection.Item1);
                        yield return _presenter.MoveHandToRipener(players[index], players[index].Ripeners.IndexOf(selection.Item2), selection.Item1).ToYieldInstruction();
                        break;
                    }
                }
            }
        }

        Debug.Log("End PhaseControl");
    }


}
