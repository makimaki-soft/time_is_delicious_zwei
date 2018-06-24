using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class DummyHand : MonoBehaviour {

    ObservableEventTrigger _eventTrigger;

    [SerializeField]
    private GameObject _cardPrefab;

    // Use this for initialization
    void Start () {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();

        _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    List<DummyCard> _cards = new List<DummyCard>();

    public List<DummyCard> CurrentCard
    {
        get { return _cards; }
    }

    // 手札にカードを追加するアニメーションのダミー関数
    public IObservable<Unit> AddHand(int playerIndex)
    {
        var coroutine = Observable.Timer(TimeSpan.FromMilliseconds(200)).Do(_=> Debug.Log("Opened : " + playerIndex.ToString())).Select(_ => Unit.Default).Publish().RefCount();
        coroutine.Subscribe();
        return coroutine;
    }

    public IObservable<Vector3> OnDragAsObservabale
    {
        get
        {
            return _eventTrigger.OnMouseDragAsObservable().Select(_=>Input.mousePosition);
        }
    }
}
