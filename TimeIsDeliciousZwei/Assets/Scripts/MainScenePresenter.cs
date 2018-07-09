using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TIDZ;
using UniRx;
using UnityEngine;
using static TIDZ.MeatDef;

public partial class MainScenePresenter : MonoBehaviour
{
    private List<HandView> _handViews;

    private List<List<RipenerView>> _ripenerVeiws;

    [SerializeField]
    private GameObject _handPrefab;

    [SerializeField]
    private GameObject _ripenerPrefab;

    private CommonResourceView _commonRes;

    List<BacteriaPlaceView> _bacterias;

    CardView _nextCard;

    [SerializeField]
    private MainDeck mainDeck;

    // Model //
    private Deck<MeatCard> deckModel;
    private List<Player> playerModels;
    private CommonResource resourceModel;
    private BacteriaSource bacteriaSrc;
    private List<BacteriaPlace> bacteriaPlaces;

    private Dictionary<CardView, MeatCard> cardVM = new Dictionary<CardView, MeatCard>();

    /* 定数 */
    int NumberOfPlayers = 4;            // プレイヤー数
    int CommonResourcesCapacity = 4;    // 共通リソースにおけるカードの枚数
    int InitialHand = 8;                // 初期手札枚数
    int NumberOfRipeners = 2;           // 一人あたりの熟成器の数
    int ActionCount = 2;                // １ラウンドあたりのプレイヤーのアクション数
    int NumberOfBacterias = 5;

    // Use this for initialization
    void Start()
    {
        // View 初期化
        _handViews = new List<HandView>(NumberOfPlayers);
        for (int i = 0; i < NumberOfPlayers; i++)
        {
            _handViews.Add(Instantiate(_handPrefab).GetComponent<HandView>());
            _handViews[i].transform.position = new Vector3(_handViews[i].transform.position.x + i * 15, _handViews[i].transform.position.y, _handViews[i].transform.position.z);
        }

        _commonRes = GameObject.Find("ComRes").GetComponent<CommonResourceView>();

        // Model 初期化
        deckModel = CreateDeck();
        bacteriaSrc = new BacteriaSource();
        playerModels = new List<Player>();
        for (int index = 0; index < NumberOfPlayers; index++)
        {
            playerModels.Add(new Player());
        }
        resourceModel = new CommonResource(CommonResourcesCapacity);
        bacteriaPlaces = new List<BacteriaPlace>();
        foreach (ColorElement color in Enum.GetValues(typeof(ColorElement)))
        {
            var place = new BacteriaPlace(color);
            bacteriaPlaces.Add(place);
        }

        bacteriaPlaces[0].LeftSide = bacteriaPlaces[4];
        for(int i=0; i< bacteriaPlaces.Count; i++)
        {
            bacteriaPlaces[i].RightSide = bacteriaPlaces[i% bacteriaPlaces.Count];
            bacteriaPlaces[i % bacteriaPlaces.Count].LeftSide = bacteriaPlaces[i].RightSide;
        }

        _ripenerVeiws = new List<List<RipenerView>>();
        for (int i = 0; i < NumberOfPlayers; i++)
        {
            var ripener = new List<RipenerView>();
            for (int j = 0; j < NumberOfRipeners; j++)
            {
                var view = Instantiate(_ripenerPrefab).GetComponent<RipenerView>();
                view.ModelID = playerModels[i].Ripeners[j].ID;
                ripener.Add(view);
                ripener[j].transform.position = new Vector3(ripener[j].transform.position.x + i * 15, ripener[j].transform.position.y + j * 17, ripener[j].transform.position.z);
            }
            _ripenerVeiws.Add(ripener);
        }

        _bacterias = new List<BacteriaPlaceView>();
        _bacterias.Add(GameObject.Find("Bac_Red").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("Bac_Blue").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("Bac_Green").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("Bac_Yellow").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("Bac_Purple").GetComponent<BacteriaPlaceView>());

        Observable.FromCoroutine(PhaseControlMain).Subscribe().AddTo(gameObject);
    }

    Deck<MeatCard> CreateDeck()
    {
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

        return new Deck<MeatCard>(cards);
    }

