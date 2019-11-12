using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 街机输入输出,(锋板) 
/// </summary>
//[RequireComponent(typeof(NemoUsbHid))] 
public class ArcadeIO_Feng : MonoBehaviour {

    /// <summary>
    /// 按键
    /// </summary>
    public enum Key
    {
        Up,
        Down,
        Left,
        Right,
        A,B,C,D,
        E,F,G,H,
        I,J,K,L//预留
    }

    /// <summary>
    /// 按键灯模式
    /// </summary>
    public enum ButtomLightMode
    {
        AwalyOff= 0,        //灯常灭
        AwalyOn,           //灯常亮
        Flash,              //灯闪烁
        OffWhenClick,       //按键松开灯常亮，按下灯常灭
        OnWhenClick,        //按键按下灯常亮，松开灯常灭
        OffWhenClickFlash,  //按键松开灯闪烁，按下灯常灭
        OnWhenClickFlash,   //按键按下灯闪烁，松开灯常灭
    }

    /// <summary>
    /// 布景灯模式
    /// </summary>
    public enum ScenicLightMode
    {
        Off,        //全部灯常灭
        On,         //全部灯常亮
        FlashA,     //全部灯闪烁
        TraceA,     //追光（16只灯）
        FlashB,     //单双灯闪烁
        TraceB      //单灯跑马灯
    }

    public struct SendParam
    {
        public byte HD;//高位
        public byte LD;//低位
    }

    public delegate void GenericEvent(uint count, int playerIdx);

    /// <summary>
    /// 进入后台
    /// </summary>
    public delegate void EnterBackground();
    /// <summary>
    /// 按键时间
    /// </summary>
    /// <param name="ctrllerId">机台id</param>
    /// <param name="k">按键枚举</param>
    /// <param name="downOrUp">true:按下,false:松开</param>
    public delegate void KeyEvent(int ctrllerId, Key k, bool downOrUp);

    /// <summary>
    /// 后台按键
    /// </summary>
    /// <param name="k"></param>
    /// <param name="downOrUp"></param>
    public delegate void BackGroundKeyEvent(Key k, bool downOrUp);

    /// <summary>
    /// 硬件回应版本号
    /// </summary>
    /// <param name="main">主版本号</param>
    /// <param name="sub">次版本号</param>
    public delegate void VersionResponEvent(uint main, uint sub);

    /// <summary>
    /// 验证结果
    /// </summary>
    /// <param name="sucess"></param>
    public delegate void VerifyResultEvent(bool sucess);

    /// <summary>
    /// 文本验证
    /// </summary>
    /// <param name="sucess"></param>
    public delegate void TextVerifyEvent(byte[] sucess);


    /// <summary>
    /// 状态监测
    /// </summary>
    /// <param name="usbEquipOk">usb设备是否正常</param>
    /// <param name="arcIOOk">控制板是否正常</param>
    public delegate void StateCheckEvent(bool usbEquipOk, bool arcIOOk);

    public event GenericEvent HandlerInsertCoin;//投币时间
    /// <summary>
    /// 按键处理
    /// </summary>
    public event KeyEvent HandlerKey;
    public event GenericEvent HandlerOutCoinReflect;//出币回应
    public event GenericEvent HandlerOutTicketReflect;//出票回应
    public event GenericEvent HandlerLackCoin;//出币不足
    public event GenericEvent HandlerLackTicket;//出票不足
    //public event VersionResponEvent HandlerVersionRespon;//版本请求回应
    //public event VerifyResultEvent HandlerVerifyResult;//验证正版结果
    //public event EnterBackground HandlerEnterBackground;//进入后台
    public event BackGroundKeyEvent HandlerBackGroundKeyEvent;
    //public event TextVerifyEvent HandlerTextVerifyResult;//文本验证结果

    
    public event StateCheckEvent HandlerStateCheck;//状态监测


    private NemoUsbHid mSerial;
    
    private static int mPlayerNum = 6;//玩家数目,(机台数)
    private static int mDataByteNum = 6;//数据字节数
    private static int mKeyCountPerPlayer = 16;//一个玩家的按键数目

  
    private byte[][] mKeyBuff;//八个机台所有按键(16个,4个预留)的状态
    private BitArray[] mKeyBitBuff;//与KeyBuff同步

