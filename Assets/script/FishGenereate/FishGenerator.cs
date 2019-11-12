using UnityEngine;
using System.Collections;
using System.Collections.Generic;
//using DictFish = System.Collections.Generic.Dictionary<int,Fish>;
/// <summary>
/// 
/// </summary>
/// <remarks>不要将go作为该go的child,因为清鱼时会清除</remarks>
public class FishGenerator : MonoBehaviour
{
    
    struct Segment
    {
        public Segment(Vector2 p1, Vector2 p2)
        {
            P1 = p1;
            P2 = p2;
        }
        public Vector2 P1;
        public Vector2 P2;
        public Vector2 Direct()
        {
            return P2-P1;
        }
    }

    [System.Serializable]
    public class FishGenerateData
    {
        public Fish Prefab_Fish;
        public int Weight = 100;//权重
        [System.NonSerialized]
        public int HittedWeight;//命中权重
    }
    [System.Serializable]
    public class FishQueueGenerateData
    {
        public FishEmitter_Queue Prefab_Queue;
        public int Weight = 100;//权重
        [System.NonSerialized]
        public int HittedWeight;//命中权重
    }
    [System.Serializable]
    public class FloatRnd
    {
        public float Min;
        public float Max = 2F;
        public float Value
        {
            get { return Random.Range(Min, Max); }
        }
    }
    //变量
    public GameObjectSet FishSet;//鱼集合

    public Fish[] Prefab_FishAll
    {
        get
        {
            if (mFishAll == null)
            {

                mFishAll = new Fish[FishSet.GameObjects.Length];
                for (int i = 0; i != FishSet.GameObjects.Length; ++i)
                {
                    mFishAll[i] = FishSet.GameObjects[i].GetComponent<Fish>();
                }
            }
            return mFishAll;
        }
    }

    //public Fish[] Prefab_FishAll;
    public FishGenerateData[] FishGenerateDatas;
    public FishGenerateData[] FishGenerateUniqueDatas;
    public FishQueueGenerateData[] FishQueueData;
    public FishEmitter_Flock Prefab_FishEmitterFlock;
    //public FishBomb Prefab_FishBomb;

    public FloatRnd Interval_FishGenerate;
    public FloatRnd Interval_FishUniqueGenerate;
    public FloatRnd Interval_QueuGenerate;
    public FloatRnd Interval_FlockGenerate;

    private int WeightTotal_FishGenerate;
    private int WeightTotal_FishUniqueGenerate;
    private int WeightTotal_QueuGenerate;

    //public FloatRnd Interval_FishBomb;
    //最大鱼数限制
    public int MaxFishAtUnitWorld = 100;//单一屏幕出鱼数
    [System.NonSerialized]
    public int MaxFishAtWorld = 100;

    public bool IsGenerate = true;
    //private const float FishDepthAdditive = 1F;
    private float mCurrentFishDepth;
    private Rect mWorldDim;//来至于GameMain.WorldDimension;
    private Fish[] mFishAll;

    [System.NonSerialized]
    public Dictionary<int,Fish>[] FishTypeIndexMap;//Fish.TypeIndex为数组索引,为同类鱼服务

    public Dictionary<uint,Fish> FishLockable;//可被锁定鱼

    public Dictionary<int, Swimmer> LeadersAll;//所有领队(主要用于定身炸弹)

    public  Queue<Fish> mUniqueFishToGenerate;//唯一鱼生成队列
	public Dictionary<int, Fish> SameTypeBombs;//同类炸弹
    public void Handle_LeaderInstance(Swimmer s)
    {
        //Debug.Log(s.name + " instance",s.gameObject);
        //Debug.Log("LeadersAll.count = " + LeadersAll.Count);
        LeadersAll.Add(s.GetInstanceID(), s);
        s.EvtSwimOutLiveArea += () =>
        {
            LeadersAll.Remove(s.GetInstanceID()); 
        };
    } 
    public void Handle_InstanceFish(Fish f)
    {
        if (FishTypeIndexMap[f.TypeIndex] == null)
            FishTypeIndexMap[f.TypeIndex] = new Dictionary<int, Fish>();
        FishTypeIndexMap[f.TypeIndex].Add(f.GetInstanceID(), f);

        if (f.IsLockable)
            FishLockable.Add(f.ID, f);

        if (f.HittableTypeS == "SameTypeBomb")
        {
            SameTypeBombs.Add(f.GetInstanceID(), f);
        }
    }

