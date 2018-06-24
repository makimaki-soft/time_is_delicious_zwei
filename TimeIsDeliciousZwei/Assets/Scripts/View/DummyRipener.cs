using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class DummyRipener : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    List<DummyCard> _cards = new List<DummyCard>();

    // 手札にカードを追加するアニメーションのダミー関数
    public IObservable<Unit> AddCard(DummyCard card)
    {
        _cards.Add(card);
        return Observable.FromCoroutine(_ => AddCardAnimation(card));
    }

    IEnumerator AddCardAnimation(DummyCard card)
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

}
