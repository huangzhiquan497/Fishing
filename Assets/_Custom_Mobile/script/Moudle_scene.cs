
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using wazServer;

public class Moudle_scene : MonoBehaviour
{

    const string IP_TEST = "115.29.160.55";
    const string IP_ADDRESS = "112.124.2.40";//"112.124.2.40";
    public static Packager mP;
    public static WazClient mC;
   // long sendInterval = 10;

    public static WazSession session;
    long startTime;
    float[] Experience = {660,680,680,740,920,1260,1840,2680,3880,5440,7480,10000,13100,16800,21180,26280,32180,38900,46520,55100,64680,75340,87100,100060,114220,129700,146500,164720,184380,205560,228300,252680,278720,306500,336080,367500,400840,436120,473440,512800,554320,598000,643940,692160,742740,795720,851180,909140,969680,1032860};

    private int Ten = 6;
    private int Ge = 0;

    private int ServerHour;
    private int ServerMin;
    private int ServerSec;

    ArrayList aList;

    ArrayList bList;

    public static int[] RankData;
    public static string[] RankName;
   // public Moudle_light mlight;

    public int successCount = 0;                //获得的成就数量

    public  GameObject Prefab_Scene;
    public static  GameObject go_scene;

    public GameObject Prefab_Experience;
    public GameObject go_Experience;

    public GameObject Prefab_Text;
    public GameObject go_text;

    public GameObject Prefab_Score;
    public GameObject go_Score;

    public GameObject Prefab_levelUP;
    public GameObject go_levelUP;
	
	public GameObject[] Prefab_Achievement;
	public GameObject[] go_Achievement;


    public GameObject Prefab_TimeTen;
    public GameObject go_TimeTen;

    public GameObject Prefab_TimeGe;
    public GameObject go_TimeGe;

    public GameObject Prefab_ServerHour;
    public static GameObject go_ServerHour;

    public GameObject Prefab_ServerMin;
    public static GameObject go_ServerMin;

    public Moudle_FileRead Ref_Module_FileRead;

    public bool everyfish = false;

    public GameObject Prefab_Lingqu;
    public static GameObject go_Lingqu;

    public static bool isLing = true;

