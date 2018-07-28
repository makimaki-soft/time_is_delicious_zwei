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

    public CardControl CreateCard(MeatType type, ColorElement color, Guid ID)
    {
        var view = Instantiate(_cardPrefab).GetComponent<CardControl>();
        view.ModelID = ID;
        view.Color = color;
        view.Type = type;

        var position = transform.position;
        position.z -= 1f;
        view.transform.position = position;

        return view;
    }

    public IObservable<Unit> OpenAnimation(CardControl view)
    {
        return Observable.FromMicroCoroutine(_ => OpenAnimationCoroutine(view));
    }

    IEnumerator OpenAnimationCoroutine(CardControl view)
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
