using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class StartLoading : MonoBehaviour
{
    public Text txtLoading;
    public Image progressBar;
    private string sceneName;
    public int countSecond;
    Coroutine coroutineLoad;
    public bool wasCoolDown;

    public void Init()
    {
        wasCoolDown = true;
        progressBar.fillAmount = 0f;
        countSecond = 0;
        coroutineLoad = StartCoroutine(LoadAdsToChangeScene());
        StartCoroutine(LoadingText());
    }

    // Use this for initialization
    IEnumerator ChangeScene()
    {
        yield return new WaitForSeconds(1);
        progressBar.fillAmount = 0f;
        string name = "";
        if (UseProfile.LevelEggChest == 1)
        {
            name = SceneName.GAME_PLAY;
        }
        else
        {
            name = SceneName.HOME_SCENE;
        }
        name = SceneName.HOME_SCENE;
        var _asyncOperation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);
       
        while (!_asyncOperation.isDone)
        {
            progressBar.fillAmount = Mathf.Clamp01(_asyncOperation.progress / 0.9f);
            yield return null;

        
        }
    }

    private IEnumerator LoadAdsToChangeScene()
    {
        yield return new WaitForSeconds(2f);
        if (UseProfile.CurrentLevel == 1)
        {
            name = SceneName.GAME_PLAY;
        }
        else
        {
            name = SceneName.HOME_SCENE;
        }
        Initiate.Fade(name, Color.black, 2f);

    }


    IEnumerator LoadingText()
    {
        var wait = new WaitForSeconds(1f);
        while (true)
        {
            txtLoading.text = "LOADING .";
            yield return wait;

            txtLoading.text = "LOADING ..";
            yield return wait;

            txtLoading.text = "LOADING ...";
            yield return wait;

        }
    }
}
