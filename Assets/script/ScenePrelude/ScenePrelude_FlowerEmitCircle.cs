using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ScenePrelude_FlowerEmitCircle : ScenePrelude {
    [System.Serializable]
    public class FlowerEmitData
    {
        public Fish PrefabFish;
        public int NumLine = 5;//发射行数
        public float SwimSpd = 0.16F;//游动速度
        //public float EmitInterval = 0.1F;//发射间隔
        public float EmitCooldown = 0.5F;//发射冷却
    }
    //public tk2dAnimatedSprite AniSpr_YunMu;
    public FlowerEmitData[] FlowerEmitDatas;//三种鱼随机一种出
    public float TimeLimit = 32F;//时间限制

    public float OffsetYInitPos = -0.1F;//鱼圈初始化位置偏移
    private bool mIsEnded = false; 

 
    public override void Go()
    {
        //StartCoroutine(_Coro_Process());
        //StartCoroutine(_Coro_TimeCountdown());

        int numScreen = GameMain.Singleton.ScreenNumUsing;
        ScenePrelude_FlowerEmitCircle[] preludes = new ScenePrelude_FlowerEmitCircle[numScreen];
        preludes[0] = this;
        for (int i = 1; i != preludes.Length; ++i)
        {
            GameObject newPresudeInst = (Instantiate(gameObject) as GameObject);
            preludes[i] = newPresudeInst.GetComponent<ScenePrelude_FlowerEmitCircle>();
            preludes[i].transform.parent = transform.parent;
        }

        for (int i = 0; i != preludes.Length; ++i)
        {
            preludes[i].transform.localPosition =
                new Vector3(GameMain.Singleton.WorldDimension.x + GameMain.Singleton.WorldDimension.width / numScreen * (0.5F + i)
                , 0F,Defines.GMDepth_Fish);
            preludes[i]._Go();
        }

 
    }

    //内部调用,防止递归
    void _Go()
    {
        StartCoroutine(_Coro_Process());
        StartCoroutine(_Coro_TimeCountdown());
    }


    void EndPrelude()
    {
        if (!mIsEnded)
        {
            mIsEnded = true;
            if (Evt_PreludeEnd != null)
                Evt_PreludeEnd();
            GameMain.Singleton.FishGenerator.KillAllImmediate();
            Destroy(gameObject);
        }
    }

    IEnumerator _Coro_TimeCountdown()
    {
        yield return new WaitForSeconds(TimeLimit);
        EndPrelude();
    }


    IEnumerator _Coro_Process()
    { 
 

 
       //散花出鱼
        int flowEmitNum = FlowerEmitDatas.Length;
        int curflowEmitNum = 0;

        float baseRotateAngle = 90F; 
        
        float depth = 0F;
        while (curflowEmitNum < flowEmitNum)
        {
            FlowerEmitData emitData = FlowerEmitDatas[curflowEmitNum];
   
            //float emitElapse = 0F;
            float angleLineDelta = 360F / emitData.NumLine;
            //一群
            //while (emitElapse < emitData.EmitUseTime)
            {
                //6方向一起出
                for (int i = 0; i != emitData.NumLine; ++i)
                {
                    Fish f = Instantiate(emitData.PrefabFish) as Fish;
                    f.ClearAI();
                    f.transform.parent = transform;

                    f.transform.localRotation = Quaternion.AngleAxis(baseRotateAngle + angleLineDelta * i /*+ Random.Range(-15F, 15F)*/, Vector3.forward);
                    f.transform.localPosition = new Vector3(0F, 0F, depth) + f.transform.up * OffsetYInitPos;
                    depth += 0.001F;

                    f.swimmer.Speed = emitData.SwimSpd;
                    f.swimmer.Go();
                }

                //emitElapse += emitData.EmitInterval;
                //yield return new WaitForSeconds(emitData.EmitInterval);
                yield return new WaitForSeconds(emitData.EmitCooldown);
            }

            baseRotateAngle += 45F;
            ++curflowEmitNum;
            yield return new WaitForSeconds(1F);
        }

 
        //等待清鱼,[过期]改用时间作限制
        while (GameMain.Singleton.NumFishAlive != 0)
        {
            yield return 0;
        }
        EndPrelude();
        
    }
      
}
