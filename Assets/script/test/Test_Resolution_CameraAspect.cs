using UnityEngine;
using System.Collections;

public class Test_Resolution_CameraAspect : MonoBehaviour {

	// Use this for initialization
	void Start () {
	    GetComponent<Camera>().aspect = 16F/9F;
        Debug.Log("Screen.currentResolution.width = " + Screen.currentResolution.width);
        Debug.Log("Screen.currentResolution.height = "+Screen.currentResolution.height);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
