using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

public class PassButtonView : MonoBehaviour {

    [SerializeField]
    Button button;

    public IObservable<MonoBehaviour> OnTouchAsObservabale
    {
        get
        {
            return button.onClick.AsObservable().Select(_ => this);
        }
    }

    public bool Enabled
    {
        get
        {   
            return button.enabled;
        }
        set
        {
            button.enabled = value;
            button.gameObject.SetActive(value);
        }
    }
}
