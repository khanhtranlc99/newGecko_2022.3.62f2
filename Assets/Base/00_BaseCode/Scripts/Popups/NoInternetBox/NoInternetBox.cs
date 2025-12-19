using UnityEngine;
using UnityEngine.UI;
using System;
public class NoInternetBox : BaseBox
{
    public static NoInternetBox instance;
    public static NoInternetBox Setup(Action callBack , bool isSaveBox = false, Action actionOpenBoxSave = null)
    {
        if (instance == null)
        {
            instance = Instantiate(Resources.Load<NoInternetBox>(PathPrefabs.NO_INTERNET_BOX));
            instance.Init(callBack);
        }

        instance.InitState();
        return instance;
    }
    public Action callBackBtn; 
    public Button btnOK;

    public void Init(Action actionParam)
    {
        callBackBtn = null;
        callBackBtn = actionParam;
        btnOK.onClick.AddListener(HandleOk);
    }
    public void InitState()
    {

    }

    public void HandleOk()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            GameController.Instance.moneyEffectController.SpawnEffectText_FlyUp_UI
                          (
                  btnOK.transform,
                          btnOK.transform.position,
                          "No internet!",
                          Color.white,
                          isSpawnItemPlayer: true
                          );


        }
        else
        {
            Close();
            callBackBtn?.Invoke();
        }    
    }    
}
