#define ENABLE_MASK
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using Mono.Data.Sqlite;
using System.Runtime.InteropServices;

public interface IReleasable 
{
    void Release();
}

//解决在模板类中有多个静态对象问题
public class StaticValueContainer 
{
    protected static string DataBaseName = "HpyFishDB.db";
    protected static SqliteConnection mConn;
    protected static int SqliteConnRefCount = 0;//引用计数
    protected static SqliteCommand mCMD;
}
public class HotFileDatabaseIO<ValueType> : StaticValueContainer, IReleasable 
{
    
    private string mKey;//键,对应数据库中的表
    //private bool mFileInited = false;//文件已经初始化,路劲,等数据已准备 
    //private static ValueType DefVal;
    private System.Data.DbType mDBType;//数据库类型 
    private string mUpdateSQLText;
    public HotFileDatabaseIO(string key, string filePath)
    {
        
        //初始化数据连接
        if (mConn == null)
        {
            //Debug.Log(Application.persistentDataPath);
            if (DataBaseName == "")
                DataBaseName = "GameDB.db";
#if UNITY_ANDROID
            string dataPath =  Application.persistentDataPath +"/DataFiles";
#else
            string dataPath = System.Environment.CurrentDirectory + "/DataFiles";
#endif
            //Debug.Log("dataPath=" + dataPath);
            if (!Directory.Exists(dataPath))
            {
                Directory.CreateDirectory(dataPath);
            }

#if UNITY_ANDROID
            mConn = new SqliteConnection("URI=file:"+ dataPath +"/"+ DataBaseName + ";");
#else
            mConn = new SqliteConnection("Data Source = DataFiles/" + DataBaseName + ";");
#endif
            mConn.Open();

            if (mCMD == null)
            {
                mCMD = mConn.CreateCommand();
                mCMD.Parameters.Add(new SqliteParameter());
            }

            mCMD.CommandText = "PRAGMA journal_mode =wal;";//wal模式
            mCMD.ExecuteNonQuery();
             
        }

        

        mKey = key;
        mDBType = GetDBType();
        //创建表

        mCMD.CommandText = string.Format("CREATE TABLE IF NOT EXISTS {0:s}(Val {1:s} KEY DEFAULT {2:s});", mKey, GetDBTypeString(), GetDefaultDBValString());
        mCMD.ExecuteNonQuery();
 
        //初始化插入一行
        mCMD.CommandText = " SELECT COUNT(*) FROM " + mKey + ";";
        long rowNum = System.Convert.ToInt64(mCMD.ExecuteScalar());
   
        if (rowNum < 1)
        {
#if ENABLE_MASK
            if (typeof(ValueType) == typeof(int))
            {
                mCMD.CommandText = string.Format("INSERT INTO {0:s} (Val) VALUES ({1})", mKey, 0x7129A9AD);
            }
            else
            {
                mCMD.CommandText = string.Format("INSERT INTO {0:s} DEFAULT VALUES", mKey);
            }
            
#else
            mCMD.CommandText = string.Format("INSERT INTO {0:s} DEFAULT VALUES", mKey);
#endif

            mCMD.ExecuteNonQuery();
        }
//         if (typeof(ValueType) == typeof(string))
//             mUpdateSQLText = "UPDATE  " + mKey + " SET Val = '?';";
//         else
            mUpdateSQLText = "UPDATE " + mKey + " SET Val = ?;";

        ++SqliteConnRefCount;
        HotFileDBIOReleaser.HotFileDBReg(this);
 
    }
 

    public void Release()
    {
 

        --SqliteConnRefCount;
        if (SqliteConnRefCount == 0)
        {
            //Debug.Log("Close Conn");
            if (mCMD != null)
            {
                mCMD.Dispose();//如果不调用该函数,会导致不能关闭数据库
                mCMD = null;
            }
            if (mConn != null)
            {
                mConn.Close();
                mConn.Dispose();
                mConn = null;
            }
            
        }
    }

    /// <summary>
    /// 清扫文件(清空文件内容)
    /// </summary>
    public void Clearup()
    {
        //重置文件
        //ValueType tmpVal = Read();

        //mFileDescriptor = _wopen(mFileFullPath, 0x0200 | 0x0001, 0x0080 | 0x0100);//trunc|writeOnly,read|write
        //_close(mFileDescriptor);
        //mFileDescriptor = 0;

        //FlushWrite(tmpVal);
    }


    public ValueType Read()
    {
 
        mCMD.CommandText = string.Format(" SELECT * FROM {0:s};", mKey);
        //object outVal = mCMD.ExecuteScalar();
        return ObjectToVal(mCMD.ExecuteScalar());
    }

