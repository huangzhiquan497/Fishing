using UnityEngine;
using System.Collections;

public enum NemoCtrlIO_Key
{
    Up,
    Down,
    Left,
    Right,
    A, B, C, D,
    E, F, G, H,
    I, J, K, L//???

}
public delegate void NemoCtrlIO_EventKey(int ctrlIdx,NemoCtrlIO_Key k,bool downOrUp);
public delegate void NemoCtrlIO_EventGeneral();
public delegate void NemoCtrlIO_EventController(int ctrllerIdx);
public delegate void NemoCtrlIO_EventInsertCoin(uint count,int ctrllerIdx);
public delegate void NemoCtrlIO_EventCtrlBoardStateChanged(int controlBoard, bool state);
public delegate void NemoCtrlIO_EventResultReadWrite(bool IsWrite, uint address, byte datalen,byte resultCode, byte[] data);

//public delegate void KeyEvent(int playerIdx, NemoCtrlIO_Key k, bool downOrUp);  
//public delegate void GenericEvent();
//public delegate void EventController(int ctrllerIdx);


public delegate void NemoCtrlIO_EventHardwareInfo(int gameIdx,int mainVer,int subVer,bool verifySucess);

public interface INemoControlIO 
{
    NemoCtrlIO_EventHardwareInfo EvtHardwareInfo { get; set; }
    NemoCtrlIO_EventKey EvtKey { get; set; }
    NemoCtrlIO_EventInsertCoin EvtInsertCoin { get; set; }
    NemoCtrlIO_EventController EvtOutCoinReflect { get; set; }
    NemoCtrlIO_EventController EvtOutTicketReflect { get; set; }
    NemoCtrlIO_EventController EvtLackCoin { get; set; }
    NemoCtrlIO_EventController EvtLackTicket { get; set; }
    NemoCtrlIO_EventCtrlBoardStateChanged EvtCtrlBoardStateChanged{get; set;}

    NemoCtrlIO_EventGeneral EvtOpened { get; set; }
    NemoCtrlIO_EventGeneral EvtClosed { get; set; }
    NemoCtrlIO_EventResultReadWrite EvtResultReadWrite { get; set; }
 

    void OutCoin(uint num, int player);
    void OutTicket(uint num, int player);
    void FlashLight(int ctrlIdx, int lightidx, bool isOn);
    void RequestHardwareInfo();
    void RequestReadWrite(bool isWrite, uint address, byte length, byte[] data);
    bool Read_Block(uint address, byte dataLen, out byte[] outVal);
    bool Open();
    bool IsOpen();
    /// <summary>
    /// ?????????,??????????????
    /// </summary>
    void RecivePackages();
}

public class INemoControlIOSinglton
{
    public static INemoControlIO mSingleton;
    public static INemoControlIO Get()
    {
        if (mSingleton == null)
            mSingleton = UnityEngine.Object.FindObjectOfType(typeof(NemoUsbHid_HardScan)) as NemoUsbHid_HardScan;
        return mSingleton;
    }
}