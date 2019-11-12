using UnityEngine;
using System.Collections;

public class Test_CullPerformence : MonoBehaviour {
    public GameObject prefab;
    public Transform TsTopLeft;

    public Transform TsRightBottom;
    public float ascept = 1.6f;
    public int NumToInstance = 1000;
    private Transform[] mObjs;
    private Vector3[] mObjOriPos;


	// Use this for initialization
	void Awake() {
        //for
        //TsTopLeft.localPosition
        mObjs = new Transform[NumToInstance];
        mObjOriPos = new Vector3[NumToInstance];
        int numCol = (int)Mathf.Sqrt((float)NumToInstance / ascept);
        int numRow = NumToInstance / numCol;
        float rowSpace = (TsRightBottom.localPosition.x - TsTopLeft.localPosition.x) / numRow;
        float colSpace = (TsTopLeft.localPosition.y - TsRightBottom.localPosition.y) / numCol;
        //Debug.Log(numRow);
        //Debug.Log(numCol);
        //bool isTrigger = true;
        for (int colIdx = 0; colIdx != numCol; ++colIdx)
        {
            for (int rowIdx = 0; rowIdx != numRow; ++rowIdx)
            {
                Transform ts = (Instantiate(prefab) as GameObject).transform;


                //Collider colli = ts.GetComponent<Collider>();
                //colli.isTrigger = isTrigger;
                //isTrigger = !isTrigger;


                ts.parent = transform;
                //ts.renderer.castShadows = false;
                //ts.renderer.receiveShadows = false;
                ts.localPosition = new Vector3(TsTopLeft.localPosition.x+rowSpace * rowIdx
                    , TsTopLeft.localPosition.y -colSpace * colIdx, 1F);

                mObjs[colIdx * numRow + rowIdx] = ts;
                mObjOriPos[colIdx * numRow + rowIdx] = ts.localPosition;
                //Debug.Log(colIdx * numRow + rowIdx);
            }
        }

	}
     

    void Update()
    {
        Vector3 tmp = new Vector3(0.005F * Mathf.Sin(Time.time * 5F), 0.005F * Mathf.Cos(Time.time * 5F), 0F);
        //Debug.Log(m)
        //foreach (Transform t in mObjs)
 
        for(int i = 0; i != mObjs.Length; ++i)
        {
            if (mObjs[i] != null)
            {
                mObjs[i].localPosition = mObjOriPos[i]+tmp;
            }
        }
    }
}
