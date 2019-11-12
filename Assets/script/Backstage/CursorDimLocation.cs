using UnityEngine;
using System.Collections;

public class CursorDimLocation : MonoBehaviour {
    public Vector3 Dimension = new Vector3(0.3F,0.1F,0F);//¿í¸ß
	// Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + new Vector3(Dimension.x * 0.5F, 0F, 0F), Dimension);
    }
}
