using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tester_UsbIO_Feng : MonoBehaviour
{
    
    public ArcadeIO_Feng ArcIO;
    public GUISkin Skin;


    public float TextAreaMinWidth = 50.0f;


    public int SerialPort = 4;
    public int boudarate = 9600;
    public int PlayerIdx = 0;
    private List<string> mDebugInfos = new List<string>();
    private float mScrollVal = 0.0f;
    private int mConsoloViewMaxLine = 20;

    private bool mSendingFlashLightMsg = false;
    void Start()
    {

        if(ArcIO == null)
        {
            Debug.LogError("ArcadeSinglePlayerTester初始化错误.");
            return;
        }

        ArcIO.HandlerKey += KeyEvent;
        ArcIO.HandlerOutCoinReflect += OnOutCoin;
        ArcIO.HandlerOutTicketReflect += OnOutTicket;

        ArcIO.HandlerLackCoin += OnLackCoin;
        ArcIO.HandlerLackTicket += OnLackTicket;

        //ArcIO.HandlerVersionRespon += OnVersionResponEvent;
        //ArcIO.HandlerVerifyResult += OnVerifyResultEvent;
        //ArcIO.HandlerTextVerifyResult += OnVerifyTextResultEvent;
        ArcIO.HandlerBackGroundKeyEvent += OnBackGroundKeyEvent;
        DebugLog("自动打开串口.");
        //StartCoroutine(_Coro_DebugLog());
    }

    //IEnumerator _Coro_DebugLog()
    //{
    //    int i = 0;
    //    while (true)
    //    {
    //        DebugLog("TestLog" + (i++));
    //        yield return new WaitForSeconds(0.1F);
    //    }
    //}

    void KeyEvent(int playerIdx, ArcadeIO_Feng.Key k, bool downOrUp)
    {
        DebugLog("按键:" + k + (downOrUp ? " 按下" : " 松开") + "  由玩家: " + playerIdx + " 触发.");
    }
    void OnBackGroundKeyEvent(ArcadeIO_Feng.Key k, bool downOrUp)
    {
        DebugLog("后台按键:" + k + (downOrUp ? " 按下" : " 松开") );
    }

    void OnOutCoin(uint count, int playerIdx)
    {
        DebugLog("出币返回,玩家:" + playerIdx + " 余:" + count);
    }

    void OnOutTicket(uint count, int playerIdx)
    {
        DebugLog("出票返回,玩家:" + playerIdx + " 余:" + count);
    }

    void OnLackCoin(uint count, int playerIdx)
    {
        DebugLog("缺币,玩家:" + playerIdx + " 数量:" + count);
    }

    void OnLackTicket(uint count, int playerIdx)
    {
        DebugLog("缺票,玩家:" + playerIdx + " 数量:" + count);
    }

    void OnVersionResponEvent(uint main, uint sub)
    {
        //mHardwareVersion = main.ToString() + "." + sub.ToString();
        DebugLog("硬件回应版本请求:" + main + "." + sub);
    }

    void OnVerifyResultEvent(bool sucess)
    {
        DebugLog("正版验证结果:" + sucess);
    }

    void OnVerifyTextResultEvent(byte[] data)
    {
       
        string viewstr = "";
        foreach (byte d in data)
        {
            viewstr += string.Format("{0:x}", (int)(d));
        }
        DebugLog("OnVerifyTextResultEvent:" + viewstr);
    }
    void OnGUI()
    {

        if (Skin != null)
            GUI.skin = Skin;


        GUILayout.BeginHorizontal();


        GUILayout.BeginVertical();//左边控制台
        {
            GUILayout.BeginHorizontal("box");
            GUILayout.Label("串口(1-n):");

            GUILayout_IntArea(ref SerialPort);
            GUILayout_IntArea(ref boudarate);
            if (GUILayout.Button("打开"))
            {
                NemoSerial ns = GetComponent<NemoSerial>();
                if (ns != null)
                {
                    ns.PortName = "COM" + SerialPort;
                    ns.BaudRate = boudarate;
                }
                else
                    DebugLog("找不到NemoSerial组件,打开串口失败.");
                
                //int
            
                if (ArcIO.Open())
                    DebugLog("打开串口成功");
                else
                    DebugLog("打开串口失败");
            

            }


            if (GUILayout.Button("关闭"))
            {
                ArcIO.Close();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(20.0f);

            GUILayout.BeginVertical("box");
            {
                GUILayout.BeginHorizontal("box");
                {
                    GUILayout.Label("机台号(0-5):");
                    if (GUILayout_IntArea(ref PlayerIdx))//player改变
                    {

                    }
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout_IntArea(ref mOutCoinNum);
                if (GUILayout.Button("出币"))
                {
                    ArcIO.OutCoin((uint)mOutCoinNum, PlayerIdx);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout_IntArea(ref mOutTicketNum);
                if (GUILayout.Button("出票"))
                {
                    ArcIO.OutTicket((uint)mOutTicketNum, PlayerIdx);
                }
                GUILayout.EndHorizontal();

                bool oldRTOBToggle = mRealTimeOutBounty;
                mRealTimeOutBounty = GUILayout.Toggle(mRealTimeOutBounty, "实时退币(票)");
                if (oldRTOBToggle != mRealTimeOutBounty)
                {
                    ArcIO.SetRealTimeOutBounty(mRealTimeOutBounty);
                }


                if (GUILayout.Button("补币"))
                {
                    ArcIO.ResponCoin(PlayerIdx);
                }

                if (GUILayout.Button("补票"))
                {
                    ArcIO.ResponTicket(PlayerIdx);
                }

                GUILayout.BeginHorizontal();
                {
                     if (GUILayout.Button("闪灯"))
                    {
                        ArcIO.FlashButtomLight(mButtomLightIdx);
                    }
                    GUILayout.Label("玩家ID(0-5):");
                    GUILayout_IntArea(ref mButtomLightIdx);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {

                    if (GUILayout.Button(mSendingFlashLightMsg ? "发送大量闪灯数据[停]" : "发送大量闪灯数据[开]"))
                    {
                        if (mSendingFlashLightMsg)
                        {
                            StopCoroutine("_Coro_FlashLights");
                            mSendingFlashLightMsg = false;
                        }
                        else
                        {
                            StartCoroutine("_Coro_FlashLights");
                            mSendingFlashLightMsg = true;
                        }
                        
                    } 
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                {
                    if (!mDataBackOnOff)
                    {
                        if (GUILayout.Button("返回大量数据[开始]"))
                        {
                            ArcIO.RequestDataBack((byte)mDataBackLen,(byte)mDataBackInterval,true);
                            mDataBackOnOff = true;
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("返回大量数据[停止]"))
                        {
                            ArcIO.RequestDataBack((byte)mDataBackLen, (byte)mDataBackInterval, false);
                            mDataBackOnOff = false;
                        }
                    }
                   
                    GUILayout.Label("长度(byte)");
                    GUILayout_IntArea(ref mDataBackLen);
                    GUILayout.Label("间隔(ms)");
                    GUILayout_IntArea(ref mDataBackInterval);
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();

            GUILayout.Space(20.0f);
            GUILayout.BeginVertical("box");
            {

                //if (GUILayout.Button("版本号" + mHardwareVersion))
                //{
                //    ArcIO.RequestHardwareVersion();
                //}


                GUILayout.BeginHorizontal();
                GUILayout.Label("外围跑灯模式(0-4):");
                int scenicLightMode = (int)mScenicLightMode;
                if (GUILayout_IntArea(ref scenicLightMode))
                {
                    mScenicLightMode = (ArcadeIO_Feng.ScenicLightMode)scenicLightMode;
                }

                if (GUILayout.Button("设置"))
                {
                    ArcIO.SetScenicLight(mScenicLightMode);
                }
                GUILayout.EndHorizontal();

            }
            GUILayout.EndVertical();


           mSendVisibleMsg = GUILayout.TextArea(mSendVisibleMsg, GUILayout.MinWidth(TextAreaMinWidth));
           if (GUILayout.Button("验证明文"))
           {
               byte[] ba = System.Text.Encoding.Default.GetBytes(mSendVisibleMsg);
               string viewPlainTextBytes = "";
               foreach (byte b in ba)
               {
                   viewPlainTextBytes += string.Format("  {0:x}",b);
               }
               DebugLog("发出明文(hex):" + viewPlainTextBytes);
               ArcIO.SendCheckPlainText(System.Text.Encoding.Default.GetBytes(mSendVisibleMsg));
           }

           if (GUILayout.Button("清空LOG"))
           {
               mDebugInfos.Clear();
               mScrollVal = 0F;
           }

        }
        GUILayout.EndVertical();//左边控制台结束

        //GUILayout.BeginVertical("box");
        GUILayout.TextArea(""

                , new GUILayoutOption[] { GUILayout.MinWidth(750), GUILayout.MinHeight(390) });
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();//右边combobox的显示
        {
            string viewStr = "";
            int i = 0;
            int j = 0;
            foreach (string info in mDebugInfos)
            {
                if (i >= (int)mScrollVal)
                {
                    viewStr = viewStr + info + "\n";
                    ++j;
                    if (j >= mConsoloViewMaxLine)
                    {
                        break;
                    }
                }
                ++i;
            }
            GUILayout.TextArea(viewStr
                , new GUILayoutOption[] { GUILayout.MinWidth(500), GUILayout.MinHeight(330) });


            if(mDebugInfos.Count > mConsoloViewMaxLine)
            {
                mScrollVal = GUILayout.VerticalScrollbar(mScrollVal, 1f, 0F, (1 + mDebugInfos.Count - mConsoloViewMaxLine)
                ,GUILayout.MinHeight(330));
            }
        }    
        GUILayout.EndHorizontal();

        //GUILayout.TextField(mDebugInfos.Count.ToString());
        //GUILayout.TextField(mScrollVal.ToString());
        //GUILayout.EndHorizontal();
    }
    
    void DebugLog(string str)
    {
        
        mDebugInfos.Add(str);
        mScrollVal = mDebugInfos.Count - mConsoloViewMaxLine;
    }

    IEnumerator _Coro_FlashLights()
    {
        while (true)
        {
            for (int i = 0; i != 5; ++i )
                ArcIO.FlashButtomLight(i);
            Debug.Log("flashing");
            yield return 0;
        }
    }

    private bool GUILayout_IntArea(ref int val) 
    {
        int oldVal = val;
        string valStr = GUILayout.TextArea(val.ToString(), GUILayout.MinWidth(TextAreaMinWidth));
        int newVal = int.Parse(valStr) ;
        if(newVal != oldVal)
        {
            val = newVal;
            return true;
        }
        return false;

    }

    private bool mRealTimeOutBounty = true;
 
    private int mOutCoinNum = 1;
    private int mOutTicketNum = 1;
    private int mButtomLightIdx = 0;//0-5

    private int mDataBackLen = 1;//[测试]请求返回数据长度
    private int mDataBackInterval = 10;//[测试]请求返回数据间隔
    private bool mDataBackOnOff = false;//[测试]数据当前是否在返回中

    private ArcadeIO_Feng.ButtomLightMode mButtomLightMode;//0-6

    //private string mHardwareVersion = "";
    private ArcadeIO_Feng.ScenicLightMode mScenicLightMode;//0-4

    private string mSendVisibleMsg = "";
}

