using UnityEngine;
using System.Collections;

public class Tester_DatasOverload : MonoBehaviour
{
    private int mDataBackLen = 1;//[测试]请求返回数据长度
    private int mDataBackInterval = 10;//[测试]请求返回数据间隔
    private bool mDataBackOnOff = false;//[测试]数据当前是否在返回中
    private bool mFlashLighting = false;

    IEnumerator _Coro_FlashAllPlayerLight()
    {
        //while (true)
        //{
        //    for (int i = 0; i != Defines.MaxNumPlayer; ++i )
        //    {
        //        GameMain.Singleton.ArcIO.FlashButtomLight(i);
        //    }
        //    yield return 0;
        //}
        yield break;
    }


    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            if (!mDataBackOnOff)
            {
                if (GUILayout.Button("返回大量数据[开始]"))
                {
                    //GameMain.Singleton.ArcIO.RequestDataBack((byte)mDataBackLen, (byte)mDataBackInterval, true);
                    mDataBackOnOff = true;
                }
            }
            else
            {
                if (GUILayout.Button("返回大量数据[停止]"))
                {
                    //GameMain.Singleton.ArcIO.RequestDataBack((byte)mDataBackLen, (byte)mDataBackInterval, false);
                    mDataBackOnOff = false;
                }
            }

            GUILayout.Label("长度(byte)");
            GUILayout_IntArea(ref mDataBackLen);
            GUILayout.Label("间隔(ms)");
            GUILayout_IntArea(ref mDataBackInterval);
        }
        GUILayout.EndHorizontal();


        if (mFlashLighting ? GUILayout.Button("频繁发送闪灯命令[关闭]") : GUILayout.Button("频繁发送闪灯命令[开始]"))
        {
            if (mFlashLighting)
            {
                StopCoroutine("_Coro_FlashAllPlayerLight");
                mFlashLighting = false;
            }
            else
            {
                StartCoroutine("_Coro_FlashAllPlayerLight");
                mFlashLighting = true;
            }
        }
    }

    private bool GUILayout_IntArea(ref int val)
    {
        int oldVal = val;
        string valStr = GUILayout.TextArea(val.ToString(), GUILayout.MinWidth(50F));
        int newVal = int.Parse(valStr);
        if (newVal != oldVal)
        {
            val = newVal;
            return true;
        }
        return false;

    }
}
