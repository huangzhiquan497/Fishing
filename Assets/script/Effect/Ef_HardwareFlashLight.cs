using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ef_HardwareFlashLight : MonoBehaviour {
    private bool[] mFlashLightTag;
    INemoControlIO mArcIO;

    public void FlashLight(int ctrlIdx)
    {
        if(mFlashLightTag[ctrlIdx])
            return;
        mFlashLightTag[ctrlIdx] = true; 
        StartCoroutine(_Coro_FlashLighting(ctrlIdx));
    }

    IEnumerator _Coro_FlashLighting( int ctrlIdx)
    {
        int flashTime = 3;
        while (flashTime > 0)
        { 
            mArcIO.FlashLight(ctrlIdx, 0, false);
            yield return new WaitForSeconds(0.2F);
            mArcIO.FlashLight(ctrlIdx, 0, true);
            yield return new WaitForSeconds(0.2F);
            --flashTime;
        }
        mFlashLightTag[ctrlIdx] = false;

    }
    void Awake()
    { 
        mArcIO = GameMain.Singleton.ArcIO;
        GameMain.EvtFishKilled += Handle_FishKilled;
        mFlashLightTag = new bool[32];
    }

    IEnumerator Start()
    {
        yield return 0;
        for(int i = 0; i!= Defines.MaxNumPlayer ; ++i)
        {
            mArcIO.FlashLight(i, 0, true);
            mArcIO.FlashLight(i, 1, true);
            yield return new WaitForSeconds(0.1F);
        }
    }

    void Handle_FishKilled(Player killer,Bullet b,Fish f)
    { 
        FlashLight(killer.Idx);
    }
}