    void Handle_ClearFish(Fish f)
    {
        if (FishTypeIndexMap[f.TypeIndex] != null)
        {
            FishTypeIndexMap[f.TypeIndex].Remove(f.GetInstanceID());
        }
        

        if (f.IsLockable)
            FishLockable.Remove(f.ID);

        if (f.HittableTypeS == "SameTypeBomb")
        {
            SameTypeBombs.Remove(f.GetInstanceID());
        }
    }
    /// <summary>
    /// 线段相交
    /// </summary>
    /// <param name="s1"></param>
    /// <param name="s2"></param>
    /// <param name="interectPot"></param>
    /// <returns></returns>
    /// <remarks>
    #region 参考1
        //    已知一条线段的两个端点为A(x1,y1),B(x2,y2),另一条线段的两个端点为C(x3,y3),D(x4,y4),要判断AB与CD是否有交点，若有求出. 
        //        首先判断d   =   (y2-y1)(x4-x3)-(y4-y3)(x2-x1)， 
        //  若d=0，则直线AB与CD平行或重合， 
        //  若d!=0，则直线AB与CD有交点，且交点(x0,y0)为： 
        //        x0   =   [(x2-x1)*(x4-x3)*(y3-y1)+(y2-y1)*(x4-x3)*x1-(y4-y3)*(x2-x1)*x3]/d 
        //        x0   =   [(y2-y1)*(y4-y3)*(x3-x1)+(x2-x1)*(y4-y3)*y1-(x4-x3)*(y2-y1)*y3]/(-d) 
        //求出交点后在判断交点是否在线段上，即判断以下的式子： 
        //        (x0-x1)*(x0-x2) <=0 
        //        (x0-x3)*(x0-x4) <=0 
        //        (y0-y1)*(y0-y2) <=0 
        //        (y0-y3)*(y0-y4) <=0 
        //只有上面的四个式子都成立才可判定(x0,y0)是线段AB与CD的交点，否则两线段无交点
    #endregion
    #region 参考2
        //第一条直线   
        //double x1 = 10, y1 = 20, x2 = 100, y2 = 200;    
        //double a = (y1 - y2) / (x1 - x2);   
        //double b = (x1 * y2 - x2 * y1) / (x1 - x2);   
        //System.out.println("求出该直线方程为: y=" + a + "x + " + b);   

        ////第二条   
        //double x3 = 50, y3 = 20, x4 = 20, y4 = 100;   
        //double c = (y3 - y4) / (x3 - x4);   
        //double d = (x3 * y4 - x4 * y3) / (x3 - x4);   
        //System.out.println("求出该直线方程为: y=" + c + "x + " + d);   

        //double x = ((x1 - x2) * (x3 * y4 - x4 * y3) - (x3 - x4) * (x1 * y2 - x2 * y1))   
        //    / ((x3 - x4) * (y1 - y2) - (x1 - x2) * (y3 - y4));   

        //double y = ((y1 - y2) * (x3 * y4 - x4 * y3) - (x1 * y2 - x2 * y1) * (y3 - y4))   
        //    / ((y1 - y2) * (x3 - x4) - (x1 - x2) * (y3 - y4));   

        //System.out.println("他们的交点为: (" + x + "," + y + ")");  
    #endregion
    /// </remarks>
    bool InterectSegment(Segment s1,Segment s2,out Vector2 interectPot)
    {
        Vector3 crossResult = Vector3.Cross(s1.Direct(), s2.Direct());
        if (crossResult.z == 0F)
        {
            interectPot = Vector2.zero;
            return false;
        }  
       
        float x1 = s1.P1.x, x2 = s1.P2.x, x3 = s2.P1.x, x4 = s2.P2.x;
        float y1 = s1.P1.y, y2 = s1.P2.y, y3 = s2.P1.y, y4 = s2.P2.y;
        interectPot.x = ((x1 - x2) * (x3 * y4 - x4 * y3) - (x3 - x4) * (x1 * y2 - x2 * y1)) / ((x3 - x4) * (y1 - y2) - (x1 - x2) * (y3 - y4));
        interectPot.y = ((y1 - y2) * (x3 * y4 - x4 * y3) - (x1 * y2 - x2 * y1) * (y3 - y4)) / ((y1 - y2) * (x3 - x4) - (x1 - x2) * (y3 - y4));  


        if ((interectPot.x - s1.P1.x) * (interectPot.x - s1.P2.x) <= 0
            && (interectPot.y - s1.P1.y) * (interectPot.y - s1.P2.y) <= 0
            && (interectPot.x - s2.P1.x) * (interectPot.x - s2.P2.x) <= 0
            && (interectPot.y - s2.P1.y) * (interectPot.y - s2.P2.y) <= 0
            )
        { 
            return true;
        } 
        return false;
    }

