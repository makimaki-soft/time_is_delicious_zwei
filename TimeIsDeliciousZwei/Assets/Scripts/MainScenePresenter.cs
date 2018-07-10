using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

class Dummy : ITimeIsDeliciousZwei
{
    public int NumberOfPlayers
    {
        get { return 4; }
    }
    public int ActionCount
    {
        get { return 2; }
    }
    public int RotTokenPreRound
    {
        get { return 5; }
    }
}

public class MainScenePresenter : MonoBehaviour {

    [SerializeField]
    private PhaseManager _phaseManager;

    [SerializeField]
    private CardControl _card;

    private CompositeDisposable phaseRangedDisposable = new CompositeDisposable();

    // Use this for initialization
    void Start () {

        _phaseManager.CurrentPhase
            .Subscribe(phase => onPhaseChanged(phase))
            .AddTo(gameObject);

        _phaseManager.GameMagager = new Dummy();

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
        /*
        _card.OnClickAsObservabale.SelectMany(_ => _card.FlipAnimation())
            .Subscribe(_ =>
            {
                Debug.Log("onPreparation End");
                _phaseManager.FinishPhase(PhaseManager.Phase.GamePreparation);
            });
        */

        //Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        //{
        //Debug.Log("onPreparation End");
        //    _phaseManager.FinishPhase(PhaseManager.Phase.GamePreparation);
        //});
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
