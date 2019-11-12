    #define ENABLE_MCU_VERIFY//临时禁止mcu验证
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameMain : MonoBehaviour {

    public enum State
    {
        Idle,//待机状态,游戏未开始
        Normal,//普通出鱼
        Sweeping,//过场,(一定没子弹)
        Preluding,//开场阵列 
        BackStage,//后台
        BeforeSweeping_WaitBulletClear//扫场前等待子弹消失
    }
    //事件
    public delegate void Event_InputInsertCoin(int playerControll, int n);
    /// <summary>
    /// 输入 - 按键
    /// </summary>
    /// <param name="p"></param>
    /// <param name="key"></param>
    public delegate void Event_InputKey(int control, HpyInputKey key,bool down);
    public static  Event_InputKey EvtInputKey;
    public static Event_Generic EvtStopGenerateFish;//停止出鱼
    public static Event_Generic EvtMainProcess_StartGame;//开始游戏
    public static Event_Generic EvtMainProcess_PrepareChangeScene;//准备过场.波浪过先
    public static Event_Generic EvtMainProcess_FinishChangeScene;//过场完毕.波浪过先
    public static Event_Generic EvtMainProcess_FinishPrelude;//鱼阵结束
    public static Event_Generic EvtMainProcess_FirstEnterScene;//第一次进入场景


    public delegate void Event_FishKilled(Player killer,Bullet b,Fish f);
    public static Event_FishKilled EvtFishKilled;//鱼死亡事件

    public delegate void Event_FishBombKilled(Player killer, int scoreGetted);
    public static Event_FishBombKilled EvtFishBombKilled;//鱼炸弹爆炸,(注意:先触发.其他鱼再死亡)
 
    public static Event_FishBombKilled EvtSameTypeBombKiled;//同类炸弹爆炸
    public static Event_FishBombKilled EvtSameTypeBombExKiled;//超级同类炸弹爆炸

    public static Event_Generic EvtFreezeBombActive;
    public static Event_Generic EvtFreezeBombDeactive;

    public delegate void Event_FishClear(Fish f);
    public static Event_FishClear EvtFishClear;
    public static Event_FishClear EvtFishInstance;
    public delegate void Event_LeaderInstance(Swimmer s);
    public static Event_LeaderInstance EvtLeaderInstance;//领队初始化

    public delegate void Event_PlayerGainScoreFromFish(Player p,int score,Fish firstFish,Bullet b);//玩家从鱼类获得分数
    public static Event_PlayerGainScoreFromFish Evt_PlayerGainScoreFromFish;

    public delegate void Event_PlayerScoreChanged(Player p, int ScoreNew, int scoreChange);
    public static Event_PlayerScoreChanged EvtPlayerScoreChanged;

    public delegate void Event_PlayerGunChanged(Player p, Gun newGun);
    public static Event_PlayerGunChanged EvtPlayerGunChanged;

    public delegate void Event_PlayerWonScoreChanged(Player p, int scoreNew);
    public static Event_PlayerWonScoreChanged EvtPlayerWonScoreChanged;//玩家赢得分值改变('赢得分值'用于即时退币)

    public delegate void Event_PlayerGunFired(Player p,Gun gun, int useScore);
    public static Event_PlayerGunFired EvtPlayerGunFired;//

    public delegate void Event_BulletDestroy(Bullet b);
    public static Event_BulletDestroy EvtBulletDestroy;

    public delegate void Event_SoundVolumeChanged(float curentVol);
    public static Event_SoundVolumeChanged EvtSoundVolumeChanged;

    public delegate void Event_KillLockingFish(Player killer);
    public static Event_KillLockingFish EvtKillLockingFish;//杀死正在锁定的鱼

    //public delegate void Event_FreezeAllFish();
    //public static Event_Generic EvtFreezeAllFishBegin;
    //public static Event_Generic EvtFreezeAllFishEnd;


    public delegate void Event_BackGroundClearAllData_Before();
    public static Event_BackGroundClearAllData_Before EvtBGClearAllData_Before;//后台清空所有数据

    public delegate void Event_BackGroundChangeArenaType(ArenaType oldType,ArenaType newType);
    public static Event_BackGroundChangeArenaType EvtBGChangeArenaType;//后台改变场地类型

    public delegate void Event_LanguageChanged(Language l);
    public static Event_LanguageChanged EvtLanguageChange;

    //变量
    public static int GameIdx
    {
        get { return Singleton.mGameIdx; }
        set
        {
            if (Singleton.mGameIdx != value)
            {
                
                Singleton.mGameIdx = value;

#if ENABLE_MCU_VERIFY
                Singleton.ArcIO.RequestHardwareInfo();//由于GameIdx改变,所以请求重新验证硬件
#endif
                
            }
        }
    }
    public int mGameIdx = 2;//渔乐万能
    public static int MainVersion = 211;
    public static int SubVersion = 200;
    

    public Transform TopLeftTs;
    public BackStageSetting BSSetting
    {
        get
        {
            if (mBSSetting == null)
            {
                mBSSetting = GetComponent<BackStageSetting>();
                mBSSetting.TryNewPersistentDatas();
            }
            return mBSSetting;
        }
    }
    private BackStageSetting mBSSetting;

    public PopupDigitFishDie PopupDigitFishdie;//鱼死亡数字 
    public Ef_FishDieEffectAdditive Ef_FishDieEffectAdd;//鱼炸弹爆炸效果
    public FishGenerator FishGenerator;//鱼群生成
    public SceneBGManager SceneBGMgr;//背景切换管理
    public ScenePreludeManager ScenePreludeMgr;//开场鱼阵管理
    public BackstageMain Prefab_Backstage;//后台
    //public GameStart GameStart_;//游戏开始画面
    public SoundManager SoundMgr;//声音

    public Player Prefab_Player;//玩家Prefab
    public GameObject[] Prefab_PlayerCreatorSet;//玩家创建者集合（即所有PlayerCreator的父对象）
    [System.NonSerialized]
    public Player[] Players;
    [System.NonSerialized]
    public Rect WorldDimension; 

    [System.NonSerialized]
    public int NumFishAlive = 0;//活鱼数目

    //public WebScoreScaleRatio[] WebScoreSizeRatio;

    public float TimeNormalScene = 240F;//普通场景持续时间 
    [System.NonSerialized]
    public int ScreenNumUsing = 1;//使用到得屏幕数

    [System.NonSerialized]
    public bool IsRightControlBoard = true;//是否通过验证的控制板
//#if UNITY_EDITOR
	public bool OneKillDie = false;//

//#endif
    public float GameSpeed = 1F;//游戏速度
    //public float 
    [System.NonSerialized]
    public INemoControlIO ArcIO;//串口连接
    
    //零碎对象
    public GameObject Prefab_GOPrepareEnterBS;//将进设置
    public GameDataViewer Prefab_GameDataViewer;//左上,游戏数据小面板
    public GameObject Prefab_SprGameErrorBG;//游戏打断提示背景
    public tk2dTextMesh Prefab_TextGameError;//游戏打断提示信息

    public LanguageItem LI_USBNotAvailable;//本地化:usb设备不存在
    public LanguageItem[] LI_BoardNotAvailable;//本地化:控制板通信故障(序列号0:单屏,序列号1:双屏)
    public LanguageItem LI_RunTimeout;//本地化:运行时间到,    请延时 21

    public int[] GunStylesScore;//根据该分数改变枪类型,如:= new int[]{49,999,10000}代表: 1-49,二管炮;50~999,三管炮; 1000~10000代表四管炮
    //public List<Fish> FishInScene;//在场景上的鱼
    public bool IsAutoStart = false;
    //静态变量
    public static float WorldWidth = 3.555555555555556F;
    public static float WorldHeight = 2F;

    public static bool IsEditorShutdown = false;//程序是否已经关闭(用于在Editor模式中,如果是程序已经关闭,则不创建对象)
    public static State State_;//GameMain状态
    
    //私有变量
    private static GameMain mSingleton;
    private int mCurrentControlPlayerId = 0;
    private bool mIsEnteringBackstage = false;//是否正在进入后台
    private GameDataViewer mCurGameDataViewer;

    private bool mDisableAllPlayerKey = false;
    private bool mDisableBackGroundKey = false;

    private static float mLocalY_usbError = 49.5F;//usb设备不存在 y定位
    private static float mLocalY_remainTimeZero = 0F;//剩余时间为0,y定位
    private static float mLocalY_ioBoardError = -49.5F;//控制板通信故障,y定位

    private GameObject mGoUSBErroHint;//Usb错误显示
    private GameObject mGoIoBoardErroHint;//控制板连接显示

    private int[] mPControllerIDToPlayerID;//CtrlID转PlayerID (数组索引是PControllerID,内容是PlayerID)

    private bool[] mControlBoardState;//控制板连接状态,由 Handle_CtrlBoardStateChanged 更新true:连接,false:断开

    public static bool IsMainProcessPause = false;//主进程暂停
    //属性
    public static GameMain Singleton
    {
        get
        {
            if (mSingleton == null)
            {
                mSingleton = GameObject.FindObjectOfType(typeof(GameMain)) as GameMain; 
                if (mSingleton!=null && mSingleton.ArcIO == null)
                {
                    mSingleton.ArcIO = mSingleton.GetComponent($"INemoControlIO") as INemoControlIO;

                    INemoControlIO arcio = mSingleton.ArcIO;
                    //ArcIO = GetComponent("INemoControlIO") as INemoControlIO;
                    if (arcio != null)
                    {
                        arcio.EvtKey += mSingleton.Handle_ArcKeyEvent;
                        //arcio.HandlerBackGroundKeyEvent += Handle_ArcBackStageKeyEvent;
                        arcio.EvtInsertCoin += mSingleton.Hanlde_ArcInsertCoin;
                        //NemoUsbHid usbHid = arcio.GetComponent<NemoUsbHid>();
                        arcio.EvtOpened += mSingleton.Handle_UsbHid_Opened;
                        arcio.EvtClosed += mSingleton.Handle_UsbHid_Closed;
                        arcio.EvtHardwareInfo += mSingleton.Handle_HardwareInfo;
                        arcio.EvtCtrlBoardStateChanged += mSingleton.Handle_CtrlBoardStateChanged;

#if !UNITY_EDITOR//在编辑器模式默认不连接硬件
#if ENABLE_MCU_VERIFY
                        mSingleton.IsRightControlBoard = false;
#endif
#endif
                        arcio.Open();

                    }

                    
                }
            }
            return mSingleton;
        }
    }

    /// <summary>
    /// 是否联屏幕
    /// </summary>
    /// <param name="glt"></param>
    /// <returns></returns>
    public bool IsScreenNet()
    {
        return BSSetting.GunLayoutType_.Val >= GunLayoutType.L_L2S;
    }

    /// <summary>
    /// 是否大于一个屏幕
    /// </summary>
    /// <returns></returns>
    public bool IsMoreThanOneScreen( )
    {
        return BSSetting.GunLayoutType_.Val >= GunLayoutType.S_L6S && BSSetting.GunLayoutType_.Val <= GunLayoutType.L_W16S;
    }

    /// <summary>
    /// 是否单分机
    /// </summary>
    /// <returns></returns>
    public bool IsSingleContorlBoard()
    {
        switch(BSSetting.GunLayoutType_.Val)
        {
            case GunLayoutType.L1:
            case GunLayoutType.L2:
            case GunLayoutType.L3:
            case GunLayoutType.L4:
            case GunLayoutType.W4:
            case GunLayoutType.W6:
            case GunLayoutType.W8:
            case GunLayoutType.W10:
            case GunLayoutType.S_L6D:
            case GunLayoutType.S_L8D:
            case GunLayoutType.S_W8D:
            case GunLayoutType.L_L2D:
            case GunLayoutType.L_L4D:
            case GunLayoutType.L_L6D:
            case GunLayoutType.L_L8D:
            case GunLayoutType.L_W8D:
            case GunLayoutType.L_W10D: 
                return true;
            
        }
        return false;

    }
    /// <summary>
    /// 是否在后台中
    /// </summary>
    public bool IsInBackstage
    {
        get
        {
            return mIsEnteringBackstage;//暂用该属性,因为进入后台后也不会改变为true
        }
    }

    /// <summary>
    /// 通过script oder一定在最开头初始化
    /// </summary>
    void Awake()
    {
        mSingleton = Singleton;//初始化数据
        //Singleton.ArcIO.Open();

        EvtInputKey += Handle_InputKey;


        State_ = State.Idle;

        //使用的屏幕数量
        ScreenNumUsing = IsMoreThanOneScreen() ? 2 : 1;

        //初始化 2D世界范围
        WorldDimension.x = Defines.WorldDimensionUnit.x - Defines.WorldDimensionUnit.width * 0.5F * (ScreenNumUsing - 1);
        WorldDimension.y = Defines.WorldDimensionUnit.y;
        WorldDimension.width = Defines.WorldDimensionUnit.width * ScreenNumUsing;
        WorldDimension.height = Defines.WorldDimensionUnit.height;


        //鱼序号赋值
        for (int i = 0; i != FishGenerator.Prefab_FishAll.Length; ++i)
        {
            FishGenerator.Prefab_FishAll[i].TypeIndex = i;
        }


        mPControllerIDToPlayerID = new int[Defines.MaxNumPlayer];
        //初始化人数
        Defines.NumPlayer = Defines.GunLayoutTypeToNumPlay[(int)BSSetting.GunLayoutType_.Val];

        
       


 
        //设置分辨率
#if UNITY_EDITOR
        UnityEditor.PlayerSettings.defaultScreenWidth = Defines.ScreenWidthUnit * ScreenNumUsing;
        UnityEditor.PlayerSettings.defaultScreenHeight = Defines.ScreenHeightUnit ;
#else
        Screen.SetResolution(Defines.ScreenWidthUnit * ScreenNumUsing,Defines.ScreenHeightUnit,false);
#endif
        //显示USB连接信息
#if !UNITY_EDITOR
        if(!ArcIO.IsOpen())
            ViewUsbHidErroHint();
#endif

        //设置是否锁帧
        //Application.targetFrameRate = 60;

        mControlBoardState = new bool[ScreenNumUsing];
        for (int i = 0; i != ScreenNumUsing; ++i)
            mControlBoardState[i] = true;

        Time.timeScale = GameSpeed;


    }

    void Start()
    {
        if (BSSetting.CodePrintCurrentAction.Val)//打码则全清状态,观察玩家分数是否改变
        {
            Event_PlayerScoreChanged evtScoreChanged = null;
            evtScoreChanged = (Player p, int ScoreNew, int scoreChange) =>
            {
                BSSetting.CodePrintCurrentAction.Val = false;//还原到第一步

                EvtPlayerScoreChanged -= evtScoreChanged;

            };
            EvtPlayerScoreChanged += evtScoreChanged;
        }


        mDisableAllPlayerKey = true;
        mDisableBackGroundKey = true;
        //if (GameStart_ != null)
        //{
        //    GameStart_.EvtGameInitFinish += Handle_GameInitFinish;
        //}
        //else
        //{
        //    Handle_GameInitFinish();
        //}
        if (IsAutoStart)
            StartCoroutine(_Coro_DelayStartGame());
    }

    /// <summary>
    /// 用于其他脚本可以在Start订阅StartGame事件,并响应
    /// </summary>
    /// <returns></returns>
    IEnumerator _Coro_DelayStartGame()
    {
        yield return 0;
        StartGame();
    }
    //void OnDestroy()
    //{
    //    ArcIO.Close();
    //}

    /// <summary>
    /// 开始游戏
    /// </summary>
    public void StartGame()
    {
        if (State_ != State.Idle)
            return;
        State_ = State.Normal;


        //初始化所有玩家
        Transform prefabPlayerCreatorTs = Prefab_PlayerCreatorSet[(int)BSSetting.GunLayoutType_.Val].transform;
        GameObject playersParent = new GameObject("Players");
        playersParent.transform.parent = transform;
        playersParent.transform.localPosition = Vector3.zero;
        Players = new Player[Defines.NumPlayer];

        foreach (Transform t in prefabPlayerCreatorTs)
        {

            PlayerCreator pc = t.GetComponent<PlayerCreator>();
            Player pNew = pc.CreatePlayer();
            pNew.transform.parent = playersParent.transform;

            Players[pNew.Idx] = pNew;
            mPControllerIDToPlayerID[pNew.CtrllerIdx] = pNew.Idx;
        }

        //打开所有按键
        mDisableAllPlayerKey = false;
        mDisableBackGroundKey = false;
        //延时检查
        //StartCoroutine(_Coro_CheckRemainRunTime());

        //StartCoroutine(_Coro_DeleteNullFishInScene());

        //难度调节
        if (BSSetting.Dat_RemoteDiffucltFactor.Val != 0)
        {
            GameOdds.DifficultFactor = (float)(BSSetting.Dat_RemoteDiffucltFactor.Val + 1);
        }
        
        GameOdds.GainRatio = GameOdds.GainRatios[(int)(BSSetting.GameDifficult_.Val)];
        GameOdds.GainRatioConditFactor = GameOdds.GainRatioConditFactors[(int)(BSSetting.ArenaType_.Val)];
        //ScenePrelude pl = ScenePreludeMgr.DoPrelude();

        StartCoroutine("_Coro_MainProcess");
        
        Pool_GameObj.Init();
 
        if (BSSetting.IsNeedPrintCodeAtGameStart.Val)
        {
            EnterPrintCode();
        }

        //SceneBGMgr.NewBG();

        if (EvtMainProcess_StartGame != null)
            EvtMainProcess_StartGame();
    }
  
    void Handle_ArcKeyEvent(int ctrllerID, NemoCtrlIO_Key k, bool downOrUp)
    {
        if (ctrllerID == 21)//后台按键
        {
            Handle_ArcBackStageKeyEvent(k, downOrUp);
            return;
        }
        if (!mDisableAllPlayerKey)
        {
            switch (k)
            {
                case NemoCtrlIO_Key.Up:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.Up, downOrUp);
                    break;
                case NemoCtrlIO_Key.Down:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.Down, downOrUp);
                    break;
                case NemoCtrlIO_Key.Left:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.Left, downOrUp);
                    break;
                case NemoCtrlIO_Key.Right:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.Right, downOrUp);
                    break;
                case NemoCtrlIO_Key.B:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.Fire, downOrUp);
                    break;
                case NemoCtrlIO_Key.A:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.Advance, downOrUp);
                    break;
                case NemoCtrlIO_Key.D:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.ScoreUp, downOrUp);
                    break;
                case NemoCtrlIO_Key.C:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.ScoreDown, downOrUp);
                    break;
                case NemoCtrlIO_Key.F:
                    EvtInputKey(mPControllerIDToPlayerID[ctrllerID], HpyInputKey.OutBounty, downOrUp);
                    break;
            }
        
        }
   
        
    }
    void Handle_ArcBackStageKeyEvent(NemoCtrlIO_Key k, bool downOrUp)
    {
        if (!mDisableBackGroundKey)
        {
            switch (k)
            {
                case NemoCtrlIO_Key.Up:
                    EvtInputKey(0, HpyInputKey.BS_Up, downOrUp);
                    break;
                case NemoCtrlIO_Key.Down:
                    EvtInputKey(0, HpyInputKey.BS_Down, downOrUp);
                    break;
                case NemoCtrlIO_Key.Left:
                    EvtInputKey(0, HpyInputKey.BS_Left, downOrUp);
                    break;
                case NemoCtrlIO_Key.Right:
                    EvtInputKey(0, HpyInputKey.BS_Right, downOrUp);
                    break;
                case NemoCtrlIO_Key.A:
                    EvtInputKey(0, HpyInputKey.BS_Confirm, downOrUp);
                    break;
                case NemoCtrlIO_Key.B:
                    EvtInputKey(0, HpyInputKey.BS_Cancel, downOrUp);
                    break;
                case NemoCtrlIO_Key.C:
                    EvtInputKey(0, HpyInputKey.BS_GameLite, downOrUp);
                    break;
            }
        }
    
        
    }

    void Hanlde_ArcInsertCoin(uint count, int ctrllerID)
    { 
        if (!mIsEnteringBackstage)
        {
            int num = (int)count;

            Players[mPControllerIDToPlayerID[ctrllerID]].ChangeScore(num * BSSetting.InsertCoinScoreRatio.Val);

            BSSetting.His_CoinInsert.Val += num;
            BSSetting.His_GainCurrent.Val += num;
            BSSetting.UpdateGainCurrentAndTotal();
        }
    }

    IEnumerator _Coro_MainProcess()
    {
		if (EvtMainProcess_FirstEnterScene != null)
            EvtMainProcess_FirstEnterScene();

        float tmpTime = 0F;
        while (true)
        {
            State_ = State.Normal;
            SoundMgr.PlayNewBgm();

            FishGenerator.StartFishGenerate(); 
            //yield return new WaitForSeconds(TimeNormalScene);

            tmpTime = Time.time;
            while (IsMainProcessPause || Time.time - tmpTime < TimeNormalScene)
            {
                yield return 0;
            }
 
            State_ = State.BeforeSweeping_WaitBulletClear;
            //*准备过场
            if (EvtMainProcess_PrepareChangeScene != null)
                EvtMainProcess_PrepareChangeScene();

            //停止玩家攻击
            foreach (Player p in Players)
            {
                p.GunInst.Fireable = false;
                p.CanChangeScore = false;
            } 
            //等待场景所有子弹消失
            Player[] tmpPlayers = new Player[Players.Length];
            for (int i = 0; i != Players.Length; ++i)
                tmpPlayers[i] = Players[i];
            while (true)
            {
                float numPlayerHaventBullet = 0;
                for (int i = 0; i != tmpPlayers.Length; ++i)
                {
                    if (tmpPlayers[i] != null && tmpPlayers[i].GunInst.NumBulletInWorld == 0)
                    {
                        tmpPlayers[i] = null;
                    }
                    if (tmpPlayers[i] == null)
                        ++numPlayerHaventBullet;
                }
                if (numPlayerHaventBullet == tmpPlayers.Length)
                    break;
                yield return 0;
            }
            State_ = State.Sweeping;
            //停止出鱼
            FishGenerator.StopFishGenerate();
            SoundMgr.StopBgm();

            //开始播放过场,清鱼
            SceneBGMgr.Sweep();
            yield return new WaitForSeconds(SceneBGMgr.UseTime+0.3F);
            State_ = State.Preluding;

            SoundMgr.PlayNewBgm();

            if (EvtMainProcess_FinishChangeScene != null)
                EvtMainProcess_FinishChangeScene();
            //恢复玩家攻击
            foreach (Player p in Players)
            {
                p.GunInst.Fireable = true;
                p.CanChangeScore = true;
            }
            

            //开场鱼阵

            ScenePrelude pl = ScenePreludeMgr.DoPrelude();
            //string preludeName = pl.gameObject.name;
            //Debug.Log("sceneprelude start = " + preludeName+ "   fish num = "+NumFishAlive);
            bool waitPrelude = true;
            pl.Evt_PreludeEnd += () => { waitPrelude = false; };
            while (waitPrelude)
            {
                yield return new WaitForSeconds(0.1F);
            }
            //Debug.Log("sceneprelude end = " + preludeName + "   fish num = " + NumFishAlive);
 
            //loop:下一次正常出鱼
            if (EvtMainProcess_FinishPrelude != null)
                EvtMainProcess_FinishPrelude();
        }
    }

    //检查游戏剩余时间
    IEnumerator _Coro_CheckRemainRunTime()
    {
        yield return 0;
        while (true)
        {
            if (BSSetting.GetRemainRuntime() <= 0)
            {
                //显示到时消息
                for (int i = 0; i != ScreenNumUsing; ++i)
                {
                    GameObject goErrBG = Instantiate(Prefab_SprGameErrorBG) as GameObject;
                    goErrBG.transform.parent = transform;

                    goErrBG.transform.localPosition = new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_remainTimeZero, 0.03F);
                    tk2dTextMesh textErr = Instantiate(Prefab_TextGameError) as tk2dTextMesh;
                    textErr.transform.parent = goErrBG.transform;
                    textErr.transform.localPosition = new Vector3(0F, 0F, -0.001F);
                    textErr.text = LI_RunTimeout.CurrentText;// "运行时间到,    请延时 21";
                    textErr.Commit();
                }
                

                while(true)
                {
                    foreach (Player p in Players)
                        p.GunInst.Fireable = false;

                    yield return 0;
                }
            }

            yield return new WaitForSeconds(30F);
        }
    }

    //检查控制板,USB设备是否存在
    //IEnumerator _Coro_CheckHardwareState()
    //{
    //    bool hardWareRespon = false;
    //    bool isUsbOk = true;
    //    bool isArcOk= true;
    //    ArcIO.HandlerStateCheck += (bool usbOk, bool arcOk) =>
    //    {
    //        isUsbOk = usbOk;
    //        isArcOk = arcOk;
    //        hardWareRespon = true;
    //    };

    //    while (true)
    //    {
    //        ArcIO.CheckState();//发送检查命令
    //        yield return new WaitForSeconds(1F);
    //        if (hardWareRespon)
    //        {
    //            if (!isArcOk)
    //            {
    //                tk2dSprite sprErrBG = Instantiate(Prefab_SprGameErrorBG) as tk2dSprite;
    //                sprErrBG.transform.parent = transform;
    //                sprErrBG.transform.localPosition = new Vector3(0, mLocalY_ioBoardError, 0.03F);
    //                tk2dTextMesh textErr = Instantiate(Prefab_TextGameError) as tk2dTextMesh;
    //                textErr.transform.parent = sprErrBG.transform;
    //                textErr.transform.localPosition = new Vector3(0F, 0F, -0.001F);
    //                textErr.text = "控制板通信故障";
    //                textErr.Commit();
    //            }
    //        }
    //    }
    //}

    //IEnumerator _Coro_DeleteNullFishInScene()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(20);
    //        List<Fish> fishToDelete = new List<Fish>();
    //        foreach (Fish f in FishInScene)
    //        {
    //            if (f == null)
    //                fishToDelete.Add(f);
    //        }
    //        foreach (Fish f in fishToDelete)
    //        {
    //            FishInScene.Remove(f);
                
    //        }
    //    }
    //}

    //IEnumerator _Coro_ClearPool()
    //{
    //    List<Pool_GameObj> clearQueue = new List<Pool_GameObj>();
    //    while (true)
    //    {
    //        foreach (KeyValuePair<int, Pool_GameObj> kvp in Pool_GameObj.msPoolsDict)
    //        {
    //            clearQueue.Add(kvp.Value);
    //        }

    //        foreach (Pool_GameObj obj in clearQueue)
    //        {
    //            obj.GC_Lite();
    //        }
    //        clearQueue.Clear();
    //        yield return new WaitForSeconds(Defines.Interval_PoolGC);
    //    }
    //}

    bool mIsAutoFiring = true;
    void Update()
    {
        if (mDisableAllPlayerKey)
            goto TAG_BackStageKeyProcess;
        //模拟按键
        if (  Input.GetKeyDown(KeyCode.P))//所有玩家一起开火
        {
            if (EvtInputKey != null)
                for (int i = 0; i != Defines.NumPlayer; ++i )
                    EvtInputKey(i, HpyInputKey.Fire, mIsAutoFiring);
            mIsAutoFiring = !mIsAutoFiring;
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Fire,true);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.K))//所有玩家一起加强
        {
            if (EvtInputKey != null)
                for(int i = 0; i != Defines.NumPlayer; ++i)
                    EvtInputKey(i, HpyInputKey.Advance, true);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Advance, true);
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.OutBounty, true);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Up, true);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Down, true);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Left, true);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Right, true);
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            mCurrentControlPlayerId = (mCurrentControlPlayerId + 1) % Defines.NumPlayer;
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.LeftBracket))//所有玩家一起下分
        {
            if (EvtInputKey != null)
            {
                for (int i = 0; i != Defines.NumPlayer; ++i)
                    EvtInputKey(i, HpyInputKey.ScoreDown, true);
            }
        }
        else if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.Equals))//所有玩家一起上分 *100
        {
            if (EvtInputKey != null)
            {
                for (int i = 0; i != Defines.NumPlayer; ++i)
                {
                    for (int j = 0; j != 10; ++j)
                        Players[i].ScoreUp(true);
                }
            }
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.RightBracket))//所有玩家一起上分
        {
            if (EvtInputKey != null)
            {
                for (int i = 0; i != Defines.NumPlayer; ++i)
                    EvtInputKey(i, HpyInputKey.ScoreUp, true);
            }
        }
        else if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.ScoreDown, true);
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.ScoreUp, true);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.OutBounty, true);
        }
         
   

        ////////////////////////////////keyup/////////////////////////////////
        if (Input.GetKeyUp(KeyCode.J))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Fire, false);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.K))
        {
            if (EvtInputKey != null)
                for (int i = 0; i != Defines.NumPlayer; ++i )
                    EvtInputKey(i, HpyInputKey.Advance, false);
        }
        else if (Input.GetKeyUp(KeyCode.K))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Advance, false);
        }
        else if (Input.GetKeyUp(KeyCode.W))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Up, false);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Down, false);
        }
        else if (Input.GetKeyUp(KeyCode.A))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Left, false);
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.Right, false);
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.LeftBracket))
        {
            if (EvtInputKey != null)
            {
                for (int i = 0; i != Defines.NumPlayer; ++i)
                    EvtInputKey(i, HpyInputKey.ScoreDown, false);
            }
        }
        else if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyUp(KeyCode.RightBracket))
        {
            if (EvtInputKey != null)
            {
                for (int i = 0; i != Defines.NumPlayer; ++i)
                    EvtInputKey(i, HpyInputKey.ScoreUp, false);
            }
        }
        else if (Input.GetKeyUp(KeyCode.LeftBracket))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.ScoreDown, false);
        }
        else if (Input.GetKeyUp(KeyCode.RightBracket))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.ScoreUp, false);
        }
        else if (Input.GetKeyUp(KeyCode.E))
        {
            if (EvtInputKey != null)
                EvtInputKey(mCurrentControlPlayerId, HpyInputKey.OutBounty, false);
        }

        TAG_BackStageKeyProcess:
        //后台
        if (!mDisableBackGroundKey)
        {
            if (Input.GetKeyDown(KeyCode.N))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Confirm, true);
            }
            else if (Input.GetKeyDown(KeyCode.M))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Cancel, true);
            }
            else if (Input.GetKeyDown(KeyCode.F))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Up, true);
            }
            else if (Input.GetKeyDown(KeyCode.V))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Down, true);
            }
            else if (Input.GetKeyDown(KeyCode.C))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Left, true);
            }
            else if (Input.GetKeyDown(KeyCode.B))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Right, true);
            }

            //后台keyup 
            if (Input.GetKeyUp(KeyCode.N))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Confirm, false);
            }
            else if (Input.GetKeyUp(KeyCode.M))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Cancel, false);
            }
            else if (Input.GetKeyUp(KeyCode.F))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Up, false);
            }
            else if (Input.GetKeyUp(KeyCode.V))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Down, false);
            }
            else if (Input.GetKeyUp(KeyCode.C))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Left, false);
            }
            else if (Input.GetKeyUp(KeyCode.B))
            {
                if (EvtInputKey != null)
                    EvtInputKey(mCurrentControlPlayerId, HpyInputKey.BS_Right, false);
            }
        }
        
    }

    void Handle_InputKey(int p, HpyInputKey k, bool down)
    {
        Players[p].OnKeyDown(k, down);

        if (down && k == HpyInputKey.BS_Confirm)
        {
            StartCoroutine("_Coro_PrepareEnterBackStage");
        }
        else if (!down && k == HpyInputKey.BS_Confirm)
        {
            StopCoroutine("_Coro_PrepareEnterBackStage");
        }

        if (down && k == HpyInputKey.OutBounty)
        {
            Players[p].OutBountyButtom();
        }

        if (down && (k == HpyInputKey.BS_Up || k == HpyInputKey.BS_Down))
        {
            if (!mIsEnteringBackstage)
            {
                if (mCurGameDataViewer == null)
                {
                    mCurGameDataViewer = Instantiate(Prefab_GameDataViewer) as GameDataViewer;
                    mCurGameDataViewer.IsOnlyViewNumber = k == HpyInputKey.BS_Up ? false : true;
                    mCurGameDataViewer.transform.position = new Vector3(WorldDimension.xMin, WorldDimension.yMax, Defines.GlobleDepth_GameDataViewer);
                    if (ScreenNumUsing > 1)
                    {
                        for (int i = 0; i != ScreenNumUsing - 1; ++i)
                        {
                            GameDataViewer gdv = Instantiate(Prefab_GameDataViewer) as GameDataViewer;
                            gdv.transform.parent = mCurGameDataViewer.transform;
                            gdv.transform.position = new Vector3(WorldDimension.xMin + Defines.WorldDimensionUnit.width * (i + 1)
                                , WorldDimension.yMax, Defines.GlobleDepth_GameDataViewer);
                        }
                    }
                }
                else
                {
                    Destroy(mCurGameDataViewer.gameObject);
                    mCurGameDataViewer = null;
                }
            }

        }
    }

    public void EnterBackstage()
    {
        if (!mIsEnteringBackstage)
        {
            mIsEnteringBackstage = true;
            StartCoroutine(_Coro_EnterBackstage());
        }
    }
    void EnterPrintCode()
    {
 
        mIsEnteringBackstage = true;
        mDisableAllPlayerKey = true;
        //停止玩家攻击
        foreach (Player p in Players)
        {
            p.GunInst.Fireable = false;
        }

        //停止出鱼
        FishGenerator.StopFishGenerate();

        BackstageMain bsMain = Instantiate(Prefab_Backstage) as BackstageMain;
        bsMain.EnterPrintCode();
        State_ = State.BackStage;
    }

    IEnumerator _Coro_PrepareEnterBackStage()
    {
        yield return new WaitForSeconds(1F);
        EnterBackstage();
    }

    //void OnGUI()
    //{
    //    if (GUILayout.Button("fish"))
    //    {
    //        int nullFishNum = 0;
    //        foreach (Fish fNull in FishInScene)
    //        {
    //            if (fNull == null)
    //                ++nullFishNum;
    //        }
    //        Debug.Log("nullFishNum = " + nullFishNum);
    //    }
    //}
    IEnumerator _Coro_EnterBackstage()
    {
        

        //停止玩家攻击
        foreach (Player p in Players)
        {
            p.GunInst.Fireable = false;
        }

       

        //将进设置
        GameObject[] goPrepareEnterBSs = new GameObject[ScreenNumUsing];
        for(int i = 0; i != ScreenNumUsing; ++i)
        {
            goPrepareEnterBSs[i] = Instantiate(Prefab_GOPrepareEnterBS) as GameObject;
            goPrepareEnterBSs[i].transform.position = new Vector3(WorldDimension.x + WorldDimension.width * 0.5F * i, WorldDimension.y+WorldDimension.height, Defines.GlobleDepth_PrepareInBG);

        }
        
        yield return new WaitForSeconds(3F);

        //等待子弹结束
        Player[] tmpPlayers = new Player[Players.Length];
        for (int i = 0; i != Players.Length; ++i)
            tmpPlayers[i] = Players[i];
        while (true)
        {
            float numPlayerHaventBullet = 0;
            for (int i = 0; i != tmpPlayers.Length; ++i)
            {
                if (tmpPlayers[i] != null && tmpPlayers[i].GunInst.NumBulletInWorld == 0)
                {
                    tmpPlayers[i] = null;
                }
                if (tmpPlayers[i] == null)
                    ++numPlayerHaventBullet;
            }
            if (numPlayerHaventBullet == tmpPlayers.Length)
                break;
            yield return 0;
        }

	//todo 可能在某些过程中 终止会出现bug,待测试
        StopCoroutine("_Coro_MainProcess");
	
        //停止出鱼
        FishGenerator.StopFishGenerate();

        Instantiate(Prefab_Backstage);
        foreach (GameObject go in goPrepareEnterBSs)
        {
            Destroy(go);
        }
        if (mCurGameDataViewer != null)
        {
            Destroy(mCurGameDataViewer.gameObject);
            mCurGameDataViewer = null;
        }

        mDisableAllPlayerKey = true;
        State_ = State.BackStage;
    }


    void Handle_UsbHid_Opened()
    {
        if (mGoUSBErroHint != null)
        {
            Destroy(mGoUSBErroHint);
            mGoUSBErroHint = null;
        }

#if ENABLE_MCU_VERIFY
        //请求硬件信息
        ArcIO.RequestHardwareInfo();
#endif
    }

    void Handle_UsbHid_Closed()
    {
        if (mGoUSBErroHint != null)
        {
            Destroy(mGoUSBErroHint);
            mGoUSBErroHint = null;
        }
        //[bug],因为由OnDestroy触发,Unity可能会出现Some objects were not cleaned up when closing the scene错误提示
        if(!IsEditorShutdown)
            ViewUsbHidErroHint();
    }

    void Handle_HardwareInfo(int gameIdx, int mainVersion, int subVersion, bool isVerifySucess)
    {
        StartCoroutine(_Delay_Handle_HardwareInfo(gameIdx, mainVersion, subVersion, isVerifySucess));
    }

    IEnumerator _Delay_Handle_HardwareInfo(int gameIdx, int mainVersion, int subVersion, bool isVerifySucess)
    {
        yield return new WaitForSeconds(1F);
        HideUsbHidErroHint();
        if (GameIdx == -1)//只验证加密板归属
        {
            if (isVerifySucess)
            {
                IsRightControlBoard = true;
            }
            else
            {
                ViewUsbHidErroHint();
                IsRightControlBoard = false;
            }
            yield break;
        }
        else//带版本号
        {
            if (isVerifySucess && GameIdx == gameIdx)
            {
                IsRightControlBoard = true;
            }
            else
            {
                ViewUsbHidErroHint();
                IsRightControlBoard = false;
            }
        }
    }

    /// <summary>
    /// 需要延时处理控制板状态,因为bss的变量未初始化
    /// </summary>
    /// <param name="controlBoardId"></param>
    /// <param name="connectState"></param>
    /// <returns></returns>
    IEnumerator _Delay_Handle_CtrlBoardStateChanged(int controlBoardId, bool connectState)
    {
        yield return new WaitForSeconds(1F);
        string[] strViewStringAry = new string[ScreenNumUsing];
        if (IsSingleContorlBoard())//单分机模式
        {

            if (controlBoardId == 0)//单分机板模式时,只有第一块分机板状态变化才作出相应
            {
                //Debug.Log("connect state = " + connectState);
                if (connectState)//连接中
                {
                    if (mGoIoBoardErroHint != null)
                    {
                        Destroy(mGoIoBoardErroHint);
                        mGoIoBoardErroHint = null;
                    }
                }
                else//没连接
                {
                    if (mGoIoBoardErroHint == null)
                    {
                        for (int i = 0; i != ScreenNumUsing; ++i)
                        {
                            strViewStringAry[i] = string.Format(LI_BoardNotAvailable[ScreenNumUsing - 1].CurrentText, 1);

                            if (mGoIoBoardErroHint == null)
                            {
                                mGoIoBoardErroHint = _CreateErroHintBar(new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_ioBoardError, 0.03F), strViewStringAry[i]);
                            }
                            else
                            {
                                GameObject goHintSubGo = _CreateErroHintBar(new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_ioBoardError, 0.03F), strViewStringAry[i]);
                                goHintSubGo.transform.parent = mGoIoBoardErroHint.transform;
                            }
                        }
                    }
                    
                }
            }
        }
        else//双分机模式
        {
            if (mGoIoBoardErroHint != null)
            {
                Destroy(mGoIoBoardErroHint);
                mGoIoBoardErroHint = null;
            }

            for (int i = 0; i != ScreenNumUsing; ++i)
            {
                strViewStringAry[i] = string.Format(LI_BoardNotAvailable[ScreenNumUsing - 1].CurrentText, i + 1);// "分机板" + (i + 1) + "通信故障";
            }

 
            mControlBoardState[controlBoardId] = connectState;//更新连接状态

            for (int i = 0; i != mControlBoardState.Length; ++i)
            {
                if(!mControlBoardState[i])//非连接状态则显示
                {
                    if (mGoIoBoardErroHint == null)
                        mGoIoBoardErroHint = _CreateErroHintBar(new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_ioBoardError, 0.03F), strViewStringAry[i]);
                    else
                    {
                        GameObject goHintSubGo = _CreateErroHintBar(new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_ioBoardError, 0.03F), strViewStringAry[i]);
                        goHintSubGo.transform.parent = mGoIoBoardErroHint.transform;
                    }
                }
                
            }

        }
             
    }
    //连接状态改变
    void Handle_CtrlBoardStateChanged(int controlBoardId,bool connectState)
    {
        StartCoroutine(_Delay_Handle_CtrlBoardStateChanged(controlBoardId, connectState));
    }

    void ViewUsbHidErroHint()
    {
        //手机设备上不显示找不到USB设备字样
        return;
        for (int i = 0; i != ScreenNumUsing; ++i)
        {
            if (mGoUSBErroHint == null)
            {
                 
                mGoUSBErroHint = _CreateErroHintBar(new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_usbError, 0.03F), LI_USBNotAvailable.CurrentText);
            }
            else
            {
                GameObject goHintSubGo = _CreateErroHintBar(new Vector3(WorldDimension.x + (0.5F + i) * Defines.WorldDimensionUnit.width, mLocalY_usbError, 0.03F), LI_USBNotAvailable.CurrentText);
                goHintSubGo.transform.parent = mGoUSBErroHint.transform;
            }
        }

        ////mGoUSBErroHint = _CreateErroHintBar(new Vector3())
        //tk2dSprite sprErrBG = Instantiate(Prefab_SprGameErrorBG) as tk2dSprite;
        //mGoUSBErroHint = sprErrBG.gameObject;
        //sprErrBG.transform.parent = transform;
        //sprErrBG.transform.localPosition = new Vector3(0, mLocalY_usbError, 0.03F);
        //tk2dTextMesh textErr = Instantiate(Prefab_TextGameError) as tk2dTextMesh;
        //textErr.transform.parent = sprErrBG.transform;
        //textErr.transform.localPosition = new Vector3(0F, 0F, -0.001F);
        //textErr.text = "USB设备不存在";
        //textErr.Commit();
    }

    void HideUsbHidErroHint()
    {
        if (mGoUSBErroHint != null)
        {
            Destroy(mGoUSBErroHint);
            mGoUSBErroHint = null;
        }
    }

    GameObject _CreateErroHintBar(Vector3 worldPos, string text)
    {
        GameObject goErrBG = Instantiate(Prefab_SprGameErrorBG) as GameObject;

        goErrBG.transform.parent = transform;
        goErrBG.transform.localPosition = worldPos;
        tk2dTextMesh textErr = Instantiate(Prefab_TextGameError) as tk2dTextMesh;
        textErr.transform.parent = goErrBG.transform;
        textErr.transform.localPosition = new Vector3(0F, 0F, -0.001F);
        textErr.text = text;
        textErr.Commit();
        return goErrBG;
    }
    void OnApplicationQuit()
    {
        IsEditorShutdown = true;
    }
}
