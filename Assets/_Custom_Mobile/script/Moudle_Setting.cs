using UnityEngine;
using System.Collections;

public class Moudle_Setting : MonoBehaviour 
{
    public GameObject Prefab_Setting;
    public GameObject go_Setting;

    private int createOnce = 1;
	// Use this for initialization
	void Start ()
    {
        Moudle_main.EvtSetting += Handle_Setting;
	}
    void Handle_Setting()
    {
        if (createOnce == 1)
        {
            go_Setting = Instantiate(Prefab_Setting) as GameObject;
            createOnce = 0;
        }
        else
        {
            go_Setting.SetActive(true);
        }
        go_Setting.transform.Find("button/fanhui").GetComponent<tk2dUIItem>().OnClick += Back_Click;
        //go_Setting.transform.FindChild("button/go").GetComponent<BoxCollider>()
    }
    void Back_Click()
    {
        go_Setting.SetActive(false);
        Moudle_main.Singlton.go_Black.SetActive(false);


        go_Setting.transform.Find("button/fanhui").GetComponent<tk2dUIItem>().OnClick -= Back_Click;
    } 
	// Update is called once per frame
	void Update ()
    {
	
	}
}
