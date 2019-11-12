using UnityEngine;
using System.Collections;
using wazServer;
public class mmpay : MonoBehaviour {
    public static string AndroidClassName = "com.eg.hpyfishm_ib.CustomUnityPlayerActivity";
    #if !UNITY_EDITOR && UNITY_ANDROID
    //static AndroidJavaClass mJCUnityPlayer;
    //static AndroidJavaObject mJOpayWrapperMM;
    #endif

    //void Awake()
    //{
    //    #if !UNITY_EDITOR && UNITY_ANDROID
    //    mJCUnityPlayer = new AndroidJavaClass(AndroidClassName);
    //    mJOpayWrapperMM = mJCUnityPlayer.GetStatic<AndroidJavaObject>("payWrapperMM");
    //    #endif
    //}

    public static void EvtLog(string evtStr)
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        //mJOpayWrapperMM.Call("Unity_DatauInvokeEvent", evtStr);
         using (AndroidJavaClass jc = new AndroidJavaClass(AndroidClassName))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
            {
                jo.Call("Unity_DatauInvokeEvent", evtStr);
            }
        }
#endif
        Debug.Log(evtStr);
    }
	// Use this for initialization
	void Start ()
    {
        //yield return new WaitForSeconds(0.1F);
        #if !UNITY_EDITOR && UNITY_ANDROID
        using (AndroidJavaClass jc = new AndroidJavaClass(AndroidClassName))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
            {
                jo.Call("Unity_Init");
            }
        }
        //mJOpayWrapperMM.Call("Unity_Init");
#endif
    }
    public static void Order(int i)
    {
        #if !UNITY_EDITOR && UNITY_ANDROID
        using (AndroidJavaClass jc = new AndroidJavaClass(AndroidClassName))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
            {
                jo.Call("Unity_Order", i,SystemInfo.deviceUniqueIdentifier.Substring(0, 10));
            }
        }
        //mJOpayWrapperMM.Call("Unity_Order", i,SystemInfo.deviceUniqueIdentifier.Substring(0, 10));
#endif
    }


    void Handler_PaySucceed(string info)
    {
        CS_PayNotify paydata;
        paydata.PayTypeIdx = shop.select;
        //Debug.Log("select="+shop.select);
        Moudle_scene.mP.Send(Moudle_scene.session, paydata);
        switch (shop.select)
        { 
            case 0:
                if (Moudle_FileRead.First_Recharge.Val == true)
                {
                    Moudle_FileRead.First_Recharge.Val=false;
                    MobileInterface.ChangePlayerScore(100 * 2);
                }
                else
                {
                    MobileInterface.ChangePlayerScore(100);
                }
                break;
            case 1:
                if (Moudle_FileRead.First_Recharge.Val == true)
                {
                    Moudle_FileRead.First_Recharge.Val = false;
                    MobileInterface.ChangePlayerScore(305*2);
                }
                else
                {
                    MobileInterface.ChangePlayerScore(305);
                }
                break;
            case 2:
                if (Moudle_FileRead.First_Recharge.Val == true)
                {
                    Moudle_FileRead.First_Recharge.Val = false;
                    MobileInterface.ChangePlayerScore(1100*2);
                }
                else
                {
                    MobileInterface.ChangePlayerScore(1100);
                }
                break;
        }
        //Debug.Log(info);
        Moudle_main.Singlton.go_Black.SetActive(false);
    }

    /// <summary>
    /// ecplicse回调代码,支付失败
    /// </summary>
    /// <param name="info"></param>
    void Handler_PayFailed(string info)
    {
        //Debug.Log(info);
        Moudle_main.Singlton.go_Black.SetActive(false);
    }
}
