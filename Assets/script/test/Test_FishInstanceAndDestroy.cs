using UnityEngine;
using System.Collections;

public class Test_FishInstanceAndDestroy : MonoBehaviour {
    public GameObjectSet GoFishSet;
    public float IntervalGenerate = 0.1F;
    private int mCurrentGenIdx;
	// Use this for initialization

	void Start () {
        mts = transform;

	}
    private Transform mts;
    private float mElapse = 0F;
	// Update is called once per frame
	void Update () {
        if (mElapse > IntervalGenerate)
        {
            mElapse = 0F;

            GameObject g = Instantiate(GoFishSet.GameObjects[mCurrentGenIdx]) as GameObject;
            g.transform.position = mts.position;
            g.transform.rotation = mts.rotation;
            Fish f = g.GetComponent<Fish>();

            f.swimmer.Go();
            mCurrentGenIdx = (mCurrentGenIdx + 1) % GoFishSet.GameObjects.Length;
        }
        else
        {
            mElapse += Time.deltaTime;
        }
	}
}
