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
            return _phase;
        }
    }

    private ReactiveProperty<int> _round = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> RoundCount
    {
        get
        {
            return _round;
        }
    }

    private ObservableEnumerable<int> _playerIdx;
    public IObservable<int> CurrentPlayerIndex
    {
        get
        {
            return _playerIdx.IterationAsObservable;
        }
    }

    private ObservableEnumerable<int> _turn;
    public IObservable<int> TurnCount
    {
        get
        {
            return _turn.IterationAsObservable;
        }
    }

    public IReactiveProperty<bool> EndFlag { get; private set; } = new ReactiveProperty<bool>(false);

    // Dependency
    public ITimeIsDeliciousZwei GameMagager { private get; set; }

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

    Dictionary<Phase, bool> _phaseFlag = new Dictionary<Phase, bool>();
    Dictionary<int, bool> _turnFlag = new Dictionary<int, bool>();
    Dictionary<int, bool> _playerFlag = new Dictionary<int, bool>();

    private Subject<Unit> _syncSubject = new Subject<Unit>();

    public void FinishPhase(Phase phase)
    {
        _phaseFlag[phase] = true;
        _syncSubject.OnNext(Unit.Default);
    }

    public void AdvancePlayer(int index)
    {
        _playerFlag[index] = true;
        _syncSubject.OnNext(Unit.Default);
    }

    public void AdvanceTurn(int turn)
    {
        _turnFlag[turn] = true;
        _syncSubject.OnNext(Unit.Default);
    }

    private ObservableYieldInstruction<bool> SyncPhase(Phase phase)
    {
        return _syncSubject.StartWith(Unit.Default).Select(_ => _phaseFlag.GetOrDefault(phase)).Where(b=>b).FirstOrDefault().ToYieldInstruction();
    }

    private ObservableYieldInstruction<bool> SyncPlayer(int index)
    {
        return _syncSubject.StartWith(Unit.Default).Select(_ => _playerFlag.GetOrDefault(index)).Where(b => b).FirstOrDefault().ToYieldInstruction();
    }

    private ObservableYieldInstruction<bool> SyncAction(int count)
    {
        return _syncSubject.StartWith(Unit.Default).Select(_ => _turnFlag.GetOrDefault(count)).Where(b => b).FirstOrDefault().ToYieldInstruction();
    }

    IEnumerator PhaseControl()
    {
        _phaseFlag.Clear();
        _round.Value = 0;

        _phase.Value = Phase.GamePreparation;

        yield return SyncPhase(Phase.GamePreparation);
        
        for(_round.Value = 1; /* forever */ ; _round.Value++)
        {
            _phaseFlag.Clear();

            _phase.Value = Phase.PlayerAction;

            _playerFlag.Clear();
            foreach (var index in _playerIdx)
            {
                _turnFlag.Clear();
                foreach (var cnt in _turn)
                {
                    yield return SyncAction(cnt);
                }
                yield return SyncPlayer(index);
            }

            yield return SyncPhase(Phase.PlayerAction);
            
            _phase.Value = Phase.Putrefy;

            yield return SyncPhase(Phase.Putrefy);

            _phase.Value = Phase.CashOut;

            _playerFlag.Clear();
            foreach (var index in _playerIdx)
            {
                yield return SyncPlayer(index);

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

            _playerFlag.Clear();
            foreach (var index in _playerIdx)
            {
                yield return SyncPlayer(index);
            }

            yield return SyncPhase(Phase.FoodPreparation);

            _phase.Value = Phase.Aging;

            yield return SyncPhase(Phase.Aging);
        }

        _phase.Value = Phase.Ending;
    }
}