    Vector2 RndPosInWorldRect()
    {

        return new Vector2(Random.Range(mWorldDim.xMin, mWorldDim.xMax), Random.Range(mWorldDim.yMin, mWorldDim.yMax));

    }

    /// <summary>
    /// 从原点发出射线与世界边框相交
    /// </summary>
    /// <param name="dir"></param>
    /// <param name="interectPot"></param>
    /// <returns></returns>
    bool InterectWorldRectWithOriginRay(Vector2 rayDir, out Vector2 interectPot)
    {
        //Segment s2 = new Segment(Vector2.zero, rayDir * 10000F);
        if (rayDir.y > 0)
        {
            if(rayDir.x < 0)
                return Interect2WorldRect(0, 1, rayDir, out interectPot);
            else
                return Interect2WorldRect(1, 2, rayDir, out interectPot);
        }
        else
        {
            if (rayDir.x < 0)
                return Interect2WorldRect(0, 3, rayDir, out interectPot);
            else
                return Interect2WorldRect(2, 3, rayDir, out interectPot);
        } 
    }

    /// <summary>
    /// 与指定的两条线段相交测试
    /// </summary>
    /// <param name="idx1"></param>
    /// <param name="idx2"></param>
    /// <param name="rayDir"></param>
    /// <param name="interectPot"></param>
    /// <returns></returns>
    bool Interect2WorldRect(int idx1, int idx2, Vector2 rayDir, out Vector2 interectPot)
    {
        Segment s2 = new Segment(Vector2.zero, rayDir * 10000F);
        if (InterectSegment(mWorldRects[idx1], s2, out interectPot))
            return true;
        if (InterectSegment(mWorldRects[idx2], s2, out interectPot))
            return true;
        return false;
    }

    Segment[] mWorldRects;
    void Awake() { 
        //Rect worldRect 
        mWorldDim = GameMain.Singleton.WorldDimension;
        mWorldRects = new Segment[4];
        mWorldRects[0] = new Segment(new Vector2(mWorldDim.x, mWorldDim.yMax)
                                    , new Vector2(mWorldDim.x, mWorldDim.y));//左

        mWorldRects[1] = new Segment(new Vector2(mWorldDim.x, mWorldDim.yMax)
                                    , new Vector2(mWorldDim.xMax, mWorldDim.yMax));//上

        mWorldRects[2] = new Segment(new Vector2(mWorldDim.xMax, mWorldDim.yMax)
                                    , new Vector2(mWorldDim.xMax, mWorldDim.y));//右

        mWorldRects[3] = new Segment(new Vector2(mWorldDim.xMax, mWorldDim.y)
                                    , new Vector2(mWorldDim.x, mWorldDim.y));//下

        //mWorldRects[0] = new Segment(new Vector2(-1.777778F, 1F), new Vector2(-1.777778F, -1F));//左
        //mWorldRects[1] = new Segment(new Vector2(-1.777778F, 1F), new Vector2(1.777778F, 1F));//上
        //mWorldRects[2] = new Segment(new Vector2(1.777778F, 1F), new Vector2(1.777778F, -1F));//右
        //mWorldRects[3] = new Segment(new Vector2(1.777778F, -1F), new Vector2(-1.777778F, -1F));//下
        int numScreen = GameMain.Singleton.ScreenNumUsing;
        MaxFishAtWorld = MaxFishAtUnitWorld * numScreen ;

        Interval_FishGenerate.Min /= numScreen ;
        Interval_FishGenerate.Max /= numScreen;
        Interval_FishUniqueGenerate.Min /= numScreen ;
        Interval_FishUniqueGenerate.Max /= numScreen;
        Interval_QueuGenerate.Min /= numScreen ;
        Interval_QueuGenerate.Max /= numScreen;
        Interval_FlockGenerate.Min /= numScreen ;
        Interval_FlockGenerate.Max /= numScreen;

        FishTypeIndexMap = new Dictionary<int, Fish>[256];
        FishLockable = new Dictionary<uint, Fish>();
        LeadersAll = new Dictionary<int, Swimmer>();
        mUniqueFishToGenerate = new Queue<Fish>();
		SameTypeBombs = new Dictionary<int, Fish>();
        GameMain.EvtFishInstance += Handle_InstanceFish;
        GameMain.EvtFishClear += Handle_ClearFish;
        GameMain.EvtLeaderInstance += Handle_LeaderInstance;
    
        //测试程序
        // Vector2 interectPot;
        //for (int i = 0; i != 11; ++i)
        //{
        //    Vector3 rndPos = Random.onUnitSphere;
        //    if (!InterectWorldRectWithOriginRay(rndPos, out interectPot))
        //    {
        //        Debug.Log("no interect!!!");
        //    }
        //    else
        //    {
        //        Debug.Log("!!!");
        //    }
        //}

	}


