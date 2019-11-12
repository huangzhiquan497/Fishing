using UnityEngine;
using System.Collections;

/// <summary>
/// 枪锁定模块
/// </summary>
public class Module_GunLocker : MonoBehaviour {
    public float RotateRange = 180F;//角度

    public Module_Bullet ModuleBullet;//子弹模块
    Player_FishLocker[] mFishLocker;
    Fish[] mLockedTarget;
    GameMain mGM;
    
    void Start()
    {
        //Debug.Log("Awake");
        if (ModuleBullet == null)
            ModuleBullet = FindObjectOfType(typeof(Module_Bullet)) as Module_Bullet;
        mGM = GameMain.Singleton;
        GameMain.EvtInputKey += Handle_InputKey;
        GameMain.EvtPlayerGunChanged += Handle_PlayerGunChanged;
        GameMain.EvtPlayerGunFired += Handle_GunFire;
        HitProcessor.Evt_Hit += Handle_FishBulletHit;
        GameMain.EvtMainProcess_StartGame += Handle_StartGame;
        mFishLocker = new Player_FishLocker[Defines.MaxNumPlayer];
        mLockedTarget = new Fish[Defines.MaxNumPlayer];

       
    }
    void Handle_StartGame()
    {
        foreach (Player p in mGM.Players)
        {
            mFishLocker[p.Idx] = p.GetComponent<Player_FishLocker>();
            mFishLocker[p.Idx].EvtRelock += (Fish f, Player p2) => { LockAt(p2, f); };//重锁定指定鱼
            mFishLocker[p.Idx].EvtUnlock += Handle_FishLockerUnlock;
        }
    }
    void Handle_FishLockerUnlock(Player p)
    {
        UnLock(p);
    }

    void Handle_FishBulletHit(bool isMiss, Player p, Bullet b, Fish f)
    {
        if(isMiss)
        {
            if (b.IsLockingFish)
                b.IsLockingFish = false;
        }
    }

    void Handle_InputKey(int pIdx, HpyInputKey k, bool down)
    {
        Player p = mGM.Players[pIdx];
        if (down && k == HpyInputKey.Down)
        {
            if (mFishLocker[p.Idx] != null)
            {
                Fish f = mFishLocker[p.Idx].Lock();

                if (f != null)
                    LockAt(p,f);
            }

        }
        else if (!down && k == HpyInputKey.Down)
        {
            if (mFishLocker != null)
            {
                mFishLocker[p.Idx].UnLock();
                UnLock(p);
            }
        }
    }

    void Handle_PlayerGunChanged(Player p,Gun newGun)
    {
        if (IsPlayerLockingFish(p.Idx))
        {
            StartCoroutine(_Coro_Locking(p));
        }
    }

    void Handle_GunFire(Player p, Gun g, int score)
    {
        if (mLockedTarget[p.Idx] == null)
            return;

        //Debug.Log("Handle_GunFire fire fish = " + mLockedTarget[p.Idx].name+"  p.idx = "+p.Idx);
        Bullet prefabBullet = ModuleBullet.Get_PrefabBullet_Used(g);

        Bullet b = Pool_GameObj.GetObj(prefabBullet.gameObject).GetComponent<Bullet>();

        b.transform.position = g.local_GunFire.position;
        b.Score = score;
        Quaternion targetDir;
        Vector3 upToward = mLockedTarget[p.Idx].transform.position - g.TsGun.position;
        upToward.z = 0F;
        targetDir = Quaternion.FromToRotation(Vector3.up, upToward);
        b.Fire(p, mLockedTarget[p.Idx], targetDir);
        //Debug.Log("Handle_GunFire fire fish = " + mLockedTarget[p.Idx].name);
    }
    public void LockAt(Player p,Fish f)
    {
        //Debug.Log("LockAt " + f.name + "  p.idx = " + p.Idx);
        mLockedTarget[p.Idx] = f;

        p.GunInst.Rotatable = false;
        ModuleBullet.Fireable[p.Idx] = false;
        StartCoroutine(_Coro_Locking(p));
    }

    public void UnLock(Player p)
    {
        //Debug.Log("UnLock");
        mLockedTarget[p.Idx] = null;
        p.GunInst.Rotatable = true;
        ModuleBullet.Fireable[p.Idx] = true;
    }

     

    IEnumerator _Coro_Locking(Player p)
    {
        //Fish lockedTarget = mLockedTarget[p.Idx];
        
        //if (lockedTarget == null || !lockedTarget.Attackable)
        //    yield break;
        float rotateRrangeHalf = RotateRange * 0.5f;
       

        //

        Fish curTargetFish = mLockedTarget[p.Idx];
        Transform tsTarget = curTargetFish.transform;
        FishEx_LockPos loclLocal = curTargetFish.GetComponent<FishEx_LockPos>();
        //while (tsTarget != null && mLockedTarget[p.Idx] != null)//第一判断用于判断鱼是否游出,第二判断用于是否解锁
        while(true)
        {
            if (mLockedTarget[p.Idx] == null || tsTarget == null)
            {
                yield break;
            }

            if (curTargetFish.ID != mLockedTarget[p.Idx].ID)
            {
                curTargetFish = mLockedTarget[p.Idx];
                if (curTargetFish == null || !curTargetFish.Attackable)
                    yield break;
                loclLocal = curTargetFish.GetComponent<FishEx_LockPos>();
                tsTarget = curTargetFish.transform;
            }

           

            Transform gunTs = p.GunInst.TsGun;
            Vector3 upToward = (loclLocal != null ? loclLocal.LockBulletPos : tsTarget.position) - gunTs.position;
            upToward.z = 0F;

            //Quaternion preRotation = gunTs.localRotation;
            gunTs.rotation = Quaternion.Slerp(gunTs.rotation, Quaternion.FromToRotation(Vector3.up, upToward), 10F * Time.deltaTime);

            if (gunTs.localEulerAngles.z > rotateRrangeHalf && gunTs.localEulerAngles.z < (360F - rotateRrangeHalf))
            {
                if (gunTs.localEulerAngles.z < 180F)
                {
                    gunTs.RotateAroundLocal(Vector3.forward, -1.0F * (gunTs.localEulerAngles.z - rotateRrangeHalf) * Mathf.Deg2Rad);
                }
                else
                {
                    gunTs.RotateAroundLocal(Vector3.forward, (360F - rotateRrangeHalf - gunTs.localEulerAngles.z) * Mathf.Deg2Rad);
                }
            }

            yield return 0;
        }
    }

    public bool IsPlayerLockingFish(int pIdx)
    {
        return mLockedTarget[pIdx] != null;
    }
}