    private byte[] mVerifyInfo;//验证信息

    
 

    void Awake()
    {
           
        mKeyBuff = new byte[mPlayerNum][];//八个机台所有按键(16个,4个预留)的状态
        mKeyBitBuff = new BitArray[mPlayerNum];//与KeyBuff同步

        mVerifyInfo = new byte[2];//验证信息

        for (int i = 0; i != mPlayerNum; ++i)
        {
            mKeyBuff[i] = new byte[2];
        }


        for (int i = 0; i != mPlayerNum; ++i)
        {
            mKeyBitBuff[i] = new BitArray(mKeyCountPerPlayer, false);
        }

        mPackageSetup_ReadBuf = new byte[512]; 
        mPackageSetup_Readed = new List<byte>();
    }

	void Start () {
        if (mSerial == null)
        {
            mSerial = GetComponent<NemoUsbHid>();
            mSerial.HandleRecivePackage += OnRecivePackage;
            mSerial.HandleSetupPackage += Handle_PackageSetupProcess2;
            //mSerial.HandleSetupPackageT += Handle_PackageSetupProcess;//test
        }

        Open();
	} 

    public bool Open()
    {
        if (mSerial == null)
        {
            mSerial = GetComponent<NemoUsbHid>();
            mSerial.HandleRecivePackage += OnRecivePackage;
        }
 
        
 
        return mSerial.Open();
    }

    public void Close()
    {
        //mSeriasl.Close();
    }

    public class PackageGeneric : NemoUsbHid.Package
    {
        public int ID;
        public int Length;
    }
    public class PackTest1 : PackageGeneric
    {
        public int PlayerId;
        public int OutCointNum;
    }
    enum ReadPackageStage
    {
        NotStart,//未开始
        Head,//包头
        DataToEnd//读取数据和结束阶段
    };

    byte[] mPackageSetup_ReadBuf;
    int mPackageSetup_RestNum;
    List<byte> mPackageSetup_Readed;//一个包已读取字节

    int mPackSetupingLen;//组装中包的长度
    //byte mPackSetupingID;//组装中包的id
    ReadPackageStage mReadPackStage;

    int mPackHeadSize = 5;

    /// <summary>
    ///  组包处理
    /// </summary>
    /// <param name="datas"></param>
    /// <returns></returns>
    List<NemoUsbHid.Package> Handle_PackageSetupProcess2(byte[] datas)
    {
        List<NemoUsbHid.Package> packsOut = new List<NemoUsbHid.Package>();

        //int readNum = mPackageSetup_RestNum + datas.Length; 
        //System.Array.Copy(datas, 0, mPackageSetup_ReadBuf, mPackageSetup_RestNum, datas.Length);

        //for (int i = 0; i != datas.Length; ++i)
        foreach(byte b in datas)
        { 

            switch (mReadPackStage)
            {
                case ReadPackageStage.NotStart:
                    {
                        if (b == 0xFB)
                        { 
                            mPackageSetup_Readed.Add(b);
                            mReadPackStage = ReadPackageStage.Head; 
                        }
                    }
                    break;
                case ReadPackageStage.Head:
                    {
                        mPackageSetup_Readed.Add(b); 
                        if (mPackageSetup_Readed.Count == 5)
                        { 
                            mPackSetupingLen = 256 * mPackageSetup_Readed[3] + mPackageSetup_Readed[4];
                            mReadPackStage = ReadPackageStage.DataToEnd;

                            if (mPackSetupingLen > 256)
                            {
                                mReadPackStage = ReadPackageStage.NotStart;
                                mPackageSetup_Readed.Clear();
                                Debug.LogWarning("[ArcadeIO_Cheng] PackSetupingLen > 256,pack may wrong!");
                                //return packsOut;
                            }
                        }
                    }
                    break;
                case ReadPackageStage.DataToEnd:
                    {
                        mPackageSetup_Readed.Add(b);
                        if (mPackageSetup_Readed.Count == mPackSetupingLen + 5 + 1)
                        {
                            if (mPackageSetup_Readed[mPackageSetup_Readed.Count - 1]
                                == (mPackageSetup_Readed[1] ^ mPackageSetup_Readed[2]))//结尾副符合-组包完成
                            {
                                mReadPackStage = ReadPackageStage.NotStart;

                                //System.Array.Copy(mPackageSetup_ReadBuf, i + 1, mPackageSetup_ReadBuf, 0, readNum - i - 1);
        
                                //mPackageSetup_RestNum = readNum - i - 1;
                                NemoUsbHid.Package pOut = new NemoUsbHid.Package();
                                pOut.data = mPackageSetup_Readed.ToArray();
                                mPackageSetup_Readed.Clear();

                                packsOut.Add(pOut);
                            }
                            else//有错误包出现
                            { 
                                mReadPackStage = ReadPackageStage.NotStart;
                                mPackageSetup_Readed.Clear();
                                Debug.LogWarning("[ArcadeIO_Cheng] Wrong package appear in PackageSetup!");
                                //return packsOut;
                            }
                        }
                    }
                    break;

            }
        }
        //Debug.Log("outpack num = " + packsOut.Count.ToString());
        return packsOut;
    }

