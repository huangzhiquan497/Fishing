using UnityEngine;
using System.Collections;

public class FlyingCoinManager : MonoBehaviour {

    public FlyingCoin Prefab_Gold;
    public FlyingCoin Prefab_GoldBig;
    public float AppearOffsetX = 0.1F;
    public float AppearOffsetY = 0.01F;
    public float FlySpeed = 5F;
    public float AppearInterval = 0.05F;
    public AnimationCurve Curve_Scale;



    public void FlyFrom(Vector3 fishDieWorldPos,int oddsTotal,float delay)
    {
        int numFly = 1;
        FlyingCoinType typeCoin = FlyingCoinType.Sliver;
        if (oddsTotal < 10)
        {
            numFly = oddsTotal;
        }
        else
        { 
            numFly = oddsTotal / 5 + (oddsTotal % 5 != 0 ? 1 : 0);
            
            if (numFly > 11)//最大飞11颗
                numFly = 11;

            typeCoin = FlyingCoinType.Glod;
        }

        
        StartCoroutine(_Coro_FlytProcess(fishDieWorldPos, typeCoin, numFly, delay));

    }

    IEnumerator _Coro_FlytProcess(Vector3 fishDieWorldPos, FlyingCoinType t, int num, float delay)
    {
        yield return new WaitForSeconds(delay);

        //播放声音
        if (GameMain.Singleton.SoundMgr.snd_Coin != null&&t == FlyingCoinType.Sliver)
            GameMain.Singleton.SoundMgr.PlayOneShot(GameMain.Singleton.SoundMgr.snd_Coin);
        if(GameMain.Singleton.SoundMgr.snd_Gold != null && t == FlyingCoinType.Glod)
            GameMain.Singleton.SoundMgr.PlayOneShot( GameMain.Singleton.SoundMgr.snd_Gold );

        Vector3 fishDielocalPos = transform.InverseTransformPoint(fishDieWorldPos);
        fishDielocalPos.z = 0F;
        FlyingCoin useGoldPrefab = t==FlyingCoinType.Glod ? Prefab_GoldBig : Prefab_Gold;
  

        //生成币
        FlyingCoin[] golds = new FlyingCoin[num];
        Vector3 startLocalPos = fishDielocalPos - new Vector3(AppearOffsetX * (num - 1) * 0.5F, AppearOffsetY * (num - 1) * 0.5F, 0F);
        startLocalPos += startLocalPos.normalized * 38.4F;//*1.3F,为延长距离,使得金币更像是鱼中飞出
        float maxTime = 0F;
        for (int i = 0; i != num; ++i)
        {
            //golds[i] = Instantiate(useGoldPrefab) as FlyingCoin;
            golds[i] = Pool_GameObj.GetObj(useGoldPrefab.gameObject).GetComponent<FlyingCoin>();
            golds[i].FlySpeed = FlySpeed;
            Transform goldTs = golds[i].transform;
            goldTs.parent = transform;
            goldTs.localRotation = Quaternion.identity;
            goldTs.localPosition = startLocalPos + new Vector3(AppearOffsetX * i, AppearOffsetY * i, 0F);
            float flyNeedTime = golds[i].FlytoPosZero(Curve_Scale);
            if (flyNeedTime > maxTime)
                maxTime = flyNeedTime;
            
            yield return new WaitForSeconds(AppearInterval);
        }

        yield return new WaitForSeconds(maxTime - AppearInterval * num);

        for (int i = 0; i != num; ++i)
        {
            Pool_GameObj.RecycleGO(null, golds[i].gameObject);
            //Destroy(golds[i].gameObject);
        }

    }
    //void OnGUI()
    //{
    //    if (GUILayout.Button("Fly"))
    //    {
    //        FlyFrom(new Vector3(0F, 0F, 0),FlyingCoinType.Sliver, 10);
    //    }
    //}

}
