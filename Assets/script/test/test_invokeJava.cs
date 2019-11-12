using UnityEngine;
using System.Collections;

public class test_invokeJava : MonoBehaviour {

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

    void OnGUI()
    {
        if (GUI.Button(new Rect(0, 0, 150, 100), "InvDataUEvt"))
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.easygame.hpyfishm.CustomUnityPlayerActivity"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
                {

                    jo.Call("Unity_DatauInvokeEvent","TestEvt1");
                }
            } 
            
        }

        if (GUI.Button(new Rect(0, 100, 150, 100), "order"))
        {
            using (AndroidJavaClass jc = new AndroidJavaClass("com.easygame.hpyfishm.CustomUnityPlayerActivity"))
            {
                using (AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("payWrapperMM"))
                {
                    jo.Call("Unity_Order", 1, SystemInfo.deviceUniqueIdentifier);
                }
            } 
        }

    }
}
