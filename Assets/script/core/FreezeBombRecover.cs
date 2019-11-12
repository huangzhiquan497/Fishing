using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 定时停止恢复
/// </summary>
/// <remarks>几种情况需要处理恢复问题:
/// 1.恢复的时间点在正常游戏时间内.
/// 2.恢复的时间点在过场(sweep)中,:恢复出鱼,因为会有子弹不能消除BUG
/// 4.开始的时间点在过场中,等待子弹消失过程中->再有子弹击中定时炸弹 : 直接恢复,原因同上
/// </remarks>
public class FreezeBombRecover : MonoBehaviour {

    void Awake()
    {
        StartCoroutine(_Coro_DelayRecover());
        //if (GameMain.State_ == GameMain.State.Normal)
        //{
        //    StartCoroutine(_Coro_DelayRecover());
        //}
        //else //不在普通游戏中开始,情况4(见类说明)
        //{
        //    Recover();

        //    Destroy(gameObject);
        //} 
    } 
    //void Handle_PrepareChangeScene()
    //{ 
    //    StopAllCoroutines();
    //    Recover();
    //    Destroy(gameObject);
    //}

    /// <summary>
    /// 定身炸弹恢复出鱼和鱼的移动
    /// </summary>
    /// <returns></returns>
    IEnumerator _Coro_DelayRecover()
    {

        yield return new WaitForSeconds(10F); 

        Recover();
        
        Destroy(gameObject);
    }

    //恢复出鱼,和鱼的移动
    void Recover()
    {
        //GameMain gm = GameMain.Singleton;
        GameMain.IsMainProcessPause = false;
        if (GameMain.EvtFreezeBombDeactive != null)
            GameMain.EvtFreezeBombDeactive();
        
    }
}
