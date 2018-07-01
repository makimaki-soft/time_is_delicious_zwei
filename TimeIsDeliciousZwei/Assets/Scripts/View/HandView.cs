using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class HandView : MonoBehaviour {

    public int CardCount { get; set; }

    List<CardView> cardList = new List<CardView>();

    public CardView GetCardView(Guid ID)
    {
        return cardList.Find(v => v.ModelID == ID);
    }

    public IObservable<Unit> AddHandAnimation(CardView card)
    {
        cardList.Add(card);
        return Observable.FromCoroutine(_ => AddHandAnimationCoroutine(card));
    }

    IEnumerator AddHandAnimationCoroutine(CardView card)
    {
        var targetpos = transform.position;
        targetpos.x += CardCount * 1.5f;
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
