using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// dbFRAM更新者(当有加密板链接时,单DB_GUID和FRAM_GUID不一致时更新所有DB数据到FRAM)
/// </summary>
public class HotFileDBFRAM_Updater : MonoBehaviour {
    public interface IUpdatable
    {
        void UpdateToFRAM();
    }

    public static HotFileDBFRAM_Updater Singleton;
    public List<IUpdatable> Updatables;
    private INemoControlIO mMCU;
    
	// Use this for initialization
	public static void Reg (IUpdatable i) 
    {
        if (Singleton == null)
        {
            GameObject go = new GameObject("HotFileDBFRAM_Updater");
            Singleton = go.AddComponent<HotFileDBFRAM_Updater>();

            Singleton.Updatables = new List<IUpdatable>();
            Singleton.mMCU = INemoControlIOSinglton.Get();
            Singleton.mMCU.EvtOpened += Singleton.Handle_MCU_Connect;
        }

        Singleton.Updatables.Add(i);

	}
	
    public void Handle_MCU_Connect()
    {
        //从FRAM获得guid
        byte[] guidFRAMByte = null;
        if (mMCU.Read_Block(0, 16, out guidFRAMByte))//从FRAM读取guid成功
        {
            Debug.Log("open guid:" + NemoSerial.ByteArrayToString(StaticValueContainerDBFRAM.GetDBID().ToByteArray()));
            System.Guid FRAMEGUID = new System.Guid(guidFRAMByte);
            if (StaticValueContainerDBFRAM.GetDBID() != System.Guid.Empty
                && FRAMEGUID != StaticValueContainerDBFRAM.GetDBID())//FRAM和db的guid不一致
            {
                Debug.Log("open guid不一致:" + NemoSerial.ByteArrayToString(StaticValueContainerDBFRAM.GetDBID().ToByteArray()));
                mMCU.RequestReadWrite(true, 0, 16, StaticValueContainerDBFRAM.GetDBID().ToByteArray());//将dbid写入FRAM
                //更新所有IUpdatable
                foreach (IUpdatable i in Updatables)
                {
                    i.UpdateToFRAM();
                }
                
            }
        }

        //if(StaticValueContainerDBFRAM.GetDBID() 
    }
}
