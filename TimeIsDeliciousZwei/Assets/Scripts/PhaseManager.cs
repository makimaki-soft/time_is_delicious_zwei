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

    // Notify
    private Subject<Unit> _phaseSubject = new Subject<Unit>();
    public void FinishPhase(Phase phase)
    {
        _phaseFlag[phase] = true;
        _phaseSubject.OnNext(Unit.Default);
    }

    private Subject<int> _playerIdxSubject = new Subject<int>();
    public void AdvancePlayer(int index)
    {
        _playerFlag[index] = true;
        _playerIdxSubject.OnNext(index);
    }

    private Subject<int> _turnSubject = new Subject<int>();
    public void AdvanceTurn(int turn)
    {
        _turnFlag[turn] = true;
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

    private ObservableYieldInstruction<bool> SyncPhase(Phase phase)
    {
        return _phaseSubject.StartWith(Unit.Default).Select(_ => _phaseFlag[phase]).Where(b=>b).FirstOrDefault().ToYieldInstruction();
    }

    Dictionary<Phase, bool> _phaseFlag = new Dictionary<Phase, bool>();
    Dictionary<int, bool> _turnFlag = new Dictionary<int, bool>();
    Dictionary<int, bool> _playerFlag = new Dictionary<int, bool>();

    IEnumerator PhaseControl()
    {
        foreach (Phase phase in Enum.GetValues(typeof(Phase)))
        {
            _phaseFlag[phase] = false;
        }

        _round.Value = 0;

        _phase.Value = Phase.GamePreparation;

        yield return SyncPhase(Phase.GamePreparation);
        
        for(_round.Value = 1; /* forever */ ; _round.Value++)
        {
            foreach (Phase phase in Enum.GetValues(typeof(Phase)))
            {
                _phaseFlag[phase] = false;
            }

            _phase.Value = Phase.PlayerAction;

            for (int i = 0; i < _playerIdx.Count; i++)
            {
                _playerFlag[i] = false;
            }

            foreach (var index in _playerIdx)
            {
                for (int i = 0; i < _turn.Count; i++)
                {
                    _turnFlag[i] = false;
                }

                foreach (var cnt in _turn)
                {

                    while(!_turnFlag[cnt])
                    {
                        yield return _turnSubject.Where(idx => cnt == idx).FirstOrDefault().ToYieldInstruction(); // プレイヤーのアクション実行を待ち合わせ 
                    }
                }

                while(!_playerFlag[index])
                {
                    yield return _playerIdxSubject.Where(idx => index == idx).FirstOrDefault().ToYieldInstruction();
                }   
            }

            yield return SyncPhase(Phase.PlayerAction);
            
            _phase.Value = Phase.Putrefy;

            // 腐敗トークン処理の待ち合わせ
            yield return SyncPhase(Phase.Putrefy);

            _phase.Value = Phase.CashOut;

            for (int i = 0; i < _playerIdx.Count; i++)
            {
                _playerFlag[i] = false;
            }

            foreach (var index in _playerIdx)
            {

                // 売却を待ち合わせ
                while (!_playerFlag[index])
                {
                    yield return _playerIdxSubject.Where(idx => index == idx).FirstOrDefault().ToYieldInstruction();
                }

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

            for (int i = 0; i < _playerIdx.Count; i++)
            {
                _playerFlag[i] = false;
            }

            foreach (var index in _playerIdx)
            {
                // 仕込みを待ち合わせ
                while (!_playerFlag[index])
                {
                    yield return _playerIdxSubject.Where(idx => index == idx).FirstOrDefault().ToYieldInstruction();
                }
            }

            yield return SyncPhase(Phase.FoodPreparation);

            _phase.Value = Phase.Aging;

            // 日数経過を待ち合わせ
            yield return SyncPhase(Phase.Aging);
        }

        _phase.Value = Phase.Ending;
    }
}