    tk2dSprite tk2dtemp;
    public void Set_Exp(int level,float exp)
    {
        go_Experience.transform.localScale = new Vector3(exp / Experience[level], 1, 1);
    }
    IEnumerator Sec_Reward()
    {
        while (true)
        {
            if (MobileInterface.GetPlayerScore() <= 200)
            {
                //本地时间
                Ge--;
                if (Ten == 0 && Ge < 0)
                {
                    Ten = 6;
                    Ge = 0;
                    MobileInterface.ChangePlayerScore(10);
                }
                if (Ge < 0)
                {
                    Ten--;
                    Ge = 9;
                }
            }
            else
            {
                Ten = 6;
                Ge = 0;
            }
            go_TimeTen.GetComponent<tk2dTextMesh>().text = Ten.ToString();
            go_TimeGe.GetComponent<tk2dTextMesh>().text = Ge.ToString();
            yield return new WaitForSeconds(1);
        }
    }
    IEnumerator LingquEff()
    {
        int currentA = 0;
        while (true)
        {
            //float f = Random.Range(0F, 1F);
            //go_Lingqu.transform.FindChild("Sprite").GetComponent<tk2dSprite>().color = new Color(f, f, f);

            currentA = (currentA + 1) % 2;
            go_Lingqu.transform.Find("Sprite").GetComponent<tk2dSprite>().color = new Color(1, 1, 1, currentA);

            yield return new WaitForSeconds(0.5F);
        }
    }
    //计算倒计时时间
    IEnumerator Min_Reward()
    {
        while (true)
        {
            if (mC.IsConnect() == false)
            {
                //Debug.Log("没网");
                //   return;
            }
            else
            {
                //服务器时间
                Moudle_FileRead.ServerSec.Val--;

                if (Moudle_FileRead.ServerHour.Val == 0 && Moudle_FileRead.ServerMin.Val == 0 && Moudle_FileRead.ServerSec.Val < 0)
                {
                    isLing = false;
                    go_ServerHour.SetActive(false);
                    go_ServerMin.SetActive(false);
                    go_Lingqu.SetActive(true);
                    go_Lingqu.transform.Find("Sprite").gameObject.SetActive(true);
                    go_Lingqu.transform.Find("FlyingCoin").gameObject.SetActive(true);
                    StartCoroutine(LingquEff());
                    go_scene.transform.Find("ServerTime/0").gameObject.SetActive(false);
                    go_scene.transform.Find("ServerTime/冒号").gameObject.SetActive(false);

                    if (go_Lingqu.GetComponent<Collider>() != null)
                        go_Lingqu.GetComponent<Collider>().enabled = true;

                }
                if (Moudle_FileRead.ServerSec.Val < 0)
                {
                    Moudle_FileRead.ServerMin.Val--;
                    Moudle_FileRead.ServerSec.Val = 59;
                }
                if (Moudle_FileRead.ServerMin.Val < 0)
                {
                    Moudle_FileRead.ServerHour.Val--;
                    Moudle_FileRead.ServerMin.Val = 59;
                }
            }
            go_ServerHour.GetComponent<tk2dTextMesh>().text = Moudle_FileRead.ServerHour.Val.ToString();

            if (Moudle_FileRead.ServerMin.Val >= 10)
            {
                go_ServerMin.GetComponent<tk2dTextMesh>().text = Moudle_FileRead.ServerMin.Val.ToString();
            }
            else
            {
                go_ServerMin.GetComponent<tk2dTextMesh>().text = "0" + Moudle_FileRead.ServerMin.Val.ToString();
            }
           // Debug.Log(1);
            yield return new WaitForSeconds(1F);
            //yield return 1000;
        }
    }
	IEnumerator createAndDelete(int index)
	{
		go_Achievement[index] = Instantiate(Prefab_Achievement[index]) as GameObject;
        bList.Add(go_Achievement[index]);
        if (bList.Count > 1)
        {
            for (int i = 0; i < bList.Count - 1; i++)
                Destroy((GameObject)bList[i]);
        }
        yield return new WaitForSeconds(5);
		Destroy(go_Achievement[index]);
		//StopCoroutine("Destory");
	}
    // Use this for initialization
    void Start()
    {
        RankData = new int[5];
        RankName = new string[5];
        
        aList = new ArrayList();
        bList = new ArrayList();
      //  wiipay.init();    MM支付初始化
        Moudle_main.EvtGameStart += Handle_GameStart;
       // GameServer.SocketConnect();
        go_Experience = Instantiate(Prefab_Experience) as GameObject;
        go_Experience.SetActive(false);

        go_text = Instantiate(Prefab_Text) as GameObject;
        go_text.SetActive(false);

        go_TimeTen = Instantiate(Prefab_TimeTen) as GameObject;
        go_TimeTen.SetActive(false);

        go_TimeGe = Instantiate(Prefab_TimeGe) as GameObject;
        go_TimeGe.SetActive(false);

        go_ServerHour = Instantiate(Prefab_ServerHour) as GameObject;
        go_ServerHour.SetActive(false);

        go_ServerMin = Instantiate(Prefab_ServerMin) as GameObject;
        go_ServerMin.SetActive(false);

        go_Lingqu = Instantiate(Prefab_Lingqu) as GameObject;
        go_Lingqu.SetActive(false);

        GameMain.EvtFishKilled += Handle_FishKill;
       // MobileInterface.EvtBulletHitFish += Hit;
        MobileInterface.EvtHitFishResult += Hit_Result;

        go_Score = Instantiate(Prefab_Score) as GameObject;
        go_Score.SetActive(false);
      //  Moudle_main.EvtLevelUP += Handle_LevelUP;
        mC = new WazClient();
        mP = new Packager(mC);

        mP.EvtRecevicePack += Handle_ReceivePack;
        mC.EvtSessionAccept += SendData;

        session = mC.Connect(IP_ADDRESS, 9873);
        StartCoroutine(ConnectToInternet());  //更新分数跟别的数据
	}
    IEnumerator ConnectToInternet()
    {
        while (true)
        {
            //Debug.Log("每十秒连一下");
            if (!mC.IsConnect())
                session = mC.Connect(IP_ADDRESS, 9873);
            if (mC.IsConnect())
            {
                CS_CommitScore iscore;
                iscore.score = MobileInterface.GetPlayerScore();
                mP.Send(session, iscore);
            }
            else
            {
    
            }
            yield return new WaitForSeconds(10.0f);
        }
    }
    public  static uint CurRank;
    public  void SetRank(uint rank)
    {
        CurRank = rank;
    }
    public static uint GetRank()
    {
        return CurRank;
    }
    public static int getRankData(int index)
    {
        return RankData[index];
    }
    public static string getRankName(int index)
    {
        return RankName[index];
    }
    void SendData(WazSession waz)
    {
        mP.RegPack(typeof(CS_RequestUserID));
        mP.RegPack(typeof(SC_AllocUserID));
        mP.RegPack(typeof(CS_GamePlayerLogin));
        mP.RegPack(typeof(CS_GamePlayerLogout));
        mP.RegPack(typeof(CS_RequestRank));
        mP.RegPack(typeof(SC_ResponRank));
        mP.RegPack(typeof(CS_CommitScore));
        mP.RegPack(typeof(SC_RewardDaily));
        mP.RegPack(typeof(CS_PayNotify));
        mP.RegPack(typeof(CS_RequestChangeName));
        mP.RegPack(typeof(SC_RequestChangeNameResp));
        CS_RequestUserID RegID;
        // RegID.MachineID = 1;
        //  RegID.MachineID = (SystemInfo.deviceUniqueIdentifier);
        ///Debug.Log(Moudle_FileRead.GameID.Val);
        if (Moudle_FileRead.GameFirstRun.Val == true)
        {
            RegID.MachineID = SystemInfo.deviceUniqueIdentifier;
            RegID.Resolution = Screen.currentResolution.width.ToString() + "*" + Screen.currentResolution.height.ToString();
            mP.Send(session, RegID);
        }
        else
        {
           // Debug.Log("不是第一次登录");
            CS_GamePlayerLogin id;
            id.UserID = Moudle_FileRead.GameID.Val;
            //id.UserID = 21;
           // Debug.Log(id.UserID);
            mP.Send(session, id);
        }
    }
    public static bool bEveryDayReward = false;
    void Handle_ReceivePack(WazSession s, object pack)
    {
        if (Moudle_FileRead.GameFirstRun.Val == true)
        {
            if (pack.GetType() == typeof(SC_AllocUserID))
            {
               // Debug.Log("第一次运行玩家登录成功");
                SC_AllocUserID Receive = (SC_AllocUserID)pack;
               // Debug.Log(Receive.UserID);
                CS_GamePlayerLogin id;
                id.UserID = Receive.UserID;
               // id.UserID = 21;
                mP.Send(session, id);

                if (Moudle_FileRead.GameID.Val == uint.MaxValue)
                {
                    Moudle_FileRead.GameID.Val = id.UserID;
                }
                Moudle_FileRead.GameFirstRun.Val = false;
            }
        }
        if(pack.GetType()==typeof(SC_RewardDaily))
        {
            bEveryDayReward = true;
         //   Debug.Log("每日奖励");
            Moudle_FileRead.EveryRewardDays.Val++;
            //Moudle_FileRead.EveryRewardDays.Val = Moudle_FileRead.EveryRewardDays.Val%7 + 1;
            //Debug.Log(Moudle_FileRead.EveryRewardDays.Val);
            if (Moudle_FileRead.EveryRewardDays.Val > 7)
            {
                Moudle_FileRead.EveryRewardDays.Val = 1;
            }
            //Debug.Log(Moudle_FileRead.EveryRewardDays.Val++);
        }
        if (pack.GetType() == typeof(SC_ResponRank))
        {
            SC_ResponRank Receive = (SC_ResponRank)pack;
            //Debug.Log("当前排名"+Receive.Rank);
            SetRank(Receive.Rank);
            switch (Receive.Rank)
            {
                case 1:
                 //   Debug.Log("第一名");
                    RankData[0] = MobileInterface.GetPlayerScore();
                    RankName[0] = Receive.Name;
                    RankData[1] = Receive.RankOffsetN1;
                    RankName[1] = Receive.RankNameOffsetN1;
                    RankData[2] = Receive.RankOffsetN2;
                    RankName[2] = Receive.RankNameOffsetN2;
                    break;
                case 2:
                    RankData[0] = Receive.RankOffset1;
                    RankName[0] = Receive.RankNameOffset1;
                    RankData[1] = MobileInterface.GetPlayerScore();
                    RankName[1] = Receive.Name;
                    RankData[2] = Receive.RankOffsetN1;
                    RankName[2] = Receive.RankNameOffsetN1;
                    RankData[3] = Receive.RankOffsetN2;
                    RankName[3] = Receive.RankNameOffsetN2;
                    break;
                default:
                    RankData[0] = Receive.RankOffset2;
                    RankName[0] = Receive.RankNameOffset2;
                    RankData[1] = Receive.RankOffset1;
                    RankName[1] = Receive.RankNameOffset1;
                    RankData[2] = MobileInterface.GetPlayerScore();
                    RankName[2] = Receive.Name;
                    RankData[3] = Receive.RankOffsetN1;
                    RankName[3] = Receive.RankNameOffsetN1;
                    RankData[4] = Receive.RankOffsetN2;
                    RankName[4] = Receive.RankNameOffsetN2;
                    break;
            }
           if (Module_Rank.EvtR != null)//+= Data_Finish;
               Module_Rank.EvtR();
        }
        if (pack.GetType() == typeof(SC_RequestChangeNameResp))
        {
            SC_RequestChangeNameResp resp = (SC_RequestChangeNameResp)pack;
            if (resp.Result)
            {
                CS_CommitScore iscore;
                iscore.score = MobileInterface.GetPlayerScore();
                mP.Send(session, iscore);

                //Debug.Log("请求排行榜");
                CS_RequestRank rank;
                mP.Send(session, rank);
              //  Debug.Log("改名成功");
                Moudle_FileRead.FirstChangeName.Val = false;
                Module_Rank.SetRank();

                Module_Rank.go_Rank.transform.Find("改名字").gameObject.SetActive(false);
                //go_Rank.SetActive(false);
                Moudle_main.Singlton.go_Black.SetActive(false);
            }
        }
    }
    bool check_everyfish()
    {
        for (int i = 0; i < Ref_Module_FileRead.Prefab_AllFish.Length; i++)
        {
            if (Moudle_FileRead.GameEveryFish[i].Val == 0)
            {
                return false;
            }
        }
        return true;
    }
    void Hit_Result(bool b, Player p, Bullet bullet, Fish f)
    {
        if (!b)
        {
            Moudle_FileRead.MissHitSum.Val++;
            Moudle_FileRead.HitSum.Val = 0;
        }
        else
        {
            Moudle_FileRead.MissHitSum.Val = 0;
            Moudle_FileRead.HitSum.Val++;
        }
    }
    IEnumerator destory_Levelup()
    {
        yield return new WaitForSeconds(2);
        Destroy(go_levelUP);
    }
    void Handle_FishKill(Player killer, Bullet b, Fish f)
    {
        Moudle_FileRead.GameExp.Val += 300;
       // Debug.Log("打中的鱼是"+f.TypeIndex);
        int index;
        Ref_Module_FileRead.mAllFish.TryGetValue(f.TypeIndex, out index);
        //Debug.Log(index);  知道打中哪条鱼
        Moudle_FileRead.GameEveryFish[index].Val = 1;

        everyfish = check_everyfish();  //每次打中鱼都监测是否打中过所有鱼

        Moudle_FileRead.GameFishSum.Val++;   //打中鱼总数+1
       // Debug.Log(f.TypeIndex);
       // Moudle_FileRead.GameEveryFish[f.TypeIndex].Val = 1;
        if (f.TypeIndex == 21)
        {
            Moudle_FileRead.GameLanternSum.Val++;               //判断特定鱼成就
        }
		if(f.TypeIndex==24)
        {
            Moudle_FileRead.GameSharkSum.Val++;   //判断特定鱼成就
		}
        if (Moudle_FileRead.GameExp.Val > Experience[Moudle_FileRead.GameLevel.Val])
        {
            go_levelUP = Instantiate(Prefab_levelUP) as GameObject;
            StopCoroutine("destory_Levelup");
            StartCoroutine("destory_Levelup");
            aList.Add(go_levelUP);
            if (aList.Count > 1)
            {
                for(int i=0;i<aList.Count-1;i++)
                    Destroy((GameObject)aList[i]);
            }
           // go_levelUP.transform.FindChild("button/确定").GetComponent<tk2dUIItem>().OnClick += OK_Click;
            Moudle_FileRead.GameExp.Val -= Experience[Moudle_FileRead.GameLevel.Val];
            Moudle_FileRead.GameLevel.Val++;
            //tk2dTextMesh text = 
            go_levelUP.transform.Find("level").GetComponent<tk2dTextMesh>().text = Moudle_FileRead.GameLevel.Val.ToString();
           // mlight.reset_move();
            go_text.GetComponent<tk2dTextMesh>().text = Moudle_FileRead.GameLevel.Val.ToString();
          //  Debug.Log("升级了，当前等级为" + Moudle_FileRead.GameLevel.Val + "下一级需要的经验是" + Experience[Moudle_FileRead.GameLevel.Val]);
        }
    }
    void OK_Click()
    {
        //go_levelUP.SetActive(false);
        Destroy(go_levelUP);
    }
    void OnGUI()
    {

    }
    void Handle_GameStart()
    {
        go_scene = Instantiate(Prefab_Scene) as GameObject;

        go_scene.transform.Find("button/control/function").GetComponent<tk2dUIItem>().OnClick += Help_Click;
        go_scene.transform.Find("button/control/shezhi").GetComponent<tk2dUIItem>().OnClick += Setting_Click;
        go_scene.transform.Find("button/control/chengjiu").GetComponent<tk2dUIItem>().OnClick += Achievement_Click;
        go_scene.transform.Find("button/control/paihangbang").GetComponent<tk2dUIItem>().OnClick += Rank_Click;

        go_scene.transform.Find("button/shangdian").GetComponent<tk2dUIItem>().OnClick += Recharge_Click;

        //go_scene.transform.FindChild("称号").GetComponent<tk2dSprite>().Collection.name = "HD3";
        go_text.GetComponent<tk2dTextMesh>().text = Moudle_FileRead.GameLevel.Val.ToString();

        go_Score.GetComponent<tk2dTextMesh>().text = MobileInterface.GetPlayerScore().ToString();
        go_Score.SetActive(true);

        go_Experience.SetActive(true);
        go_text.SetActive(true);

        go_TimeTen.SetActive(true);
        go_TimeGe.SetActive(true);

        go_ServerHour.SetActive(true);
        go_ServerMin.SetActive(true);
        //go_ServerSec.SetActive(true);
        StartCoroutine(Min_Reward());
        StartCoroutine(Sec_Reward());
        tk2dtemp = go_scene.transform.Find("称号").GetComponent<tk2dSprite>();
    }

