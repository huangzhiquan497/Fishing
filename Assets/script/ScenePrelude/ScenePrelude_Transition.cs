using UnityEngine;
using System.Collections;
using System.Collections.Generic;
/// <summary>
/// 鱼群平移序幕
/// </summary>
/// <remarks>
///  规则:
///    1.移动方向是transform.right
///    2.原点需要在鱼群最后,但transform.position超过
/// </remarks>
public class ScenePrelude_Transition : ScenePrelude
{
    [System.Serializable]
    public class EmitData
    {
        public Transform TsShoalOfFish;
        public FishGenerateWhenEnterWorld LastFishEnterWorld;
        public Transform[] TsPosStart;//开始位置(维数是屏幕数量)//todo:可以使用gamemain.worldDimemsion推算出
    }
    public EmitData[] EmitDatas;

    //public Transform[] TsShoalOfFish;
    public float Speed = 1F;
    //public FishGenerateWhenEnterWorld[] LastFishEnterWorld;//最后进入世界的鱼(必须与TsShoalOfFish一一对应)
    public bool IsDepthAdvanceAuto = true;//自动深度递增
    private bool mIsEnded = false;

    public override void Go() 
    {
        StartCoroutine(_Coro_Transiting());
        StartCoroutine(_Coro_WaitNullFish());
    }

    public IEnumerator _Coro_Transiting()
    {
        //设置开始位置
        for (int i = 0; i != EmitDatas.Length; ++i)
        {
            EmitDatas[i].TsShoalOfFish.localPosition = EmitDatas[i].TsPosStart[GameMain.Singleton.ScreenNumUsing - 1].localPosition;
        }

        if (IsDepthAdvanceAuto)
        {
            for (int i = 0; i != EmitDatas.Length; ++i)
            {
                float depth = EmitDatas[i].TsShoalOfFish.GetChild(0).localPosition.z;
                Vector3 posTmp;
                foreach (Transform t in EmitDatas[i].TsShoalOfFish)
                {
                    posTmp = t.localPosition;
                    posTmp.z = depth;
                    depth -= Defines.OffsetAdv_FishGlobleDepth;
                    t.localPosition = posTmp;
                    
                }

            }
        }
        //开始移动
        Transform ts;
        while (true)
        {
            int nullNum = 0;
            for (int i = 0; i != EmitDatas.Length; ++i)
            {
                if (EmitDatas[i].TsShoalOfFish == null)
                {
                    ++nullNum;
                    continue;
                }

                ts = EmitDatas[i].TsShoalOfFish;
                ts.position += ts.right * Speed * Time.deltaTime;
                if ((ts.right.x > 0F && ts.position.x > GameMain.Singleton.WorldDimension.xMax)//向左并达到左边屏幕边
                    || (ts.right.x <= 0F && ts.position.x < GameMain.Singleton.WorldDimension.x))//向右移动并达到右边屏幕边
                {
                    List<Fish> fishToClear = new List<Fish>();

                    foreach (Transform tChild in EmitDatas[i].TsShoalOfFish )
                    {
                        Fish f = tChild.GetComponent<Fish>();
                        if (f != null && f.Attackable)
                        {
                            fishToClear.Add(f);
                        }
                    }
                    foreach (Fish f in fishToClear)
                    {
                        f.Clear();
 
                    }

                    Destroy(EmitDatas[i].TsShoalOfFish.gameObject);
                    EmitDatas[i].TsShoalOfFish = null;
                }
            }



            if (nullNum == EmitDatas.Length)
            {
                EndPrelude();
            }

            yield return 0;
        }

    }

    
    public IEnumerator _Coro_WaitNullFish()
    {
        int numLastFishEnterWorld = 0;
        foreach (EmitData ed in EmitDatas)
        {
            ed.LastFishEnterWorld.EvtFishGenerated += (Fish f) =>
            {
                ++numLastFishEnterWorld;
                //Debug.Log("lastFishGenerated");
            };
        }

        while (numLastFishEnterWorld != EmitDatas.Length)
        {
            yield return 0;
        }
        //Debug.Log("waitZeroFish");

        while (GameMain.Singleton.NumFishAlive != 0)
        {
            yield return 0;
        }

        EndPrelude();
    }

    void EndPrelude()
    {
        if (!mIsEnded)
        {
            mIsEnded = true;
            if (Evt_PreludeEnd != null)
                Evt_PreludeEnd();
            GameMain.Singleton.FishGenerator.KillAllImmediate();//在Destroy(gameObject);之前删除所有在场鱼,防止漏删
            Destroy(gameObject);
        }
    }
}
