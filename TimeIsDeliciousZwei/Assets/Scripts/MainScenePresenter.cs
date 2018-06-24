﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

using TIDZ;
using System.Linq;

public class MainScenePresenter : MonoBehaviour {

    [SerializeField]
    private PhaseManager _phaseManager;

    [SerializeField]
    private DummyDeck _deckView;

    [SerializeField]
    private List<DummyHand> _handViews;

    [SerializeField]
    private GameObject _handPrefab;

    private CompositeDisposable phaseRangedDisposable = new CompositeDisposable();


    /* Game Componets */
    private GameMaster _gameMaster;
    private Deck<MeatCard> _playingDeck;
    private List<Player> _players;
    private CommonResource _resource;

    // Use this for initialization
    void Start () {

        _phaseManager.CurrentPhase
            .Subscribe(phase => onPhaseChanged(phase))
            .AddTo(gameObject);

        _gameMaster = new GameMaster();
        _gameMaster.NumberOfPlayers = 4;
        _gameMaster.Initialize();
        _playingDeck = _gameMaster.PlayingDeck;
        _players = _gameMaster.Players;
        _resource = _gameMaster.CommonResources;

        _phaseManager.GameMagager = _gameMaster;

        // View生成
        _handViews = new List<DummyHand>(_gameMaster.NumberOfPlayers);
        for (int i=0; i< _gameMaster.NumberOfPlayers; i++)
        {
            _handViews.Add(Instantiate(_handPrefab).GetComponent<DummyHand>());
        }

        _phaseManager.StartGame();
    }

    void onPhaseChanged(PhaseManager.Phase phase)
    {
        Debug.Log("OnPhase : " + phase.ToString());

        phaseRangedDisposable.Clear();

        switch (phase)
        {
            case PhaseManager.Phase.GamePreparation:
                onGamePreparation();
                break;
            case PhaseManager.Phase.InitialPreparation:
                onInitialPreparation();
                break;
            case PhaseManager.Phase.PlayerAction:
                onPlayerAction();
                break;
            case PhaseManager.Phase.Putrefy:
                onPutrefy();
                break;
            case PhaseManager.Phase.CashOut:
                onCashOut();
                break;
            case PhaseManager.Phase.FoodPreparation:
                onFoodPreparation();
                break;
            case PhaseManager.Phase.Aging:
                onAging();
                break;
            case PhaseManager.Phase.Ending:
                onEnding();
                break;
            default:
                Debug.Log("Unknown Phase");
                break;
        }
    }

    void onGamePreparation()
    {
#if false
        // 山札→各プレイヤーの手札へのアニメーション処理
        var oneByOneNext = new Subject<Unit>();

        /* 各プレイヤーの手札増加イベントと直前の山札オープンイベントを結合して<Player, AddEvent>のタプルに変換するLinq */
        var cardDistribute = _players.Select(player => player.Hand.ObserveAdd()
                                                                  .Select(addEvent => Tuple.Create(player, addEvent))
                                                                  .ZipLatest(_playingDeck.ObserveOpen, (tuple, addEvent) => tuple));
        Observable.Merge(cardDistribute)
                  .OneByOne(oneByOneNext)
                  .SelectMany(t => _deckView.OpenCard(t.Item2.Value.Type, t.Item2.Value.Color, t))
                  .SelectMany(t => _handView.AddHand(t.Item1.Index))
                  .Subscribe(_=> oneByOneNext.OnNext(Unit.Default))
                  .AddTo(phaseRangedDisposable);


        var oneByOneCommon = new Subject<Unit>();
        _playingDeck.ObserveOpen.ZipLatest(_resource.Cards.ObserveAdd(), (a, b) => a)
                                .OneByOne(oneByOneCommon)
                                .SelectMany(e => _deckView.OpenCard(e.Value.Type, e.Value.Color, e))
                                .SelectMany(e => Observable.Return(e.Value)) // 共通リソースに肉を配置するアニメーションに変更する
                                .Subscribe(_ => oneByOneCommon.OnNext(Unit.Default))
                                .AddTo(phaseRangedDisposable);


#endif
        _gameMaster.Prepare();

        var prepareStream = Observable.Return<Unit>(Unit.Default);

        for(int i=0; i<_gameMaster.InitialHand; i++)
        {
            foreach (var player in _players)
            {
                var card = player.Hand[i];
                prepareStream = prepareStream.SelectMany(_ => _deckView.OpenCard(card.Type, card.Color))
                                             .SelectMany(_ => _handViews[player.Index].AddHand(player.Index));
            }
        }

        foreach( var res in _resource.Cards )
        {
            prepareStream.SelectMany(_ => _deckView.OpenCard(res.Type, res.Color))
                         .SelectMany(_ => Observable.Return(res.GUID));
        }

        prepareStream.Subscribe(_ =>
        {
            _phaseManager.FinishPhase(PhaseManager.Phase.GamePreparation);
        }).AddTo(phaseRangedDisposable);
    }

