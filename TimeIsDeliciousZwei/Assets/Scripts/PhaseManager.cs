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

    private ReactiveProperty<int> _currPlayer = new ReactiveProperty<int>(0);
    private ObservableEnumerable<int> _playerIdx; // これは消す方向で。
    public IReactiveProperty<int> CurrentPlayerIndex
    {
        get
        {
            return _currPlayer;
        }
    }

    private Subject<int> _currPlayerSubject = new Subject<int>();
    public IObservable<int> CurrentPlayerIndex_
    {
        get { return _currPlayerSubject; }
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
    public GameMaster GameMagager { private get; set; }

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

        /* 初期仕込みフェイズ */
        _phase.Value = Phase.InitialPreparation;

        for (int i = 0; i < GameMagager.NumberOfRipeners; i++)
        {
            for (int idx = 0; idx < GameMagager.NumberOfPlayers; idx++)
            {
                Debug.Log("Player Change " + idx);
                _currPlayer.Value = idx;
                _currPlayerSubject.OnNext(idx);

                // プレイヤー持つ熟成器からのEmptyプロパティの変更通知をうけたら、
                // 他の熟成器も含めてEmptyでない熟成器の数を調べ、それが最初は1,次は2だったら次のプレイヤーへ。
                yield return Observable.Merge(GameMagager.Players[idx].Ripeners.Select(r => r.Empty))
                                       .Select(_ => GameMagager.Players[idx].Ripeners.Select(r => r.Empty.Value).Where(b => !b).Count())
                                       .Where(notEmptyNum => notEmptyNum == i+1)
                                       .First()
                                       .ToYieldInstruction();
            }
        }

        yield return SyncPhase(Phase.InitialPreparation);

        for (_round.Value = 1; /* forever */ ; _round.Value++)
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
