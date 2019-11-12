using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class ScenePrelude_UpDownFlow : ScenePrelude {
    [System.Serializable]
    public class GenerateData
    {
        public Fish Prefab_Fish;
        public float Delay = 0F;//开始生成延迟
        public float Elapse = 2F;// 持续时间
        public float IntervalGenerate = 0.2F;//生成间隔
        public bool IsUp = true;
    }

    public GenerateData[] GenData;
    public float TimeLimit = 60;
    public float FishMoveSpd = 0.4F;
    public float SpaceToLeftRight = 0.5F;
    //public GenerateData[] Down;

    private static List<float> LstRndLineX;
    private Stack<float> mRndLineX;
    
    //private FishGenerator mFishGenerator;
    private bool mIsEnded = false;


    private static int NumRndLineOneScreen = 10;//一个屏幕生成随机线数目
    private static Quaternion TowardUpRotation;
    private static Quaternion TowardDownRotation;
    private static int NumScreen;
    private static float RndOffsetX = 0.1F;//在随机线的基础上再偏移一个随机位置:+/-RndOffsetX
    
    IEnumerator _Coro_TimeCountdown()
    {
        yield return new WaitForSeconds(TimeLimit);
        EndPrelude();
    }
    IEnumerator _Coro_WaitFishAllDie()
    {
        while (GameMain.Singleton.NumFishAlive <= 2)
        {
            yield return 0;

        }

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
            GameMain.Singleton.FishGenerator.KillAllImmediate();
            Destroy(gameObject);
        }
    }



    void Awake()
    {
        //mFishGenerator = GameMain.Singleton.FishGenerator;
        TowardUpRotation = Quaternion.AngleAxis(90F, Vector3.forward);
        TowardDownRotation = Quaternion.AngleAxis(-90F, Vector3.forward);
        NumScreen =   GameMain.Singleton.ScreenNumUsing;

        LstRndLineX = new List<float>();
        mRndLineX = new Stack<float>();

        float space = (GameMain.Singleton.WorldDimension.width - SpaceToLeftRight * 2F) / (NumRndLineOneScreen * NumScreen);
        RndOffsetX = space * 0.5F;

        for (int i = 0; i != NumScreen * NumRndLineOneScreen; ++i)
        {
            LstRndLineX.Add(GameMain.Singleton.WorldDimension.x + SpaceToLeftRight + space * (0.5F + i));
        }

        LstRndLineX.Sort((float a, float b) => { return Random.Range(0, 3) - 1; });

        foreach (float val in LstRndLineX)
        {
            mRndLineX.Push(val);
        }
 
    }
    float GetRndLineX()
    {
        if (mRndLineX.Count == 0)
        {
            LstRndLineX.Sort((float a, float b) => { return Random.Range(0, 3) - 1; });

            foreach (float val in LstRndLineX)
            {
                mRndLineX.Push(val);
            }
        }

        return mRndLineX.Pop() + Random.Range(-RndOffsetX, RndOffsetX);
    }
    
    public override void Go()
    {

        //_Coro_GenerateDelay()
        foreach(GenerateData gd in GenData)
        {
            StartCoroutine(_Coro_GenerateDelay(gd));
        }

        StartCoroutine(_Coro_TimeCountdown());
        StartCoroutine(_Coro_WaitFishAllDie());
    }

    private float mFishDepthOffset = 0F;
    IEnumerator _Coro_GenerateDelay(GenerateData gd)
    { 
        yield return new WaitForSeconds(gd.Delay);
        //
        Rect worldDim = GameMain.Singleton.WorldDimension;
        float elapse = 0F; 
        while (elapse < gd.Elapse)
        {
 
            Fish f = Instantiate(gd.Prefab_Fish) as Fish;
            Swimmer s = f.swimmer;
            f.transform.localPosition = new Vector3(GetRndLineX()
                ,gd.IsUp? (worldDim.yMax + s.BoundCircleRadius) :(worldDim.yMin - s.BoundCircleRadius)
                , Defines.GMDepth_Fish + mFishDepthOffset);
            mFishDepthOffset -= 0.025F;
            if (mFishDepthOffset < -100F)
                mFishDepthOffset = 0F;
            f.transform.localRotation = gd.IsUp ? TowardDownRotation : TowardUpRotation;
            f.ClearAI();
            f.swimmer.Speed = FishMoveSpd;
            f.swimmer.Go();
            
            elapse += gd.IntervalGenerate / NumScreen;
            yield return new WaitForSeconds(gd.IntervalGenerate / NumScreen);
        }
    }

}
