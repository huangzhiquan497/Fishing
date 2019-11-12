using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System.Security.Cryptography;

public class Tester_NemoUsbHidHardScan : MonoBehaviour
{
    
    public NemoUsbHid_HardScan ArcIO;
    public GUISkin Skin;



    public float TextAreaMinWidth = 50.0f;


    public int SerialPort = 4;
    public int boudarate = 9600;
    public int PlayerIdx = 0;
    private List<string> mDebugInfos = new List<string>();
    private float mScrollVal = 0.0f;
    private int mConsoloViewMaxLine = 20;




    //private bool mRealTimeOutBounty = true;

    //private int mOutCoinNum = 1;
    //private int mOutTicketNum = 1;
    private int mButtomLightIdx = 0;//0-5

    //private int mDataBackLen = 1;//[测试]请求返回数据长度
    //private int mDataBackInterval = 10;//[测试]请求返回数据间隔
    //private bool mDataBackOnOff = false;//[测试]数据当前是否在返回中
  
    private bool[] mLighttingTag;

    private uint mReadWrite_address;
    private byte mReadWrite_dataLen = 4;
    private ulong mReadWrite_data;
    private int mNumToWrite = 1;
    private bool mViewReadWrite = false;
    void Start()
    {
        mLighttingTag = new bool[30];
        if (ArcIO == null)
        {
            ArcIO = GetComponent<NemoUsbHid_HardScan>();
            if (ArcIO == null)
                Debug.LogError("ArcadeSinglePlayerTester初始化错误."); 
        }

        ArcIO.EvtKey += KeyEvent;
        ArcIO.EvtCtrlBoardStateChanged += Handle_CtrlBoardStateChanged;
        ArcIO.EvtOutCoinReflect += OnOutCoin;
        ArcIO.EvtOutTicketReflect += OnOutTicket;
        ArcIO.EvtInsertCoin += OnInsertCoin;
        ArcIO.EvtHardwareInfo += Hnalde_HardwareInfoRespon;
        ArcIO.EvtResultReadWrite += Handle_ResultReadWrite;
        //ArcIO.EvtLackCoin += OnLackCoin;
        //ArcIO.EvtLackTicket += OnLackTicket;
        ArcIO.Open();
        if (ArcIO.IsOpen())
        {
            DebugLog("已连接");
        }
        else
            DebugLog("未连接");
  
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
    void Handle_ResultReadWrite(bool isWrite, uint adress, byte datalen, byte resultCode,byte[] data)
    {
        //return;
        if (!mViewReadWrite)
            return;

        DebugLog(string.Format("[读写请求返回] 类型:{0}  地址:{1:d} 数据长度:{2:d} 结果码:{3:d} 内容:{4}"
            ,isWrite ? "写" : "读"
            ,adress
            ,datalen
            ,resultCode
            , data==null?"无":NemoSerial.ByteArrayToString(data)));
        //Debug.Log(string.Format("[读写请求返回] 类型:{0}  地址:{1:d} 数据长度:{2:d} 结果码:{3:d} 内容:{4}"
        //    , isWrite ? "写" : "读"
        //    , adress
        //    , datalen
        //    , resultCode
        //    , data == null ? "无" : NemoSerial.ByteArrayToString(data)));
    }
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
        DebugLog("控制板 :"+ctrlBoardid + (state?"已连接":"未连接"));
    }

    void Hnalde_HardwareInfoRespon(int gameIdx, int mainVer, int subVer, bool verifySucess)
    {
        DebugLog("验证信息返回 :" + (verifySucess? "验证成功":"验证失败") 
            +"  所属游戏:"+gameIdx+"  主版本号:"+mainVer+"  副版本号:"+subVer);
    }
    void OnBackGroundKeyEvent(NemoCtrlIO_Key k, bool downOrUp)
    {
        
        DebugLog("后台按键:" + k + (downOrUp ? " 按下" : " 松开"));
    }

    void OnInsertCoin(uint coinnum,int playerIdx)
    {
        DebugLog("投币,玩家:" + playerIdx);
    }
    void OnOutCoin( int playerIdx)
    {
        DebugLog("出币返回,玩家:" + playerIdx );
    }

    void OnOutTicket( int playerIdx)
    {
        DebugLog("出票返回,玩家:" + playerIdx);
    }

    void OnLackCoin(int playerIdx)
    {
        DebugLog("缺币,玩家:" + playerIdx );
    }

