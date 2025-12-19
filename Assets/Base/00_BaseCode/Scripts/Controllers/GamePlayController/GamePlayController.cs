 using Crystal;
using DG.Tweening;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.SocialPlatforms.Impl;


public enum StateGame
{
    Loading = 0,
    Playing = 1,
    Win = 2,
    Lose = 3,
    Pause = 4
}

public class GamePlayController : Singleton<GamePlayController>
{
    public StateGame stateGame;
    public PlayerContain playerContain;
    public GameScene gameScene;
    public CameraController cameraController;
    
 
 
    
    protected override void OnAwake()
    {
        //  GameController.Instance.currentScene = SceneType.GamePlay;

     
        Init();

    }

    public void Init()
    {
      
      playerContain.Init();
      gameScene.Init();

    



    }
    [Button]
    public void next()
    {
        UseProfile.CurrentLevel += 1;
        Initiate.Fade(SceneName.GAME_PLAY, Color.black, 2f);
    }
    

}
