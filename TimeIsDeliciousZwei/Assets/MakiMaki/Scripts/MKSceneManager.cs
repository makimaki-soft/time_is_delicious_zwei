using System;
using System.Collections;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MKSceneManager : MonoBehaviour {

    [SerializeField]
    private FadeImage _fade;

    [SerializeField]
    private AnimationCurve _fadeCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [SerializeField, Range(0, 10)]
    private float _duration = 1.0f;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

	// Use this for initialization
	void Start () {
        
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void SceneLoad(string sceneName)
    {
        Observable.FromMicroCoroutine(FadeCoroutine).Subscribe().AddTo(gameObject);
    }

    IEnumerator FadeCoroutine()
    {
        var startTime = Time.time;
        
        float currentTime = 0f;

        _fade.Range = 0f;

        while ((currentTime = (Time.time - startTime)) < _duration / 2)
        {
            var progressRate = currentTime / _duration;
            var range = _fadeCurve.Evaluate(progressRate);

            _fade.Range = range;

            yield return null;
        }

        SceneManager.LoadScene("MainScene");

        while ((currentTime = (Time.time - startTime)) < _duration )
        {
            var progressRate = currentTime / _duration;
            var range = _fadeCurve.Evaluate(progressRate);

            _fade.Range = range;

            yield return null;
        }
    }
}
