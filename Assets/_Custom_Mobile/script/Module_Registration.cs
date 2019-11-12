using UnityEngine;
using System.Collections;

public class Module_Registration : MonoBehaviour {

    public GameObject Prefab_Registration;
    public GameObject go_Registration;

    public int[] sendgold={50,60,80,100,120,150,180};
	// Use this for initialization
	void Start () 
    {
        Moudle_main.EvtEveryDayReward += Handle_Registration;
	}
    void Handle_Registration()
    {
        if (go_Registration == null)
        {
            go_Registration = Instantiate(Prefab_Registration) as GameObject;
            go_Registration.transform.Find("button/queding").GetComponent<tk2dUIItem>().OnClick += queding_Click;
            //-365,-18  -245
           // Debug.Log(Moudle_FileRead.EveryRewardDays.Val);
            go_Registration.transform.Find("Sprite").GetComponent<tk2dSprite>().transform.localPosition = new Vector3(-365 + 121 * (Moudle_FileRead.EveryRewardDays.Val - 1), -18, -1);
        }
        else
        {
            go_Registration.SetActive(true);
        }
    }
    void queding_Click()
    {
        go_Registration.SetActive(false);
       // Debug.Log(Moudle_FileRead.EveryRewardDays.Val);
        MobileInterface.ChangePlayerScore(sendgold[Moudle_FileRead.EveryRewardDays.Val - 1]);
      //  Debug.Log(MobileInterface.GetPlayerScore());
        Moudle_main.Singlton.go_Black.SetActive(false);

        Moudle_scene.bEveryDayReward = false;
    }
	// Update is called once per frame
	void Update () 
    {
	
	}
}
