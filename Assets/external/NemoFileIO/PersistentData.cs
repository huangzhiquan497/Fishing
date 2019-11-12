#define ENABLE_MASK
//#if UNITY_EDITOR1//编辑模式下打开写数据库
    #define ENABLE_WRITE_DB//不写入数据库(只写入FARM,提高效率)
//#endif
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// 持久数据封装
/// </summary>
/// <remarks>主要作用是第一次读取时读取硬盘,之后就读取内存,而不用每次都读取硬盘</remarks>
/// <typeparam name="ValType">输出值类型</typeparam>
/// <typeparam name="StoreType">存储的类型</typeparam>
/// <remark>
/// 注意:
///     1.保证文件操作在另外一条线程中进行,
/// </remark>
public class PersistentData<ValType, StoreType>  
//where ValType : StoreType ,IEnumerable ,IEnumerator
{
    private HotFileDatabaseIO<StoreType> mDataInFile;
    private bool mHaveReaded = false;//已经从硬盘读取了
    private ValType mVal;
//#if ENABLE_WRITE_DB
    private bool mIsRequestingWrite = false;
//#endif
    private string mName;
    public PersistentData(string name)
    {

        string path = Directory.GetCurrentDirectory() + "/DataFiles/HotRecord/";
        mDataInFile = new HotFileDatabaseIO<StoreType>(name, path);
        mName = name;
        mVal = Mask(Val);
    }
    public ValType Mask(ValType val)
    {
#if ENABLE_MASK
        if (typeof(StoreType) == typeof(int))
        {
            return (ValType)(System.Object)((int)(System.Object)(val) ^ 0x7129A9AD);
        }
#endif
        return val;
    }

    
    public ValType Val
    {
        get
        {
            if (!mHaveReaded)//第一次需要读文件
            {
                //mVal = (ValType)(System.Object)mDataInFile.Read();
                mVal = (ValType)(System.Object)mDataInFile.Read();
                
                mHaveReaded = true;
            }
            //Debug.Log(" get Val = " + mVal + "   maskValue =" + Mask(mVal) + "  type = " + typeof(ValType) + "  mName=" + mName);
            return Mask(mVal);//UnMask
        }

        set
        {
            //mVal = value;
            //return;
            if ((System.Object)mVal != (System.Object)Mask(value))
            {
                //Debug.LogWarning(" set Val = " + value + "   maskValue =" + Mask(value) + "  type = " + typeof(ValType) + "  mName=" + mName);
                mVal = Mask(value);
                //mVal = value;
                //mDataInFile.WriteToFRAM((StoreType)(System.Object)mVal);//改为立即写入
        
            //#if ENABLE_WRITE_DB 
                if (!mIsRequestingWrite)
                {
                    mIsRequestingWrite = true;
                    PersistentDataCoroWriter.CommitWriteRequest(Handle_WritePermission);
                }
            //#endif
                
            }

        }
    }

    //得到请求答复
    void Handle_WritePermission()
    {
        //return;
#if ENABLE_WRITE_DB
        mDataInFile.FlushWrite((StoreType)(System.Object)mVal);
#endif
        //mDataInFile.WriteToFRAM((StoreType)(System.Object)mVal);
        mIsRequestingWrite = false;
    }


    /// <summary>
    /// 设置数值并立即写入硬盘
    /// </summary>
    /// <param name="val"></param>
    public void SetImmdiately(ValType val)
    {
        //mVal = val;
        //return;
        if ((System.Object)mVal != (System.Object)Mask(val))
        {
            mVal = Mask(val);
            //mVal = val;
            //mDataInFile.WriteToFRAM((StoreType)(System.Object)mVal);//改为立即写入
#if ENABLE_WRITE_DB 
            mDataInFile.FlushWrite((StoreType)(System.Object)mVal);
#endif
        }
    }
}