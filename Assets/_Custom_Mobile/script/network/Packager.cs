﻿using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Linq;
namespace wazServer
{
    using PackageID = UInt16;//修改时注意:在读包时修改读取的大小(不得而为之,为效率,牺牲代码兼容性)
	using PackageOffsetToEnd = UInt16;//修改时注意:在读包时修改读取的大小(不得而为之,为效率,牺牲代码兼容性)

    [System.AttributeUsage(System.AttributeTargets.Class | System.AttributeTargets.Struct)]
    public class Packable : System.Attribute
    {
//        private string name;
        public PackageID ID;
        public Packable(PackageID id)
        {
            ID = id;
        }
    }

    [System.AttributeUsage(System.AttributeTargets.Struct)]
    public class WazSerializableAttribute : System.Attribute
    {
    }

    public class WazSerializerHelper
    {
        public static object Read(BinaryReader br,Type t)
        { 
            if (t == typeof(int))
            {
                return br.ReadInt32();
            }
            else if (t == typeof(byte))
            {
                return br.ReadByte();
            }
            else if (t == typeof(string))
            {
                return br.ReadString();
            }
            else if (t == typeof(uint))
            {
                return br.ReadUInt32();
            }
            else if (t == typeof(ushort))
            {
                return br.ReadUInt16();
            }
            else if (t == typeof(float))
            {
                return br.ReadSingle();
            }
            else if (t == typeof(bool))
            {
                return br.ReadBoolean();
            }
            else if (t == typeof(long))
            {
                return br.ReadInt64();
            }
            else if (t == typeof(sbyte))
            {
                return br.ReadSByte();
            }
            else if (t == typeof(short))
            {
                return br.ReadInt16();
            }
            else if (t == typeof(char))
            {
                return br.ReadChar();
            }
            else if (t == typeof(ulong))
            {
                return br.ReadUInt64();
            }
            else if (t == typeof(double))
            {
                return br.ReadDouble();
            }
            else if (t == typeof(decimal))
            {
                return br.ReadDecimal();
            }
            else if (t == typeof(Array))
            {
                Array ary = (Array)Activator.CreateInstance(t);
                Type eType = t.GetElementType();
                ushort size = br.ReadUInt16();
                for (int i = 0; i != size; ++i)
                {
                    ary.SetValue(Read(br, eType), i);
                }
                return ary;
            }
            else if (typeof(IList).IsAssignableFrom(t))
            {
                
                IList l = (IList)Activator.CreateInstance(t);
                Type eType = l.AsQueryable().ElementType;

                ushort size = br.ReadUInt16();

                for (int i = 0; i != size; ++i)
                {
                    object item = Read(br, eType);
                    if (item != null)
                    {
                        l.Add(item);
                    }
                    else
                    {
                        throw new Exception("[erro]反串行化出错,结构体中出现不可串行化类(或结构体).");
                        //Debug.WriteLine("[erro]反串行化出错,结构体中出现不可串行化类(或结构体).");
                        return null;
                    }
                    
                }
                return l;
            
            }
            else if (Attribute.IsDefined(t, typeof(WazSerializableAttribute)))
            {
                FieldInfo[] members = t.GetFields();
                
                //生成这个结构
                object inst = Activator.CreateInstance(t,true);
                //t.GetConstructor
                foreach (FieldInfo fi in members)
                {
                    fi.SetValue(inst, Read(br, fi.FieldType));//读取出对应的属性后赋值
                }
                return inst;
            } 
            else 
            {
                throw new Exception("[erro]接收数据包时发现未识别类型.可能出现错误包");
                //Debug.WriteLine ("[erro]接收数据包时发现未识别类型.可能出现错误包");
//                break;
            }
            return null;
        }

