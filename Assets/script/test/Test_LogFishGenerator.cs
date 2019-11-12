using UnityEngine;
using System.Collections;
using System.Collections.Generic;
public class Test_LogFishGenerator : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

    void OnGUI()
    {
        if (GUILayout.Button("output fishGenerator data"))
        {
            FishGenerator fg = GameMain.Singleton.FishGenerator;
            int totalFishTypeIndexMap = 0;

            foreach (Dictionary<int, Fish> dict in fg.FishTypeIndexMap)
            {
                if(dict != null)
                    totalFishTypeIndexMap += dict.Count;
            }
            Debug.Log("totalFishTypeIndexMap =" + totalFishTypeIndexMap);
            Debug.Log("FishLockable = " + fg.FishLockable.Count);
            Debug.Log("LeadersAll = " + fg.LeadersAll.Count);
            Debug.Log("mUniqueFishToGenerate = " + fg.mUniqueFishToGenerate.Count);
            Debug.Log("SameTypeBombs = " + fg.SameTypeBombs.Count);

        }
    }
}
