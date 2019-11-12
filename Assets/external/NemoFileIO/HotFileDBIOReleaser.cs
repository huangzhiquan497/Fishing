using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class HotFileDBIOReleaser : MonoBehaviour {
    public static List<IReleasable> HotFileDBs;
    public static bool IsIntanceInScene = false;
    public static void HotFileDBReg(IReleasable r)
    {
        if (!IsIntanceInScene)
        {
            GameObject go = new GameObject("HotFileDBIOReleaser");
            go.AddComponent<HotFileDBIOReleaser>();
            IsIntanceInScene = true;
        }


        if (HotFileDBs == null)
            HotFileDBs = new List<IReleasable>();


        
        HotFileDBs.Add(r);
    }
	 

    void OnApplicationQuit()
    {
 
        foreach (IReleasable kv in HotFileDBs)
        {
            kv.Release();
        }
    }
}
