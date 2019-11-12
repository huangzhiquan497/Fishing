using System;
using System.Net;
using System.Net.Sockets;
using System.Diagnostics;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace wazServer
{
	public struct WazSession
	{
		public WazSession(Int64 h,Socket s)
		{
			Handle = h;
			Sock = s;
			DataToSendQueue = new Queue<byte[]> ();
		}
		public Int64 Handle;
		public Socket Sock;

		public Queue<byte[]> DataToSendQueue;
	}


	public class WazNetwork
	{
		public Action<WazSession,byte[],int> EvtRecive;
		public Action<WazSession> EvtSessionAccept;
        public Action<WazSession> EvtSessionClose;

		public virtual void Send (WazSession session, byte[] data, bool copyData){}
		public virtual void Send (WazSession session, byte[] data){}
		public virtual void Close (){}
		public virtual void Update (){}
		public virtual int GetAliveSession (){return 0;}
		public virtual void CloseSession (WazSession s){}
	}
}

