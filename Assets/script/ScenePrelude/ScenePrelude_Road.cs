using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ScenePrelude_Road : ScenePrelude
{
    public Fish Prefab_FishCompositeRoad;//80 条左右

    public Transform[] TsLineFish;
    
    public float Speed = 1F;

    public float RoadToLeftRightSpace = 0.1F;//辣椒到左右间距
    public float RoadFishStartOffsetY = 0.2F;//开始位置
    public float RoadFishSwimDistance = 1F;//辣椒鱼开始游的距离
    public float RoadFishSwimDistanceMax = 1.02F;//辣椒鱼开始游的距离
    public float TimeLimit = 60F;//时间限制

    private bool mIsEnded = false;

    public override void Go()
    {
        Vector3 pos = transform.position;
        pos.z = Defines.GlobleDepth_FishBase;
        transform.position = pos;

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

    public IEnumerator _Coro_GenerateRoadSideFish(Fish[,] fishUpDown, int oneSideNum)
    {
        ////生成位置
        yield return 0;
        int generateNum = 0;
        Vector2 leftRightLimit = new Vector2(GameMain.Singleton.WorldDimension.x + RoadToLeftRightSpace, GameMain.Singleton.WorldDimension.xMax - RoadToLeftRightSpace);
        float fishStopTime = 0F;
        //FishGenerator fGen = GameMain.Singleton.FishGenerator;
        float depth = 0F;
        while (generateNum < oneSideNum)
        {
            Fish[] fish = new Fish[2] { Instantiate(Prefab_FishCompositeRoad) as Fish, Instantiate(Prefab_FishCompositeRoad) as Fish };

            for (int i = 0; i != 2; ++i)
            {
                fishUpDown[i, generateNum] = fish[i];
                fish[i].swimmer.SetLiveDimension(10F);
                fish[i].ClearAI();

                fish[i].transform.parent = transform;
                fish[i].transform.localPosition = new Vector3(Random.Range(leftRightLimit.x, leftRightLimit.y)
                    , i == 0 ? (GameMain.Singleton.WorldDimension.yMax + RoadFishStartOffsetY) : (GameMain.Singleton.WorldDimension.y - RoadFishStartOffsetY), depth);
                fish[i].transform.right = Vector3.up * (i == 0 ? -1F : 1F);//上面
           
                fish[i].swimmer.Go();

                fishStopTime = Random.Range(RoadFishSwimDistance,RoadFishSwimDistanceMax) / fish[i].swimmer.Speed;


                StartCoroutine(_Coro_FishStopAfter(fish[i].swimmer, fishStopTime));

            }
            depth -= Defines.OffsetAdv_FishGlobleDepth;
            ++generateNum;
            yield return new WaitForSeconds(Random.Range(0F, 0.05F));
        }
    }
    //public 
    public IEnumerator _Coro_Process()
    {

        //辣椒鱼往中间走
        int oneSideNum = 160;
        Fish[,] fishUpDown = new Fish[2,oneSideNum];
        StartCoroutine(_Coro_GenerateRoadSideFish(fishUpDown, oneSideNum));
        //yield return new WaitForSeconds(fishStopTime + 0.1F);

        //中间开始过
        ////清ai,设碰撞框
        //for (int i = 0; i != TsLineFish.Length; ++i )
        //{
        //    Fish[] fs = TsLineFish[i].GetComponentsInChildren<Fish>();
        //    for (int j = 0; j != fs.Length; ++j)
        //    {
        //        if (fs[j] != null)
        //        {
        //            //fs[j].SetLiveDimension(5000F);
        //            fs[j].ClearAI();
        //        }

        //    }
        //}

        Vector3[] posLineFish = new Vector3[2] { TsLineFish[0].position, TsLineFish[1].position };
        posLineFish[0].x -= GameMain.Singleton.WorldDimension.width * 0.5F;
        posLineFish[1].x += GameMain.Singleton.WorldDimension.width * 0.5F;
        for (int i = 0; i != 2; ++i)
            TsLineFish[i].position = posLineFish[i];

        Transform ts;
        while (true)
        {
           
            int nullNum = 0;
            for (int i = 0; i != TsLineFish.Length; ++i)
            {
                if (TsLineFish[i] == null)
                {
                    ++nullNum;
                    continue;
                }

                ts = TsLineFish[i];
                ts.position += ts.right * Speed * Time.deltaTime;
                if ((ts.right.x > 0F && ts.position.x > GameMain.Singleton.WorldDimension.xMax)//向左并达到左边屏幕边
                    || (ts.right.x <= 0F && ts.position.x < GameMain.Singleton.WorldDimension.x))//向右移动并达到右边屏幕边
                {
                    List<Fish> fishToClear = new List<Fish>();

                    foreach (Transform tChild in TsLineFish[i])
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

                    Destroy(TsLineFish[i].gameObject);
                    TsLineFish[i] = null;
                }
            }

            if (nullNum == TsLineFish.Length)
                break;

            yield return 0;
        }

        //两边路散走
        ////设置碰撞框
 
        //float fishSpeedHalf = Prefab_FishCompositeRoad.swimmer.Speed * 0.5F;
        Swimmer swimmerTmp = null;
        for (int i = 0; i != 2; ++i)
        {
            for (int j = 0; j != oneSideNum; ++j)
            {

                if (fishUpDown[i, j] == null)
                    continue;
                swimmerTmp = fishUpDown[i, j].swimmer;
                swimmerTmp.SetLiveDimension(Defines.ClearFishRadius);
                swimmerTmp.Speed = Random.Range(swimmerTmp.Speed * 0.9F, swimmerTmp.Speed * 1.5F);
                swimmerTmp.Go();
            }
        }



        //等待清鱼,改用时间限制
        while (GameMain.Singleton.NumFishAlive != 0)
        {
            yield return 0;
        }

        EndPrelude();
    }

   
    IEnumerator _Coro_FishStopAfter(Swimmer f,float time)
    {
        yield return new WaitForSeconds(time);
        if(f != null)
            f.StopImm();
    }
    //void OnDrawGizmos()
    //{
    //    //Gizmos.DrawIcon(transform.position, "Light.tiff",true);
    //}
}
