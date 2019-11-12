using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Test_PlayerAutoLockFish : MonoBehaviour {

    public Fish TargetFish;//如果是空,忽略该条件
    public Module_GunLocker ModuleGunLocker;
    //public bool IsTargetBomb = false;
    public bool IsTargetSameTypeBomb = false;
    

    public int OddsMin = 50;
    public int OddsMax = 100;
    private bool mIsLocking = false;
    private int mCurLockIdx = 0;
	// Use this for initialization
	void Start () {
        if(ModuleGunLocker == null)
            ModuleGunLocker = GameObject.FindObjectOfType(typeof(Module_GunLocker)) as Module_GunLocker;
        //GameMain.EvtFishInstance += Handle_FishInstance;

        StartCoroutine(_Coro_LockFishInterval());
        //GameMain.EvtFishClear += Handle_FishClear;
        
	}

 

    void Handle_FishInstance(Fish f)
    {
        if (mIsLocking)
            return;
        if (f == null || !f.Attackable)
            return;
        if (TargetFish == null || f.TypeIndex != TargetFish.TypeIndex)
            return;
        
 
        StopCoroutine("_Coro_AllPlayerLock");
        StartCoroutine("_Coro_AllPlayerLock",f);
    }

    IEnumerator _Coro_LockFishInterval()
    {
        while (true)
        {

            Fish f = FindLockableFish();
            if (mIsLocking)
            {
                //Debug.Log("isLocking");
                yield return new WaitForSeconds(1F);
                continue;
            }
            if (f == null || !f.Attackable)
            {
                //Debug.Log("f == null || !f.Attackable");
                yield return null;
                continue;
            }


            if (!IsTargetSameTypeBomb && TargetFish != null && f.TypeIndex != TargetFish.TypeIndex)
            {
                yield return null;
                continue;
                //goto Tag_NextFind;
            }
            

            StopCoroutine("_Coro_AllPlayerLock");
            StartCoroutine("_Coro_AllPlayerLock", f);
            yield return new WaitForSeconds(0.1F);
        }
    }
    Fish FindLockableFish()
    {
        int lockIdxStart = mCurLockIdx;
        Dictionary<int, Fish>[] fishMap = GameMain.Singleton.FishGenerator.FishTypeIndexMap;
        Fish lockFish = null;
        Fish[] fishLockable = Player_FishLocker.Prefab_FishLockabe;
        if (fishLockable == null)
            return null;
        do
        {
            
            Dictionary<int, Fish> tmpFishDict = fishMap[fishLockable[mCurLockIdx].TypeIndex];
            if (tmpFishDict != null
                && tmpFishDict.Count != 0)
            {
                foreach (KeyValuePair<int, Fish> kvp in tmpFishDict)
                {
                    //判断范围..todo
                    if (kvp.Value != null
                        && kvp.Value.Attackable
                        && GameMain.Singleton.Players[0].AtScreenArea.Contains(kvp.Value.transform.position)
                        )
                    {
                        lockFish = kvp.Value;

                        mCurLockIdx = (mCurLockIdx + 1) % fishLockable.Length;//让下次从不同的鱼开始找
                        break;
                    }
                }

                if (lockFish != null)
                    break;
            }

            mCurLockIdx = (mCurLockIdx + 1) % fishLockable.Length;
            if (lockIdxStart == mCurLockIdx)
                break;

        } while (true);

        return lockFish;
    }

    IEnumerator _Coro_AllPlayerLock(Fish f)
    {
        mIsLocking = true;
        yield return new WaitForSeconds(0.1F);
        //Debug.Log("_Coro_AllPlayerLock0");
        while (true)
        {

            if (f == null || !f.Attackable)
            {
                mIsLocking = false;
                yield break;
            }
 
            switch(f.HittableTypeS)
            {
                case "Normal":
                    {
                        if(f.Odds < OddsMin)
                        {
                            mIsLocking = false;
                            yield break;
                        }
                    }
                    break;
                case "SameTypeBomb":
                    {
                        if(!IsTargetSameTypeBomb)
                        {
                            mIsLocking = false;
                            yield break;
                        }
                    }
                    break;
                case "FreezeBomb":
                    {
                        mIsLocking = false;
                        yield break;
                    }
                    //break;
            }



            if (GameMain.Singleton.WorldDimension.Contains(f.transform.position))
            {
                break;
            }
            yield return 0;
        }
        //Debug.Log("_Coro_AllPlayerLock1");
        foreach (Player p in GameMain.Singleton.Players)
        {
            Player_FishLocker fl = p.GetComponent<Player_FishLocker>();
            fl.Lock(f);

            //p.GunInst.LockAt(f);
            ModuleGunLocker.LockAt(p, f);

            p.GunInst.StopFire();
            p.GunInst.StartFire();
        }

        while (true)
        {
            if (f==null||
                !f.Attackable ||
                !GameMain.Singleton.WorldDimension.Contains(f.transform.position))
            {
                foreach (Player p in GameMain.Singleton.Players)
                {
                    Player_FishLocker fl = p.GetComponent<Player_FishLocker>();
                    fl.UnLock();
                    //p.GunInst.UnLock();
                    ModuleGunLocker.UnLock(p);
                    p.GunInst.StopFire();
                }
                mIsLocking = false;
                yield break;
            }
            yield return 0;
        } 
    }


    void OnGUI()
    {
        GUILayout.BeginVertical("Box");
        string oddStr = OddsMin.ToString();
        GUILayout.BeginHorizontal();
        GUILayout.Label("大于倍率才瞄准");
        oddStr = GUILayout.TextArea(oddStr);
        GUILayout.EndHorizontal();
        int.TryParse(oddStr,out OddsMin);

        //IsTargetBomb = GUILayout.Toggle(IsTargetBomb, "瞄准局部/超级炸弹");
        IsTargetSameTypeBomb = GUILayout.Toggle(IsTargetSameTypeBomb, "瞄准同类炸弹");

        GUILayout.EndVertical();
    }
}
