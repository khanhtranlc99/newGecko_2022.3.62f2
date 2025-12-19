using System.Collections;

using UnityEngine;

using UnityEngine.SceneManagement;
using UnityEngine.UI;
 
public class ButtonLevel : MonoBehaviour
{
    public int idLevel;
    public GameObject blind;
    public Button btnClick;
    public Image bgThumb;
    public Sprite spriteOff;
    public Sprite spriteOn;
     
    public GameObject thumbComplete;
    public GameObject thumbNotReady;
    public bool wasComplete;
  
    public void Init(bool param)
    {  
        wasComplete = param; 
        if(wasComplete)
        {
            blind.gameObject.SetActive(false);
            bgThumb.sprite = spriteOn;
          
        }
        else
        {
            blind.gameObject.SetActive(true);
            bgThumb.sprite = spriteOff;
          
        }

        if(wasComplete)
        {
            thumbComplete.SetActive(true);
            thumbNotReady.SetActive(false);
        }   
        else
        {
            thumbNotReady.SetActive(true);
            thumbComplete.SetActive(false);
        }    
       
        btnClick.onClick.AddListener(HandleButtonOnClick);
    }    
    public void HandleButtonOnClick()
    {
        if(wasComplete)
        {
            GameController.Instance.musicManager.PlayClickSound();
            UseProfile.LevelEggChest = idLevel;
            Initiate.Fade(SceneName.GAME_PLAY, Color.black, 2f);
        }    
     
    }
    IEnumerator ChangeScene()
    {
        UseProfile.LevelEggChest = this.idLevel;
   
       
        string name = "";
    
        name = SceneName.GAME_PLAY;
        var _asyncOperation = SceneManager.LoadSceneAsync(name, LoadSceneMode.Single);

        while (!_asyncOperation.isDone)
        {
        
            yield return null;


        }
    }
    
}