    public void StartFishGenerate()
    {
        if (!IsGenerate)
            return;

        StopCoroutine("_Coro_FishGenerating");
        StopCoroutine("_Coro_FishUniqueGenerating");
        StopCoroutine("_Coro_FishQueueGenerating");
        StopCoroutine("_Coro_FishFlockGenerating");


        StartCoroutine("_Coro_FishGenerating");
        StartCoroutine("_Coro_FishUniqueGenerating");
        StartCoroutine("_Coro_FishQueueGenerating");
        StartCoroutine("_Coro_FishFlockGenerating");
       
    }

    public void StopFishGenerate()
    {
 
        if (GameMain.EvtStopGenerateFish != null)
            GameMain.EvtStopGenerateFish();
        StopCoroutine("_Coro_FishGenerating");
        StopCoroutine("_Coro_FishUniqueGenerating");
        StopCoroutine("_Coro_FishQueueGenerating");
        StopCoroutine("_Coro_FishFlockGenerating");
        
    }

    //void OnGUI()
    //{
    //    if (GUILayout.Button("StopGenerate"))
    //    {
    //        KillAllImmediate();
    //    }
    //}
    public void KillAllImmediate()
    {
        //Fish fTmp = null;
  
        List<Fish> fishToClear = new List<Fish>();
        //List<FishBomb> fishBombToClear = new List<FishBomb>();//todo临时解决方案,这样写会导致以后每新加鱼种就需要再此添加代码
                                                                //.待同类鱼炸弹加入再统一解决该问题

        foreach (Dictionary<int, Fish> dictFishType in FishTypeIndexMap)
        {
            if (dictFishType != null)
            {
                foreach (Fish f in dictFishType.Values)
                {
                    if(f != null)
                        fishToClear.Add(f);
                }
            }
        }
        //foreach (Transform ts in transform)
        //{
            
            
        //    fTmp = ts.GetComponent<Fish>();

        //    if (fTmp != null)
        //        fishToClear.Add(fTmp);
                
        //}
        foreach (Fish f in fishToClear)
        {
            if(f.Attackable)
                f.Clear();
        }

        //foreach (FishBomb fb in fishBombToClear)
        //{
        //    Destroy(fb.gameObject);
        //}
    }
    IEnumerator _Coro_FishFlockGenerating()
    {
        if (Prefab_FishEmitterFlock==null)
            yield break;
        while (true)
        {
            if (GameMain.Singleton.NumFishAlive >= MaxFishAtWorld)
                goto TAG_NextGenerateWait;

            FishEmitter_Flock fe = Instantiate(Prefab_FishEmitterFlock) as FishEmitter_Flock;
            Vector2 interectPot;
            Vector2 rndPos = RndPosInWorldRect();
            rndPos.Normalize();
            if (InterectWorldRectWithOriginRay(rndPos, out interectPot))
            {
                Vector3 posInstance = interectPot + rndPos * (fe.Radius);
                fe.transform.parent = transform;
                fe.transform.localPosition = posInstance;
                //fe.transform.right = -rndPos /*+ ((Vector2)Random.onUnitSphere).normalized * 0.5F*/;
                fe.transform.rotation = PubFunc.RightToRotation(-rndPos);
                fe.Generate();
            }

        TAG_NextGenerateWait:
            yield return new WaitForSeconds(Interval_FlockGenerate.Value);
        }
    }
    //public Transform tmpTsEmitFishPot;
    IEnumerator _Coro_FishQueueGenerating()
    {
        WeightTotal_QueuGenerate = 0;
        foreach (FishQueueGenerateData d in FishQueueData)
        {
            WeightTotal_QueuGenerate += d.Weight;
            d.HittedWeight = WeightTotal_QueuGenerate;
        }
        while (true)
        {
            if (GameMain.Singleton.NumFishAlive >= MaxFishAtWorld)
                goto TAG_NextGenerateWait;

            //随机
            int curWeight = Random.Range(0, WeightTotal_QueuGenerate);
            int queueIdx = 0;
            for (; queueIdx != FishQueueData.Length; ++queueIdx)
            {
                if (curWeight < FishQueueData[queueIdx].HittedWeight)
                {
                    break;
                }
            }
            if (queueIdx >= FishQueueData.Length)
                goto TAG_NextGenerateWait;

            FishEmitter_Queue fg = Instantiate(FishQueueData[queueIdx].Prefab_Queue) as FishEmitter_Queue;
            Vector2 interectPot;
            Vector2 rndPos = Random.onUnitSphere;
            rndPos.Normalize();
            if (InterectWorldRectWithOriginRay(rndPos, out interectPot))
            {
                Vector3 posInstance = interectPot + rndPos * (fg.Prefab_Fish.swimmer.BoundCircleRadius);
                fg.transform.parent = transform;
                fg.transform.localPosition = posInstance;
                //fg.transform.right = -rndPos /*+ ((Vector2)Random.onUnitSphere).normalized * 0.5F*/;
                fg.transform.rotation = PubFunc.RightToRotation(-rndPos);
                fg.Generate();
            }

        TAG_NextGenerateWait:
            yield return new WaitForSeconds(Interval_QueuGenerate.Value);
        }
    }



