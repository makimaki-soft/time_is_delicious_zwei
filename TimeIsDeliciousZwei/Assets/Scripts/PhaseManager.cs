using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class PhaseManager : MonoBehaviour
{
    public enum Phase
    {
        GamePreparation,    // ゲームの準備中
        PlayerAction,       // アクションフェイズ
        Putrefy,            // 腐敗フェイズ
        CashOut,            // 売却フェイズ
        FoodPreparation,    // 仕込みフェイズ
        Aging,              // 日数経過フェイズ
        Ending,             // ゲーム終了
        Unknown = 99        // 不明なフェイズ
    }

    // Reactive Property
    public IReactiveProperty<Phase> CurrentPhase { get; private set; } = new ReactiveProperty<Phase>(Phase.Unknown);
    public IReactiveProperty<bool> EndFlag { get; private set; } = new ReactiveProperty<bool>(false);

    // Dependency
    public ITimeIsDeliciousZwei GameMagager { private get; set; }

    // Notify
    private IReactiveProperty<bool> _isReady = new ReactiveProperty<bool>(false);
    public void NotifyPreparationComplete()
    {
        _isReady.Value = true;
    }

    public void StartGame()
    {
        Observable.FromCoroutine(PhaseControl).Subscribe().AddTo(gameObject);
    }

    IEnumerator PhaseControl()
    {
        Debug.Log("Game Preparation");

        CurrentPhase.Value = Phase.GamePreparation;

        if(!_isReady.Value)
        {
            yield return _isReady.Where(_=>true).FirstOrDefault(_ => _).ToYieldInstruction();
        }

        Debug.Log("Game Start");

        while(true)
        {
            CurrentPhase.Value = Phase.PlayerAction;

            for (int playerIndex=0; playerIndex< GameMagager.NumberOfPlayers; playerIndex++)
            {
                for(int actionIndex=0; actionIndex< GameMagager.ActionCount; actionIndex++)
                {
                    yield return null; // プレイヤーのアクション実行を待ち合わせ
                }
            }

            CurrentPhase.Value = Phase.Putrefy;

            yield return null; // 腐敗トークン処理の待ち合わせ

            CurrentPhase.Value = Phase.CashOut;

            for (int i = 0; i < GameMagager.NumberOfPlayers; i++)
            {
                yield return null; // 売却を待ち合わせ
                if(false)
                {
                    EndFlag.Value = true;
                }
            }

            if(EndFlag.Value)
            {
                break;
            }

            CurrentPhase.Value = Phase.FoodPreparation;

            for (int i = 0; i < GameMagager.NumberOfPlayers; i++)
            {
                yield return null; // 仕込みを待ち合わせ
            }

            CurrentPhase.Value = Phase.Aging;

            yield return null; // 日数経過を待ち合わせ
        }

        CurrentPhase.Value = Phase.Ending;
    }
}
