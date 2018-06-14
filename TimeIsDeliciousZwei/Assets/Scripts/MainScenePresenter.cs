using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class MainScenePresenter : MonoBehaviour {

    [SerializeField]
    private PhaseManager _phaseManager;

    // Use this for initialization
    void Start () {

        _phaseManager.CurrentPhase
            .Subscribe(phase => onPhaseChanged(phase))
            .AddTo(gameObject);

        _phaseManager.StartGame();

    }

    void onPhaseChanged(PhaseManager.Phase phase)
    {
        Debug.Log("OnPhase : " + phase.ToString());
        
        switch (phase)
        {
            case PhaseManager.Phase.GamePreparation:
                onPreparation();
                break;
            case PhaseManager.Phase.PlayerAction:
                break;
            case PhaseManager.Phase.Putrefy:
                break;
            case PhaseManager.Phase.CashOut:
                break;
            case PhaseManager.Phase.FoodPreparation:
                break;
            case PhaseManager.Phase.Aging:
                break;
            case PhaseManager.Phase.Ending:
                break;
            default:
                Debug.Log("Unknown Phase");
                break;
        }
    }

    void onPreparation()
    {
        Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ =>
        {
            _phaseManager.NotifyPreparationComplete();
        });

        Debug.Log("ゲームの準備");
    }
}
