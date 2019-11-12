using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;




public class HotFileBinaryLowIO<ValueType>
{ 

    private string mFileFullPath;
    private static byte mCheckByte = 0xaa;//§¹????¦ËmCheckByte^dataByte1^dataByte2^dataByte3^...,???§¹???????0

    //private bool mFileInited= false;//???????????,¡¤??,???????????
    private int mFileDescriptor;
    private static ValueType DefVal;

   // [DllImport("msvcrt.dll",CharSet = CharSet.Unicode)]
    extern static int _wcreat(string filename, int pmode);

  //  [DllImport("msvcrt.dll", CharSet = CharSet.Unicode)]
    extern static int _wopen(string filename, int oflag, int pmode);

  //  [DllImport("msvcrt.dll")]
    extern static int _read(int fd, [Out]byte[] buffer, uint count);

   // [DllImport("msvcrt.dll")]
    extern static int _write(int fd, byte[] buffer, uint count);

  //  [DllImport("msvcrt.dll")]
    extern static int _commit(int fd);

  //  [DllImport("msvcrt.dll")]
    extern static int _lseek(int fd, int offset, int origin);

//    [DllImport("msvcrt.dll")]
    extern static int _close(int fd);
    private void TryOpen()
    {
        if (mFileDescriptor == 0)
        {
            mFileDescriptor = _wopen(mFileFullPath, 0x0002 | 0x8000, 0x0080 | 0x0100);//binary,read|write
            _lseek(mFileDescriptor, 0, 2);
        }
    }

    public HotFileBinaryLowIO(string key,string filePath)
    {
        


        if (!Directory.Exists(filePath))
        {
            Directory.CreateDirectory(filePath);
        }

        mFileFullPath = filePath + key;

        if (!File.Exists(mFileFullPath))
        {
            mFileDescriptor = _wcreat(mFileFullPath, 0x0080 | 0x0100);//read|write
            _close(mFileDescriptor);
            mFileDescriptor = 0;
        }


        
    }
    ~HotFileBinaryLowIO()
    {
        Close();
    }

    public void Close()
    {
        _close(mFileDescriptor);
        mFileDescriptor = 0;
    }

    /// <summary>
    /// ??????(??????????)
    /// </summary>
    public void Clearup()
    {
        //???????
        //ValueType tmpVal = Read();

        mFileDescriptor = _wopen(mFileFullPath, 0x0200 | 0x0001, 0x0080 | 0x0100);//trunc|writeOnly,read|write
        _close(mFileDescriptor);
        mFileDescriptor = 0;

        //FlushWrite(tmpVal);
    }

    
    public ValueType Read()
    {
        TryOpen();
        //long endPos = mFS.Seek(0, SeekOrigin.End);//?????,????
        int endPos = _lseek(mFileDescriptor, 0, 2);
        if (endPos == 0)
        {
            Close();
            return DefVal;
        }
 
        int valueValidLen = DataBytesConvert(DefVal).Length + 1;//System.Runtime.InteropServices.Marshal.SizeOf(typeof(ValueType)) + 1;

        int curPos = endPos - endPos % valueValidLen;
        //mFS.Seek(curPos, SeekOrigin.Begin);
        _lseek(mFileDescriptor, curPos, 0);
         
        byte[] readBuff = new byte[valueValidLen];
        byte checkByteBuf = 0;
        while (true)
        {
            if (curPos < valueValidLen)//???????????????§¹?????,????
            {
                Close(); 
                return DefVal;
            }
            //curPos = mFS.Seek(-valueValidLen, SeekOrigin.Current);
            curPos = _lseek(mFileDescriptor, -valueValidLen, 1);

            //mFS.Read(readBuff, 0, valueValidLen);
            _read(mFileDescriptor, readBuff, (uint)valueValidLen);
            checkByteBuf = mCheckByte;
            for (int i = 0; i != valueValidLen - 1; ++i)
                checkByteBuf ^= readBuff[i];
            
            if(checkByteBuf == readBuff[valueValidLen - 1])
            {
                byte[] outValueBuf = new byte[valueValidLen - 1];
                System.Array.Copy(readBuff,outValueBuf,valueValidLen - 1);

                Close();
 
                return DataBytesConvert(outValueBuf);
            }
            //curPos = mFS.Seek(-valueValidLen, SeekOrigin.Current);
            curPos = _lseek(mFileDescriptor, -valueValidLen, 1);
        }


    }
 
