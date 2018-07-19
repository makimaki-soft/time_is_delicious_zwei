using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class HandView : MonoBehaviour {

    public int CardCount { get; set; }

    List<CardControl> cardList = new List<CardControl>();

    public CardControl GetCardView(Guid ID)
    {
        return cardList.Find(v => v.ModelID == ID);
    }

    public void RemoveHand(CardControl card)
    {
        cardList.Remove(card);
    }

    public IObservable<Unit> AddHandAnimation(CardControl card)
    {
        cardList.Add(card);
        return Observable.FromCoroutine(_ => AddHandAnimationCoroutine(card));
    }

    IEnumerator AddHandAnimationCoroutine(CardControl card)
    {
        var targetpos = transform.position;
        targetpos.x += (CardCount-1) * 6f;
        var srcpos = card.transform.position;

        var d = new Vector3((targetpos.x - srcpos.x) / 20f, (targetpos.y - srcpos.y) / 20f, (targetpos.z - srcpos.z) / 20f);

        for (int i = 0; i < 20; i++)
        {
            card.transform.Translate(d);
            yield return null;
        }

        card.transform.position = targetpos;
    }

    /*
	選択したカード以外を非選択状態にする。
	*/
    private void resetSelected(CardControl selectedCard)
    {
        foreach (CardControl _card in cardList)
        {
            Debug.Log(selectedCard.isSelected);
            if (selectedCard != _card)
            {
                _card.isSelected = false;
            }
        }
    }

    /*
     *  手札を見やすいように整理する
     */
    public IObservable<Unit> ArrangeHandAnimation()
    {
        return Observable.FromCoroutine(_ => ArrangeHandAnimationCoroutine());
    }

    IEnumerator ArrangeHandAnimationCoroutine()
    {
        int index = 0;
        foreach (var card in cardList)
        {
            var targetpos = transform.position;
            targetpos.x += index * 6f;
            var srcpos = card.transform.position;

            float duration = 5f;
            var d = new Vector3((targetpos.x - srcpos.x) / duration, (targetpos.y - srcpos.y) / duration, (targetpos.z - srcpos.z) / duration);

            for (int i = 0; i < (int)duration; i++)
            {
                card.transform.Translate(d);
                yield return null;
            }

            index++;
            card.transform.position = targetpos;
        }
    }
}