    IEnumerator _Coro_FishGenerating()
    {
        WeightTotal_FishGenerate = 0;
        foreach (FishGenerateData d in FishGenerateDatas)
        {
            WeightTotal_FishGenerate += d.Weight;
            d.HittedWeight = WeightTotal_FishGenerate;
        }
        while (true)
        {
            if (GameMain.Singleton.NumFishAlive >= MaxFishAtWorld)
                goto TAG_NextGenerateWait;

            //随机
            int curWeight = Random.Range(0, WeightTotal_FishGenerate); 

            Fish prefabFishToGen = null;
            foreach (FishGenerateData fd in FishGenerateDatas)
            {
                if(curWeight < fd.HittedWeight)
                {
                    prefabFishToGen = fd.Prefab_Fish;
                    break;
                }
            }

            if (prefabFishToGen == null)
                goto TAG_NextGenerateWait;


            //Fish f = Pool_GameObj.GetObj(prefabFishToGen.gameObject).GetComponent<Fish>();


            Fish f = Instantiate(prefabFishToGen) as Fish; 

            f.name = prefabFishToGen.name;


            Vector2 interectPot;
            Vector2 rndPos = RndPosInWorldRect();
            rndPos.Normalize();
            if (InterectWorldRectWithOriginRay(rndPos, out interectPot))
            {
                Vector3 posInstance = interectPot + rndPos * (f.swimmer.BoundCircleRadius );
                posInstance.z = Defines.GMDepth_Fish + ApplyFishDepth();
                f.transform.parent = transform;
                f.transform.localPosition = posInstance;
                //fe.transform.right = -rndPos /*+ ((Vector2)Random.onUnitSphere).normalized * 0.5F*/;
                f.transform.rotation = PubFunc.RightToRotation(-rndPos);
                f.swimmer.Go();
            }

        TAG_NextGenerateWait:
            yield return new WaitForSeconds(Interval_FishGenerate.Value);
        }
    }