        public static void Write(BinaryWriter bw,object o)
        {
            Type t = o.GetType();
            if (t == typeof(int))
            {
                bw.Write((int)o);
            }
            else if (t == typeof(byte))
            {
                bw.Write((byte)o);
            }
            else if (t == typeof(string))
            {
                bw.Write((string)o);
            }
            else if (t == typeof(uint))
            {
                bw.Write((uint)o);
            }
            else if (t == typeof(ushort))
            {
                bw.Write((ushort)o);
            }
            else if (t == typeof(float))
            {
                bw.Write((float)o);
            }
            else if (t == typeof(bool))
            {
                bw.Write((bool)o);
            }
            else if (t == typeof(long))
            {
                bw.Write((long)o);
            }
            else if (t == typeof(sbyte))
            {
                bw.Write((sbyte)o);
            }
            else if (t == typeof(short))
            {
                bw.Write((short)o);
            }
            else if (t == typeof(char))
            {
                bw.Write((char)o);
            }
            else if (t == typeof(ulong))
            {
                bw.Write((ulong)o);
            }
            else if (t == typeof(double))
            {
                bw.Write((double)o);
            }
            else if (t == typeof(decimal))
            {
                bw.Write((decimal)o);
            }
            else if (t == typeof(Array))
            {
                Array ary = (Array)o;
                Write(bw, (ushort)ary.Length);
                foreach (object it in ary)
                {
                    Write(bw, it);
                }
            }
            else if (typeof(IList).IsAssignableFrom(t))
            {
                IList l = (IList)o;
                Write(bw, (ushort)l.Count);
                foreach (object it in l)
                {
                    Write(bw, it);
                }
            }
            else if(Attribute.IsDefined(t, typeof(WazSerializableAttribute)))
            {
                FieldInfo[] members = t.GetFields();
                foreach (FieldInfo fi in members)
                {
                    Write(bw, fi.GetValue(o));
                }
            } 
            else 
            {
                throw new Exception("[erro]写入包时发现未识别类型.");
                //Debug.WriteLine ("[erro]写入包时发现未识别类型.");
            }
        }
    }

    public class Packager   
    {
        public System.Action<WazSession, object> EvtRecevicePack;
        Dictionary<PackageID, Type> mPackageMap = new Dictionary<PackageID, Type>();
        Dictionary<Int64, Dictionary<Type, Action<object>>> mPackDispatchMap = new Dictionary<Int64, Dictionary<Type, Action<object>>>();
        Dictionary<Type, Action<WazSession, object>> mPackDispathToAllSessionMap = new Dictionary<Type, Action<WazSession, object>>();

        WazNetwork mNetwork;
        MemoryStream mMemSendStream = new MemoryStream(1024);
        //        MemoryStream mMemRecevieStream = new MemoryStream(1024);
        BinaryWriter mBinWriter;
        //        BinaryReader mBinReader ;
        byte mPackHead = 0xff;
        byte mPackEnd = 0xfe;
        class PackRecHelper
        {
            public MemoryStream Stream = new MemoryStream(1024);
            public BinaryReader Reader;
            //			public object PackReceving = null;
            //			public int PreReadFieldOffset;//上一次读取成员的偏移值
            public long PreReadStreamOffset;

            public PackRecHelper()
            {
                Reader = new BinaryReader(Stream);
            }
        }
        Dictionary<Int64, PackRecHelper> mSessionRecHelpers = new Dictionary<long, PackRecHelper>();


