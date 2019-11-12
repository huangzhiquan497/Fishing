using UnityEngine;
using System.Collections;

public class Test_WriteDataToDisk : MonoBehaviour {

    public PersistentData<int, int> DataWriteToDisk;
    private float mWriteTimeInterval = 0.1F;
    int mReadedVal = 0;
    private int mStartFrom = 0;
	// Use this for initialization
    void Start() 
    {
        DataWriteToDisk = new PersistentData<int, int>("DataWriteToDisk");
        mReadedVal = DataWriteToDisk.Val;
    }
	IEnumerator _Coro_Write () { 
        DataWriteToDisk.Val = mStartFrom;
	    while ( true)
	    {
            DataWriteToDisk.Val += 1;
            yield return new WaitForSeconds(mWriteTimeInterval);
	    }
	    
	}

    void OnGUI()
    {
        //string intervalStr = "0.1";
        //float.TryParse(intervalStr,)
        GUILayout.Label("read Origin = " + mReadedVal.ToString());
        GUILayout.BeginHorizontal();
        GUILayout.Label("interval:");
        float interval = 0.1F;
        if(float.TryParse( GUILayout.TextField(mWriteTimeInterval.ToString()),out interval))
            mWriteTimeInterval =interval;
        else
            mWriteTimeInterval = 0.1F;
        GUILayout.EndHorizontal();
        GUILayout.TextArea("WriteData:" + DataWriteToDisk.Val);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Start From"))
        {
            StartCoroutine(_Coro_Write());
        }
        mStartFrom = int.Parse(GUILayout.TextField(mStartFrom.ToString()));
        GUILayout.EndHorizontal();
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
