using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class GameScene : BaseScene
{
    public List<Image> imgHeart;
    public Sprite activeHeart;
    public Sprite unactiveHeart;
    public TMP_Text txtLevel;
   
    public void Init(   )
    {
        txtLevel.text = "Level " + UseProfile.CurrentLevel;
        for(int i = 0; i < imgHeart.Count; i++)
        {
            imgHeart[i].sprite = activeHeart;
        }
    }

    public void SubNumMove()
    {
        //GamePlayController.Instance.playerContain.levelController.levelData.numMove--;
        //txtNumMove.text = GamePlayController.Instance.playerContain.levelController.levelData.numMove.ToString();
        //if (GamePlayController.Instance.playerContain.levelController.levelData.numMove <= 0)
        //{ 
        //       Debug.LogError("numMove----- " + GamePlayController.Instance.playerContain.levelController.levelData.numMove);
        //     if(GamePlayController.Instance.playerContain.levelController.levelData.lsWormsInGame.Count > 0)
        //       {
        //           if(GamePlayController.Instance.stateGame == StateGame.Playing)
        //           {
        //                GamePlayController.Instance.stateGame = StateGame.Lose;
        //                 LoseBox.Setup().Show();
        //                 Debug.LogError("Lose");
        //                 return;
        //           }     
        //       }

        //}
        GamePlayController.Instance.playerContain.levelController.levelData.numMove--;
        imgHeart[GamePlayController.Instance.playerContain.levelController.levelData.numMove].sprite = unactiveHeart;
        if (GamePlayController.Instance.playerContain.levelController.levelData.numMove <= 0)
        {
            if (GamePlayController.Instance.playerContain.levelController.levelData.lsWormsInGame.Count > 0)
            {
                if (GamePlayController.Instance.stateGame == StateGame.Playing)
                {
                    GamePlayController.Instance.playerContain.isPopupUp = true;
                    GamePlayController.Instance.stateGame = StateGame.Lose;
                    GameController.Instance.musicManager.PlayLoseSound();
                    LoseBox.Setup().Show();
                    Debug.LogError("Lose");
                    return;
                }
            }

        }



    }
    

    public void PlusNumMove(int num)
    {
        GamePlayController.Instance.playerContain.levelController.levelData.numMove += num;
        //txtNumMove.text = GamePlayController.Instance.playerContain.levelController.levelData.numMove.ToString();
    }

    /// <summary>
    /// Khôi phục full 3 mạng (sau khi xem quảng cáo revive).
    /// </summary>
    public void RestoreFullHearts()
    {
        var levelData = GamePlayController.Instance.playerContain.levelController.levelData;
        levelData.numMove = 3;
        for (int i = 0; i < imgHeart.Count; i++)
            imgHeart[i].sprite = activeHeart;
    }


    public void HandleCheckWin()
    {
        if(GamePlayController.Instance.playerContain.levelController.levelData.lsWormsInGame.Count <= 0 && GamePlayController.Instance.playerContain.levelController.levelData.numMove > 0 )
        {
            if(GamePlayController.Instance.stateGame == StateGame.Playing)
            {
                GamePlayController.Instance.stateGame = StateGame.Win;
                GameController.Instance.musicManager.PlayWinSound();
                Debug.LogError("Win");
                    Winbox.Setup().Show();
                
            }       
        }
    }


   
    public override void OnEscapeWhenStackBoxEmpty()
    {
     
    }
}
