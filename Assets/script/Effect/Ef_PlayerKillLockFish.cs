using UnityEngine;
using System.Collections;

public class Ef_PlayerKillLockFish : MonoBehaviour {
    public GameObject Prefab_Effect;
	// Use this for initialization
	void Awake () {
        GameMain.EvtKillLockingFish += Handle_KillLockFish;   
	}
    void Handle_KillLockFish(Player p)
    {
        Vector3 worldPos = p.transform.position;
        worldPos.z = Defines.GlobleDepth_BombParticle;
        GameObject goEf = Instantiate(Prefab_Effect) as GameObject;
        goEf.transform.position = worldPos;

    }
}
