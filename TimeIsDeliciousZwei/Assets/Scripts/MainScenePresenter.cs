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

    CardControl _nextCard;

    [SerializeField]
    private MainDeck mainDeck;

    // Model //
    private Deck<MeatCard> deckModel;
    private List<Player> playerModels;
    private CommonResource resourceModel;
    private BacteriaSource bacteriaSrc;
    private List<BacteriaPlace> bacteriaPlaces;

    private Dictionary<CardControl, MeatCard> cardVM = new Dictionary<CardControl, MeatCard>();

    /* 定数 */
    int NumberOfPlayers = 1;            // プレイヤー数
    int CommonResourcesCapacity = 4;    // 共通リソースにおけるカードの枚数
    int InitialHand = 8;                // 初期手札枚数
    int NumberOfRipeners = 2;           // 一人あたりの熟成器の数
    int ActionCount = 2;                // １ラウンドあたりのプレイヤーのアクション数
    int NumberOfBacterias = 5;


    // 選択されている肉
    public MeatCard curentSelectedMeat { get; private set; }

    // Use this for initialization
    void Start()
    {
        // View 初期化
        _handViews = new List<HandView>(NumberOfPlayers);
        for (int i = 0; i < NumberOfPlayers; i++)
        {
            _handViews.Add(Instantiate(_handPrefab).GetComponent<HandView>());
            _handViews[i].transform.position = new Vector3(-22, (i+1) * -21, 0);
        }

        _commonRes = GameObject.Find("ComRes").GetComponent<CommonResourceView>();

        _bacterias = new List<BacteriaPlaceView>();
        _bacterias.Add(GameObject.Find("bacteria_place_red").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("bacteria_place_blue").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("bacteria_place_yellow").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("bacteria_place_green").GetComponent<BacteriaPlaceView>());
        _bacterias.Add(GameObject.Find("bacteria_place_purple").GetComponent<BacteriaPlaceView>());

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
        for (int i = 0; i < bacteriaPlaces.Count; i++)
        {
            bacteriaPlaces[i].RightSide = bacteriaPlaces[i % bacteriaPlaces.Count];
            bacteriaPlaces[i % bacteriaPlaces.Count].LeftSide = bacteriaPlaces[i].RightSide;
            _bacterias[i].bacteriaPlace = bacteriaPlaces[i];
        }

        _ripenerVeiws = new List<List<RipenerView>>();
        for (int i = 0; i < NumberOfPlayers; i++)
        {
            var ripener = new List<RipenerView>();
            for (int j = 0; j < NumberOfRipeners; j++)
            {
                var view = Instantiate(_ripenerPrefab).GetComponent<RipenerView>();
                view.ModelID = playerModels[i].Ripeners[j].ID;
                view.ripener = playerModels[i].Ripeners[j];
                ripener.Add(view);
                ripener[j].transform.position = new Vector3((i+1) * 32.5f, 7 - (j * 20), 0);
            }
            _ripenerVeiws.Add(ripener);
        }



        Observable.FromCoroutine(PhaseControlMain).Subscribe().AddTo(gameObject);
    }

    void onGamePreparation()
    {
        /*
        _card.OnClickAsObservabale.SelectMany(_ => _card.FlipAnimation())
            .Subscribe(_ =>
            {
                Debug.Log("onPreparation End");
                _phaseManager.FinishPhase(PhaseManager.Phase.GamePreparation);
            });
        */

        
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
                cardView.transform.parent = _handViews[playerModel.Index].transform;
                cardVM[cardView] = cardModel;
                cardView.isHand = true;

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
        GameObject ComRes =  GameObject.Find("ComRes");
        for (int n = 0; n < CommonResourcesCapacity; n++)
        {
            var cardModel = deckModel.Open();
            var cardView = mainDeck.CreateCard(cardModel.Type, cardModel.Color, cardModel.ID);
            cardVM[cardView] = cardModel;
            cardView.transform.parent = ComRes.transform;
            cardView.commonResourceIndex = n+1;

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

                    var selectedCard = cardSelection.Result as CardControl;
                    var selectedCardModel = cardVM[selectedCard];

                    // みんなに選択された肉の情報を伝える
                    curentSelectedMeat = selectedCardModel; 

                    yield return selectedCard.SelectedAnimation().ToYieldInstruction();

                    // 熟成器選択
                    var ripenersModel = playerModel.Ripeners;
                    var ripenersViews = ripenersModel.Where(r => r.Empty.Value).Select(model => _ripenerVeiws[index].FirstOrDefault(v => v.ModelID == model.ID));

                    var ripenerSelection = Observable.Amb(ripenersViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return ripenerSelection;

                    if (!ripenerSelection.HasResult)
                    {
                        continue;
                    }

                    // みんなに肉の選択が終わったことを伝える
                    curentSelectedMeat = null;

                    var selectedRipener = ripenerSelection.Result as RipenerView;

                    var selectedRipenerModel = ripenersModel.FirstOrDefault(m => m.ID == selectedRipener.ModelID);
                    
                    playerModel.RemoveHand(selectedCardModel);
                    handView.RemoveHand(selectedCard);
                    selectedRipenerModel.AddCard(selectedCardModel);
                    yield return selectedRipener.AddCardAnimation(selectedCard).ToYieldInstruction();
                    
                    // カード削除アニメーション
                    yield return selectedCard.RemovedAnimation().ToYieldInstruction();

                    // カード整頓
                    yield return _handViews[playerModel.Index].ArrangeHandAnimation().ToYieldInstruction();

                    break;
                }
            }
        }

        foreach(var ripener in playerModels.SelectMany(player => player.Ripeners))
        {
            ripener.AddDay();
        }

        var cardTop = deckModel.Open();
        var tmpCardTopView = mainDeck.CreateCard(cardTop.Type, cardTop.Color, cardTop.ID);
        tmpCardTopView.isBack = true;
        cardVM[tmpCardTopView] = cardTop;

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
                    var handCardControls = playerModel.Hand.Select(model => handView.GetCardView(model.ID));
                    var resCardControls = resourceModel.Availables.Select(c => cardVM.First(_ => _.Key.ModelID == c.ID).Key);
                    var cardTopView = cardVM.Where(c => c.Key.ModelID == cardTop.ID).Select(kv => kv.Key);
                    var cardViews = handCardControls.Concat(resCardControls).Concat(cardTopView);

                    Debug.Log(cardViews.Count());

                    // カード選択
                    var cardSelection = Observable.Amb(cardViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return cardSelection;

                    if (!cardSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedCardControl = cardSelection.Result as CardControl;
                    yield return selectedCardControl.SelectedAnimation().ToYieldInstruction();
                    var selectedCardModel = cardVM[selectedCardControl];
                    if (handCardControls.Contains(selectedCardControl))
                    {
                        Debug.Log("手札がクリックされた");

                        // みんなに選択された肉の情報を伝える
                        curentSelectedMeat = selectedCardModel;

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
                            handView.RemoveHand(selectedCardControl);
                            yield return (touched as RipenerView).AddCardAnimation(selectedCardControl).ToYieldInstruction();
                        }
                        else if (touched is BacteriaPlaceView)
                        {
                            Debug.Log("菌トークンを除去");
                            playerModel.RemoveHand(selectedCardModel);

                            // todo: 除去できるできない判定
                            BacteriaPlace bp = (touched as BacteriaPlaceView).bacteriaPlace;
                            int removeBacteriaCount = bp.Removable(selectedCardModel);
                            bp.Remove(selectedCardModel);
                            Debug.Log("Removable End");
                            handView.RemoveHand(selectedCardControl);
                            yield return (touched as BacteriaPlaceView).AddCardAnimation(selectedCardControl, removeBacteriaCount).ToYieldInstruction();
                        }
                        // みんなに肉の選択が終わったことを伝える
                        curentSelectedMeat = null;

                        // カード削除アニメーション
                        yield return selectedCardControl.RemovedAnimation().ToYieldInstruction();

                        // カード整頓
                        yield return _handViews[playerModel.Index].ArrangeHandAnimation().ToYieldInstruction();
                    }
                    else if (resCardControls.Contains(selectedCardControl))
                    {
                        Debug.Log("共通リソースがクリックされた");
                        playerModel.AddHand(resourceModel.GetCard(selectedCardModel));
                        selectedCardControl.isHand = true;
                        yield return _handViews[playerModel.Index].AddHandAnimation(selectedCardControl).ToYieldInstruction();

                        var cardModel = deckModel.Open();
                        var cardView = mainDeck.CreateCard(cardModel.Type, cardModel.Color, cardModel.ID);
                        cardVM[cardView] = cardModel;
                        cardView.transform.parent = ComRes.transform;
                        yield return mainDeck.OpenAnimation(cardView).ToYieldInstruction();

                        resourceModel.AddCard(cardModel);
                        yield return _commonRes.AddResourceAnimation(cardView, selectedCardControl.commonResourceIndex).ToYieldInstruction();

                        // 親を手札オブジェクトに変えたほうがよさそう
                    }
                    else if (cardTopView.Contains(selectedCardControl))
                    {
                        Debug.Log("山札がクリックされた");
                        playerModel.AddHand(selectedCardModel);
                        selectedCardControl.ShowFront();

                        yield return _handViews[playerModel.Index].AddHandAnimation(selectedCardControl).ToYieldInstruction();

                        cardTop = deckModel.Open();
                        var cardView = mainDeck.CreateCard(cardTop.Type, cardTop.Color, cardTop.ID);
                        cardVM[cardView] = cardTop;
                        cardView.isBack = true;
                        // 親を手札オブジェクトに変えたほうがよさそう
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
            Debug.Log("腐敗フェイズ start");
            for (int i=0; i< NumberOfBacterias; i++)
            {
                var token = bacteriaSrc.Draw();
                Debug.Log("Token : " + token.Color);

                // トークンが出てくるアニメーションと待ち合わせ

                var tagetPlace = bacteriaPlaces.Find(bp => bp.Color == token.Color);
                var targetBacteriaPlaceView = _bacterias.Find(bpv => bpv._color == token.Color);
                yield return targetBacteriaPlaceView.AddBacteriaAnimation().ToYieldInstruction();


                if (!tagetPlace.OutBreakInCurrRound)
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
            Debug.Log("腐敗フェイズ end");


            // 売却フェイズ
            Debug.Log("売却フェイズ start");
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

                    var ripenersViews = ripenersModel.Where(r => !r.Empty.Value).Select(model => _ripenerVeiws[index].FirstOrDefault(v => v.ModelID == model.ID));

                    // Todo : パスボタンを追加する
                    var ripenerSelection = Observable.Amb(ripenersViews.Select(_ => _.OnTouchAsObservabale)).First().ToYieldInstruction();
                    yield return ripenerSelection;

                    if (!ripenerSelection.HasResult)
                    {
                        continue;
                    }

                    var selectedRipener = ripenerSelection.Result as RipenerView;
                    Debug.Log("熟成機がタッチされた");

                    var selectedRipenerModel = ripenersModel.FirstOrDefault(m => m.ID == selectedRipener.ModelID);

                    var cashout = selectedRipenerModel.AgingPeriod.Value * selectedRipenerModel.Maturity;
                    playerModel.Point += cashout;
                    selectedRipenerModel.Reset();

                    yield return selectedRipener.ResetAnimation().ToYieldInstruction();
                }
            }
            Debug.Log("売却フェイズ end");

            // 仕込みフェイズ
            Debug.Log("仕込みフェイズ start");
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

                    var selectedCard = cardSelection.Result as CardControl;

                    yield return selectedCard.SelectedAnimation().ToYieldInstruction();

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

                    // カード削除アニメーション
                    yield return selectedCard.RemovedAnimation().ToYieldInstruction();

                    // カード整頓
                    yield return _handViews[playerModel.Index].ArrangeHandAnimation().ToYieldInstruction();

                    break;
                }
            }
            Debug.Log("仕込みフェイズ end");

            // 熟成フェイズ
            Debug.Log("熟成フェイズ start");
            foreach (var ripener in playerModels.SelectMany(player => player.Ripeners))
            {
                if(!ripener.Empty.Value)
                {
                    ripener.AddDay();
                }
            }
            Debug.Log("熟成フェイズ end");
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


