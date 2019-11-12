using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Pool_GameObj   {
    public static Dictionary<int, Pool_GameObj> msPoolsDict;
    private static Transform msTsPoolMain;//????????????GameObject

    private GameObject mPrefab;
    private Stack<GameObject> mPoolGo;
    private int mVolume = 10;//???????,????????????????????

    public Pool_GameObj(GameObject prefab)
    {
        mPrefab = prefab;
        if (mPoolGo == null)
            mPoolGo = new Stack<GameObject>();
    }
    public void GC_Lite()
    {
        if (mPoolGo.Count != 0)
            GameObject.Destroy(mPoolGo.Pop());

    }

    public GameObject GetGO()
    { 
        GameObject outGO ;
        if (mPoolGo.Count == 0)
        {
            outGO = GameObject.Instantiate(mPrefab) as GameObject;

            Component poolObj = outGO.GetComponent(type: $"IPoolObj");
            if (poolObj != null)
                ((IPoolObj)poolObj).Prefab = mPrefab;
        }
        else
        {
            outGO = mPoolGo.Pop();

            Component poolObj = outGO.GetComponent($"IPoolObj");
            if (poolObj != null)
                ((IPoolObj)poolObj).On_Reuse(mPrefab);

            
        }

        

        return outGO;
    }

    
    public void RecycleGO(GameObject go)
    {
        Component poolObj = go.GetComponent($"IPoolObj");
        if (poolObj != null)
            ((IPoolObj)poolObj).On_Recycle();

        if (msTsPoolMain == null)
        {
            msTsPoolMain = new GameObject("PoolObjectMain").transform;
            msTsPoolMain.gameObject.isStatic = true;
        }

        //?????????????????????
        if (mPoolGo.Count > mVolume)
        {
            GameObject.Destroy(go);
        }
        else
        {
            go.transform.parent = msTsPoolMain;
            mPoolGo.Push(go);
        }
    }

    /// <summary>
    /// ?????????????
    /// </summary>
    public static void Init()
    {
        Pool_GameObj_GC_Interval.StartGC();
    }

    public static GameObject GetObj(GameObject prefab)
    {
        if (msPoolsDict == null)
        {
            msPoolsDict = new Dictionary<int, Pool_GameObj>();
        }

        //????????PoolGameObject
        Pool_GameObj poolGo = null;
        if (!msPoolsDict.TryGetValue(prefab.GetInstanceID(),out poolGo))
        {
            poolGo = new Pool_GameObj(prefab);
            msPoolsDict.Add(prefab.GetInstanceID(), poolGo);
        }

        return poolGo.GetGO();
    }


    public static bool RecycleGO(GameObject prefab,GameObject instGO)
    {
        if (msPoolsDict == null)
        {
            msPoolsDict = new Dictionary<int, Pool_GameObj>();
        }

        //????????PoolGameObject
        if(prefab == null)
        {
            IPoolObj poolObj = instGO.GetComponent(typeof(IPoolObj)) as IPoolObj;
            prefab = poolObj.Prefab;
            if (prefab == null)
            {
                //Debug.LogWarning("noPrefab ="+instGO.name);
                return false;
            }
         }

        Pool_GameObj poolGo = null;
        if (!msPoolsDict.TryGetValue(prefab.GetInstanceID(), out poolGo))
        {
            poolGo = new Pool_GameObj(prefab);
            msPoolsDict.Add(prefab.GetInstanceID(), poolGo);
        } 
        poolGo.RecycleGO(instGO);
        return true;
    }

    //???ио?????
    public static void SetPoolVolume(GameObject prefab,int Volume)
    {
        if (msPoolsDict == null)
        {
            msPoolsDict = new Dictionary<int, Pool_GameObj>();
            return;
        }

        //????????PoolGameObject
        Pool_GameObj poolGo = null;
        if (!msPoolsDict.TryGetValue(prefab.GetInstanceID(), out poolGo))
        {
            poolGo = new Pool_GameObj(prefab);
            msPoolsDict.Add(prefab.GetInstanceID(), poolGo);
        }

        poolGo.mVolume = Volume;
    }
}
