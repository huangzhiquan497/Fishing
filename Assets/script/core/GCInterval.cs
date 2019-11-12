using UnityEngine;
using System.Collections;

public class GCInterval : MonoBehaviour {

	 
	// Update is called once per frame
	void Update () {
        if (Time.frameCount % 30 == 0)
        {
            System.GC.Collect();
        }
	}
}
