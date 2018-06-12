using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;
using UniRx.Triggers;

public class TitleSceneDirector : MonoBehaviour
{
    [SerializeField]
    private AlphaFade _logoImage;

    [SerializeField]
    private GameObject _title;

    [SerializeField]
    private MKSceneManager _sceneLoder;

    // Use this for initialization
    void Start()
    {
        _title.SetActive(false);
        Observable.FromCoroutine(SceneControl).Subscribe();
    }

    IEnumerator SceneControl()
    {
        // タイトルロゴのアニメーション完了を待ち合わせ
        yield return _logoImage.FadeAsObservable.ToYieldInstruction();

        _title.SetActive(true);

        // 画面タッチを待ち合わせ
        yield return this.UpdateAsObservable().Where(_ => Input.GetMouseButton(0))
            .First().ToYieldInstruction();

        // MainSceneに遷移
        _sceneLoder.SceneLoad("MainScene");
    }
}
