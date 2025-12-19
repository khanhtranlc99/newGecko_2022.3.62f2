using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSceneController : SceneBase
{
    public Button btnHome;
    public Text tvLevel;
    public Text tvLevelBtn;
    public RandomWatchVideo btnReward;
 
    public override void Init()
    {
        tvLevel.text =   UseProfile.CurrentLevel.ToString();
        tvLevelBtn.text = "Level " + UseProfile.CurrentLevel;
        btnHome.onClick.AddListener(delegate { OnClickPlay(); });
        btnReward.Init();
    }



    private void OnClickPlay()
    {
 
    }
}