    void OnLackTicket(int playerIdx)
    {
        DebugLog("缺票,玩家:" + playerIdx);
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
                    GUILayout.BeginHorizontal("box");
                    {
                        GUILayout.Label("机台号(0-5):");
                        if (GUILayout_IntArea(ref PlayerIdx))//player改变
                        {

                        }
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button("出币"))
                    {
                        ArcIO.OutCoin(PlayerIdx);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("出票"))
                    {
                        ArcIO.OutTicket(PlayerIdx);
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();

                    if (GUILayout.Button("请求控制板信息"))
                    {

                        ArcIO.RequestHardwareInfo();
                    }
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("闪灯" + (mLighttingTag[PlayerIdx] ? "(亮ing)" : "(灭ing_")))
                        {
                            //if (mLighttingTag[PlayerIdx] != null)
                                mLighttingTag[PlayerIdx] = !mLighttingTag[PlayerIdx];

                            ArcIO.FlashLight(PlayerIdx, mButtomLightIdx, mLighttingTag[PlayerIdx]);

                        }
                        GUILayout.Label("灯ID(0-1):");
                        GUILayout_IntArea(ref mButtomLightIdx);
                    }

                    GUILayout.EndHorizontal();

                    //读写请求
                    GUILayout.BeginVertical("box");
                    {
                        int tagReadWrite = -1;
                        GUILayout.BeginHorizontal();
                        {
                            
                            if (GUILayout.Button("读取"))
                                tagReadWrite = 0;
                            if (GUILayout.Button("写入"))
                                tagReadWrite = 1;
                            GUILayout.Label("地址:");
                            GUILayout_UIntArea(ref mReadWrite_address);
                            GUILayout.Label("长度:");
                            GUILayout_ByteArea(ref mReadWrite_dataLen);
                            //GUILayout.Label("显示:");
                            mViewReadWrite = GUILayout.Toggle(mViewReadWrite,"显示");

                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("连续写入次数:");
                            GUILayout_IntArea(ref mNumToWrite);
                            GUILayout.Label("数据:");
                            GUILayout_ULongArea(ref mReadWrite_data);
                        }
                        GUILayout.EndHorizontal();

                        if (tagReadWrite != -1)
                        {
                            bool IsWrite = tagReadWrite == 1 ? true : false;


                            byte[] dataToWrite = null;
                            if (IsWrite)
                            {
                                dataToWrite = new byte[mReadWrite_dataLen];
                                byte[] sourceData = System.BitConverter.GetBytes(mReadWrite_data);
                                int copyLen = sourceData.Length > mReadWrite_dataLen ? mReadWrite_dataLen : sourceData.Length;
                                System.Array.Copy(sourceData, 0, dataToWrite, 0, copyLen);

                                for (int i = 0; i != mNumToWrite; ++i)
                                {
                                    ArcIO.RequestReadWrite(IsWrite, mReadWrite_address, mReadWrite_dataLen, dataToWrite);
                                }
                            }
                            else
                            {
                                ArcIO.RequestReadWrite(IsWrite, mReadWrite_address, mReadWrite_dataLen, dataToWrite);
                            }

                            
                        }
                    }
                    GUILayout.EndVertical();
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

    public void DebugLog(string str)
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

    private bool GUILayout_UIntArea(ref uint val)
    {
        uint oldVal = val;
        string valStr = GUILayout.TextArea(val.ToString(), GUILayout.MinWidth(TextAreaMinWidth));
        uint newVal = uint.Parse(valStr);
        if (newVal != oldVal)
        {
            val = newVal;
            return true;
        }
        return false;

    }

    private bool GUILayout_ByteArea(ref byte val)
    {
        byte oldVal = val;
        string valStr = GUILayout.TextArea(val.ToString(), GUILayout.MinWidth(TextAreaMinWidth));
        byte newVal = byte.Parse(valStr);
        if (newVal != oldVal)
        {
            val = newVal;
            return true;
        }
        return false;
    }

    private bool GUILayout_ULongArea(ref ulong val)
    {
        ulong oldVal = val;
        string valStr = GUILayout.TextArea(val.ToString(), GUILayout.MinWidth(TextAreaMinWidth));
        ulong newVal = ulong.Parse(valStr);
        if (newVal != oldVal)
        {
            val = newVal;
            return true;
        }
        return false;
    }

}

