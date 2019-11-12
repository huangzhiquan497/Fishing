using UnityEngine;
using System.Collections;

public class Test_InstancePerform : MonoBehaviour {
    public GameObject ObjToInst;
	// Use this for initialization
	void Start () {
        Debug.Log(ObjToInst.GetInstanceID());
        //while (true)
        //{
        //    GameObject go = Instantiate(ObjToInst) as GameObject;
        //    yield return 0;
        //    StartCoroutine(_DestroyAfter(go));
            
        //}
        
	}
    IEnumerator _DestroyAfter(GameObject go )
    {
        yield return new WaitForSeconds(1F);
        Destroy(go);

    }
	// Update is called once per frame
	void Update () {
	
	}
}
