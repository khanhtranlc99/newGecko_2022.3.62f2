using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
public class HomeController : Singleton<HomeController>
{
    public HomeScene homeScene;
    public List<ButtonLevel> lsBtnLevel;

    protected override void OnAwake()
    {
      //  GameController.Instance.currentScene = SceneType.MainHome;

    }

    private void Start()
    {
        var temp = JsonConvert.DeserializeObject<List<int>>(UseProfile.ListSave);
        //foreach(var item in temp)
        //{
        //    Debug.Log(item);
        //}

        //foreach(var item in lsBtnLevel)
        //{
          
        //    if(temp.Contains(item.idLevel))
        //    {
        //        item.Init(true);
        //        Debug.Log("1111" + item.idLevel);
        //    }    
        //    else
        //    {
        //        item.Init(false);
        //    }    
          
        //}
        homeScene.Init();
    }

}
