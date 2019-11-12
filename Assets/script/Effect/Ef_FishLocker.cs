using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Ef_FishLocker : MonoBehaviour {
    //public Fish[] FishCards;
    [System.Serializable]
    public class FishRelateCard
    {
        public Fish Prefab;
        //public tk2dSprite Prefab_Card;
        public string SprName;
    }

    public FishRelateCard[] FishAndCard;
    public tk2dSprite Prefab_Card;
    private Dictionary<int, string> mFishTypeIdToCard;
    private tk2dSprite mSprCard;
    private Transform mTsCard;
    private Renderer mRdCard;
	// Use this for initialization
	void Awake () 
    {
        Player_FishLocker fishLocker = GetComponent<Player_FishLocker>();
        fishLocker.EvtTargetOnFish += Handler_TargetOnFish;
        fishLocker.EvtTargetLeaveFish += Handler_TargetLeavefish;

        mSprCard = Instantiate(Prefab_Card) as tk2dSprite;
        mTsCard = mSprCard.transform;
        mRdCard = mSprCard.GetComponent<Renderer>();


        mTsCard.parent = transform;
        mTsCard.localRotation = Quaternion.identity;

        mRdCard.enabled = false; 

        mFishTypeIdToCard = new Dictionary<int, string>();
        foreach (FishRelateCard fc in FishAndCard)
        {
            mFishTypeIdToCard.Add(fc.Prefab.TypeIndex, fc.SprName);

        }
	}

    void Handler_TargetOnFish(Fish f,Player p)
    {
        mRdCard.enabled = true;
        StopCoroutine("_Coro_FishCardViewing");
        StartCoroutine("_Coro_FishCardViewing",f);
    }

    void Handler_TargetLeavefish()
    {
        mRdCard.enabled = false;
        StopCoroutine("_Coro_FishCardViewing");
    }

    IEnumerator _Coro_FishCardViewing(Fish f)
    {
        //tk2dSprite prefabTargetFishCard = null;
        string nameSpr;
        if (mFishTypeIdToCard.TryGetValue(f.TypeIndex, out nameSpr))
            mSprCard.spriteId = mSprCard.GetSpriteIdByName(nameSpr);

        Vector3 localPos = new Vector3(98.5615488F, 55.5742848F, -19.67065344F);

        float rotateOffset = 4.42762752F;
        mTsCard.localScale = Vector3.one;
        float rotateSpd = 640F;
        float rotateAngle = 0F;
        
        while (true)
        {
            mTsCard.localPosition = localPos + new Vector3(rotateOffset * Mathf.Sin(rotateAngle * Mathf.Deg2Rad), rotateOffset * Mathf.Cos(rotateAngle * Mathf.Deg2Rad), 0F);

            rotateAngle += Time.deltaTime * rotateSpd;
        
            yield return 0;
        }
    }
}