    //组包处理
    //1.mPackageSetup_ReadBuf考虑删除?
    NemoUsbHid.Package Handle_PackageSetupProcess(byte[] datas)
    {
        //Debug.Log("        count:" + datas.Length + "   read:" + NemoUsbHid.ByteArrayToString(datas));
        int readNum = mPackageSetup_RestNum + datas.Length;
        //Debug.Log("datas.Length = " + datas.Length.ToString() + "  mPackageSetup_RestNum = " + mPackageSetup_RestNum);
        System.Array.Copy(datas, 0, mPackageSetup_ReadBuf, mPackageSetup_RestNum, datas.Length);

        //Debug.Log("Handle_PackageSetupProcess readNum = " + readNum + " mPackageSetup_ReadBuf.l = " + mPackageSetup_ReadBuf.Length);
        for (int i = 0; i != readNum; ++i ) 
        {
            byte b = mPackageSetup_ReadBuf[i];

            switch (mReadPackStage)
            {
                case ReadPackageStage.NotStart:
                    {
                        if (b == 0xFB)
                        {
                            mPackageSetup_Readed.Add(b);
                            mReadPackStage = ReadPackageStage.Head;
                            //Debug.Log("NotStart to Head");
                        }
                    }
                    break;
                case ReadPackageStage.Head:
                    {
                        mPackageSetup_Readed.Add(b);
                        //Debug.Log("Head   mPackageSetup_Readed.length = " + mPackageSetup_Readed.Count + "b = " + b);
                        if (mPackageSetup_Readed.Count == 5)
                        {
                            //mPackSetupingID = 256 * mPackageSetup_Readed[1] + mPackageSetup_Readed[2];
                            mPackSetupingLen = 256 * mPackageSetup_Readed[3] + mPackageSetup_Readed[4];
                            mReadPackStage = ReadPackageStage.DataToEnd;
                            //Debug.Log("Head to DataToEnd");
                        }
                    }
                    break;
                case ReadPackageStage.DataToEnd:
                    {
                        mPackageSetup_Readed.Add(b);
                        if (mPackageSetup_Readed.Count == mPackSetupingLen + 5 + 1)
                        {
                            if (mPackageSetup_Readed[mPackageSetup_Readed.Count - 1] 
                                == (mPackageSetup_Readed[1] ^ mPackageSetup_Readed[2]))//结尾副符合-组包完成
                            {
                                mReadPackStage = ReadPackageStage.NotStart;

                                System.Array.Copy(mPackageSetup_ReadBuf, i+1, mPackageSetup_ReadBuf,0, readNum - i -1);
                                //Debug.Log("package setuping:" + NemoUsbHid.ByteArrayToString(mPackageSetup_Readed.ToArray()));

                                //test Start
                                //byte[] testBytes = new byte[readNum - i - 1];
                                //System.Array.Copy(mPackageSetup_ReadBuf, testBytes, readNum - i - 1);
                                //Debug.Log("package setuping:"+NemoUsbHid.ByteArrayToString(testBytes));
                                //test End

                                mPackageSetup_RestNum = readNum - i - 1;
                                NemoUsbHid.Package p = new NemoUsbHid.Package();
                                p.data = mPackageSetup_Readed.ToArray();
                                mPackageSetup_Readed.Clear();

                                //Debug.Log("package : " + NemoUsbHid.ByteArrayToString(p.data));
                                return p;
                            }
                            else//有错误包出现
                            {
                                //test Start
                                //Debug.Log("package erro msg:" + NemoUsbHid.ByteArrayToString(mPackageSetup_Readed.ToArray()));
                                //test End

                                mReadPackStage = ReadPackageStage.NotStart;
                                mPackageSetup_Readed.Clear();
                                Debug.LogWarning("[ArcadeIO_Cheng] Wrong package appear in PackageSetup!");
                                return null;
                            }
                        }
                    }
                    break;

            } 
        } 
        mPackageSetup_RestNum = 0;
        return null;
    }


