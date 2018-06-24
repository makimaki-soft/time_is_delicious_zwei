using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using static TIDZ.MeatDef;

public class DummyDeck : MonoBehaviour
{

    [SerializeField]
    private GameObject _cardPrefab;

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // 山札からカードをオープンするアニメーションのダミー関数
    public IObservable<DummyCard> OpenCard(MeatType type, ColorElement color, Guid ID)
    {
        // TODO : 呼び出し側でSubscribeしなかった場合にインスタンス化したCardがリークする
        var newCard = Instantiate(_cardPrefab).GetComponent<DummyCard>();
        newCard.transform.position = transform.position;

        newCard.MeatColor = color;
        newCard.Type = type;
        newCard.ID = ID;

        return Observable.FromCoroutine(_ => OpenAnimation(newCard)).Select(_ => newCard);
    }

    IEnumerator OpenAnimation(DummyCard card)
    {
        for(int i=0; i<20; i++)
        {
            card.transform.position = new Vector3(card.transform.position.x, card.transform.position.y, card.transform.position.z - 0.5f);
            yield return null;
        }
    }
}
