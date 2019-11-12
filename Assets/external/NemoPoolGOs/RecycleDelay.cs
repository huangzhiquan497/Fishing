using UnityEngine;
using System.Collections;

public class RecycleDelay : MonoBehaviour {
    public float delay = 1F;
    public GameObject Prefab;

    // Use this for initialization
    IEnumerator Start()
    {
        if (Prefab == null)
        {
            Debug.LogError("Prefab ²»ÄÜÎª ¿Õ!!");
            yield break;
        }
        yield return new WaitForSeconds(delay);
        Pool_GameObj.RecycleGO(Prefab, gameObject);
        gameObject.SetActive(false);
        transform.position = new Vector3(1000F, 1000F, 0F);
        Destroy(this);
    }
}
