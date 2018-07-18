using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UniRx.Triggers;
using SpriteGlow;
using static TIDZ.MeatDef;

public class CardControl : MonoBehaviour
{
    // private Subject<string> flipAnimationSubject = new Subject<string>();

    ObservableEventTrigger _eventTrigger;
    SpriteGlowEffect _spriteGlow;

    // SpriteRenderer _spriteRenderer;
    public MeatType Type { get; set; }
    public ColorElement Color { get; set; }
    public Guid ModelID { get; set; }

    GameObject _typeSprite;
    GameObject _colorElementSprite;
    List<Color> meatColors = new List<Color>
    {
        UnityEngine.Color.red,
        UnityEngine.Color.blue,
        UnityEngine.Color.yellow,
        UnityEngine.Color.green,
        UnityEngine.Color.magenta
    };

    //[SerializeField]
    //public AnimationCurve animCurve = AnimationCurve.Linear(0, 0, 1, 1);

    // 点滅用
    public float speed = 1.0f;
    private float time;
    public bool isSelected = false; // このカードが選択状態の時はtrue

    // 初期位置
    // public Vector3 initPosition;

    // Use this for initialization
    void Start () {
        _eventTrigger = gameObject.AddComponent<ObservableEventTrigger>();
        _spriteGlow = GetComponent<SpriteGlowEffect>();
        // _spriteRenderer = GetComponent<SpriteRenderer>();

        //handControl.addClickObserver(this);
        //handControl.addDealObserver(this);

        // typeとcolorElementに合わせて見た目を変える
        _typeSprite = transform.Find("Type").gameObject;
        _typeSprite.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Type/" + Type.ToString());

        _colorElementSprite = transform.Find("ColorElement").gameObject;
        _colorElementSprite.GetComponent<SpriteGlowEffect>().GlowColor = meatColors[(int)Color];
    }

    public IObservable<MonoBehaviour> OnTouchAsObservabale
    {
        get
        {
            return _eventTrigger.OnPointerClickAsObservable().Select(_ => this);
        }
    }

    /*
    public IObservable<Unit> OnClickAsObservabale
    {
        get
        {
            return _eventTrigger.OnPointerDownAsObservable().Select(_=>Unit.Default);
        }
    }
    */

    /*
    public IObservable<Unit> FlipAnimation()
    {
        // めくるアニメーションと移動アニメーションを分けてる。
        var coroutine = Observable.WhenAll(
            Observable.FromCoroutine(MoveAnimationCoroutine).Publish().RefCount(),
            Observable.FromCoroutine(FlipAnimationCoroutine).Publish().RefCount()
        );
        //var coroutine = Observable.FromCoroutine(FlipAnimationCoroutine).Publish().RefCount();
        // coroutine.Subscribe();
        return coroutine;
    }

    IEnumerator FlipAnimationCoroutine()
    {
        float angle = -180f;
        float Speed = 400f;

        // 最初裏
        transform.eulerAngles = new Vector3 (0, 180, 0);
        _spriteRenderer.color = new Color(1, 1, 1, 0);

        //90度を超えるまで回転
        while (angle < -90f) {
            angle += Speed * Time.deltaTime;
            transform.eulerAngles = new Vector3 (0, angle, 0);
            yield return null;
        }

        _spriteRenderer.color = new Color(1, 1, 1, 1);

        // 180度まで回転
        while (angle < 0f) {
            angle += Speed * Time.deltaTime;
            transform.eulerAngles = new Vector3 (0, angle, 0);
            yield return null;
        }

        transform.eulerAngles = new Vector3 (0, 0, 0);

        // いる？
        flipAnimationSubject.OnNext("FlipAnimationFinished");
    }
 

    IEnumerator MoveAnimationCoroutine()
    {
        float startTime = Time.timeSinceLevelLoad;
        float duration = 0.5f;    // スライド時間（秒）
        Vector3 startPosition = transform.position;
        Vector3 endPosition = initPosition;

        while((Time.timeSinceLevelLoad - startTime) < duration)
        {
            var pos = animCurve.Evaluate(Time.timeSinceLevelLoad - startTime);
            transform.position = Vector3.Lerp (startPosition, endPosition, pos);
            yield return null;
        }

        transform.position = endPosition;

        // いる？
        flipAnimationSubject.OnNext("MoveAnimationFinished");
    }
    */

    // カード選択時のアニメーション
    public IObservable<Unit> SelectedAnimation()
    {
        isSelected = !isSelected;
        var coroutine = Observable.FromCoroutine(SelectedAnimationCoroutine).Publish().RefCount();
        return coroutine;
    }

    // 選択したときにわかりやすいようにカードを1回転する
    IEnumerator SelectedAnimationCoroutine()
    {
        float angle = 0f;
        float Speed = 3000f;

        // 1回転
        while (angle < 360f) {
            angle += Speed * Time.deltaTime;
            transform.eulerAngles = new Vector3 (0, angle, 0);
            yield return null;
        }

        transform.eulerAngles = new Vector3 (0,360, 0);
    }

    // カード削除時のアニメーション
    public IObservable<Unit> RemovedAnimation()
    {
        var coroutine = Observable.FromCoroutine(RemovedAnimationCoroutine).Publish().RefCount();
        return coroutine;
    }

    // カードを消すアニメーション
    public IEnumerator RemovedAnimationCoroutine()
    {
        float angle = 0f;
        float Speed = 3000f;

        // 1回転
        while (angle < 360f)
        {
            angle += Speed * Time.deltaTime;
            transform.eulerAngles = new Vector3(0, angle, 0);
            yield return null;
        }

        transform.eulerAngles = new Vector3(0, 360, 0);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update () {

        // 選択されているときは枠を点滅
        if (isSelected)
        {
            _spriteGlow.GlowColor = GetAlphaColor(_spriteGlow.GlowColor);
        }
    }

    //Alpha値を更新してColorを返す
    Color GetAlphaColor(Color color) {
        time += Time.deltaTime * 5.0f * speed;
        color.a = Mathf.Sin(time) * 0.5f + 0.5f;

        return color;
    }
}
