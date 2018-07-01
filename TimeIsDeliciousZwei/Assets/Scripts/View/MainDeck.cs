using System;
using System.Collections;
using System.Collections.Generic;
using TIDZ;
using UniRx;
using UnityEngine;
using static TIDZ.MeatDef;

public class MainDeck : MonoBehaviour {

    [SerializeField]
    private GameObject _cardPrefab;

    public CardView CreateCard(MeatType type, ColorElement color, Guid ID)
    {
        var view = Instantiate(_cardPrefab).GetComponent<CardView>();
        view.ModelID = ID;

        var position = transform.position;
        position.z -= 2f;
        view.transform.position = position;

        return view;
    }

    public IObservable<Unit> OpenAnimation(CardView view)
    {
        return Observable.FromMicroCoroutine(_ => OpenAnimationCoroutine(view));
    }

    IEnumerator OpenAnimationCoroutine(CardView view)
    {
        var position = transform.position;
        position.z -= 2f;
        view.transform.position = position;

        for (int i = 0; i < 20; i++)
        {
            view.transform.position = new Vector3(view.transform.position.x, view.transform.position.y, view.transform.position.z - 0.5f);
            yield return null;
        }
    }
}
