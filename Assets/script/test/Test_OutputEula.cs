using UnityEngine;
using System.Collections;

public class Test_OutputEula : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        if (GUILayout.Button("output elua"))
        {
            Debug.Log(transform.eulerAngles);
        }
    }
}