    IEnumerator _Coro_FishUniqueGenerating()
    {
        if (FishGenerateUniqueDatas.Length == 0)
            yield break;

        WeightTotal_FishUniqueGenerate = 0;
        foreach (FishGenerateData d in FishGenerateUniqueDatas)
        {
            WeightTotal_FishUniqueGenerate += d.Weight;
            d.HittedWeight = WeightTotal_FishUniqueGenerate;
        }

        while (true)
        {
            if (GameMain.Singleton.NumFishAlive >= MaxFishAtWorld)
                goto TAG_NextGenerateWait;

            if (mUniqueFishToGenerate.Count == 0)
            {
                List<FishGenerateData> tmpFishGdLst = new List<FishGenerateData>();
                foreach (FishGenerateData fgd in FishGenerateUniqueDatas)
                {
                    tmpFishGdLst.Add(fgd);
                }

                tmpFishGdLst.Sort((FishGenerateData a, FishGenerateData b) => { return Random.Range(0, a.Weight) < Random.Range(0, b.Weight) ? 1 : -1; });
                int gdNum = FishGenerateUniqueDatas.Length * 7 / 10;//队列70%的鱼
          
                foreach (FishGenerateData fgd in tmpFishGdLst)
                {
                    
                    mUniqueFishToGenerate.Enqueue(fgd.Prefab_Fish);
                    if (--gdNum < 0)
                        break;
                    //Debug.Log(fgd.Prefab_Fish.name);
                }
           
            }
            Fish prefabFishToGen = mUniqueFishToGenerate.Dequeue();

            if (FishTypeIndexMap[prefabFishToGen.TypeIndex] != null
                && FishTypeIndexMap[prefabFishToGen.TypeIndex].Count != 0)
            {
                goto TAG_NextGenerateWait;
            }



            Fish f = Instantiate(prefabFishToGen) as Fish;

            f.name = prefabFishToGen.name; 
            Vector2 interectPot;
            Vector2 rndPos = RndPosInWorldRect();
            rndPos.Normalize();
            if (InterectWorldRectWithOriginRay(rndPos, out interectPot))
            {
                Vector3 posInstance = interectPot + rndPos * (f.swimmer.BoundCircleRadius);
                posInstance.z = Defines.GMDepth_Fish + ApplyFishDepth();
                f.transform.parent = transform;
                f.transform.localPosition = posInstance;
                //f.transform.right = -rndPos + ((Vector2)Random.onUnitSphere).normalized * 0.2F;
                f.transform.rotation = PubFunc.RightToRotation(-rndPos + ((Vector2)Random.onUnitSphere).normalized * 0.2F);
                f.swimmer.Go();
            }

        TAG_NextGenerateWait:
            yield return new WaitForSeconds(Interval_FishUniqueGenerate.Value);
        }
    }
 
    //获得鱼深度
    public float ApplyFishDepth()
    {
        mCurrentFishDepth -= Defines.OffsetAdv_FishGlobleDepth;
        if (mCurrentFishDepth < -100F)
            mCurrentFishDepth = 0F;
        return mCurrentFishDepth;
        //mCurrentFishDepth -= FishDepthAdditive;
        //if (mCurrentFishDepth < 0F)
        //    mCurrentFishDepth = 3F;
        //return mCurrentFishDepth;
    }

    /// <summary>
    /// 刷新所有权重
    /// </summary>
    public void RefreshAllGenerateWeight()
    {
         
        WeightTotal_FishGenerate = 0;
        foreach (FishGenerateData d in FishGenerateDatas)
        {
            WeightTotal_FishGenerate += d.Weight;
            d.HittedWeight = WeightTotal_FishGenerate;
        }
        WeightTotal_FishUniqueGenerate = 0;
        foreach (FishGenerateData d in FishGenerateUniqueDatas)
        {
            WeightTotal_FishUniqueGenerate += d.Weight;
            d.HittedWeight = WeightTotal_FishUniqueGenerate;
        }
        WeightTotal_QueuGenerate = 0;

        foreach (FishQueueGenerateData d in FishQueueData)
        {
            WeightTotal_QueuGenerate += d.Weight;
            d.HittedWeight = WeightTotal_QueuGenerate;
        }


    }
    //void OnGUI()
    //{
    //    if (GUILayout.Button("interatect"))
    //    {
    //        Vector2 interectPot;
    //        Vector3 rndPos = Random.onUnitSphere;
    //        if (InterectWorldRectWithOriginRay(rndPos, out interectPot))
    //        {
    //            GameObject goNew = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //            goNew.transform.localScale = Vector3.one * 0.4F;
    //            rndPos.z = 0F;
    //            rndPos.Normalize();
    //            goNew.transform.position = new Vector3(rndPos.x * 3F, rndPos.y * 3F, -10F);

    //            GameObject goInterect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    //            goInterect.transform.localScale = Vector3.one * 0.2F;
    //            goInterect.transform.position = new Vector3(interectPot.x,interectPot.y,-10F);
 
    //        } 
    //    }
    //} 
}
