using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
public class Moudle_main : MonoBehaviour
{
    //0为开始界面,1:游戏主界面
    public static int iState;

    public delegate void Event_Generic();
    public static Event_Generic EvtBackStart;           //返回主界面
    public static Event_Generic EvtGameStart;           //游戏开始
    public static Event_Generic EvtSceneSelect;         //选择场景
    public static Event_Generic EvtHelp;                //帮助
    public static Event_Generic EvtRank;                //排行榜
    public static Event_Generic EvtWikipedia;           //百科
    public static Event_Generic EvtJiaocheng;           //新手教程
    public static Event_Generic EvtSetting;             //设置
    public static Event_Generic EvtAchievement;         //成就
    public static Event_Generic EvtRecharge;            //充值
    public static Event_Generic EvtShop;                //商店
    public static Event_Generic EvtRechState;           //充值方式
    public static Event_Generic EvtLevelUP;             //升级
    public static Event_Generic EvtEveryDayReward;      //每日奖励

    public static Event_Generic EvtChangeName;          //改名

    public static Moudle_main Singlton
    {
        get
        {
            if (mSingleton == null)
                mSingleton = GameObject.FindObjectOfType(typeof(Moudle_main)) as Moudle_main;
            return mSingleton;
        }
    }

    private static Moudle_main mSingleton;
    public  GameObject Prefab_Black;
    public  GameObject go_Black;

    public void EnterGame()
    {

    }
    public void SocketConnect()
    {
        int port = 9876;
        string host = "115.29.160.55";
        IPAddress ip = IPAddress.Parse(host);
        IPEndPoint ipe = new IPEndPoint(ip, port);
        Socket c = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        // Con
        c.Connect(ipe);
        string sendStr = "456";
        byte[] bs = Encoding.ASCII.GetBytes(sendStr);
        //Console.WriteLine("Send Message");
        c.Send(bs, bs.Length, 0);//发送测试信息
        // c.Send(bbs, bbs.Length, 0);
        string recvStr = "";
        byte[] recvBytes = new byte[1024];
        int bytes;
        bytes = c.Receive(recvBytes, recvBytes.Length, 0);//从服务器端接受返回信息 字串3
        recvStr += Encoding.ASCII.GetString(recvBytes, 0, bytes);
        // Console.WriteLine("Client Get Message:{0}", recvStr);//显示服务器返回信息
        c.Close();
        //System.Threading.Thread.Sleep(10000);//
    }
	// Use this for initialization
	void Start ()
    {
        iState = 0;
        go_Black = Instantiate(Prefab_Black) as GameObject;
        go_Black.SetActive(false);

        //try
        {
            //AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            //AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            //int rlt = jo.Call<int>("min", new object[] { 2, 3 });
            //Debug.Log(rlt);
        }
        //catch (System.Exception e)
        //{
        //    Debug.Log(e);
        //}
        //SocketConnect();
       // main = new Moudle_main();
	}
	
	// Update is called once per frame
	void Update ()
    {
	    //wiipay.
	}
}