using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

using TIDZ;
using System.Linq;

public class MainScenePresenter : MonoBehaviour
{
    [SerializeField]
    private DummyDeck _deckView;
    
    private List<DummyHand> _handViews;

    private List<List<DummyRipener>> _ripenerVeiws;

    [SerializeField]
    private GameObject _handPrefab;

    [SerializeField]
    private GameObject _ripenerPrefab;

    private GameObject _commonRes;

    // ModelとViewの対応関係(GameObjectが削除されるときにDictionaryからも削除しないとリークする気がする）
    Dictionary<MeatCard, DummyCard> _vmCard = new Dictionary<MeatCard, DummyCard>();

    // Use this for initialization
    void Start () {

        int playerNum = 4;
        int NumberOfRipeners = 2;

        // View生成
        _commonRes = GameObject.Find("ComRes");

        _handViews = new List<DummyHand>(playerNum);
        for (int i = 0; i < playerNum; i++)
        {
            _handViews.Add(Instantiate(_handPrefab).GetComponent<DummyHand>());
            _handViews[i].transform.position = new Vector3(_handViews[i].transform.position.x + i * 10, _handViews[i].transform.position.y, _handViews[i].transform.position.z);
        }

        _ripenerVeiws = new List<List<DummyRipener>>();
        for (int i = 0; i < playerNum; i++)
        {
            var ripener = new List<DummyRipener>();
            for (int j = 0; j < NumberOfRipeners; j++)
            {
                ripener.Add(Instantiate(_ripenerPrefab).GetComponent<DummyRipener>());

                ripener[j].transform.position = new Vector3(ripener[j].transform.position.x + i * 15, ripener[j].transform.position.y + j * 17, ripener[j].transform.position.z);
            }
            _ripenerVeiws.Add(ripener);
        }
    }

    public IObservable<Unit> OpenCard(MeatCard card)
    {
        return _deckView.OpenCard(card.Type, card.Color, card.ID)
                        .Do(cardView => _vmCard[card] = cardView)
                        .AsUnitObservable();
    }

    public IObservable<Unit> AddHand(int playerIdx, MeatCard card)
    {
        return _handViews[playerIdx].AddHand(_vmCard[card]);
    }

    public IObservable<Unit> RemoveHand(int playerIdx, MeatCard card)
    {
        return _handViews[playerIdx].RemoveHand(_vmCard[card]);
    }

    public IObservable<Unit> MoveHandToRipener(Player player, int ripenerIndex, MeatCard card)
    {
        var index = player.Index;
        var r = _ripenerVeiws[index][ripenerIndex];

        return _handViews[index].RemoveHand(_vmCard[card]).SelectMany(_ => r.AddCard(_vmCard[card]));
    }

    public IObservable<Unit> AddCommonResource(MeatCard card)
    {
        var view = _vmCard[card];
        view.transform.position = _commonRes.transform.position;
        return Observable.Return(Unit.Default);
    }

    public IObservable<Tuple<MeatCard, Ripener>> Preparation(Player player)
    {
        // 手札カードのドラッグを有効にする
        // 最初に触ったカードをドラッグ状態にする
        // そのカードが、そのプレイヤーの熟成器の近くだったら、熟成器を強調する
        // 手を離してドラッグが終わったとき、有効な熟成器の近くだったら、その熟成器とカードの情報をonNextする
        // なにもなかったら、カードを最初の位置に戻して、nullをonNextする
        var playerIdx = player.Index;
        var ripenersModel = player.Ripeners;
        var ripenersView = _ripenerVeiws[playerIdx];

        bool validChoise = false;
        DummyCard cardChoise = null;

        Vector3 initialPosition = new Vector3(0, 0, 0);
        bool first = true;

        return Observable.Amb(_handViews[playerIdx].CurrentCard.Select(_ => _.OnDragAsObservabale))
            .Do(e =>
            {
                if(first)
                {
                    cardChoise = e.Source;
                    initialPosition.x = e.Source.transform.position.x;
                    initialPosition.y = e.Source.transform.position.y;
                    initialPosition.z = e.Source.transform.position.z;
                    first = false;
                }
            })
            .Do(e =>
            {
                var card = e.Source;
                var position = e.MousePosition;
                position.z = card.transform.position.z - Camera.main.transform.position.z;
                card.transform.position = Camera.main.ScreenToWorldPoint(position);
            })
            .Select(e =>
            {
                var card = e.Source;
                if (ripenersModel[0].Empty.Value && Vector3.Distance(ripenersView[0].transform.position, card.transform.position) < 50)
                {
                    validChoise = true;
                    return Tuple.Create(_vmCard.First(v => v.Value == card).Key, ripenersModel[0]);
                }
                else if (ripenersModel[1].Empty.Value && Vector3.Distance(ripenersView[1].transform.position, card.transform.position) < 50)
                {
                    validChoise = true;
                    return Tuple.Create(_vmCard.First(v => v.Value == card).Key, ripenersModel[1]);
                }
                else
                {
                    validChoise = false;
                    return Tuple.Create<MeatCard, Ripener>(_vmCard.First(v => v.Value == card).Key, null);
                }
            })
            .Finally(()=>
            {
                // 有効な選択でなかった場合はもとの位置に戻す
                if(!validChoise && cardChoise != null)
                {
                    cardChoise.transform.position = initialPosition;
                }
            });
    }
}
