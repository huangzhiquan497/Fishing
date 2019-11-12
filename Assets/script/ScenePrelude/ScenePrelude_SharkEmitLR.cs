using UnityEngine;
using System.Collections;

public class ScenePrelude_SharkEmitLR : ScenePrelude
{
    public Fish Prefab_LaJiao;
    public Fish[] Prefab_Sharks;

    public float IntervalGenerateSharkMin = 0.5F;//生成鲨鱼间隔 最少
    public float IntervalGenerateSharkMax = 1.5F;//生成鲨鱼间隔 最大
    public float AngleGenerateShark = 20F;//生成鲨鱼角度
    public float SpeedShark = 0.2F;//鲨鱼移动速度
    public float TimeEmitShark = 10F;//发射鲨鱼时间

    public float EmitLaJiaoDelay = 1F;//出辣椒鱼延迟
    public float IntervalEmitLaJiao = 1F;//出辣椒鱼延迟
    public float RoadToLeftRightSpace = 0.1F;//辣椒鱼与左右间距
    public float SpeedLaJiao = 0.3F;//辣椒移动速度
    public float TimeLimit = 54F;//时间限制

    //private float mMaxSharkRadius = 0F;
    private bool mIsEnded = false;
    private float mDepthOffset;
    public override void Go()
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
        yield return 0;
        StartCoroutine("_Coro_LaJiaoGenerating");
//         //求出最大鲨鱼半径
//         foreach (Fish f in Prefab_Sharks)
//         {
//             if (f.swimmer.BoundCircleRadius > mMaxSharkRadius)
//                 mMaxSharkRadius = f.swimmer.BoundCircleRadius;
//         }

        //随机左右边
        bool isLeft = Random.Range(0, 2) == 0;
        

        float elapse = 0F;
        int emitSharkIdx = 0;
        while (elapse < TimeEmitShark)
        {

           
            Fish shark = Instantiate(Prefab_Sharks[emitSharkIdx]) as Fish;
            Vector3 generatorPot = new Vector3(isLeft ? GameMain.Singleton.WorldDimension.x - shark.swimmer.BoundCircleRadius
                     : GameMain.Singleton.WorldDimension.xMax + shark.swimmer.BoundCircleRadius
                 , 0F, 0F);

            emitSharkIdx = (emitSharkIdx + 1) % Prefab_Sharks.Length;
            shark.ClearAI();
            shark.swimmer.Speed = SpeedShark;

            shark.transform.parent = transform;
            shark.transform.rotation =  Quaternion.AngleAxis((isLeft?0F:180F)+Random.Range(-AngleGenerateShark,AngleGenerateShark),Vector3.forward);
            generatorPot.z = Defines.GlobleDepth_FishBase + mDepthOffset;
            mDepthOffset -= Defines.OffsetAdv_FishGlobleDepth;
            shark.transform.localPosition = generatorPot;
            
            shark.swimmer.Go();

            float delta = Random.Range(IntervalGenerateSharkMin, IntervalGenerateSharkMax);
            elapse += delta;
            yield return new WaitForSeconds(delta);
        }

        float swimNeedTime = GameMain.Singleton.WorldDimension.width / SpeedShark;//todo 不准确
        yield return new WaitForSeconds(swimNeedTime);

        StopCoroutine("_Coro_LaJiaoGenerating");
        StartCoroutine(_Coro_WaitNullFish());
        ////等待清鱼,改用时间限制
        //while (GameMain.Singleton.NumFishAlive != 0)
        //{
        //    yield return 0;
        //}

        //EndPrelude();
    }
    
    IEnumerator _Coro_LaJiaoGenerating()
    {
        yield return new WaitForSeconds(EmitLaJiaoDelay);
        Vector2 leftRightLimit = new Vector2(GameMain.Singleton.WorldDimension.x + RoadToLeftRightSpace, GameMain.Singleton.WorldDimension.xMax - RoadToLeftRightSpace);
  
        while (true)
        {
            Fish[] fish = new Fish[2] { Instantiate(Prefab_LaJiao) as Fish, Instantiate(Prefab_LaJiao) as Fish };

            for (int i = 0; i != 2; ++i)
            { 
                fish[i].ClearAI();
                fish[i].swimmer.Speed = SpeedLaJiao;
                fish[i].transform.parent = transform;
                fish[i].transform.position = new Vector3(Random.Range(leftRightLimit.x, leftRightLimit.y)
                    , i == 0 ? (GameMain.Singleton.WorldDimension.yMax + fish[i].swimmer.BoundCircleRadius) : (GameMain.Singleton.WorldDimension.y - fish[i].swimmer.BoundCircleRadius), Defines.GlobleDepth_FishBase + mDepthOffset);
                mDepthOffset -= Defines.OffsetAdv_FishGlobleDepth;
                fish[i].transform.right = Vector3.up * (i == 0 ? -1F : 1F);//上面
                fish[i].swimmer.Go();
 
 
            }

            yield return new WaitForSeconds(IntervalEmitLaJiao);
        }
    }

    public IEnumerator _Coro_WaitNullFish()
    {
        while (GameMain.Singleton.NumFishAlive != 0)
        {
            yield return 0;
        }

        EndPrelude();
    }
}
