using UnityEngine;
using System.Collections;

public class Ef_DestroyDelay : MonoBehaviour {
    public float delay = 1F;
	// Use this for initialization
	IEnumerator Start () {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
	}

}
