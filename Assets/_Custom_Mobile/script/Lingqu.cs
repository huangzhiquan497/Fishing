using UnityEngine;
using System.Collections;

public class Lingqu : MonoBehaviour {

    public FlyingCoinManager flycoin;
	// Use this for initialization
	void Start () {
	
	}
    void OnMouseDown()
    {
        Moudle_scene.isLing = true;
        StartCoroutine(EFF());
        if (Moudle_FileRead.First_ReceiveCoin.Val)
        {
            Moudle_FileRead.First_ReceiveCoin.Val = false;
            MobileInterface.ChangePlayerScore(5000);
        }
        else
        {
            MobileInterface.ChangePlayerScore(5000);
        }
        flycoin.FlyFrom(new Vector3(450, 325, 450), 100, 0);

        Moudle_scene.go_scene.transform.Find("ServerTime/0").gameObject.SetActive(true);
        Moudle_scene.go_scene.transform.Find("ServerTime/冒号").gameObject.SetActive(true);

        Moudle_scene.go_ServerHour.SetActive(true);
        Moudle_scene.go_ServerMin.SetActive(true);
        Moudle_FileRead.ServerHour.Val = 4;
        Moudle_FileRead.ServerMin.Val =0;
        Moudle_FileRead.ServerSec.Val = 0;

        if(GetComponent<Collider>() != null)
            GetComponent<Collider>().enabled = false;
    }
    IEnumerator EFF()
    {
        Moudle_scene.go_Lingqu.transform.Find("Sprite").gameObject.SetActive(false);
        yield return new WaitForSeconds(6F);
        Moudle_scene.go_Lingqu.transform.Find("FlyingCoin").gameObject.SetActive(false);
    }
	// Update is called once per frame
	void Update () {
	
	}
}
