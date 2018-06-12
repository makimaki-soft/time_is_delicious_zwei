using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class TextFade : MonoBehaviour {

    [SerializeField]
    private AnimationCurve _fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField, Range(0, 10)]
    private float _duration = 1.0f;

    [SerializeField]
    private bool _wakeUpRun = false;

    [SerializeField]
    private bool _loop = true;

    public IObservable<Unit> FadeAsObservable
    {
        get
        {
            return Observable.FromMicroCoroutine(FadeCoroutine);
        }
    }

    void Start()
    {
        if (_wakeUpRun)
        {
            FadeAsObservable.Subscribe().AddTo(gameObject);
        }
    }

    IEnumerator FadeCoroutine()
    {
        do
        {
            var startTime = Time.timeSinceLevelLoad;
            var color = GetComponent<Text>().color;
            float currentTime = 0f;

            color.a = 0f;

            while ((currentTime = (Time.timeSinceLevelLoad - startTime)) < _duration)
            {
                var progressRate = currentTime / _duration;
                var alpha = _fadeCurve.Evaluate(progressRate);

                GetComponent<Text>().color = color;

                color.a = alpha;

                yield return null;
            }
        } while (_loop);
    }
}
