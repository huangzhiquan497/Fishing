using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

/// <summary>
/// hotFile的延迟写入类
/// </summary>
/// <remarks>利用到gameobject的协同特性</remarks>
public class PersistentDataCoroWriter : MonoBehaviour
{
    public delegate void Event_WriteRequestPermission();
    private static PersistentDataCoroWriter mSingleton;


    struct TimeEvt
    {
        public float TimeStart;
        public Event_WriteRequestPermission Evt;
    }
    private Queue<TimeEvt> mQueueTimeEvt;

    private static void TryInitSingleton()
    {
        if (mSingleton == null)
        {
            GameObject go = new GameObject("PersistentDataCoroWriter");
            mSingleton = go.AddComponent<PersistentDataCoroWriter>();
            
        }
    }

    public static void CommitWriteRequest(Event_WriteRequestPermission evt)
    {
        TryInitSingleton();
   
        //mSingleton.StartCoroutine(mSingleton._Coro_WriteRequestProcessing(evt));
        TimeEvt te;
        te.TimeStart = Time.time;
        te.Evt = evt;
        mSingleton.mQueueTimeEvt.Enqueue(te);
     
    }

    //public IEnumerator _Coro_WriteRequestProcessing(Event_WriteRequestPermission evtWriteRequestPermission)
    //{
    //    yield return new WaitForSeconds(0.5F);
    //    evtWriteRequestPermission();
    //}


    void Awake()
    {
        mQueueTimeEvt = new Queue<TimeEvt>();
    }
#if UNITY_STANDALONE
    void Update()
    {
        TAG_UPDATE_AGAIN:
        if (mQueueTimeEvt.Count > 0)
        {
            if (Time.time - mQueueTimeEvt.Peek().TimeStart > 2F)
            {
                mQueueTimeEvt.Dequeue().Evt();
                goto TAG_UPDATE_AGAIN;
            }
        }
    }
#endif
    void OnApplicationPause(bool pause)
    {
        //Debug.Log("OnApplicationPause");
        SaveAllRequestImm();
    }

    void SaveAllRequestImm()
    {
        while (mQueueTimeEvt.Count > 0)
        {
            //Debug.Log("saved: mQueueTimeEvt.Count" + mQueueTimeEvt.Count);
            mQueueTimeEvt.Dequeue().Evt(); 
        }
    }
    void OnApplicationQuit()
    {
        //Debug.Log("OnApplicationQuit");
        SaveAllRequestImm();
    }

}
