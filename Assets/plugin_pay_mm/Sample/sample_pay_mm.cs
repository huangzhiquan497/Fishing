using UnityEngine;
using System.Collections;

public class sample_pay_mm : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        using (AndroidJavaClass jc = new AndroidJavaClass("com.easygame.hpyfishm.CustomUnityPlayerActivity"))
        {
            using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
            {
                jo.Call("Unity_Init");
            }
        }
	}
	
	// Update is called once per frame
	void Update ()
    {
		if(Input.GetKeyDown(KeyCode.Escape)||Input.GetKeyDown(KeyCode.Home))
		{
			Application.Quit();
		}
	}
	void OnGUI()
    {
		if(GUILayout.Button("Order",GUILayout.Height(100),GUILayout.Width(Screen.width)))
		{
            using (AndroidJavaClass jc = new AndroidJavaClass("com.easygame.hpyfishm.CustomUnityPlayerActivity"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
                {
                    jo.Call("Unity_Order",1);
                }
            }
        }
	}
    /// <summary>
    /// ecplicse回调代码,支付成功
    /// </summary>
    /// <param name="info"></param>
    void Handler_PaySucceed(string info)
    {
        Debug.Log(info);
    }

    /// <summary>
    /// ecplicse回调代码,支付失败
    /// </summary>
    /// <param name="info"></param>
    void Handler_PayFailed(string info)
    {
        Debug.Log(info);
    }
}
