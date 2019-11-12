using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ef_FishDieEffectAdditive : MonoBehaviour {

    [System.Serializable]
    public class EffectToFishSet
    {
        public Fish[] Prefab_FishAffect;//影响到的鱼
        public GameObject TriggerEffect;//触发的效果
    }
    public AnimationClip Ani_SceneBGShaker;
    //public GameObject Prefab_Par_FishBomb;
    //public GameObject Prefab_TuLongExplode;

    //public Fish[] Prefab_FishAffect;//影响到的鱼

    public EffectToFishSet[] EffectSet;
    private Dictionary<int, Object>[] mEffectSetCache;

    //private Dictionary<int, Object> mFishAffectCache;
    void Start()
    {

        GameMain.EvtFishKilled += Handle_FishKilled;

        //mFishAffectCache = new Dictionary<int, Object>();
        //foreach (Fish f in Prefab_FishAffect)
        //{
        //    mFishAffectCache.Add(f.TypeIndex,null);
        //}

        mEffectSetCache = new Dictionary<int, Object>[EffectSet.Length];
        for (int i = 0; i != mEffectSetCache.Length; ++i)
        {
            mEffectSetCache[i] = new Dictionary<int, Object>();
            foreach (Fish f in EffectSet[i].Prefab_FishAffect)
            {
                mEffectSetCache[i].Add(f.TypeIndex, null);
            }

            //mEffectSetCache[i].add
        }

    }

    void Handle_FishKilled(Player killer, Bullet b, Fish f)
    {
        //if (mFishAffectCache.ContainsKey(f.TypeIndex))
        //{
        //    StartCoroutine(_Coro_ShakeCurrentBG());
        //    Bomb(Prefab_Par_FishBomb, f.transform.position); 
        //}

        //foreach (Dictionary<int, Object> effectSet in mEffectSetCache)
        for(int i = 0; i != mEffectSetCache.Length; ++i)
        {
            if (mEffectSetCache[i].ContainsKey(f.TypeIndex))
            {
                StartCoroutine(_Coro_ShakeCurrentBG());
                Bomb(EffectSet[i].TriggerEffect, f.transform.position);
            }    
        }
    }
 

    public void ShakeBG()
    {
        StartCoroutine(_Coro_ShakeCurrentBG());
    }
    //public void ShakeCurrentBG(Vector3 worldPos)
    //{
    //    StartCoroutine(_Coro_ShakeCurrentBG(worldPos));
    //}
    IEnumerator _Coro_ShakeCurrentBG()
    {
        
        GameObject goBGShakeParent = new GameObject("_tempBGShake");
        goBGShakeParent.transform.position = new Vector3(0F, 0F, Defines.GlobleDepth_TempSceneShake);

        GameObject goBgShake = Instantiate(GameMain.Singleton.SceneBGMgr.CurrentBG) as GameObject;
        goBgShake.transform.parent = goBGShakeParent.transform;
 
        Animation aniBGShake = goBgShake.AddComponent<Animation>();
        
        aniBGShake.AddClip(Ani_SceneBGShaker, "shakeBG"); 
        aniBGShake.wrapMode = WrapMode.Once;
        aniBGShake.Play("shakeBG");

        yield return new WaitForSeconds(1.1F);
        Destroy(aniBGShake.GetClip("shakeBG"));//必须删除,否则内存泄露
        Destroy(goBGShakeParent); 
    }

    void Bomb(GameObject prefab,Vector3 worldPos)
    {
        GameObject goParFishBomb = Instantiate(prefab) as GameObject;
        worldPos.z = Defines.GlobleDepth_BombParticle;
        goParFishBomb.transform.position = worldPos;
        //yield return new WaitForSeconds(4.1F);
        //Destroy(goParFishBomb);
    }
}
