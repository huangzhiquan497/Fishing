using UnityEngine;
using System.Collections;

public class Test_DelegateInvokeTimes : MonoBehaviour {
    public delegate int TestDeleg();
    TestDeleg d;
    int GetI1()
    {
        return 3;
    }

    int GetI2()
    {
        return 4;
    }
	// Use this for initialization
	void Start () {
        d += GetI2;
        d += GetI1;
        
	}
	

	// Update is called once per frame
	void Update () {
        //System.Delegate[] ds = ()d.GetInvocationList();


	}

    void OnGUI()
    {
        if (GUILayout.Button("fire"))
        {
            Debug.Log(d().ToString());
        }
    }
}