    void OnRecivePackage(NemoUsbHid.Package p)
    { 
 
 
        //////////////////////////////////////////////////////////////////////////
        //新命令
        //投币
        if (p.data[2] == 0x11)
        {
            int playeridx = p.data[mPackHeadSize];
            if (HandlerInsertCoin != null)
                HandlerInsertCoin(1, playeridx);
        }

        //控制器控制
        if (p.data[2] == 0x21)
        {
            //Debug.Log("p.data.len = " + p.data.Length);
            int playeridx = p.data[mPackHeadSize];
            Key k = (Key)p.data[mPackHeadSize + 1];
            bool isDown = p.data[mPackHeadSize + 2] == 1?true:false;

            if (HandlerKey != null)
                HandlerKey(playeridx, k, isDown);
        }

        //退币响应
        if (p.data[2] == 0x51)
        {
            int playeridx = p.data[mPackHeadSize];
            if (HandlerOutCoinReflect != null)
                HandlerOutCoinReflect(1, playeridx);
        }
        //退票响应
        if (p.data[2] == 0x61)
        {
            int playeridx = p.data[mPackHeadSize];
            if (HandlerOutTicketReflect != null)
                HandlerOutTicketReflect(1, playeridx);
        }

        //退币不足
        if (p.data[2] == 0x32)
        {
            int playeridx = p.data[mPackHeadSize];
            uint num = System.BitConverter.ToUInt32(p.data, mPackHeadSize + 1);
            if (HandlerLackCoin != null)
                HandlerLackCoin(num, playeridx);
        }
        //退票不足
        if (p.data[2] == 0x42)
        {
            int playeridx = p.data[mPackHeadSize];
            uint num = System.BitConverter.ToUInt32(p.data, mPackHeadSize + 1);
            if (HandlerLackTicket != null)
                HandlerLackTicket(num, playeridx);
        }

        //后台按键
        if (p.data[2] == 0xc3)
        { 
            Key k = (Key)p.data[mPackHeadSize  ];
            bool isDown = p.data[mPackHeadSize + 1] == 1?true:false;

            if (HandlerBackGroundKeyEvent != null)
                HandlerBackGroundKeyEvent( k, isDown); 
        }

        //状态返回
        if (p.data[2] == 0x71)
        {
            byte usbState = p.data[mPackHeadSize];
            byte arcState = p.data[mPackHeadSize + 1];
            if (HandlerStateCheck != null)
                HandlerStateCheck(usbState == 1 ? true : false, arcState == 1 ? true : false);
        }
    }


    /// <summary>
    /// 数字转高地位
    /// </summary>
    /// <param name="num">数字</param>
    /// <param name="hd">高位</param>
    /// <param name="ld">低位</param>
    private void _NumberToHDLD(uint num,out byte hd,out byte ld)
    {
        hd = (byte)(num / 256);
        ld = (byte)(num % 256);
    }
    
    /// <summary>
    /// 高低位转数字
    /// </summary>
    /// <param name="hd">高位</param>
    /// <param name="ld">低位</param>
    /// <returns></returns>
    private uint _HDLDToNumber(byte hd,byte ld)
    {
        return (uint)(hd * 256 + ld);
    }

