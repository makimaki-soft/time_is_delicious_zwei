using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using TIDZ;
using SpriteGlow;

public class RipenerView : MonoBehaviour {

    ObservableEventTrigger _eventTrigger;
    public Guid ModelID { get; set; }

    [SerializeField]
    private GameObject _type;

    [SerializeField]
    private List<GameObject> _virus = new List<GameObject>();


    // 自分がいまクリック対象かどうか調べるため
    public Ripener ripener; // ModelIdと役割がかぶっている。要相談。
    private MainScenePresenter _mspresenter;

    // 点滅用
    public float speed = 1.0f;
    private float time;
    SpriteGlowEffect _spriteGlow;

    // Use this for initialization
    void Start()
    {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
        _mspresenter = GameObject.Find("MainScenePresenter").GetComponent<MainScenePresenter>();
        _spriteGlow = GetComponent<SpriteGlowEffect>();
    }

    public IObservable<MonoBehaviour> OnTouchAsObservabale
    {
        get
        {
            return _eventTrigger.OnPointerClickAsObservable().Select(_ => this);
        }
    }

    public IObservable<Unit> AddCardAnimation(CardControl card)
    {
        return Observable.FromCoroutine(_ => AddCardAnimationCoroutine(card));
    }

    IEnumerator AddCardAnimationCoroutine(CardControl card)
    {
        var targetpos = transform.position;
        targetpos.z -= 0.2f;
        var srcpos = card.transform.position;

        var d = new Vector3((targetpos.x - srcpos.x) / 20f, (targetpos.y - srcpos.y) / 20f, (targetpos.z - srcpos.z) / 20f);

        for (int i = 0; i < 20; i++)
        {
            card.transform.Translate(d);
            yield return null;
        }

        card.transform.position = targetpos;

        // Typeをactiveにする
        _type.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Type/" + card.Type.ToString());
        _type.SetActive(true);

        // ウイルスの色を点灯
        _virus[(int)card.Color].SetActive(true);
    }

    void Update()
    {
        MeatCard curentSelectedMeat = _mspresenter.curentSelectedMeat;

        if (curentSelectedMeat != null && ripener != null)
        {
           if (ripener.CanAdd(curentSelectedMeat))
           {
                _spriteGlow.GlowColor = GetAlphaColor(_spriteGlow.GlowColor);
            }
           

        }
    }

    //Alpha値を更新してColorを返す
    Color GetAlphaColor(Color color)
    {

        time += Time.deltaTime * 5.0f * speed;
        color.a = Mathf.Sin(time) * 0.5f + 0.5f;

        return color;
    }
}
