using UnityEngine;
using System.Collections;
using wazServer;
public class wiipay : MonoBehaviour
{
    public static void EvtLog(string evtStr)
    {
        Debug.Log(evtStr);
    }
 
    public static void Order(int i)
    {
        //Debug.Log("Order"+i);
        #if !UNITY_EDITOR && UNITY_ANDROID
        using (AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity"))
            {
                string payCode = "000" + (i+1);//调用传入，默认是0.wii开始时1
                jo.Call("Pay", payCode, "1cb728357d03ba1a958e78c49212670d", "100205", "");
            }
        }
#endif

        Moudle_main.Singlton.go_Black.SetActive(false);
    }

    void resultMessgae(string str)
    {
        Debug.Log("void resultMessgae(string str) str:" + str);
        if (str.Length >= 7 && str.Substring(0, 7) == "success")
        {
            Handler_PaySucceed(str);
        }
        else
        {
            Moudle_main.Singlton.go_Black.SetActive(false);
        }
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
                MobileInterface.ChangePlayerScore(100);
                break;
            case 1:
                MobileInterface.ChangePlayerScore(305);
                break;
            case 2:
                MobileInterface.ChangePlayerScore(1100);
                break;
        }
        //Debug.Log(info);
        Moudle_main.Singlton.go_Black.SetActive(false);
    }
 
}