    /// <summary>
    /// 玩家台号id转成传输参数byte
    /// </summary>
    /// <remarks> 规则参见串口通讯 </remarks>
    /// <param name="playerIdx"></param>
    /// <returns></returns>
    private byte _PlayerIdxToParamByte(int playerIdx)
    {
        return (byte)Mathf.Pow(2F, (float)playerIdx);
    }

    /// <summary>
    /// 与_PlayerIdxToParamByte相反,参见_PlayerIdxToParamByte
    /// </summary>
    /// <param name="paramByte"></param>
    /// <returns></returns>
    private int _ParamByteToPlayerIdx(byte paramByte)
    {
        return (byte)Mathf.Log((float)paramByte, 2F);
    }
    private void _Send(byte cmd,SendParam param)
    {
        byte[] DataToSend = new byte[mDataByteNum];
        DataToSend[0] = 0Xfa;
        DataToSend[1] = cmd;
        DataToSend[2] = param.HD;
        DataToSend[3] = param.LD;
        DataToSend[4] = (byte)(cmd + param.HD + param.LD);
        DataToSend[5] = (byte)(DataToSend[4] + 1);

        mSerial.Send(DataToSend);
    }

    /// <summary>
    /// 发送byte数据
    /// </summary>
    /// <remarks>
    /// 1.忽略效验数据位(2字节)
    /// 2.data长度必须为4
    /// </remarks>
    /// <param name="cmd"></param>
    /// <param name="data"></param>
    private void _SendByteData(byte cmd, byte[] data)
    {
        if(data == null)
        {
            return;
        }
 

        byte[] DataToSend = new byte[mDataByteNum];
        DataToSend[0] = 0Xfa;
        DataToSend[1] = cmd;
        //DataToSend[2] = data[0];
        //DataToSend[3] = data[1];
        //DataToSend[4] = data[2];
        //DataToSend[5] = data[3];
        data.CopyTo(DataToSend, 2);
        mSerial.Send(DataToSend);
    }

    /// <summary>
    /// 出币
    /// </summary>
    /// <param name="num">数量</param>
    /// <param name="PlayerIdx">玩家id(即机台号,从0开始)</param>
    public void OutCoin(uint num, int playerIdx)
    {
        //Debug.Log("out coin playerIdx=" + playerIdx);
        //byte cmd = (byte)(0x11 + playerIdx);
        //SendParam param;
        //_NumberToHDLD(num, out param.HD, out param.LD);
        //_Send(cmd, param);
        if (mSerial == null)
            return;
        byte[] numBytes = System.BitConverter.GetBytes(num);
        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x11, 0x00, 0x05
                                     , (byte)playerIdx, numBytes[0], numBytes[1], numBytes[2], numBytes[3]
                                     , 0x11};
        
