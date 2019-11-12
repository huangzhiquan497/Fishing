using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Moudle_FileRead : MonoBehaviour
{
    //public struct player_data
    //{
    //    public int level;
    //    public float CurExp;
    //    public int AchieveSum;
    //    public int AchieveData;
    //    public int coin;
    //};
    
    //public static player_data newdata;
    public static PersistentData<bool, bool> FirstGiveMoney;        //第一次运行给分数

    public static PersistentData<bool,bool> GameFirstRun;          //第一次运行记录
    public static PersistentData<uint, uint> GameID;               //服务器分配的ID

    public static PersistentData<int, int> ServerHour;          //倒计时小时

    public static PersistentData<int, int> ServerMin;           //倒计时分钟

    public static PersistentData<int, int> ServerSec;           //倒计时秒

    public static PersistentData<float, float> GameExp;         //记录游戏经验
    public static PersistentData<int, int> GameLevel;           //记录游戏等级

    public static PersistentData<int, int> MissHitSum;          //连续打不到鱼次数

    public static PersistentData<int, int> HitSum;              //连续打中鱼次数

    public static PersistentData<int, int> GameSharkSum;        //记录打中鲨鱼总次数

    public static PersistentData<int, int> GameLanternSum;      //记录打中灯笼鱼总次数

    public static PersistentData<int, int> GameFishSum;         //记录打中多少条鱼

    public static PersistentData<int, int>[] GameEveryFish;    //记录每种鱼都打过一次

    public static PersistentData<bool, bool> firstRun;                 //记录第一次运行初始化一些数值
	
    public static PersistentData<bool, bool> BLV5;              //升到5级

    public static PersistentData<bool, bool> BLV10;             //升到10级

    public static PersistentData<bool, bool> BLV20;             //20

    public static PersistentData<bool, bool> BLV30;             //30

    public static PersistentData<bool, bool> BLV40;             //40
	
    public static PersistentData<bool, bool> NotLuck10;         //连续10发子弹没捕捉到鱼

    public static PersistentData<bool, bool> NotLuck20;         //连续20发子弹没捕捉到鱼

    public static PersistentData<bool, bool> Luck10;            //连续10发子弹都捉到鱼

    public static PersistentData<bool, bool> Luck20;            //连续20发子弹都捉到鱼
	
    public static PersistentData<bool, bool> GameFirstShark;    //记录游戏第一次打中鲨鱼

    public static PersistentData<bool, bool> CatchFish20;       //捕捉到20条鱼

    public static PersistentData<bool, bool> CatchLantern50;    //捕捉到50条灯笼鱼

    public static PersistentData<bool, bool> CatchShark100;     //捕捉到100条鲨鱼

    public static PersistentData<bool, bool> CatchEveryFish;    //每种鱼都捉到1次

    //public static PersistentData<bool, bool> Min30;             //1分钟捕捉到30条鱼

   // public static PersistentData<bool, bool> Min50;             //1分钟捕捉到50条鱼

    public static PersistentData<bool, bool> Gold500;           //金币达到500

    public static PersistentData<bool, bool> Gold1000;          //1000

    public static PersistentData<bool, bool> Gold10000;         //10000

    public static PersistentData<bool, bool> Gold100000;        //100000

    public static PersistentData<bool, bool> Gold1000000;       //1000000

    public static PersistentData<bool, bool> EveryReward;       //每日奖励

    public static PersistentData<int, int> EveryRewardDays;     //累计第几日登录奖励

    public static PersistentData<bool, bool> FirstChangeName;         //第一次改名

    public static PersistentData<bool, bool> First_Recharge;            //第一次充值

    public static PersistentData<bool, bool> First_ReceiveCoin;             //第一次领取倒计时4小时金币

    public Fish[] Prefab_AllFish;

    public Dictionary<int, int> mAllFish;

    void check_levle()
    {
        if (GameLevel.Val >= 5)
        {
            BLV5.Val = true;
        }
        if (GameLevel.Val >= 10)
        {
            BLV10.Val = true;
        }
        if (GameLevel.Val >= 20)
        {
            BLV10.Val = true;
        }
        if (GameLevel.Val >= 30)
        {
            BLV10.Val = true;
        }
        if (GameLevel.Val >= 40)
        {
            BLV10.Val = true;
        }
    }
    void check_gold()
    {
        if (MobileInterface.GetPlayerScore() >= 500)
        {
            Gold500.Val = true;
        }
        if (MobileInterface.GetPlayerScore() >= 1000)
        {
            Gold1000.Val = true;
        }
        if (MobileInterface.GetPlayerScore() >= 10000)
        {
            Gold10000.Val = true;
        }
        if (MobileInterface.GetPlayerScore() >= 100000)
        {
            Gold100000.Val = true;
        }
        if (MobileInterface.GetPlayerScore() >= 1000000)
        {
            Gold1000000.Val = true;
        }
    }
	// Use this for initialization
    void Awake()
    {
        if (FirstGiveMoney == null) FirstGiveMoney = new PersistentData<bool, bool>("FirstGiveMoney");

        if (GameFirstRun == null) GameFirstRun = new PersistentData<bool, bool>("GameFirstRun");
        if (GameID == null) GameID = new PersistentData<uint, uint>("GameID");

        mAllFish = new Dictionary<int, int>();
        for(int i = 0; i!=Prefab_AllFish.Length; ++i)
        {
            mAllFish.Add(Prefab_AllFish[i].TypeIndex,i);
        }
        if (ServerHour == null) ServerHour = new PersistentData<int, int>("ServerHour");
        if (ServerMin == null) ServerMin = new PersistentData<int, int>("ServerMin");
        if (ServerSec == null) ServerSec = new PersistentData<int, int>("ServerSec");

        if (firstRun == null) firstRun = new PersistentData<bool, bool>("firstRun");
        if (GameExp == null) GameExp = new PersistentData<float, float>("GameExp");
        if (GameLevel == null) GameLevel = new PersistentData<int, int>("GameLevel");
        if (HitSum == null) HitSum = new PersistentData<int, int>("HitSum");
        if (MissHitSum == null) MissHitSum = new PersistentData<int, int>("MissHitSum");
        if (GameFirstShark == null) GameFirstShark = new PersistentData<bool, bool>("GameFirstShark");
        if (GameSharkSum == null) GameSharkSum = new PersistentData<int, int>("GameSharkSum");
        if (GameLanternSum == null) GameLanternSum = new PersistentData<int, int>("GameLanternSum");
        if (GameFishSum == null) GameFishSum = new PersistentData<int, int>("GameFishSum");
        if (GameEveryFish == null) GameEveryFish = new PersistentData<int, int>[Prefab_AllFish.Length];

        if (CatchFish20 == null) CatchFish20 = new PersistentData<bool, bool>("CatchFish20");
        if (CatchLantern50 == null) CatchLantern50 = new PersistentData<bool, bool>("CatchLantern50");
        if (CatchShark100 == null) CatchShark100 = new PersistentData<bool, bool>("CatchShark100");
        if (CatchEveryFish == null) CatchEveryFish = new PersistentData<bool, bool>("CatchEveryFish");

        if (NotLuck10 == null) NotLuck10 = new PersistentData<bool, bool>("NotLuck10");
        if (NotLuck20 == null) NotLuck20 = new PersistentData<bool, bool>("NotLuck20");
        if (Luck10 == null) Luck10 = new PersistentData<bool, bool>("Luck10");
        if (Luck20 == null) Luck20 = new PersistentData<bool, bool>("Luck20");

        if (BLV5 == null) BLV5 = new PersistentData<bool, bool>("BLV5");
        if (BLV10 == null) BLV10 = new PersistentData<bool, bool>("BLV10");
        if (BLV20 == null) BLV20 = new PersistentData<bool, bool>("BLV20");
        if (BLV30 == null) BLV30 = new PersistentData<bool, bool>("BLV30");
        if (BLV40 == null) BLV40 = new PersistentData<bool, bool>("BLV40");

        if (Gold500 == null) Gold500 = new PersistentData<bool, bool>("Gold500");
        if (Gold1000 == null) Gold1000 = new PersistentData<bool, bool>("Gold1000");
        if (Gold10000 == null) Gold10000 = new PersistentData<bool, bool>("Gold10000");
        if (Gold100000 == null) Gold100000 = new PersistentData<bool, bool>("Gold100000");
        if (Gold1000000 == null) Gold1000000 = new PersistentData<bool, bool>("Gold1000000");

        if (EveryReward == null) EveryReward = new PersistentData<bool, bool>("EveryReward");
        if (EveryRewardDays == null) EveryRewardDays = new PersistentData<int, int>("EveryRewardDays");
        if (FirstChangeName == null) FirstChangeName = new PersistentData<bool, bool>("FirstChangeName");

        if (First_Recharge == null) First_Recharge = new PersistentData<bool, bool>("First_Recharge");

        if (First_ReceiveCoin == null) First_ReceiveCoin = new PersistentData<bool, bool>("First_ReceiveCoin");

        for (int i = 0; i < Prefab_AllFish.Length; i++)
        {
            if (GameEveryFish[i] == null) GameEveryFish[i] = new PersistentData<int, int>("GameEveryFish" + i);
        }
        if (firstRun.Val == false) //正式版不注释
        {
            GameFirstRun.Val = true;
            //GameFirstRun.Val = false;
            GameID.Val = uint.MaxValue;

            ServerHour.Val = 0;
            ServerMin.Val = 0;
            ServerSec.Val = 0;

            GameExp.Val = 0;
            GameLevel.Val = 0;
            MissHitSum.Val = 0;
            HitSum.Val = 0;

            GameSharkSum.Val = 0;
            firstRun.Val = true;
            GameLanternSum.Val = 0;
            GameFishSum.Val=0;
            //Debug.Log(Prefab_AllFish.Length);  //鱼总数
            for (int i = 0; i < Prefab_AllFish.Length; i++)
            {
                //if (GameEveryFish[i] == null) GameEveryFish[i] = new PersistentData<int, int>("GameEveryFish" + i);
                GameEveryFish[i].Val = 0;
            }
		    check_levle();
            NotLuck10.Val =false;
            NotLuck20.Val=false;
            Luck10.Val=false;
            Luck20.Val=false;
            GameFirstShark.Val = false;
            CatchFish20.Val = false;
            CatchLantern50.Val = false;
            CatchShark100.Val = false;
            CatchEveryFish.Val = false;

            Gold500.Val = false;
            Gold1000.Val = false;
            Gold10000.Val = false;
            Gold100000.Val = false;
            Gold1000000.Val = false;
			
			BLV5.Val=false;
			BLV10.Val=false;
			BLV20.Val=false;
			BLV30.Val=false;
            BLV40.Val = false;
            EveryReward.Val = true;

            First_Recharge.Val = true;

            EveryRewardDays.Val = 0;
            FirstChangeName.Val = true;
            First_ReceiveCoin.Val = true;
            check_gold();
        }
	}
	// Update is called once per frame
	void Update ()
    {
	    
	}
    void OnApplicationQuit()
    {
        
    }
}