    void Recharge_Click()
    {
        wiipay.EvtLog("Game_Gaming_BtnRecharge");
        Moudle_main.Singlton.go_Black.SetActive(true);

        if (Moudle_main.EvtShop != null)
            Moudle_main.EvtShop();
    }
    void Help_Click()
    {
        wiipay.EvtLog("Game_Gaming_BtnHelp");
        //wiipay.Order(1);
        Moudle_main.Singlton.go_Black.SetActive(true);
        if (Moudle_main.EvtHelp != null)
            Moudle_main.EvtHelp();
    }
    void Setting_Click()
    {
        wiipay.EvtLog("Game_Gaming_BtnSetting");
        Moudle_main.Singlton.go_Black.SetActive(true);
        if (Moudle_main.EvtSetting != null)
            Moudle_main.EvtSetting();
    }
    void Achievement_Click()
    {

        wiipay.EvtLog("Game_Gaming_BtnAchievement");
        Moudle_main.Singlton.go_Black.SetActive(true);
        if (Moudle_main.EvtAchievement != null)
            Moudle_main.EvtAchievement();
    }
    void Rank_Click()
    {
        wiipay.EvtLog("Game_Gaming_BtnRank");
        CS_CommitScore iscore;
        iscore.score = MobileInterface.GetPlayerScore();
        mP.Send(session, iscore);

        //Debug.Log("请求排行榜");
        CS_RequestRank rank;
        mP.Send(session, rank);
        Moudle_main.Singlton.go_Black.SetActive(true);
        if (Moudle_main.EvtRank != null)
            Moudle_main.EvtRank();
    }
    public bool Shop = false;
	// Update is called once per frame
	void Update ()
    {
		//达到5级
        if (Moudle_FileRead.BLV5.Val == false && Moudle_FileRead.GameLevel.Val >= 5)
        {
            MobileInterface.ChangePlayerScore(150);
			StartCoroutine(createAndDelete(0));
            Moudle_FileRead.BLV5.Val = true;
        }
		//10级
        if (Moudle_FileRead.BLV10.Val == false && Moudle_FileRead.GameLevel.Val >= 10)
        {
            MobileInterface.ChangePlayerScore(200);
			StartCoroutine(createAndDelete(1));
            Moudle_FileRead.BLV10.Val = true;
        }
		//20级
        if (Moudle_FileRead.BLV20.Val == false && Moudle_FileRead.GameLevel.Val >= 20)
        {
            MobileInterface.ChangePlayerScore(500);
			StartCoroutine(createAndDelete(2));
            Moudle_FileRead.BLV20.Val = true;
        }
		//30级
        if (Moudle_FileRead.BLV30.Val == false && Moudle_FileRead.GameLevel.Val >= 30)
        {
            MobileInterface.ChangePlayerScore(1500);
			StartCoroutine(createAndDelete(3));
            Moudle_FileRead.BLV30.Val = true;
        }
		//40级
        if (Moudle_FileRead.BLV40.Val == false && Moudle_FileRead.GameLevel.Val >= 40)
        {
            MobileInterface.ChangePlayerScore(3000);
			StartCoroutine(createAndDelete(4));
            Moudle_FileRead.BLV40.Val = true;
        }
		
		//连续10发子弹没捕捉到鱼if()
        if (Moudle_FileRead.NotLuck10.Val == false && Moudle_FileRead.MissHitSum.Val == 10)
        {
            MobileInterface.ChangePlayerScore(30);
            StartCoroutine(createAndDelete(5));
            Moudle_FileRead.NotLuck10.Val = true;
        }
        if (Moudle_FileRead.NotLuck20.Val == false && Moudle_FileRead.MissHitSum.Val == 20)
        {
            MobileInterface.ChangePlayerScore(150);
            StartCoroutine(createAndDelete(6));
            Moudle_FileRead.NotLuck20.Val = true;
        }
        if (Moudle_FileRead.Luck10.Val == false && Moudle_FileRead.HitSum.Val == 10)
        {
            MobileInterface.ChangePlayerScore(30);
            StartCoroutine(createAndDelete(7));
            Moudle_FileRead.Luck10.Val = true;
        }
        if (Moudle_FileRead.Luck20.Val == false && Moudle_FileRead.HitSum.Val == 20)
        {
            MobileInterface.ChangePlayerScore(200);
            StartCoroutine(createAndDelete(8));
            Moudle_FileRead.Luck20.Val = true;
        }
		//连续20发子弹没捕捉到鱼if()
		//连续10发子弹捕捉到鱼if()
		//连续20发子弹捕捉到鱼if()
		//鲨鱼出没 第一次打到鲨鱼
		if(Moudle_FileRead.GameFirstShark.Val==false&&Moudle_FileRead.GameSharkSum.Val==1)
        {
            MobileInterface.ChangePlayerScore(30);
			StartCoroutine(createAndDelete(9));
			Moudle_FileRead.GameFirstShark.Val=true;
		}
		//捉到20条鱼
		if(Moudle_FileRead.CatchFish20.Val==false&&Moudle_FileRead.GameFishSum.Val==20)
        {
            MobileInterface.ChangePlayerScore(120);
			StartCoroutine(createAndDelete(10));
			Moudle_FileRead.CatchFish20.Val=true;
		}
		//捉到50条灯笼鱼
		if(Moudle_FileRead.CatchLantern50.Val==false&&Moudle_FileRead.GameLanternSum.Val==50)
        {
            MobileInterface.ChangePlayerScore(300);
			StartCoroutine(createAndDelete(11));
			Moudle_FileRead.CatchLantern50.Val=true;
		}
		if(Moudle_FileRead.CatchShark100.Val==false&&Moudle_FileRead.GameSharkSum.Val==100)
        {
            MobileInterface.ChangePlayerScore(1000);
			StartCoroutine(createAndDelete(12));
			Moudle_FileRead.CatchShark100.Val=true;
		}
		//任何鱼都捉到1次
		if(Moudle_FileRead.CatchEveryFish.Val==false&&everyfish==true)
        {
            MobileInterface.ChangePlayerScore(5000);
			StartCoroutine(createAndDelete(13));
			Moudle_FileRead.CatchEveryFish.Val=true;
		}
        if (Moudle_FileRead.Gold500.Val == false && MobileInterface.GetPlayerScore() >= 500)
        {
            MobileInterface.ChangePlayerScore(50);
			StartCoroutine(createAndDelete(14));
            Moudle_FileRead.Gold500.Val = true;
        }
        if (Moudle_FileRead.Gold1000.Val == false && MobileInterface.GetPlayerScore() >= 1000)
        {
            MobileInterface.ChangePlayerScore(100);
			StartCoroutine(createAndDelete(15));
            Moudle_FileRead.Gold1000.Val = true;
        }
        if (Moudle_FileRead.Gold10000.Val == false && MobileInterface.GetPlayerScore() >= 10000)
        {
            MobileInterface.ChangePlayerScore(500);
			StartCoroutine(createAndDelete(16));
            Moudle_FileRead.Gold10000.Val = true;
        }
        if (Moudle_FileRead.Gold100000.Val == false && MobileInterface.GetPlayerScore() >= 100000)
        {
            MobileInterface.ChangePlayerScore(4000);
			StartCoroutine(createAndDelete(17));
            Moudle_FileRead.Gold100000.Val = true;
        }
        if (Moudle_FileRead.Gold1000000.Val == false && MobileInterface.GetPlayerScore() >= 1000000)
        {
            MobileInterface.ChangePlayerScore(120000);
			StartCoroutine(createAndDelete(18));
            Moudle_FileRead.Gold1000000.Val = true;
        }
        go_Score.GetComponent<tk2dTextMesh>().text = MobileInterface.GetPlayerScore().ToString();
        Set_Exp(Moudle_FileRead.GameLevel.Val, Moudle_FileRead.GameExp.Val);


        //tk2dSprite tk2dtemp = go_scene.transform.FindChild("称号").GetComponent<tk2dSprite>();
        if (Moudle_main.iState == 1 && Module_SceneSelect.bSelect)
        {
            if (Moudle_FileRead.GameLevel.Val < 5)
            {
                if (go_scene != null)
                    tk2dtemp.SetSprite("ui_cj_01");
                //else
                 //   Debug.Log("null");
            }
            else if (Moudle_FileRead.GameLevel.Val < 10)
            {
                tk2dtemp.SetSprite("ui_cj_02");
            }
            else if (Moudle_FileRead.GameLevel.Val < 20)
            {
                tk2dtemp.SetSprite("ui_cj_03");
            }
            else if (Moudle_FileRead.GameLevel.Val < 30)
            {
                tk2dtemp.SetSprite("ui_cj_04");
            }
            else
            {
                tk2dtemp.SetSprite("ui_cj_05");
            }
        }
        mC.Update();
        //Debug.Log(bEveryDayReward);
        
	}
    void OnApplicationQuit()
    {
        CS_GamePlayerLogout playerout;
        mP.Send(session, playerout);
        mC.Close();
    }
}