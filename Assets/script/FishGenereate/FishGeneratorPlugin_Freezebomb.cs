using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FishGeneratorPlugin_Freezebomb : MonoBehaviour {
    private GameMain mGm;
	// Use this for initialization
	void Awake () {
        GameMain.EvtFreezeBombActive += Handle_FreezeBombActive;
        GameMain.EvtFreezeBombDeactive += Handle_FreezeBombDeactive;
        mGm = GameMain.Singleton;
	}

    void Handle_FreezeBombActive()
    {
        
        //??????
        if (GameMain.State_ == GameMain.State.Normal)
        {
            mGm.FishGenerator.StopFishGenerate();
        }
        //??????????

        foreach (Fish prefabFish in mGm.FishGenerator.Prefab_FishAll)
        {
            int typeIdx = prefabFish.TypeIndex;
            Dictionary<int, Fish> fishTypeDict = mGm.FishGenerator.FishTypeIndexMap[typeIdx];
            if (fishTypeDict == null)
                continue;

            foreach (KeyValuePair<int, Fish> kvp in fishTypeDict)
            {
                Fish fInScene = kvp.Value;
                if (fInScene != null && fInScene.Attackable)
                {
                    IFishAI aiFish = fInScene.GetComponent($"IFishAI") as IFishAI;

                    if (aiFish != null)
                    {
                        aiFish.Pause();
                    }

                    Swimmer swm = fInScene.GetComponent<Swimmer>();
                    if (swm != null)
                        swm.StopImm();
                }
            }

            foreach (KeyValuePair<int, Swimmer> kvpLeader in mGm.FishGenerator.LeadersAll)
            {
                kvpLeader.Value.StopImm();
            }
        }
    }
    void Handle_FreezeBombDeactive()
    {
        if (GameMain.State_ == GameMain.State.Normal)
        {
            mGm.FishGenerator.StartFishGenerate();
        }
        //???????
        
        //???????????
        foreach (Fish prefabFish in mGm.FishGenerator.Prefab_FishAll)
        {
            int typeIdx = prefabFish.TypeIndex;
            Dictionary<int, Fish> fishTypeDict = mGm.FishGenerator.FishTypeIndexMap[typeIdx];
            if (fishTypeDict == null)
                continue;
            foreach (KeyValuePair<int, Fish> kvp in fishTypeDict)
            {
                Fish fInScene = kvp.Value;
                if (fInScene != null && fInScene.Attackable)
                {
                    IFishAI aiFish = fInScene.GetComponent($"IFishAI") as IFishAI;

                    if (aiFish != null)
                        aiFish.Resume();

                    Swimmer swm = fInScene.GetComponent<Swimmer>();
                    if (swm != null)
                        swm.Go();
                }
            }

        }

        foreach (KeyValuePair<int, Swimmer> kvpLeader in mGm.FishGenerator.LeadersAll)
        {
            kvpLeader.Value.Go();
        }
    }
	 
}
