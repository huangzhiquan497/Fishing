using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
namespace wazServer
{
    public class WazClient : WazNetwork
    {
        Socket mSock;
        //byte[] mReciveBuff;
        System.IO.MemoryStream mReceiveStream;
        WazSession mSession;


        // ManualResetEvent instances signal completion.
        private ManualResetEvent mConnectDone = new ManualResetEvent(false);
        //private static ManualResetEvent mSendDone = new ManualResetEvent(false);
        //private static ManualResetEvent mReceiveDone = new ManualResetEvent(false);
        private Object mRecevieLock = new Object();
        //List<ArraySegment<int>> mReciveBuff;
        private bool mIsAccepConnect = false;

        bool mLoopThreadReceive = true;
        Thread mThreadRec;
        //MainThreadPasser<byte[]> mSendPasser = new MainThreadPasser<byte[]>();
        byte[] mWorkThreadReceviceBuff = new byte[1024];
        public WazClient()
        {
            mSock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            mSession = new WazSession(mSock.Handle.ToInt64(), mSock);
            //mReciveBuff = new byte[mSock.ReceiveBufferSize];
            mReceiveStream = new System.IO.MemoryStream();
            //mSendPasser.Evt_WorkThread_Received += Handle_WorkThread_Received;
            //mReciveBuffExchange = new byte[mSock.ReceiveBufferSize]; 
            //mReciveBuff = new List<ArraySegment<int>>();
            
            //mSock.Blocking = false;
           
        }

        public bool IsConnect()
        {
            bool part1 = mSock.Poll(1000, SelectMode.SelectRead);
            bool part2 = (mSock.Available == 0);
            if (part1 & part2)
                return false;
            else
                return true;
             
        }

        public WazSession Connect(string ipStr, int port)
        {

            //mSock.BeginConnect(new IPEndPoint(IPAddress.Parse(ipStr), port), new AsyncCallback(Handle_Connected_WorkThread), mSock);
            mSock.Connect(new IPEndPoint(IPAddress.Parse(ipStr), port));
            mSock.ReceiveTimeout = 1;
            
            if (mSock.Connected)
            {
                //UnityEngine.Debug.Log("mSock.RecBuffSize = " + mSock.ReceiveBufferSize);
                if (EvtSessionAccept != null)
                    EvtSessionAccept(mSession);

                mThreadRec = new Thread(Thread_Recevie);
                mThreadRec.Start();
                //mSock.BeginReceive(mReciveBuff, 0, mReciveBuff.Length, SocketFlags.None, new AsyncCallback(Handle_Recevie_WorkThread), mRecevieLock);
            }

            return mSession;
        }

        //void Handle_Recevie_WorkThread(IAsyncResult ar)
        //{
        //    int byteReceive = mSock.EndReceive(ar);

        //    Monitor.Enter(ar.AsyncState);
        //    mReceiveStream.Write(mReciveBuff, 0, byteReceive);
        //    Monitor.Exit(ar.AsyncState);
        //    mSock.BeginReceive(mReciveBuff, 0, mReciveBuff.Length, SocketFlags.None, new AsyncCallback(Handle_Recevie_WorkThread), mRecevieLock);

        //}

        void Thread_Recevie()
        {
            while (mLoopThreadReceive)
            {

                //mSendPasser.WorkThread_Update();

                int sizeRecevie = 0;
                try
                {
                    sizeRecevie = mSock.Receive(mWorkThreadReceviceBuff);
                }
                catch 
                {
                    //UnityEngine.Debug.Log("Thread_Recevie e = " + e);
                }

                if (sizeRecevie != 0)
                {
                    //UnityEngine.Debug.Log("sizeRecevie = " + sizeRecevie.ToString());

                    Monitor.Enter(mRecevieLock);
                    mReceiveStream.Write(mWorkThreadReceviceBuff, 0, sizeRecevie);
                    Monitor.Exit(mRecevieLock);
                }


                //Thread.Sleep(1);
            }

            //UnityEngine.Debug.Log("Thread_Recevie quit");
        }

        //void Handle_WorkThread_Received(byte[] sendData)
        //{
        //    try
        //    {
        //        mSock.Send(sendData);
        //    }
        //    catch(Exception e)
        //    {
        //        UnityEngine.Debug.Log("Handle_WorkThread_Received e = " + e);
        //    }
        //}

        void Handle_Connected_WorkThread(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                // Signal that the connection has been made.
                mConnectDone.Set();
                mIsAccepConnect = true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.Log(e.ToString());
            }
        }

        public override void Send(WazSession session, byte[] data, bool copyData)
        {
            byte[] dataToSend = data;
            if (copyData)
            {

                dataToSend = new byte[data.Length];
                Array.Copy(data, dataToSend, data.Length);
            }

            //mSendPasser.MainThread_Pass(dataToSend);
            try
            {

                mSock.Send(dataToSend);

                //mSock.BeginSend(dataToSend, 0, dataToSend.Length, SocketFlags.None,null, null);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

        }
        public override void Send(WazSession session, byte[] data)
        {
            Send(session, data, false);
        }
        public override void Close()
        {
            mLoopThreadReceive = false;
            //mThreadRec.Join();
            if (EvtSessionClose != null)
                EvtSessionClose(mSession);
            //if(mSock.Connected)
            //    mSock.Shutdown(SocketShutdown.Both);
            mSock.Close();
        }
        public override void Update()
        {
            if (Monitor.TryEnter(mRecevieLock))
            {
                byte[] receiveData = null;

                if (mReceiveStream.Length != 0)
                {
                    receiveData = mReceiveStream.ToArray();
                    mReceiveStream.Position = 0;
                    mReceiveStream.SetLength(0);
                }
                Monitor.Exit(mRecevieLock);


                if (receiveData != null)
                {
                    if (EvtRecive != null)
                    {
                        //UnityEngine.Debug.Log("receiveData.Length = " + receiveData.Length);
                        EvtRecive(mSession, receiveData, receiveData.Length);
                    }
                }
            }

            //if (mIsAccepConnect)
            //{
            //    if (EvtSessionAccept != null)
            //        EvtSessionAccept(mSession);
            //    mSock.BeginReceive(mReciveBuff, 0, mReciveBuff.Length, SocketFlags.None, new AsyncCallback(Handle_Recevie_WorkThread), mRecevieLock);

            //    mIsAccepConnect = false;
            //}
        }

        public override int GetAliveSession()
        {
            if (mSock.Connected)
                return 1;

            return 0;
        }
        public override void CloseSession(WazSession s)
        {
            //mSock.Shutdown(SocketShutdown.Both);
            mSock.Close();
        }
    }
}

