using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;

public class NemoMCUEditor : MonoBehaviour
{

    public NemoUsbHid_HardScan ArcIO;
    public GUISkin Skin;



    public float TextAreaMinWidth = 50.0f;


    public int SerialPort = 4;
    public int boudarate = 9600;
    
    private List<string> mDebugInfos = new List<string>();
    private float mScrollVal = 0.0f;
    private int mConsoloViewMaxLine = 20;




    //private bool mRealTimeOutBounty = true;

    //private int mOutCoinNum = 1;
    //private int mOutTicketNum = 1;
    //private int mButtomLightIdx = 0;//0-5

    //private int mDataBackLen = 1;//[测试]请求返回数据长度
    //private int mDataBackInterval = 10;//[测试]请求返回数据间隔
    //private bool mDataBackOnOff = false;//[测试]数据当前是否在返回中

    //private bool[] mLighttingTag;

    private int mGameIdx = 0;
    private int mMainVersion = 0;
    private int mSubVersion = 0;
    void Start()
    {
        //mLighttingTag = new bool[30];
        if (ArcIO == null)
        {
            ArcIO = GetComponent<NemoUsbHid_HardScan>();
            ArcIO.Open();
            if (ArcIO == null)
                Debug.LogError("ArcadeSinglePlayerTester初始化错误.");
        }

        //ArcIO.EvtKey += KeyEvent;
        //ArcIO.EvtCtrlBoardStateChanged += Handle_CtrlBoardStateChanged;
        //ArcIO.EvtOutCoinReflect += OnOutCoin;
        //ArcIO.EvtOutTicketReflect += OnOutTicket;
        //ArcIO.EvtInsertCoin += OnInsertCoin;
        ArcIO.EvtHardwareInfo += Hnalde_HardwareInfoRespon;
        ArcIO.EvtResultEditMCU += Handle_ResultEditMCU;
        
        //ArcIO.EvtLackCoin += OnLackCoin;
        //ArcIO.EvtLackTicket += OnLackTicket;
        DebugLog("所属游戏说明:  0:	渔乐无穷 1:渔乐世界 2:霹雳打渔 3:摇钱树 4:渔乐万能 5:爱骨者 6:疯狂鳄鱼 7:一网打尽 8:一网打尽2 9:八鲨闹海");

        if (ArcIO.IsOpen())
        {
            ArcIO.RequestHardwareInfo();//请求版本信息    
            DebugLog("已连接");
        }
        else
            DebugLog("未连接");
 
        //HMACMD5 cryptor = new HMACMD5(System.Text.Encoding.ASCII.GetBytes("FX20120927YIDINGYAOCHANGAEF51FM2"));

        //byte[] challengeAnswer = cryptor.ComputeHash(System.BitConverter.GetBytes(123));
        //byte[] testAry = new byte[]{
        //    0,0,0,0,
        //    0,0,0,0,
        //    0,0,0,0,
        //    0,0,0,7B
        //};
        //byte[] challengeAnswer = cryptor.ComputeHash(System.Text.Encoding.ASCII.GetBytes("123456789A123456"));
        
        //Debug.Log(NemoUsbHid_HardScan.ByteArrayToString(challengeAnswer));
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
    void KeyEvent2(int ctrlIdx, NemoCtrlIO_Key k, bool downOrUp)
    {

    }
    void KeyEvent(int playerIdx, NemoCtrlIO_Key k, bool downOrUp)
    {
        string keyStr = "";
        switch (k)
        {
            case NemoCtrlIO_Key.A:
                keyStr = "押分";
                break;
            case NemoCtrlIO_Key.B:
                keyStr = "发跑";
                break;
            case NemoCtrlIO_Key.C:
                keyStr = "下分";
                break;
            case NemoCtrlIO_Key.D:
                keyStr = "上分";
                break;
            case NemoCtrlIO_Key.F:
                keyStr = "按退";
                break;
            default:
                keyStr = k.ToString();
                break;

        }
        DebugLog("按键:" + keyStr + (downOrUp ? " 按下" : " 松开") + "  由玩家: " + playerIdx + " 触发.");
    }

    void Handle_CtrlBoardStateChanged(int ctrlBoardid, bool state)
    {
        DebugLog("控制板 :" + ctrlBoardid + (state ? "已连接" : "未连接"));
    }

    void Hnalde_HardwareInfoRespon(int gameIdx, int mainVer, int subVer, bool verifySucess)
    {
        DebugLog("验证信息返回 :" + (verifySucess ? "验证成功" : "验证失败")
            + "  所属游戏:" + gameIdx + "  主版本号:" + mainVer + "  副版本号:" + subVer);
    }
    void OnBackGroundKeyEvent(NemoCtrlIO_Key k, bool downOrUp)
    {

        DebugLog("后台按键:" + k + (downOrUp ? " 按下" : " 松开"));
    }

    void OnInsertCoin(uint coinnum, int playerIdx)
    {
        DebugLog("投币,玩家:" + playerIdx);
    }
    void OnOutCoin(int playerIdx)
    {
        DebugLog("出币返回,玩家:" + playerIdx);
    }

    void OnOutTicket(int playerIdx)
    {
        DebugLog("出票返回,玩家:" + playerIdx);
    }

    void OnLackCoin(int playerIdx)
    {
        DebugLog("缺币,玩家:" + playerIdx);
    }

    void OnLackTicket(int playerIdx)
    {
        DebugLog("缺票,玩家:" + playerIdx);
    }

    void Handle_ResultEditMCU(bool result)
    {
        DebugLog("修改加密板:" + (result ? "成功" : "失败"));
    }

    void OnGUI()
    {

        if (Skin != null)
            GUI.skin = Skin;


        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();//左边控制台
            {
                GUILayout.BeginVertical("box");
                {

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("所属游戏:");
                        //GUILayout.
                        GUILayout_IntArea(ref mGameIdx);//player改变 
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("主版本号:");
                        //GUILayout.
                        GUILayout_IntArea(ref mMainVersion); 
                    }
                    GUILayout.EndHorizontal();


                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("副版本号:");
                        //GUILayout.
                        GUILayout_IntArea(ref mSubVersion);
                    }
                    GUILayout.EndHorizontal();


                    if (GUILayout.Button("设置"))
                    {

                        ArcIO.EditMCUInfo(mGameIdx,mMainVersion,mSubVersion);
                    } 

                    if (GUILayout.Button("请求控制板信息"))
                    {

                        ArcIO.RequestHardwareInfo();
                    } 
                     
                }

                GUILayout.EndVertical();

                GUILayout.Space(20.0f);



                if (GUILayout.Button("清空LOG"))
                {
                    mDebugInfos.Clear();
                    mScrollVal = 0F;
                }

            }
            GUILayout.EndVertical();//左边控制台结束
        }

        //GUILayout.TextArea(""

        //        , new GUILayoutOption[] { GUILayout.MinWidth(750), GUILayout.MinHeight(390) });
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


            if (mDebugInfos.Count > mConsoloViewMaxLine)
            {
                mScrollVal = GUILayout.VerticalScrollbar(mScrollVal, 1f, 0F, (1 + mDebugInfos.Count - mConsoloViewMaxLine)
                , GUILayout.MinHeight(330));
            }
        }

        GUILayout.EndHorizontal();

        //GUILayout.BeginHorizontal();//右边combobox的显示

        //GUILayout.EndHorizontal();

        //GUILayout.TextField(mDebugInfos.Count.ToString());
        //GUILayout.TextField(mScrollVal.ToString());
        //GUILayout.EndHorizontal();
    }

    void DebugLog(string str)
    {

        mDebugInfos.Add(str);
        mScrollVal = mDebugInfos.Count - mConsoloViewMaxLine;
    }



    private bool GUILayout_IntArea(ref int val)
    {
        int oldVal = val;
        string valStr = GUILayout.TextArea(val.ToString(), GUILayout.MinWidth(TextAreaMinWidth));
        int newVal = int.Parse(valStr);
        if (newVal != oldVal)
        {
            val = newVal;
            return true;
        }
        return false;

    }
}

