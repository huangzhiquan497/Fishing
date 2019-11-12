using UnityEngine;
using System.Collections;

public class RandomFishOdds : MonoBehaviour {
    public Fish[] Prefab_RandomsFish;
	// Use this for initialization
	void Start () {
        if (Prefab_RandomsFish == null || Prefab_RandomsFish.Length == 0)
            return;

        Fish rndFish = Prefab_RandomsFish[Random.Range(0, Prefab_RandomsFish.Length)];
        if (rndFish == null)
        {
            Debug.LogError("在Prefab_RandomsFish中有鱼已经删除了~!检查下!");
            return;
        }
        Fish f = GetComponent<Fish>();
        f.Odds = rndFish.Odds;
        f.Prefab_GoAniDead = rndFish.Prefab_GoAniDead;
        //f.FlyCoinType = rndFish.FlyCoinType;
        //f.NumFlyCoin = rndFish.NumFlyCoin;
	}
}
