using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Ef_SameTypeBombDie : MonoBehaviour {
    public GameObject Prefab_EffectLine;

    public float EffectWidthScale = 0.1F;
    public Vector3 ScaleToUnit;//缩放至单位大小
    public Vector3 PositionOffset;//
    //public Color EffectColor = Color.white;
    public int MaxInstanceEffectLine = 20;//最大效果数目
    private Fish mFish;
    private Transform mTs;
	// Use this for initialization
	void Awake () {
        mFish = GetComponent<Fish>();
        mFish.EvtFishKilled += Handle_FishKilled;
        mTs = transform;
        
	}

    public void Handle_FishKilled(Player killer, Bullet b, Fish sameTypeBomb)
    {

        //找出死亡同类鱼的
        int numTypeToBomb = sameTypeBomb.Prefab_SameTypeBombAffect.Length;
        List<Fish> fishNormalDie = new List<Fish>();
        for (int i = 0; i != numTypeToBomb; ++i)
        {
            Dictionary<int,Fish> allFishDict = GameMain.Singleton.FishGenerator.FishTypeIndexMap[sameTypeBomb.Prefab_SameTypeBombAffect[i].TypeIndex];
            if(allFishDict != null)
                foreach (Fish f in allFishDict.Values)
                {
                    fishNormalDie.Add(f);
                }
        }
        GameObject goSameTypeBombDie = new GameObject("goSameTypeBombDie");
        Transform goSameTypeBombDieTs = goSameTypeBombDie.transform;
        Ef_DestroyDelay efDestroyDelay = goSameTypeBombDie.AddComponent<Ef_DestroyDelay>();
        efDestroyDelay.delay = 2F;

        
        GameObject tmpGO;
        Transform tmpTS;
        Vector3 tmpScale;
        Vector3 tmpPos;
        int currInstanceLineNum = 0;
        foreach (Fish f in fishNormalDie)
        {

            tmpGO = Instantiate(Prefab_EffectLine) as GameObject;
            tmpTS = tmpGO.transform;
            tmpTS.parent = goSameTypeBombDieTs;

            Vector3 dir = f.transform.position - mTs.position;
            float dirLen = dir.magnitude;
 
            tmpScale.x = tmpTS.localScale.x * ScaleToUnit.x * EffectWidthScale;
            tmpScale.y = tmpTS.localScale.y * ScaleToUnit.y * dirLen;
            tmpScale.z = tmpTS.localScale.z * ScaleToUnit.z;

            
            tmpTS.rotation = Quaternion.LookRotation(Vector3.forward, dir);
            tmpTS.localScale = tmpScale;

            tmpPos = mTs.position + (tmpTS.rotation * Vector3.Scale(PositionOffset, tmpScale));
            tmpPos.z = Defines.GlobleDepth_BombParticle;
            tmpTS.position = tmpPos;

            ++currInstanceLineNum;
            if(currInstanceLineNum>MaxInstanceEffectLine)
                break;
        }
        
        
    }
	

}