    IEnumerator PhaseControlMain()
    {
        // 山札シャッフル
        deckModel.Shuffle();

        foreach(var player in playerModels)
        {
            player.Hand.ObserveCountChanged().Subscribe(count => _handViews[player.Index].CardCount = count).AddTo(_handViews[player.Index]);
        }
        resourceModel.Cards.ObserveCountChanged().Subscribe(count=> _commonRes.CardCount = count).AddTo(_commonRes);


        // 各プレイヤーに８枚ずつ肉カードを配る
        for (int n = 0; n < InitialHand; n++)
        {
            foreach (var playerModel in playerModels)
            {
                var cardModel = deckModel.Open();
                var cardView = mainDeck.CreateCard(cardModel.Type, cardModel.Color, cardModel.ID);
                cardVM[cardView] = cardModel;

                yield return mainDeck.OpenAnimation(cardView).ToYieldInstruction();

                playerModel.AddHand(cardModel);

                if (n == InitialHand - 1 && playerModel.Index == NumberOfPlayers - 1)
                {
                    yield return _handViews[playerModel.Index].AddHandAnimation(cardView).ToYieldInstruction();
                }
                else
                {
                    _handViews[playerModel.Index].AddHandAnimation(cardView).Subscribe();
                }
            }
        }

        // 共通リソース置き場に肉を配置
        for (int n = 0; n < CommonResourcesCapacity; n++)
        {
            var cardModel = deckModel.Open();
            var cardView = mainDeck.CreateCard(cardModel.Type, cardModel.Color, cardModel.ID);
            cardVM[cardView] = cardModel;

            yield return mainDeck.OpenAnimation(cardView).ToYieldInstruction();

            resourceModel.AddCard(cardModel);
            yield return _commonRes.AddResourceAnimation(cardView).ToYieldInstruction();
        }

        // 初期熟成
        while (playerModels.SelectMany(player => player.Ripeners).Select(r => r.Empty.Value).Where(b => b == true).Count() != 0)
        {
            for (int index = 0; index < NumberOfPlayers; index++)
            {
                while (true)
                {
                    var playerModel = playerModels[index];
                    var handView = _handViews[index];
                    var cardViews = playerModel.Hand.Select(model => handView.GetCardView(model.ID));

                    // カード選択
                    var cardSelection = Observable.Amb(cardViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return cardSelection;

                    if(!cardSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedCard = cardSelection.Result as CardView;
                    
                    yield return selectedCard.OnTouchAnimation().ToYieldInstruction();

                    // 熟成器選択
                    var ripenersModel = playerModel.Ripeners;
                    var ripenersViews = ripenersModel.Where(r => r.Empty.Value).Select(model => _ripenerVeiws[index].FirstOrDefault(v => v.ModelID == model.ID));

                    var ripenerSelection = Observable.Amb(ripenersViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return ripenerSelection;

                    if (!ripenerSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedRipener = ripenerSelection.Result as RipenerView;

                    var selectedRipenerModel = ripenersModel.FirstOrDefault(m => m.ID == selectedRipener.ModelID);
                    var selectedCardModel = cardVM[selectedCard];
                    playerModel.RemoveHand(selectedCardModel);
                    handView.RemoveHand(selectedCard);
                    selectedRipenerModel.AddCard(selectedCardModel);
                    yield return selectedRipener.AddCardAnimation(selectedCard).ToYieldInstruction();

                    break;
                }
            }
        }

        foreach(var ripener in playerModels.SelectMany(player => player.Ripeners))
        {
            ripener.AddDay();
        }

        var cardTop = deckModel.Open();
        cardVM[mainDeck.CreateCard(cardTop.Type, cardTop.Color, cardTop.ID)] = cardTop;
        yield return null; // オープンしたカードのViewが有効になるのを待つために1フレームずらす

        for (int round=0; round < int.MaxValue; round++)
        {
            Debug.Log("Round : " + (round+1));

            int startPlayer = round % NumberOfPlayers;

            for(int i=0; i<NumberOfPlayers; i++)
            {
                int index = (round + i) % NumberOfPlayers;
                Debug.Log("プレイヤー" + index);
                int action = 0;
                while (true)
                {
                    Debug.Log("アクション : " + (action+1));
                    
                    // 手札、共通リソース、山札の選択
                    var playerModel = playerModels[index];
                    var handView = _handViews[index];
                    var handCardViews = playerModel.Hand.Select(model => handView.GetCardView(model.ID));
                    var resCardViews = resourceModel.Availables.Select(c => cardVM.First(_ => _.Key.ModelID == c.ID).Key);
                    var cardTopView = cardVM.Where(c => c.Key.ModelID == cardTop.ID).Select(kv => kv.Key);
                    var cardViews = handCardViews.Concat(resCardViews).Concat(cardTopView);

                    Debug.Log(cardViews.Count());

                    // カード選択
                    var cardSelection = Observable.Amb(cardViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return cardSelection;

                    if (!cardSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedCardView = cardSelection.Result as CardView;
                    var selectedCardModel = cardVM[selectedCardView];
                    if (handCardViews.Contains(selectedCardView))
                    {
                        Debug.Log("手札がクリックされた");

                        var touchables = _bacterias.Select(b => b.OnTouchAsObservabale);
                        var ripenersViews = playerModel.Ripeners.Where(r => r.CanAdd(selectedCardModel)).Select(model => _ripenerVeiws[index].FirstOrDefault(v => v.ModelID == model.ID));
                        touchables = touchables.Concat(ripenersViews.Select(r => r.OnTouchAsObservabale));

                        var selection = Observable.Amb(touchables).First().ToYieldInstruction();
                        yield return selection;

                        if (!selection.HasResult)
                        {
                            continue;
                        }

                        var touched = selection.Result;
                        if (touched is RipenerView)
                        {
                            Debug.Log("熟成器に追加");
                            playerModel.RemoveHand(selectedCardModel);
                            playerModel.Ripeners.Find(r => r.ID == (touched as RipenerView).ModelID).AddCard(selectedCardModel);
                            handView.RemoveHand(selectedCardView);
                            yield return (touched as RipenerView).AddCardAnimation(selectedCardView).ToYieldInstruction();
                        }
                        else if (touched is BacteriaPlaceView)
                        {
                            Debug.Log("菌トークンを除去");
                            playerModel.RemoveHand(selectedCardModel);
                            handView.RemoveHand(selectedCardView);
                            Destroy(selectedCardView.gameObject);
                        }
                    }
                    else if (resCardViews.Contains(selectedCardView))
                    {
                        Debug.Log("共通リソースがクリックされた");
                        playerModel.AddHand(resourceModel.GetCard(selectedCardModel));
                        yield return _handViews[playerModel.Index].AddHandAnimation(selectedCardView).ToYieldInstruction();

                        var cardModel = deckModel.Open();
                        var cardView = mainDeck.CreateCard(cardModel.Type, cardModel.Color, cardModel.ID);
                        cardVM[cardView] = cardModel;

                        yield return mainDeck.OpenAnimation(cardView).ToYieldInstruction();

                        resourceModel.AddCard(cardModel);
                        yield return _commonRes.AddResourceAnimation(cardView).ToYieldInstruction();
                    }
                    else if (cardTopView.Contains(selectedCardView))
                    {
                        Debug.Log("山札がクリックされた");
                        playerModel.AddHand(selectedCardModel);
                        yield return _handViews[playerModel.Index].AddHandAnimation(selectedCardView).ToYieldInstruction();

                        cardTop = deckModel.Open();
                        cardVM[mainDeck.CreateCard(cardTop.Type, cardTop.Color, cardTop.ID)] = cardTop;
                        yield return null; // オープンしたカードのViewが有効になるのを待つために1フレームずらす
                    }
                    else
                    {
                        Debug.Log("クリックされないはず");
                    }

                    action++;
                    if(action>=2)
                    {
                        break;
                    }
                }
            }

            foreach(var place in bacteriaPlaces)
            {
                place.OutBreakInCurrRound = false;
            }

            // 腐敗フェイズ
            for (int i=0; i< NumberOfBacterias; i++)
            {
                var token = bacteriaSrc.Draw();
                Debug.Log("Token : " + token.Color);

                // トークンが出てくるアニメーションと待ち合わせ

                var tagetPlace = bacteriaPlaces.Find(bp => bp.Color == token.Color);

                if(!tagetPlace.OutBreakInCurrRound)
                {
                    tagetPlace.AddToken(token);

                    // トークンを配置するアニメーションと待ち合わせ

                    if (tagetPlace.OutBreak())
                    {
                        // アウトブレイクするアニメーションと待ち合わせ

                        foreach (var rp in playerModels.SelectMany(pm => pm.Ripeners))
                        {
                            if (rp.Colors.Contains(tagetPlace.Color))
                            {
                                // この熟成器はバースト
                                rp.Reset();
                            }
                        }

                        tagetPlace.OutBreakInCurrRound = true;
                        yield return Observable.FromCoroutine(_ => RecurveOutBreak(tagetPlace)).ToYieldInstruction();
                    }
                }
            }


            // 売却フェイズ
            for (int i = 0; i < NumberOfPlayers; i++)
            {
                int index = (round + i) % NumberOfPlayers;
                while (true)
                {
                    var playerModel = playerModels[index];
                  
                    // 熟成器選択
                    var ripenersModel = playerModel.Ripeners;

                    // このプレイヤーの熟成器に何も入っていなかったら飛ばす
                    if( ripenersModel.Where(r => !r.Empty.Value).Count() == 0 )
                    {
                        break;
                    }

                    var ripenersViews = ripenersModel.Where(r => r.Empty.Value).Select(model => _ripenerVeiws[index].FirstOrDefault(v => v.ModelID == model.ID));

                    // Todo : パスボタンを追加する
                    var ripenerSelection = Observable.Amb(ripenersViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return ripenerSelection;

                    if (!ripenerSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedRipener = ripenerSelection.Result as RipenerView;

                    var selectedRipenerModel = ripenersModel.FirstOrDefault(m => m.ID == selectedRipener.ModelID);

                    var cashout = selectedRipenerModel.AgingPeriod.Value * selectedRipenerModel.Maturity;
                    playerModel.Point += cashout;
                    selectedRipenerModel.Reset();
                }
            }

            // 仕込みフェイズ
            for (int index = 0; index < NumberOfPlayers; index++)
            {
                while (true)
                {
                    var playerModel = playerModels[index];
                    var handView = _handViews[index];
                    var cardViews = playerModel.Hand.Select(model => handView.GetCardView(model.ID));

                    // カード選択
                    var cardSelection = Observable.Amb(cardViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return cardSelection;

                    if (!cardSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedCard = cardSelection.Result as CardView;

                    yield return selectedCard.OnTouchAnimation().ToYieldInstruction();

                    // 熟成器選択
                    var ripenersModel = playerModel.Ripeners;
                    var ripenersViews = ripenersModel.Where(r => r.Empty.Value).Select(model => _ripenerVeiws[index].FirstOrDefault(v => v.ModelID == model.ID));

                    var ripenerSelection = Observable.Amb(ripenersViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return ripenerSelection;

                    if (!ripenerSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedRipener = ripenerSelection.Result as RipenerView;

                    var selectedRipenerModel = ripenersModel.FirstOrDefault(m => m.ID == selectedRipener.ModelID);
                    var selectedCardModel = cardVM[selectedCard];
                    playerModel.RemoveHand(selectedCardModel);
                    handView.RemoveHand(selectedCard);
                    selectedRipenerModel.AddCard(selectedCardModel);
                    yield return selectedRipener.AddCardAnimation(selectedCard).ToYieldInstruction();

                    break;
                }
            }

            // 熟成フェイズ
            foreach (var ripener in playerModels.SelectMany(player => player.Ripeners))
            {
                if(!ripener.Empty.Value)
                {
                    ripener.AddDay();
                }
            }
        }
    }

    IEnumerator RecurveOutBreak(BacteriaPlace place)
    {
        var left = place.LeftSide;
        var right = place.RightSide;

        yield return null;

        if(!left.OutBreakInCurrRound)
        {
            for (int i = 0; i < 2; i++)
            {
                left.AddToken(bacteriaSrc.DrawByColor(left.Color));
                if (left.OutBreak())
                {
                    left.OutBreakInCurrRound = true;
                    yield return Observable.FromCoroutine(_ => RecurveOutBreak(left)).ToYieldInstruction();
                }
            }
        }

        if (!right.OutBreakInCurrRound)
        {
            for (int i = 0; i < 2; i++)
            {
                right.AddToken(bacteriaSrc.DrawByColor(right.Color));
                if (right.OutBreak())
                {
                    right.OutBreakInCurrRound = true;
                    yield return Observable.FromCoroutine(_ => RecurveOutBreak(right)).ToYieldInstruction();
                }
            }
        }       
    }
}


