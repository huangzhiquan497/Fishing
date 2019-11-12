using UnityEngine;
using System.Collections;

public class FishDimenSetWhenEnterWorld : MonoBehaviour {

    private Swimmer mSwimmer;
	// Use this for initialization
	void Start () {
        mSwimmer = GetComponent<Swimmer>();
	}

    void Update()
    {
        if (mSwimmer.IsInWorld())//进入生成区域
        {
            mSwimmer.SetLiveDimension(Defines.ClearFishRadius);
            Destroy(this);
        }
    }
}
