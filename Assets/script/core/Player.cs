using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {
    [System.NonSerialized]
    public int Idx;
    [System.NonSerialized]
    public int CtrllerIdx;
    public Gun GunInst;
    public Gun[] Prefab_Guns;//待换枪,对应GunType

    public tk2dTextMesh Text_Score;
    public CoinStackManager Ef_CoinStack;//待整理
    public FlyingCoinManager Ef_FlyCoin;
    public ParticleSystem Prefab_ParChangeGun;

    public GameObject[] Prefab_OutBountyHints;//出币出票提示.0:退币中  1:退票中 2:退币故障 3:退票故障
    //public GameObject SliSpr_ScoreBG;//分数板背景
    public tk2dSprite Spr_GunBottom;//炮台动画
    [System.NonSerialized]
    public Rect AtScreenArea;//所在屏幕区域

    [System.NonSerialized]
    public bool CanChangeScore = true;//是否可押分

    [System.NonSerialized]
    public bool CanCurrentScoreChange = true;//

    private uint mScoreChangeLowest = 0;//最低分数值
    //[System.NonSerialized]
    //public int Score;//拥有分数

    private bool[] mLeftRightKeystate;//左右键状态,true按下,false弹起

    private bool mIsUpScoreBatch = false;
    private bool mIsDownScoreBatch = false;
    private bool mIsOutingBounty = false;//正在退奖状态

    private GameObject mGOViewingOutBountyHint;//正在显示的退奖提示
    private static readonly Vector3 LocalOutBountyHint = new Vector3(192F, 26.88F, 0F);
    //private Player_FishLocker mFishLocker;
    
    void Awake()
    { 
        mLeftRightKeystate = new bool[2] { false, false };

        //mFishLocker = GetComponent<Player_FishLocker>();
        //计算所在区域
        Rect[] mWorldAreas = new Rect[GameMain.Singleton.ScreenNumUsing];

        for (int i = 0; i != mWorldAreas.Length; ++i)
        {
            mWorldAreas[i].x = GameMain.Singleton.WorldDimension.x + Defines.WorldDimensionUnit.width * i;
            mWorldAreas[i].y = GameMain.Singleton.WorldDimension.y;
            mWorldAreas[i].width = Defines.WorldDimensionUnit.width;
            mWorldAreas[i].height = Defines.WorldDimensionUnit.height;

            if (mWorldAreas[i].Contains(transform.position))
            {

                AtScreenArea = mWorldAreas[i];

                break;
            }
        }
    }

	// Use this for initialization
	void Start () {
      
       // Text_Score.text = GameMain.Singleton.BSSetting.Dat_PlayersScore[Idx].Val.ToString();
       // Text_Score.Commit();

        if (GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[Idx].Val != 0)
        {
            //Ef_CoinStack.OneStack_SetNum(GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[Idx].Val);
            StartCoroutine(_Coro_OutBountyImm());
        }
        
        //初始化Gun
        GunLevelType glt = Gun.GunNeedScoreToLevelType(GameMain.Singleton.BSSetting.Dat_PlayersGunScore[Idx].Val);
        GunType gt = Gun.CombineGunLevelAndPower(glt,GunPowerType.Normal);
        //if (GameMain.Singleton.BSSetting.Dat_PlayersGunScore[Idx].Val >= Defines.ChangeGunNeedScoreTri)
        if (GunInst != null)
            Destroy(GunInst.gameObject);

        GunInst = Instantiate(Prefab_Guns[(int)gt]) as Gun;
        GunInst.GunType_ = gt;

        GunInst.transform.parent = transform;
        GunInst.transform.localPosition = Vector3.zero;
        GunInst.transform.localRotation = Quaternion.identity;

        //if (mFishLocker != null)
        //    mFishLocker.EvtRelock += (Fish f) => { GunInst.LockAt(f); };
	}

    /// <summary>
    /// 改变分数(面板分数)
    /// </summary>
    /// <param name="change"></param>
    /// <returns>改变是否成功</returns>
    public bool ChangeScore(int change)
    {
        //if (!CanChangeScore)
        //    return false;

        BackStageSetting bss = GameMain.Singleton.BSSetting ;//
        if (bss.Dat_PlayersScore[Idx].Val + change < mScoreChangeLowest)
            return false;

        bss.Dat_PlayersScore[Idx].Val += change;
        if (GameMain.EvtPlayerScoreChanged != null)
            GameMain.EvtPlayerScoreChanged(this, bss.Dat_PlayersScore[Idx].Val, change);

        //面板分数
       // Text_Score.text = bss.Dat_PlayersScore[Idx].Val.ToString();
        //Text_Score.Commit();
        return true;
    }

    /// <summary>
    /// 赢得的分数(不放入面板)
    /// </summary>
    /// <param name="numWon"></param>
    public void WonScore(int numWon)
    {
        GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[Idx].Val += numWon;
        if (GameMain.EvtPlayerWonScoreChanged != null)
            GameMain.EvtPlayerWonScoreChanged(this, GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[Idx].Val);
        //Ef_CoinStack.OneStack_SetNum(GameMain.Singleton.BSSetting.Dat_PlayersScoreWon[Idx].Val);
        StartCoroutine(_Coro_OutBountyImm());
    }

    /// <summary>
    /// 玩家赢得币,综合ChangeScore和WonScore
    /// </summary>
    /// <param name="num"></param>
    public void GainScore(int scoreTotalGetted, int fishodd, int bulletScore)
    {
        BackStageSetting bss = GameMain.Singleton.BSSetting;
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom
            || bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
        {
            ChangeScore(scoreTotalGetted);
            //币堆.效果
            //Ef_CoinStack.RequestViewOneStack(scoreTotalGetted, fishodd, bulletScore, Idx);
        }
        else if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate
        || bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
            WonScore(scoreTotalGetted);
    }

    public void GainScore(int scoreTotalGetted)
    {
        GainScore(scoreTotalGetted, 2, 10);
    }

    /// <summary>
    /// 按键退币
    /// </summary>
    public void OutBountyButtom()
    {
        StartCoroutine(_Coro_OutBountyButtom());
    }

    /// <summary>
    /// 按键退币-协同
    /// </summary>
    /// <returns></returns>
    public IEnumerator _Coro_OutBountyButtom()
    {
        if(mIsOutingBounty)
            yield break;

        BackStageSetting bss =  GameMain.Singleton.BSSetting;
        INemoControlIO arcIO = GameMain.Singleton.ArcIO;
        int oneBountyNeedScore = 100000000;//不会退
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
            oneBountyNeedScore = bss.InsertCoinScoreRatio.Val;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
            oneBountyNeedScore = (int)(((float)bss.InsertCoinScoreRatio.Val  ) * bss.CoinTicketRatio_Coin.Val / bss.CoinTicketRatio_Ticket.Val);//投币比例(分/币) * 币票比(币/票) = 出一票需要多少分(分/票)
        //检查是否够退一币
        if (bss.Dat_PlayersScore[Idx].Val < oneBountyNeedScore)
            yield break;

        //显示退奖中提示
        if (mGOViewingOutBountyHint == null)
        {
            if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
                mGOViewingOutBountyHint = Instantiate(Prefab_OutBountyHints[0]) as GameObject;
            else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
                mGOViewingOutBountyHint = Instantiate(Prefab_OutBountyHints[1]) as GameObject;
            mGOViewingOutBountyHint.transform.parent = transform;
            mGOViewingOutBountyHint.transform.localPosition = LocalOutBountyHint;
            mGOViewingOutBountyHint.transform.localRotation = Quaternion.identity;
        }



        mScoreChangeLowest = (uint)oneBountyNeedScore;//设置分数改变下限,防止控制板返回消息时不能扣除
        mIsOutingBounty = true;

        if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
            arcIO.OutCoin(1, CtrllerIdx);
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
            arcIO.OutTicket(1, CtrllerIdx);

        bool isBoutyOutted = false;//是否已出奖励
        float timeOut = 10F;//超时时间
        float elapse = 0F;
        NemoCtrlIO_EventController outBountyreflect = (int cIdx) =>
        {
            if (CtrllerIdx == cIdx)
                isBoutyOutted = true; 
        };
        
		if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
            arcIO.EvtOutCoinReflect += outBountyreflect;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
            arcIO.EvtOutTicketReflect += outBountyreflect;
        while (!isBoutyOutted)
        {
            if (elapse > timeOut)
            {
                StartCoroutine(_Coro_OutBountyErrorView());//显示故障
                mIsOutingBounty = false;
                mScoreChangeLowest = 0;//恢复改变分数下限
                yield break;
            }
            elapse += Time.deltaTime;
            yield return 0;
        }

        //出币已应答
		if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
            arcIO.EvtOutCoinReflect -= outBountyreflect;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
            arcIO.EvtOutTicketReflect -= outBountyreflect;
        mIsOutingBounty = false;
        mScoreChangeLowest = 0;//恢复改变分数下限

        //历史记录
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
        {
            bss.His_CoinOut.Val += 1;
            bss.His_GainCurrent.Val -= 1;
            bss.UpdateGainCurrentAndTotal();
        }
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
        {
            bss.His_TicketOut.Val += 1;

            //所有玩家公用
            bss.TicketOutFragment.Val += 1;
            int outCoinNumBase = bss.TicketOutFragment.Val / bss.CoinTicketRatio_Ticket.Val;//出币基数,乘与CoinTicketRatio_Coin 得出应出多少个整币
            if (outCoinNumBase > 0)
            {
                bss.His_GainCurrent.Val -= outCoinNumBase * bss.CoinTicketRatio_Coin.Val;
                bss.TicketOutFragment.Val -= bss.CoinTicketRatio_Ticket.Val;
            } 
            bss.UpdateGainCurrentAndTotal();
        }

        ChangeScore(-oneBountyNeedScore);

        //是否可再扣分,可以则迭代 
        if (bss.Dat_PlayersScore[Idx].Val >= oneBountyNeedScore)
        {
            StartCoroutine(_Coro_OutBountyButtom());
        }
        else//退出迭代
        {
            if (mGOViewingOutBountyHint != null)
            {
                Destroy(mGOViewingOutBountyHint);
                mGOViewingOutBountyHint = null;
            }
        }
    }

    /// <summary>
    /// 按键退币 - 报故障
    /// </summary>
    /// <remarks>在按键退币模式下,不够币的话提示故障后不会重退</remarks>
    /// <returns></returns>
    IEnumerator _Coro_OutBountyErrorView()
    {
        if (mGOViewingOutBountyHint != null)
        {
            Destroy(mGOViewingOutBountyHint);
        }
        BackStageSetting bss = GameMain.Singleton.BSSetting;
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinButtom)
            mGOViewingOutBountyHint = Instantiate(Prefab_OutBountyHints[2]) as GameObject;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketButtom)
            mGOViewingOutBountyHint = Instantiate(Prefab_OutBountyHints[3]) as GameObject;
        mGOViewingOutBountyHint.transform.parent = transform;
        mGOViewingOutBountyHint.transform.localPosition = LocalOutBountyHint;
        mGOViewingOutBountyHint.transform.localRotation = Quaternion.identity;

        yield return new WaitForSeconds(5F);

        Destroy(mGOViewingOutBountyHint);
        mGOViewingOutBountyHint = null;
    }

    /// <summary>
    /// 即时退奖
    /// </summary>
    /// <remarks>Dat_PlayersScoreWon中值,可以退奖则扣下</remarks>
    /// <returns></returns>
    public IEnumerator _Coro_OutBountyImm()
    {
        if(mIsOutingBounty)
            yield break;

        BackStageSetting bss = GameMain.Singleton.BSSetting;
        INemoControlIO arcIO= GameMain.Singleton.ArcIO;

        int oneBountyNeedScore = 100000000;//防止出现bug不会退
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate)
            oneBountyNeedScore = bss.InsertCoinScoreRatio.Val;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
            oneBountyNeedScore = (int)(((float)bss.InsertCoinScoreRatio.Val) * bss.CoinTicketRatio_Coin.Val / bss.CoinTicketRatio_Ticket.Val);//投币比例(分/币) * 币票比(币/票) = 出一票需要多少分(分/票)
        if(bss.Dat_PlayersScoreWon[Idx].Val  < oneBountyNeedScore)
            yield break;

        mIsOutingBounty = true;

        //请求硬件扣币
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate)
            arcIO.OutCoin(1, CtrllerIdx);
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
            arcIO.OutTicket(1, CtrllerIdx);


        bool isBoutyOutted = false;//是否已出奖励
        float timeOut = 10F;//超时时间
        float elapse = 0F;
        NemoCtrlIO_EventController outBountyreflect = (int cIdx) =>
        {
            if (CtrllerIdx == cIdx)
                isBoutyOutted = true;
        };
		if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate)
            arcIO.EvtOutCoinReflect += outBountyreflect;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
            arcIO.EvtOutTicketReflect += outBountyreflect;

        
        while (!isBoutyOutted)
        {
            if (elapse > timeOut)// 超时
            {
                //显示退奖错误提示
                if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate)
                    mGOViewingOutBountyHint = Instantiate(Prefab_OutBountyHints[2]) as GameObject;
                else if (bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
                    mGOViewingOutBountyHint = Instantiate(Prefab_OutBountyHints[3]) as GameObject;
                mGOViewingOutBountyHint.transform.parent = transform;
                mGOViewingOutBountyHint.transform.localPosition = LocalOutBountyHint;
                mGOViewingOutBountyHint.transform.localRotation = Quaternion.identity;

                yield return new WaitForSeconds(30F);
                mIsOutingBounty = false;
                StartCoroutine(_Coro_OutBountyImm());
                yield break;
            }
            elapse += Time.deltaTime;
            yield return 0;
        }

        //出币已应答
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate)
            arcIO.EvtOutCoinReflect -= outBountyreflect;
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
            arcIO.EvtOutTicketReflect -= outBountyreflect;
        


        //历史记录
        if (bss.OutBountyType_.Val == OutBountyType.OutCoinImmediate)
        {
            bss.His_CoinOut.Val += 1;
            bss.His_GainCurrent.Val -= 1;
            bss.UpdateGainCurrentAndTotal();
        }
        else if (bss.OutBountyType_.Val == OutBountyType.OutTicketImmediate)
        {
            bss.His_TicketOut.Val += 1;

            //所有玩家公用
            bss.TicketOutFragment.Val += 1;
            int outCoinNumBase = bss.TicketOutFragment.Val / bss.CoinTicketRatio_Ticket.Val;//出币基数,乘与CoinTicketRatio_Coin 得出应出多少个整币
            if (outCoinNumBase > 0)
            {
                bss.His_GainCurrent.Val -= outCoinNumBase * bss.CoinTicketRatio_Coin.Val;
                bss.TicketOutFragment.Val -= bss.CoinTicketRatio_Ticket.Val;
            } 
            bss.UpdateGainCurrentAndTotal();
        }
        if (bss.Dat_PlayersScoreWon[Idx].Val < oneBountyNeedScore)
        {
            Debug.LogError("[退奖错误]该扣分数大于玩家已赢分数.");
            yield break;
        }

        bss.Dat_PlayersScoreWon[Idx].Val = bss.Dat_PlayersScoreWon[Idx].Val - oneBountyNeedScore;
        if (GameMain.EvtPlayerWonScoreChanged != null)
            GameMain.EvtPlayerWonScoreChanged(this, bss.Dat_PlayersScoreWon[Idx].Val);
        //Ef_CoinStack.OneStack_SetNum(bss.Dat_PlayersScoreWon[Idx].Val);
        mIsOutingBounty = false;

        //是否可再扣分,可以则迭代 
        if (bss.Dat_PlayersScoreWon[Idx].Val >= oneBountyNeedScore)
        {
            StartCoroutine(_Coro_OutBountyImm());
        }

        if (mGOViewingOutBountyHint != null)
        {
            Destroy(mGOViewingOutBountyHint);
            mGOViewingOutBountyHint = null;
        }
    }
   

    /// <summary>
    /// 换枪
    /// </summary>
    /// <param name="gt"></param>
    /// <returns></returns>
    public bool ChangeGun(GunType gt)
    {
        Gun g = Instantiate(Prefab_Guns[(int)gt]) as Gun;
        g.GunType_ = gt;

        g.transform.parent = GunInst.transform.parent;
        g.transform.localPosition = GunInst.transform.localPosition;
        g.transform.localRotation = GunInst.transform.localRotation;

        //方位
        g.TsGun.transform.localPosition = GunInst.TsGun.transform.localPosition;
        g.TsGun.transform.localRotation = GunInst.TsGun.transform.localRotation;


        GunInst.CopyDataTo(g);
        Destroy(GunInst.gameObject);
        GunInst = g;

        //额外的操作
        //在快速切分的话就继续切
//         if (g.FastChangingCoin)
//         {
//             g.StartChangeNeedCoin();
//         }

        //效果
        StartCoroutine(_Coro_Effect_ChangeGun(g));

        //音效
        if (g.Snd_Equip != null)
            GameMain.Singleton.SoundMgr.PlayOneShot(g.Snd_Equip);

        if (GameMain.EvtPlayerGunChanged != null)
            GameMain.EvtPlayerGunChanged(this, g);

        return true;
    }


    public bool ChangeGun(GunLevelType lvType, GunPowerType pwrType)
    {
        return ChangeGun(Gun.CombineGunLevelAndPower(lvType, pwrType));
    }
    IEnumerator _Coro_Effect_ChangeGun(Gun newGun)
    {
        ParticleSystem ef_particle = null;
        if (Prefab_ParChangeGun != null)
        {
            ef_particle = Instantiate(Prefab_ParChangeGun) as ParticleSystem;
            ef_particle.transform.parent = newGun.transform;
            ef_particle.transform.localPosition = new Vector3(0F, -18.55325184F, -10F);
            ef_particle.transform.localRotation = Quaternion.identity;
            ef_particle.Play(true);
 
        }
        float time = 0.1F;
        float elapse = 0F;
        Transform tsAniGun = newGun.AniSpr_GunPot.transform;
        //Vector3 oriPos = tsAniGun.localPosition;
        float scale = 1F;
        while (elapse <time)
        {
            if (tsAniGun == null)//同时两个换枪指令会使延时失效
                break;
            //tsAniGun.localPosition = oriPos + tsAniGun.localRotation * Vector3.up *(Curve_GunShakeOffset.Evaluate(elapse/time));
            scale  = 1F+ 0.5F*(1F-elapse/time);
            tsAniGun.localScale = new Vector3(scale,scale,scale);

            elapse += Time.deltaTime;
            yield return 0;
        }

        if (tsAniGun != null)
            tsAniGun.localScale = Vector3.one;

        if (ef_particle != null)
        {
            yield return new WaitForSeconds(3F);
            if (ef_particle != null)
                Destroy(ef_particle.gameObject);
        }

    }
    //GunType mCurGunType = GunType.Normal;
    //void OnGUI()
    //{
    //    if (Idx == 0 && GUILayout.Button("change gun"))
    //    {
    //        if (mCurGunType == GunType.Normal)
    //        {
    //            ChangeGun(GunType.Lizi);
    //            mCurGunType = GunType.Lizi;
    //        }
    //        else if (mCurGunType == GunType.Lizi)
    //        {
    //            ChangeGun(GunType.Normal);
    //            mCurGunType = GunType.Normal;
    //        }
    //    }
    //}


    public void ScoreUp(bool isMultiTen)
    {
        BackStageSetting bss = GameMain.Singleton.BSSetting;

        int numCoin = 10 * (isMultiTen ? 10 : 1);
        int score = bss.InsertCoinScoreRatio.Val * numCoin;
        
        //判断是否爆机
        if (bss.Dat_PlayersScore[Idx].Val + score >= Defines.NumScoreUpMax)
        {
            return;
        }
        ChangeScore(score);
        
        //上分
        bss.His_CoinUp.Val += numCoin;
        bss.His_GainCurrent.Val += numCoin;
        bss.UpdateGainCurrentAndTotal();

        //声音
        GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_AddScore);
    }

    void ScoreDown(bool isAll)
    {

        BackStageSetting bss = GameMain.Singleton.BSSetting;
        if (isAll)
        {
            int scoreDown = (bss.Dat_PlayersScore[Idx].Val / bss.InsertCoinScoreRatio.Val) * bss.InsertCoinScoreRatio.Val;
            ChangeScore(-scoreDown);

            int numCoin = scoreDown / bss.InsertCoinScoreRatio.Val;

            bss.His_CoinDown.Val += numCoin;
            bss.His_GainCurrent.Val -= numCoin;
            bss.UpdateGainCurrentAndTotal();
        }
        else
        {
            int requireDown = bss.InsertCoinScoreRatio.Val * 10;

            if (requireDown <= bss.Dat_PlayersScore[Idx].Val)
            {
                ChangeScore(-requireDown);

                int numCoin = requireDown / bss.InsertCoinScoreRatio.Val;

                bss.His_CoinDown.Val += numCoin;
                bss.His_GainCurrent.Val -= numCoin;
                bss.UpdateGainCurrentAndTotal();
            }
        } 

        //声音
        GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_AddScore);
    }
    /// <summary>
    /// 按键按下,有GameMain触发
    /// </summary>
    /// <param name="p"></param>
    /// <param name="k"></param>
    /// <param name="down"></param>
    public void OnKeyDown(HpyInputKey k, bool down)
    {

        if (down && k == HpyInputKey.Left)
        {
            mLeftRightKeystate[0] = true;
            goto tag_GunRotate;
        }
        else if (!down && k == HpyInputKey.Left)
        {
            mLeftRightKeystate[0] = false;
            goto tag_GunRotate;
        }

        else if (down && k == HpyInputKey.Right)
        {
            mLeftRightKeystate[1] = true;
            goto tag_GunRotate;
        } 
        else if (!down && k == HpyInputKey.Right)
        {
            mLeftRightKeystate[1] = false;
            goto tag_GunRotate;
        }

        //else if (down && k == HpyInputKey.Down)
        //{
        //    if (mFishLocker != null)
        //    {
        //        Fish f = mFishLocker.Lock();

        //        if (f != null)
        //            GunInst.LockAt(f);
        //    }
            
        //}
        //else if (!down && k == HpyInputKey.Down)
        //{
        //    if (mFishLocker != null)
        //    {
        //        mFishLocker.UnLock();
        //        GunInst.UnLock();
        //    }
        //}

        else if (down && k == HpyInputKey.Fire)
        {
            
            GunInst.StartFire();
        }
        else if (!down && k == HpyInputKey.Fire)
        {
            GunInst.StopNormalFire();
        }
        else if (down && k == HpyInputKey.Advance)
        {
            //GunInst.StartChangeNeedCoin();
            StartCoroutine("_Coro_ChangeNeedScore");
        }
        else if (!down && k == HpyInputKey.Advance)
        {
            //GunInst.StopChangeNeedCoin();
            StopCoroutine("_Coro_ChangeNeedScore");
        }
        else if (down && k == HpyInputKey.ScoreUp)
        {
            StartCoroutine("_Coro_ScoreBatchUp");
            mIsUpScoreBatch = false;
        }
        else if (!down && k == HpyInputKey.ScoreUp)
        {
 
            StopCoroutine("_Coro_ScoreBatchUp");
            if (!mIsUpScoreBatch)
            {
                ScoreUp(false);
            }
        }
        else if (down && k == HpyInputKey.ScoreDown)
        {
            StartCoroutine("_Coro_ScoreBatchDown");
            mIsDownScoreBatch = false;
        }
        else if (!down && k == HpyInputKey.ScoreDown)
        {
            StopCoroutine("_Coro_ScoreBatchDown");
            if (!mIsDownScoreBatch)
            {
                ScoreDown(false);
            }
        }

        return;

    tag_GunRotate:
        if (mLeftRightKeystate[0] && mLeftRightKeystate[1])
            GunInst.StopRotate();
        else if (!mLeftRightKeystate[0] && !mLeftRightKeystate[1])
            GunInst.StopRotate();
        else if (mLeftRightKeystate[0])
            GunInst.RotateTo(true);
        else if (mLeftRightKeystate[1])
            GunInst.RotateTo(false);
    }

 
    /// <summary>
    /// 批量上分
    /// </summary>
    /// <returns></returns>
    IEnumerator _Coro_ScoreBatchUp()
    {
        yield return new WaitForSeconds(1F);
        ScoreUp(true);
        mIsUpScoreBatch = true;
    }

    /// <summary>
    /// 批量下分
    /// </summary>
    /// <returns></returns>
    IEnumerator _Coro_ScoreBatchDown()
    {
        yield return new WaitForSeconds(1F);
        ScoreDown(true);
        mIsDownScoreBatch = true;
    }

    /// <summary>
    /// 改变押分
    /// </summary>
    /// <returns></returns>
    IEnumerator _Coro_ChangeNeedScore()
    {
        if (!CanChangeScore)
            yield break;
        BackStageSetting bss = GameMain.Singleton.BSSetting;
 
        GunLevelType preType = Gun.GunNeedScoreToLevelType(bss.Dat_PlayersGunScore[Idx].Val);
        GunInst.AdvanceNeedScore();
        GunLevelType curType = Gun.GunNeedScoreToLevelType(bss.Dat_PlayersGunScore[Idx].Val);
        if(curType  != preType)
            On_GunLevelTypeChanged(curType);

        yield return new WaitForSeconds(0.2F);


        while (CanChangeScore)
        {
            preType = Gun.GunNeedScoreToLevelType(bss.Dat_PlayersGunScore[Idx].Val);
            GunInst.AdvanceNeedScore();
            curType = Gun.GunNeedScoreToLevelType(bss.Dat_PlayersGunScore[Idx].Val);
            if (curType != preType)
                On_GunLevelTypeChanged(curType);

            yield return new WaitForSeconds(0.1F);
        }
    }


    public void ChangeNeedScore(int newScore)
    {
        BackStageSetting bss = GameMain.Singleton.BSSetting;

        GunLevelType preType = Gun.GunNeedScoreToLevelType(bss.Dat_PlayersGunScore[Idx].Val);
        GunInst.SetNeedScore(newScore);
        GunLevelType curType = Gun.GunNeedScoreToLevelType(bss.Dat_PlayersGunScore[Idx].Val);

        if (curType != preType)
            On_GunLevelTypeChanged(curType);
    }




    /// <summary>
    /// 押分改变(自身触发)
    /// </summary>
    /// <param name="curScore"></param>
    void On_GunLevelTypeChanged(GunLevelType curType)
    {
        ChangeGun(curType, GunInst.GetPowerType()); 
    }
}