    void onInitialPreparation()
    {
        var disposable = new CompositeDisposable();

        _phaseManager.CurrentPlayerIndex_.Subscribe(index =>
        {
            Debug.Log("Player " + index.ToString());

            disposable.Clear();

            // 当該プレイヤーの手札のドラッグを有効化
            // (手札の中から最初にドラッグ開始されたもののドラッグイベントのみを通す(Amb)

            DummyCard card;
            Vector3 position;
            Observable.Amb(_handViews[index].CurrentCard.Select(_=>_.OnDragAsObservabale))
                      // .Repeat()
                      .Subscribe(e =>
                      {
                          // もしposが熟成器UIの近くだったら、強調させる
                          card = e.Item1;
                          position = e.Item2;
                      },
                      ()=>
                      {
                          // ドラッグ終了時に熟成器UIの近くにいたら、その熟成期に肉を入れる
                          Debug.Log("(仮)熟成器にカードを入れるアニメーションを開始（１秒）");
                          Observable.Timer(TimeSpan.FromSeconds(1)).Subscribe(_ =>
                          {
                              Debug.Log("アニメーション終了");
                              var agingCard = _players[index].Hand[0]; // 実際はDummyCardに紐づくModelを選択
                              _players[index].Hand.Remove(agingCard);
                              _players[index].Ripeners[0].AddCard(agingCard);
                          });
                      }).AddTo(disposable);

        }).AddTo(phaseRangedDisposable);
    }

    void onPlayerAction()
    {
        _phaseManager.CurrentPlayerIndex.Subscribe(index =>
        {
            // PlayerAction Phaseでindex番目プレイヤーがやることを書く
            Debug.Log("player " + index.ToString());
            _phaseManager.TurnCount.Subscribe(count =>
            {
                Debug.Log("count " + count.ToString());
                //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.AdvanceTurn(count));
                _phaseManager.AdvanceTurn(count);
            },
            () =>
            {
                // Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.AdvancePlayer(index));
                _phaseManager.AdvancePlayer(index);
            });
        },
        () =>
        {
            Debug.Log("onPlayerAction End");
            // Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.FinishPhase(PhaseManager.Phase.PlayerAction));
            _phaseManager.FinishPhase(PhaseManager.Phase.PlayerAction);
        }).AddTo(phaseRangedDisposable);
    }

    void onPutrefy()
    {
        //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        //{
            Debug.Log("onPutrefy End");
            _phaseManager.FinishPhase(PhaseManager.Phase.Putrefy);
        //});
    }

    void onCashOut()
    {
        _phaseManager.CurrentPlayerIndex.Subscribe(index =>
        {
            // CashOut Phaseでindex番目プレイヤーがやることを書く
            Debug.Log("player " + index.ToString());
            //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.AdvancePlayer(index));
            _phaseManager.AdvancePlayer(index);
        },
        ()=>
        {
            Debug.Log("onCashOut End");
            //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.FinishPhase(PhaseManager.Phase.CashOut));
            _phaseManager.FinishPhase(PhaseManager.Phase.CashOut);
        }).AddTo(phaseRangedDisposable);
    }

    void onFoodPreparation()
    {
        _phaseManager.CurrentPlayerIndex.Subscribe(index =>
        {
            // CashOut Phaseでindex番目プレイヤーがやることを書く
            Debug.Log("player " + index.ToString());
            // Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.AdvancePlayer(index));
            _phaseManager.AdvancePlayer(index);
        },
        () =>
        {
            Debug.Log("onFoodPreparation End");
            // Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => _phaseManager.FinishPhase(PhaseManager.Phase.FoodPreparation));
            _phaseManager.FinishPhase(PhaseManager.Phase.FoodPreparation);
        }).AddTo(phaseRangedDisposable);
    }

    void onAging()
    {
        Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        {
            Debug.Log("onAging End");
            _phaseManager.FinishPhase(PhaseManager.Phase.Aging);
        });
    }

    void onEnding()
    {
        //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        //{
            Debug.Log("onEnding End");
            _phaseManager.FinishPhase(PhaseManager.Phase.Ending);
        //});
    }
}