    /// <summary>
    /// 刷写
    /// </summary>
    /// <param name="value"></param>
    public void FlushWrite(ValueType value)
    {
        try
        {

            mCMD.CommandText = mUpdateSQLText;
            mCMD.Parameters[0].DbType = mDBType;
            mCMD.Parameters[0].Value = (object)value;
     
            mCMD.ExecuteNonQuery();
        }
        catch (System.Exception ex)
        { 
            Debug.LogError(ex.Message);
        }

    }


    private string GetDefaultDBValString()
    {
        if (typeof(ValueType) == typeof(int)
                    || typeof(ValueType) == typeof(uint)
                    || typeof(ValueType) == typeof(bool)
                    || typeof(ValueType) == typeof(ushort)
                    || typeof(ValueType) == typeof(long)
                    || typeof(ValueType) == typeof(short)
                    || typeof(ValueType) == typeof(ulong)
                    )
        {
            return "0";
        }

        if (typeof(ValueType) == typeof(float)
            || typeof(ValueType) == typeof(double))
        {
            return "0.0";
        }

        if (typeof(ValueType) == typeof(string))
        {
            return "''";
        }
        return "0";
    }
    private string GetDBTypeString()
    {
        if (typeof(ValueType) == typeof(int)
                    || typeof(ValueType) == typeof(bool)
                    || typeof(ValueType) == typeof(ushort)
                    || typeof(ValueType) == typeof(short)
                    )
        {
            return "INTEGER";
        }

        if(typeof(ValueType) == typeof(uint)
                    || typeof(ValueType) == typeof(long)
                    || typeof(ValueType) == typeof(ulong)
            )
        {
            return "BIGINT";
        }

        if (typeof(ValueType) == typeof(float)
            || typeof(ValueType) == typeof(double))
        {
            return "REAL";
        }

        if (typeof(ValueType) == typeof(string))
        {
            return "TEXT";
        }

        return "INTEGER";
    }

    private System.Data.DbType GetDBType()
    {
        if (typeof(ValueType) == typeof(int))
            return System.Data.DbType.Int32;
        else if(typeof(ValueType) == typeof(bool))
            return System.Data.DbType.Boolean;
        else if(typeof(ValueType) == typeof(ushort))
            return System.Data.DbType.UInt16;
        else if(typeof(ValueType) == typeof(long)
            || typeof(ValueType) == typeof(uint))
            return System.Data.DbType.Int64;
        else if(typeof(ValueType) == typeof(short))
            return System.Data.DbType.Int16;
        else if(typeof(ValueType) == typeof(ulong))
            return System.Data.DbType.UInt64;
        else if(typeof(ValueType) == typeof(float))
            return System.Data.DbType.Single;
        else if(typeof(ValueType) == typeof(double))
            return System.Data.DbType.Double;
        else if(typeof(ValueType) == typeof(string))
            return System.Data.DbType.String;

        return System.Data.DbType.Int32;
    }
    private ValueType ObjectToVal(object val)
    {
        if (typeof(ValueType) == typeof(int))
        {
            return (ValueType)(System.Object)System.Convert.ToInt32(val);
        }
        else if (typeof(ValueType) == typeof(float))
        {
            return (ValueType)(System.Object)System.Convert.ToSingle(val);
        }
        else if (typeof(ValueType) == typeof(bool))
        {
            return (ValueType)(System.Object)System.Convert.ToBoolean(val);
        }
        else if (typeof(ValueType) == typeof(long))
        {
            return (ValueType)(System.Object)System.Convert.ToInt64(val);
        }

        else if (typeof(ValueType) == typeof(ushort))
        {
            return (ValueType)(System.Object)System.Convert.ToUInt16(val);
        }
        else if (typeof(ValueType) == typeof(uint))
        {
            return (ValueType)(System.Object)System.Convert.ToUInt32(val);
        }

        else if (typeof(ValueType) == typeof(char))
        {
            return (ValueType)(System.Object)System.Convert.ToChar(val);
        }
        else if (typeof(ValueType) == typeof(double))
        {
            return (ValueType)(System.Object)System.Convert.ToDouble(val);
        }

        else if (typeof(ValueType) == typeof(short))
        {
            return (ValueType)(System.Object)System.Convert.ToInt16(val);
        }
        else if (typeof(ValueType) == typeof(ulong))
        {
            return (ValueType)(System.Object)System.Convert.ToUInt64(val);
        }
        else if (typeof(ValueType) == typeof(string))
        {
            return (ValueType)(System.Object)System.Convert.ToString(val); ;
        }
        return (ValueType)(System.Object)0;


    }

 
    
}
