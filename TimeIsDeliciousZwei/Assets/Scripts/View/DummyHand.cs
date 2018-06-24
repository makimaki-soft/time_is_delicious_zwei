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

        // _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        // _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        // _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        // _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
        // _cards.Add(Instantiate(_cardPrefab).GetComponent<DummyCard>());
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
    public IObservable<Unit> AddHand(DummyCard card)
    {
        _cards.Add(card);
        return Observable.FromCoroutine(_ => AddHandAnimation(card));
    }

    IEnumerator AddHandAnimation(DummyCard card)
    {
        var targetpos = transform.position;
        targetpos.x += _cards.Count * 1.5f;
        var srcpos = card.transform.position;

        var d = new Vector3((targetpos.x - srcpos.x) / 20f, (targetpos.y - srcpos.y) / 20f, (targetpos.z - srcpos.z) / 20f);

        for (int i = 0; i < 20; i++)
        {
            card.transform.Translate(d);
            yield return null;
        }

        card.transform.position = targetpos;
    }

    public IObservable<Vector3> OnDragAsObservabale
    {
        get
        {
            return _eventTrigger.OnMouseDragAsObservable().Select(_=>Input.mousePosition);
        }
    }
}
