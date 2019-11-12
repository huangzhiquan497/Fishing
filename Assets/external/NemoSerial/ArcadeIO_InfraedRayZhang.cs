using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 触摸板通讯协议：
    波特率:38400
    
    心跳信号:
        PC->MCU   0X90
        MCU->PC   0XF5
        PC首先向MCU发送心跳信号0X90，MCU收到0X90后回复0XF5,时间为100毫秒一次


    暂停通讯信号： 
        PC->MCU   0X91
        这个信号暂停PC与MCU之间的通讯，回复通讯PC发送0X90到MCU

    触摸板复位信号：
        PC->MCU   0XF0

    坐标信号：
        一个7个数据(7字节)：
            数据1：0XF5
            数据2：第0位为1，其它数据位忽略，但数据2不能是0XF5
            数据3：可以忽略
            数据4：X坐标
            数据5：Y坐标
            数据6：X坐标
            数据7：Y坐标

            图示:
                位(bit):7	6	5	4	3	2	1	0
     
                数据4：	X7		X5		X3		X1
                数据5：	Y7		Y5		Y3		Y1		
                数据6：		X6		X4		X2		X0
                数据7：	Y6		Y4		Y2		Y0
                (除有效数据位,其他数据位为随机数)
 */

public class ArcadeIO_InfraedRayZhang : MonoBehaviour {
    public int HoriRayNum = 40;
    public int VertRayNum = 23;
    public delegate void TouchScreen(float x,float y);
    public event TouchScreen HandleTouchScreen;

    private NemoSerial mSerial;
    // Use this for initialization
    //private List<byte> mTouchScreenByteLs;
    private byte[] mTouchScreenBytes;
    private int mTouchScreenIdx;

    private List<Vector2> mTouchDatas;
    private Vector2 mPreTouchPos = Vector2.zero;
    private float mPreTouchTime = 0F;

    void Awake()
    {
        if (mSerial == null)
            mSerial = GetComponent<NemoSerial>();
 
        //mTouchScreenByteLs = new List<byte>();
        mTouchScreenBytes = new byte[4096];
        mTouchScreenIdx = 0;
        //mRecivedBytes = new List<byte>();
        mTouchDatas = new List<Vector2>();
    }
	

	// Use this for initialization
	void Start () {

        Open();
        mSerial.AutotoSend(new byte[] { 0x90 }, 100);
        //while (true)
        //{
        //    SendHeartPack(); 
        //    yield return new WaitForSeconds(0.1F);
            
        //}
        //StartCoroutine(Coro_CheckTouchDatas());
	}
	
	// Update is called once per frame
    //void Update () {
    //}

    void OnGUI1()
    {
        if (GUILayout.Button("open"))
        {
            Open();
        }

        if (GUILayout.Button("send heart pack"))
        {
            SendHeartPack();
        }

        if (GUILayout.Button("StopMCU"))
        {
            StopMCU();
        }
        if (GUILayout.Button("resetMCU"))
        {
            ResetMCU();
        }
    }

    public bool Open()
    {
        if (mSerial == null)
            mSerial = GetComponent<NemoSerial>();

        //mSerial.BaudRate = 38400;
        mSerial.HandleRecivePackage += OnRecivePackage;
// 
//         mSerial.BytesPerPackage = 7;
//         mSerial.BaudRate = 38400;
//         mSerial.PortName = "COM8";
        bool sucess = mSerial.Open();
  
        return sucess;
    }

    void OnRecivePackage(NemoSerial.Package p)
    {
        //Debug.Log("mTouchScreenIdx =" + mTouchScreenIdx + "   p.data.Length = " + ((p.data == null) ? 0:p.data.Length));
        int writtedIdx = mTouchScreenIdx;
        mTouchScreenBytes[mTouchScreenIdx] = p.data[0];
        ++mTouchScreenIdx;

        if (writtedIdx == 1 && mTouchScreenBytes[writtedIdx] == 0xf5)//之前一字节是心跳包
        {
 
            --mTouchScreenIdx;//丢弃心跳包内容
        }

        if (writtedIdx == 6)
        {
            mTouchScreenIdx = 0; 
            ProcessDataPackage(mTouchScreenBytes);
        }

         
    }

    void ProcessDataPackage(byte[] data)
    {
        //string str = "";
        //foreach (byte d in p.data)
        //{
        //    str += string.Format("{0:X},", d);
        //}
        if (data[1] == 0xf5)
            Debug.Log("erro---------------");
        if ((data[1] & 0x1) == 0)
        {
            Debug.Log("bad pack.");
        }

        int x = (data[3] & 0xaa) | (data[5] & 0x55);
        int y = (data[4] & 0xaa) | ((data[6] & 0xaa) >> 1);

        x = (x - 130) / 2;
        y = (y - 2) / 2;

        Vector2 curPos = new Vector2((float)(x + 1) / (HoriRayNum + 1), 1F - (float)(y + 1) / VertRayNum);
        if ((mPreTouchPos - curPos).magnitude > 0.075F || (Time.realtimeSinceStartup - mPreTouchTime) > 0.1F)
        {
            if (HandleTouchScreen != null)
                HandleTouchScreen(curPos.x, curPos.y);
            //mTouchDatas.Add(curPos);
        }

        mPreTouchPos = curPos;
        mPreTouchTime = Time.realtimeSinceStartup;
    }

    IEnumerator Coro_CheckTouchDatas()
    {
        while (true)
        {

            if (mTouchDatas.Count == 0)
            {
                yield return new WaitForSeconds(0.025F);
                continue;
            }

            Debug.Log("mTouchDatas.count = " + mTouchDatas.Count);
            Vector2[] poses = mTouchDatas.ToArray();
            List<Vector2> removeList = new List<Vector2>();
            for (int i = 0; i != poses.Length - 1; ++i)
            {
                for (int j = i + 1; j != poses.Length; ++j)
                {
                    if (poses[i] == Vector2.zero || poses[j] == Vector2.zero)
                        continue;
                    if ((poses[i] - poses[j]).magnitude < 0.1F)
                    {
                        removeList.Add(poses[i]);
                        //removeList.Add(poses[j]);
                        poses[i] = Vector2.zero;
                        //poses[j] = Vector2.zero;
                    }

                }
            }

            
            foreach (Vector2 delPos in removeList)
            {
                Debug.Log("remove Pos");
                mTouchDatas.Remove(delPos);

            }
            Debug.Log("mTouchDatas.length = " + mTouchDatas.Count);
            foreach (Vector2 touchPos in mTouchDatas)
            {
                if (HandleTouchScreen != null)
                    HandleTouchScreen(touchPos.x, touchPos.y);
            }
            mTouchDatas.Clear();
        }
    }
    public void SendHeartPack()
    {
        mSerial.Send(new byte[1] { 0x90 });
    }

    public void StopMCU()
    {
        mSerial.Send(new byte[1] { 0X91 });
    }

    public void ResetMCU()
    {
        mSerial.Send(new byte[1] { 0XF0 });
    }
}