    private ValueType DataBytesConvert(byte[] byteData)
    {
        if (typeof(ValueType) == typeof(int))
        {
            return (ValueType)(System.Object)System.BitConverter.ToInt32(byteData, 0);
        }
        else if (typeof(ValueType) == typeof(float))
        {
            return (ValueType)(System.Object)System.BitConverter.ToSingle(byteData, 0);
        }
        else if (typeof(ValueType) == typeof(bool))
        {
            return (ValueType)(System.Object)System.BitConverter.ToBoolean(byteData, 0);
        }
        //else if (typeof(ValueType).IsEnum)//???????
        //{
        //    int i = System.BitConverter.ToInt32(byteData, 0);
        //    return (ValueType)(System.Object)i;
        //}
        else if (typeof(ValueType) == typeof(long))
        {
            return (ValueType)(System.Object)System.BitConverter.ToInt64(byteData, 0);
        }

        else if (typeof(ValueType) == typeof(ushort))
        {
            return (ValueType)(System.Object)System.BitConverter.ToUInt16(byteData, 0);
        }
        else if (typeof(ValueType) == typeof(uint))
        {
            return (ValueType)(System.Object)System.BitConverter.ToUInt32(byteData, 0);
        }
 
        else if (typeof(ValueType) == typeof(char))
        {
            return (ValueType)(System.Object)System.BitConverter.ToChar(byteData, 0);
        }
        else if (typeof(ValueType) == typeof(double))
        {
            return (ValueType)(System.Object)System.BitConverter.ToDouble(byteData, 0);
        }

        else if (typeof(ValueType) == typeof(short))
        {
            return (ValueType)(System.Object)System.BitConverter.ToInt16(byteData, 0);
        }
        else if (typeof(ValueType) == typeof(ulong))
        {
            return (ValueType)(System.Object)System.BitConverter.ToUInt64(byteData, 0);
        }
        return (ValueType)(System.Object)0;
    }

    private byte[] DataBytesConvert(ValueType value)
    {

        if (typeof(ValueType) == typeof(int))
        {
            int toTranVal = (int)(System.Object)value;return System.BitConverter.GetBytes(toTranVal); 
        }
        else if (typeof(ValueType) == typeof(float))
        {
            float toTranVal = (float)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        else if (typeof(ValueType) == typeof(bool))
        {
            bool toTranVal = (bool)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        //else if (typeof(ValueType).IsEnum)//???????,?????int???????
        //{
        //    int toTranVal = (int)(System.Object)value;return System.BitConverter.GetBytes(toTranVal); 
        //}
        else if (typeof(ValueType) == typeof(long))
        {
            long toTranVal = (long)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        else if (typeof(ValueType) == typeof(ushort))
        {
            ushort toTranVal = (ushort)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        else if (typeof(ValueType) == typeof(uint))
        {
            uint toTranVal = (uint)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        else if (typeof(ValueType) == typeof(char))
        {
            char toTranVal = (char)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        else if (typeof(ValueType) == typeof(short))
        {
            short toTranVal = (short)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        else if (typeof(ValueType) == typeof(ulong))
        {
            ulong toTranVal = (ulong)(System.Object)value; return System.BitConverter.GetBytes(toTranVal);
        }
        


        return null;
    }
 
    /// <summary>
    /// ?§Õ
    /// </summary>
    /// <param name="value"></param>
    public void FlushWrite(ValueType value)
    {
        TryOpen();
        byte[] dataToWrite = DataBytesConvert(value);
        //mFS.Write(dataToWrite,0,dataToWrite.Length);
        _write(mFileDescriptor, dataToWrite, (uint)dataToWrite.Length);
        byte checkByte = mCheckByte;
        foreach (byte b in dataToWrite)
        {
            checkByte ^= b;
        }


        //mFS.WriteByte(checkByte);
        _write(mFileDescriptor,new byte[]{checkByte}, 1);
        //mFS.Flush(); 
        //_commit(mFileDescriptor);
        Close();
    }

}
 