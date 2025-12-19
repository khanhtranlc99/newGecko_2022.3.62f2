using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;

public class AppsFlyerManager : MonoBehaviour
{
    #region Instance
    private static AppsFlyerManager instance;
    public static AppsFlyerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<AppsFlyerManager>();
                if (!instance)
                {
                    instance = Instantiate(Resources.Load<AppsFlyerManager>("AppsFlyerManager"));
                }
            }
            return instance;
        }
    }

    public static bool Exist => instance != null;

    #endregion

    public void SendEvent(string eventName, Dictionary<string, string> eventValues)
    {    
        AppsFlyer.sendEvent(eventName, eventValues);
    }


    public void LogLevelAchieved(int level)
    {
        // Dictionary chứa parameter bạn muốn gửi kèm
        var eventValues = new Dictionary<string, string>()
    {
        { "level", level.ToString() } // thêm param level
    };

        // Gửi event af_level_achieved (AppsFlyer chuẩn định nghĩa)
        AppsFlyer.sendEvent("af_level_achieved", eventValues);
 
    }


}

public class AFEvents
{
    public const string AF_INTERS_LOGICGAME = "af_inters_logicgame";
    public const string AF_INTERS_SUCCESSFULLYLOADED = "af_inters_successfullyloaded";
    public const string AF_INTERS_DISPLAYED = "af_inters_displayed";

    public const string AF_REWARDED_LOGICGAME = "af_rewarded_logicgame";
    public const string AF_REWARDED_SUCCESSFULLYLOADED = "af_rewarded_successfullyloaded";
    public const string AF_REWARDED_DISPLAYED = "af_rewarded_displayed";


    public const string AF_BANNER_DISPLAYED = "af_banner_displayed";
    public const string AF_APPOPEN_DISPLAYED = "af_appopen_displayed";
    public const string AF_MREC_DISPLAYED = "af_mrec_displayed";
    public const string AF_NATIVE_DISPLAYED = "af_native_displayed";

    public const string AF_LEVEL_ACHIEVED = "af_level_achieved";


}
