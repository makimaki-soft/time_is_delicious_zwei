using System;
using System.Collections;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

public class CardView : MonoBehaviour {

    ObservableEventTrigger _eventTrigger;
    public Guid ModelID { get; set; } 

    // Use this for initialization
    void Start () {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
    }

    public IObservable<MonoBehaviour> OnTouchAsObservabale
    {
        get
        {
            return _eventTrigger.OnPointerClickAsObservable().Select(_ => this);
        }
    }

    public IObservable<Unit> OnTouchAnimation()
    {
        return Observable.FromMicroCoroutine(OnTouchAnimationCoroutine);
    }

    IEnumerator OnTouchAnimationCoroutine()
    {
        for (int i = 0; i < 20; i++)
        {
            var pos = transform.position;
            pos.z -= 0.3f;
            transform.position = pos;
            yield return null;
        }

    }
}
