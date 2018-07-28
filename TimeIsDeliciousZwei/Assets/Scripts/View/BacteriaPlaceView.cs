using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using TIDZ;
using static TIDZ.MeatDef;
using SpriteGlow;

public class BacteriaPlaceView : MonoBehaviour {

    ObservableEventTrigger _eventTrigger;
    public Guid ModelID { get; set; }
    public ColorElement _color;

    // 自分がいまクリック対象かどうか調べるため
    public BacteriaPlace bacteriaPlace; // ModelIdと役割がかぶっている。要相談。
    private MainScenePresenter _mspresenter;

    // 点滅用
    public float speed = 1.0f;
    private float time;
    SpriteGlowEffect _spriteGlow;

    [SerializeField]
    private List<GameObject> _bacterias = new List<GameObject>();


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

    // 菌除去時のアニーメーション
    public IObservable<Unit> AddCardAnimation(CardControl card, int removeVirusCount)
    {
        Debug.Log("菌トークンを除去!");
        return Observable.FromCoroutine(_ => AddCardAnimationCoroutine(card, removeVirusCount));
    }

    IEnumerator AddCardAnimationCoroutine(CardControl card, int removeVirusCount = 0)
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

        Debug.Log(removeVirusCount + "個を消す");
        
        for (int i = 0; i < removeVirusCount; i++)
        {
            // 菌をランダムにinactiveにする
            List<GameObject> activeBacterias = _bacterias.FindAll(bpv => bpv.activeSelf == true);
            System.Random r = new System.Random(1000);
            activeBacterias[r.Next(0, activeBacterias.Count - 1)].SetActive(false);
        }
        
    }

    // 菌追加時のアニーメーション
    public IObservable<Unit> AddBacteriaAnimation()
    {
        Debug.Log("菌を追加する！" + _color);
        return Observable.FromCoroutine(_ => AddBacteriaAnimationCoroutine());
    }

    IEnumerator AddBacteriaAnimationCoroutine()
    {
        yield return null;

        // 菌をランダムにactiveにする
        List<GameObject> inactiveBacterias = _bacterias.FindAll(bpv => bpv.activeSelf == false);
        System.Random r = new System.Random(1000);
        inactiveBacterias[r.Next(0, inactiveBacterias.Count - 1)].SetActive(true);
        
    }


    void Update()
    {
        MeatCard curentSelectedMeat = _mspresenter.curentSelectedMeat;

        if (curentSelectedMeat != null && bacteriaPlace != null)
        {
            if (bacteriaPlace.CanRemove(curentSelectedMeat))
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