        mSerial.Send(dataToSend);

    }

    /// <summary>
    /// 出票
    /// </summary>
    /// <param name="num"></param>
    /// <param name="playerIdx"></param>
    public void OutTicket(uint num, int playerIdx)
    {
        //byte cmd = (byte)(0x21 + playerIdx);
        //SendParam param;
        //_NumberToHDLD(num, out param.HD, out param.LD);
        //_Send(cmd, param);
        if (mSerial == null)
            return;
        byte[] numBytes = System.BitConverter.GetBytes(num);
        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x21, 0x00, 0x05
                                     , (byte)playerIdx, numBytes[0], numBytes[1], numBytes[2], numBytes[3]
                                     , 0x21};
 
        mSerial.Send(dataToSend);
    }

    /// <summary>
    /// 监测状态(心跳包)
    /// </summary>
    public void CheckState()
    {
        if (mSerial == null)
            return;
 
        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x71, 0x00, 0x00,   0x71}; 
        mSerial.Send(dataToSend);
    }

    /// <summary>
    /// 按键灯闪,(闪过程中再发送命令会重置)
    /// </summary>
    /// <param name="playerIdx"></param>
    public void FlashButtomLight(int playerIdx)
    {
        if (mSerial == null)
            return;
        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x51, 0x00, 0x01
                                     , (byte)playerIdx 
                                     , 0x51};
        mSerial.Send(dataToSend);
 
    }

    /// <summary>
    /// 设置灯
    /// </summary>
    /// <param name="playerIdx"></param>
    /// <param name="lightIdx"></param>
    /// <param name="mode"></param>
    public void SetButtomLight(int playerIdx, int lightIdx, ButtomLightMode mode)
    {

        SendParam param;
        param.HD = (byte)Mathf.Pow(2.0f, (float)playerIdx);
        param.LD = (byte)( (byte)mode  << 4);
        param.LD += (byte)lightIdx;

        _Send(0xAA, param);
    }

    public void SetScenicLight(ScenicLightMode mode)
    {
        SendParam param;
        param.HD = (byte)mode;
        param.LD = 0;
        _Send(0xcc, param);
    }

    ///// <summary>
    ///// 要求硬件板返回版本号
    ///// </summary>
    //public void RequestHardwareVersion()
    //{
    //    _Send(0x88, new SendParam());
    //}

    /// <summary>
    /// 请求验证硬件是否正版
    /// </summary>
    public void RequestVerifyHardware()
    {
        mVerifyInfo[0] = (byte)Random.Range(0, 255);
        mVerifyInfo[1] = (byte)Random.Range(0, 255);
        SendParam param;
        param.HD = mVerifyInfo[0];
        param.LD = mVerifyInfo[1];
        _Send(0xc4, param);
    }

    /// <summary>
    /// 设置实时奖励模式
    /// </summary>
    /// <remarks> 默认实时奖励模式为开</remarks>
    /// <param name="realtime">true:实时(每出一币返回一次),false:非实时(全部出完再返回消息)</param>
    public void SetRealTimeOutBounty(bool realtime)
    {
        SendParam param;
        param.HD = 0;
        param.LD = (byte)(realtime ? 1 : 0);
        _Send(0xbb, param);
    }
    /// <summary>
    /// 获得按键状态
    /// </summary>
    /// <param name="playerid"></param>
    /// <param name="k"></param>
    /// <returns></returns>
    public bool GetKeyState(int playerid,Key k)
    {
        return mKeyBitBuff[playerid][(int)k];
    }

    /// <summary>
    /// 补币
    /// </summary>
    /// <param name="playerIdx"></param>
    public void ResponCoin(int playerIdx)
    {
        //SendParam param;
        //param.HD = 0;
        //param.LD = 0;

        //byte cmd = (byte)(0x31 + playerIdx);
 
        //_Send(cmd, param);
 
        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x31, 0x00, 0x01
                                     , (byte)playerIdx
                                     , 0x31};
        mSerial.Send(dataToSend);
    }

    /// <summary>
    /// 补票
    /// </summary>
    /// <param name="playerIdx"></param>
    public void ResponTicket(int playerIdx)
    {
        //SendParam param;
        //param.HD = 0;
        //param.LD = 0;
        //byte cmd = (byte)(0x41 + playerIdx);
        //_Send(cmd, param);

        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x41, 0x00, 0x01
                                     , (byte)playerIdx
                                     , 0x41};
        mSerial.Send(dataToSend);
    }

    /// <summary>
    /// 发送验证信息
    /// </summary>
    /// <param name="txt"></param>
    public void SendCheckPlainText(byte[] bs)
    {
        if (bs.Length > 4 || bs.Length == 0)
        {
            Debug.LogError("SendCheckPlainText 不符合规则!");
            return;
        }
        _SendByteData(0xC1, bs);
    }

    /// <summary>
    /// 清除硬件方面的缺币缺币数据
    /// </summary>
    public void ClearLackBountyData()
    {
        SendParam param;
        param.HD = 0;
        param.LD = 0;
        byte cmd = (byte)(0xCD);
        _Send(cmd, param);
    }

    /// <summary>
    /// 请求发送大量数据
    /// </summary>
    /// <param name="dataLen"></param>
    /// <param name="sendInterval"></param>
    /// <param name="onOff"></param>
    public void RequestDataBack(byte dataLen,byte sendInterval,bool isOn)
    {
        byte[] dataToSend = new byte[]{0xFA, 0x00, 0x72, 0x00, 0x03
                                     ,dataLen,sendInterval,(byte)(isOn?1:0)
                                     , 0x72};
        mSerial.Send(dataToSend);
    }
}
