using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CommonResourceView : MonoBehaviour {

    private List<Vector3> CommonResourcePoints = new List<Vector3>()
    {
        new Vector3(2, -4, 0),
        new Vector3(10, -4, 0),
        new Vector3(2, -16, 0),
        new Vector3(10, -16, 0)
    };

    public int CardCount { get; set; }

    public IObservable<Unit> AddResourceAnimation(CardControl card)
    {
        return Observable.FromCoroutine(_ => AddResourceAnimationCoroutine(card));
    }

    IEnumerator AddResourceAnimationCoroutine(CardControl card)
    {
        var targetpos = transform.position;
        targetpos.x += CommonResourcePoints[CardCount - 1].x;
        targetpos.y += CommonResourcePoints[CardCount - 1].y;
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
