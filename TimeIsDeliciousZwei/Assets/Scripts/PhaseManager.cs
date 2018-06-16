using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

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
    private ReactiveProperty<Phase> _phase = new ReactiveProperty<Phase>(Phase.Unknown);
    public IReactiveProperty<Phase> CurrentPhase
    {
        get
        {
            return _phase.DelayFrame(1).ToReactiveProperty();
        }
    }

    private ReactiveProperty<int> _round = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> RoundCount
    {
        get
        {
            return _round.DelayFrame(1).ToReactiveProperty();
        }
    }

    private ObservableEnumerable<int> _playerIdx;
    public IObservable<int> CurrentPlayerIndex
    {
        get
        {
            return _playerIdx.IterationAsObservable.DelayFrame(1);
        }
    }

    private ObservableEnumerable<int> _turn;
    public IObservable<int> TurnCount
    {
        get
        {
            return _turn.IterationAsObservable.DelayFrame(1);
        }
    }

    public IReactiveProperty<bool> EndFlag { get; private set; } = new ReactiveProperty<bool>(false);

    // Dependency
    public ITimeIsDeliciousZwei GameMagager { private get; set; }

    // Notify
    private Subject<Phase> _phaseSubject = new Subject<Phase>();
    public void FinishPhase(Phase phase)
    {
        _phaseSubject.OnNext(phase);
    }

    private Subject<int> _playerIdxSubject = new Subject<int>();
    public void AdvancePlayer(int index)
    {
        _playerIdxSubject.OnNext(index);
    }

    private Subject<int> _turnSubject = new Subject<int>();
    public void AdvanceTurn(int turn)
    {
        _turnSubject.OnNext(turn);
    }

    public void StartGame()
    {
        var indexList = new List<int>();
        for(int i=0; i< GameMagager.NumberOfPlayers; i++)
        {
            indexList.Add(i);
        }
        _playerIdx = new ObservableEnumerable<int>(indexList);

        var turnList = new List<int>();
        for (int i = 0; i < GameMagager.ActionCount; i++)
        {
            turnList.Add(i);
        }
        _turn = new ObservableEnumerable<int>(turnList);

        Observable.FromCoroutine(PhaseControl).Subscribe().AddTo(gameObject);
    }

    private ObservableYieldInstruction<Phase> SyncPhase(Phase phase)
    {
        return _phaseSubject.Where(ph => ph == phase).FirstOrDefault().ToYieldInstruction();
    }

    IEnumerator PhaseControl()
    {
        _round.Value = 0;

        _phase.Value = Phase.GamePreparation;

        yield return SyncPhase(Phase.GamePreparation);

        for(_round.Value = 1; /* forever */ ; _round.Value++)
        {
            _phase.Value = Phase.PlayerAction;
            yield return null; // indexのsubscribeのために1フレーム待つ。条件変数方式にすべき
            yield return null;

            foreach (var index in _playerIdx)
            {
                yield return null;
                yield return null;

                foreach (var cnt in _turn)
                {
                    yield return _turnSubject.Where(idx => cnt == idx).FirstOrDefault().ToYieldInstruction(); // プレイヤーのアクション実行を待ち合わせ 
                }
                yield return _playerIdxSubject.Where(idx => index == idx).FirstOrDefault().ToYieldInstruction();
            }

            yield return SyncPhase(Phase.PlayerAction);

            _phase.Value = Phase.Putrefy;

            yield return SyncPhase(Phase.Putrefy); // 腐敗トークン処理の待ち合わせ

            _phase.Value = Phase.CashOut;
            yield return null; // indexのsubscribeのために1フレーム待つ。条件変数方式にすべき
            yield return null;
        
            foreach (var index in _playerIdx)
            {
                yield return _playerIdxSubject.Where(idx=>index==idx).FirstOrDefault().ToYieldInstruction(); // 売却を待ち合わせ
                if(false)
                {
                    EndFlag.Value = true;
                }
            }
            
            yield return SyncPhase(Phase.CashOut);

            if (EndFlag.Value)
            {
                break;
            }

            _phase.Value = Phase.FoodPreparation;
            yield return null;
            yield return null;

            foreach (var index in _playerIdx)
            {
                yield return _playerIdxSubject.Where(idx => index == idx).FirstOrDefault().ToYieldInstruction(); // 仕込みを待ち合わせ
            }

            yield return SyncPhase(Phase.FoodPreparation);

            _phase.Value = Phase.Aging;

            yield return SyncPhase(Phase.Aging); // 日数経過を待ち合わせ
        }

        _phase.Value = Phase.Ending;
    }
}
