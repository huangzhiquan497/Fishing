using UnityEngine;
using System.Collections;

public class FishEx_DepthSet : MonoBehaviour
{

    public float DepthGlobal = -5F;
	// Use this for initialization
	void Start () {

        Fish f = GetComponent<Fish>();
        if(f != null)
        {
            Vector3 pos = f.transform.position;
            pos.z = DepthGlobal;
            f.transform.position = pos;
        }
        Destroy(this);
	}
	 
}