        public Packager(WazNetwork network)
        {
            mNetwork = network;
            mNetwork.EvtRecive += Handle_RecivePackage;
            mNetwork.EvtSessionAccept += Handle_SessionAccept;
            mNetwork.EvtSessionClose += Handle_SessionClose;
            mBinWriter = new BinaryWriter(mMemSendStream);

        }
        void Handle_SessionAccept(WazSession s)
        {
            mSessionRecHelpers.Add(s.Handle, new PackRecHelper());
        }
        void Handle_SessionClose(WazSession s)
        {
            mSessionRecHelpers.Remove(s.Handle);
        }
        void Handle_RecivePackage(WazSession s, byte[] data, int size)
        {
            PackRecHelper recHelper;
            if (!mSessionRecHelpers.TryGetValue(s.Handle, out recHelper))
                return;

            //将data 放入读取stream末尾
            recHelper.Stream.Write(data, 0, size);
            recHelper.Stream.Seek(recHelper.PreReadStreamOffset, SeekOrigin.Begin);
            bool havePackageCut = false;
            //开始读取.
            while (recHelper.Stream.Position < recHelper.Stream.Length - 1)
            {
                long posStartRead = recHelper.Stream.Position;
                object objPack;
                Type typePack;
                try
                {
                    //读包头
                    byte head = recHelper.Reader.ReadByte();
                    if (head != mPackHead)
                        continue;
                    //读长度(实际为到包尾offset)
                    PackageOffsetToEnd offsetToEnd = recHelper.Reader.ReadUInt16();

                    //判断超出当前流(一个包在两段流中)
                    if (recHelper.Stream.Position + offsetToEnd + 1 > recHelper.Stream.Length)
                    {
                        recHelper.PreReadStreamOffset = posStartRead;
                        havePackageCut = true;
                        break;
                    }

                    //判断包尾是否正确
                    long posTmp = recHelper.Stream.Position;
                    recHelper.Stream.Seek(offsetToEnd, SeekOrigin.Current);
                    byte end = recHelper.Reader.ReadByte();
                    recHelper.Stream.Position = posTmp;//还原pos到读位置之后
                    if (end != mPackEnd)
                        continue;

                    //读命令id
                    PackageID pID = recHelper.Reader.ReadUInt16();

                    if (!mPackageMap.TryGetValue(pID, out typePack))
                    {
                        Debug.WriteLine("[erro]解析包时出现未注册包ID");
                        continue;
                    }

                    //正式开始数据
                    objPack = Activator.CreateInstance(typePack);
                    FieldInfo[] packFields = typePack.GetFields();
                    foreach (FieldInfo f in packFields)
                    {
                        f.SetValue(objPack, WazSerializerHelper.Read(recHelper.Reader, f.FieldType));
                    }

                    //跳过包尾
                    recHelper.Stream.Seek(1, SeekOrigin.Current);
                }
                catch (Exception e)
                {
                    //					recHelper.PreReadStreamOffset = posStartRead;
                    //					havePackageCut = true;
                    Debug.WriteLine("[erro]接收数据包异常.可能出现错误包 \r\n" + e.ToString());
                    break;
                }

                if (objPack != null)
                {

                    if (EvtRecevicePack != null)
                        EvtRecevicePack(s, objPack);

                    //触发pack绑定处理函数
                    Action<WazSession, object> packBindHanlders;
                    if (mPackDispathToAllSessionMap.TryGetValue(typePack, out packBindHanlders))
                    {
                        packBindHanlders(s, objPack);
                    }

                    //触发session,pack绑定处理函数
                    Dictionary<Type, Action<object>> typePackMap;
                    if (mPackDispatchMap.TryGetValue(s.Handle, out typePackMap))
                    {
                        Action<object> handler;
                        if (typePackMap.TryGetValue(typePack, out handler))
                        {
                            handler(objPack);
                        }
                    }


                }
            }

            //证明完成读包,重置缓冲区
            if (!havePackageCut)
            {
                recHelper.PreReadStreamOffset = 0;
                recHelper.Stream.Position = 0;
                recHelper.Stream.SetLength(0);
            }
        }

