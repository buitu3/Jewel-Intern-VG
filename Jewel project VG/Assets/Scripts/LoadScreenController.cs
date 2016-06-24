using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class LoadScreenController : MonoBehaviour {

    public Text loadText;

	// Use this for initialization
	void Start ()
    {       
        loadText.DOFade(0f, 0.7f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

        if (LevelsManager.Instance == null)
        {
            StartCoroutine(loadSelectLevelScene());
        }
        else
        {
            StartCoroutine(loadMainGameScene());
        }
    }
	
    IEnumerator loadMainGameScene()
    {
        yield return new WaitForSeconds(1f);

        AsyncOperation async = SceneManager.LoadSceneAsync("Main Game Scene");

        while (!async.isDone)
        {
            yield return null;
        }
    }

    IEnumerator loadSelectLevelScene()
    {
        yield return new WaitForSeconds(1f);

        AsyncOperation async = SceneManager.LoadSceneAsync("Select Level Scene");

        while (!async.isDone)
        {
            yield return null;
        }
    }
}