        public bool Send(WazSession s, object data)
        {

            Packable packAttr = (Packable)Attribute.GetCustomAttribute(data.GetType(), typeof(Packable));
            if (packAttr == null)
                return false;

            int sizePackOffsetLen = sizeof(PackageOffsetToEnd);
            try
            {
                //序列化
                FieldInfo[] members = data.GetType().GetFields();
                mMemSendStream.Position = 0;
                mMemSendStream.SetLength(0);
                mBinWriter.Write(mPackHead);

                long posPackLen = mMemSendStream.Position;
                //留空位给等下回来写包长度
                mBinWriter.Seek(sizePackOffsetLen, SeekOrigin.Current);
                mBinWriter.Write(packAttr.ID);
                foreach (FieldInfo f in members)
                {
                    object o = f.GetValue(data);

                    WazSerializerHelper.Write(mBinWriter, o);

                }

                PackageOffsetToEnd offsetToEnd = (PackageOffsetToEnd)(mMemSendStream.Position - posPackLen - sizePackOffsetLen);

                mBinWriter.Write(mPackEnd);

                mBinWriter.Seek((int)posPackLen, SeekOrigin.Begin);
                mBinWriter.Write(offsetToEnd);

                mNetwork.Send(s, mMemSendStream.ToArray());
                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool RegPack(PackageID id, Type t)
        {
            //查看是否存在于
            if (mPackageMap.ContainsKey(id))
            {
                Debug.WriteLine("[erro]重复注册packid.");
                return false;
            }

            mPackageMap.Add(id, t);

            return true;
        }

        public bool RegPack(PackageID id)
        {
            return RegPack(id, null);
        }
        public bool RegPack(Type t)
        {
            Packable pid = (Packable)Attribute.GetCustomAttribute(t, typeof(Packable));
            if (pid == null)
            {
                Debug.WriteLine("[erro]待注册的类型不含Packable Attribute.");
                return false;
            }

            return RegPack(pid.ID, t);
        }

        /// <summary>
        /// 注册包触发的处理函数
        /// </summary>
        /// <param name="packType">Pack type.</param>
        /// <param name="handler">Handler.</param>
        public void RegEvtHandler(Type packType, Action<WazSession, object> handler)
        {
            Action<WazSession, object> tmpInvoker;
            if (!mPackDispathToAllSessionMap.TryGetValue(packType, out tmpInvoker))
            {
                mPackDispathToAllSessionMap.Add(packType, tmpInvoker);

            }
            mPackDispathToAllSessionMap[packType] += handler;
        }

        public bool UnRegEvtHandler(Type packType, Action<WazSession, object> handler)
        {
            Action<WazSession, object> tmpInvoker;
            if (!mPackDispathToAllSessionMap.TryGetValue(packType, out tmpInvoker))
            {
                return false;
            }

            mPackDispathToAllSessionMap[packType] -= handler;
            return true;
        }

        /// <summary>
        /// 注册指定session,包触发的处理函数
        /// </summary>
        /// <param name="s">S.</param>
        /// <param name="packType">Pack type.</param>
        /// <param name="handler">Handler.</param>
        public void RegEvtHandler(WazSession s, Type packType, Action<object> handler)
        {
            Dictionary<Type, Action<object>> packTypeMap;
            if (!mPackDispatchMap.TryGetValue(s.Handle, out packTypeMap))
            {
                packTypeMap = new Dictionary<Type, Action<object>>();
                mPackDispatchMap.Add(s.Handle, packTypeMap);
            }

            Action<object> tmpHandler;
            if (!packTypeMap.TryGetValue(packType, out tmpHandler))
            {
                packTypeMap.Add(packType, tmpHandler);
            }

            packTypeMap[packType] += handler;

        }

        public bool UnRegEvtHandler(WazSession s, Type packType, Action<object> handler)
        {
            Dictionary<Type, Action<object>> packTypeMap;
            if (!mPackDispatchMap.TryGetValue(s.Handle, out packTypeMap))
            {
                return false;
            }

            Action<object> tmpHandler;
            if (!packTypeMap.TryGetValue(packType, out tmpHandler))
            {
                return false;
            }

            packTypeMap[packType] -= handler;
            return true;
        }
    }
}