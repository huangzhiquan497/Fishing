using UnityEngine;
using System.Collections;

public class Module_SceneSelect : MonoBehaviour {

    public GameObject Prefab_Selcet;
    public GameObject go_Selcet;
    public float R,G,B ;
    public static bool bSelect=false;
	// Use this for initialization
	void Start () 
    {
        Moudle_main.EvtSceneSelect += Handle_SceneSelect;
        R = G = B = 0.2f;
	}
    void Handle_SceneSelect()
    {
        go_Selcet = Instantiate(Prefab_Selcet) as GameObject;
        go_Selcet.transform.Find("北海渔场").GetComponent<tk2dUIItem>().OnClick += beihai_Select;
        if (Moudle_FileRead.GameLevel.Val < 10)
        {
            go_Selcet.transform.Find("北海道渔场").GetComponent<BoxCollider>().enabled = false;
            go_Selcet.transform.Find("北海道渔场/up").GetComponent<tk2dSprite>().color = new Color(R, G, B, 255);
            go_Selcet.transform.Find("北海道渔场/down").GetComponent<tk2dSprite>().color = new Color(R, G, B, 255);
        }
        else
        {
            go_Selcet.transform.Find("北海道渔场").GetComponent<tk2dUIItem>().OnClick += beihaidao_Select;
        }
        if (Moudle_FileRead.GameLevel.Val < 15)
        {
            go_Selcet.transform.Find("秘鲁渔场").GetComponent<BoxCollider>().enabled = false;
            go_Selcet.transform.Find("秘鲁渔场/up").GetComponent<tk2dSprite>().color = new Color(R, G, B, 255);
            go_Selcet.transform.Find("秘鲁渔场/down").GetComponent<tk2dSprite>().color = new Color(R, G, B, 255);
        }
        else
        {
            go_Selcet.transform.Find("秘鲁渔场").GetComponent<tk2dUIItem>().OnClick += bilu_Select;
        }
        if (Moudle_FileRead.GameFishSum.Val < 3000)
        {
            go_Selcet.transform.Find("纽芬兰渔场").GetComponent<BoxCollider>().enabled = false;
            go_Selcet.transform.Find("纽芬兰渔场/up").GetComponent<tk2dSprite>().color = new Color(R, G, B, 255);
            go_Selcet.transform.Find("纽芬兰渔场/down").GetComponent<tk2dSprite>().color = new Color(R, G, B, 255);
        }
        else
        {
            go_Selcet.transform.Find("纽芬兰渔场").GetComponent<tk2dUIItem>().OnClick += niufenlan_Select;
        }
    }
    void ActionGame(int Scene)
    {
        MobileInterface.StartGame(Scene);
        MobileInterface.FishGenerate_StartGen();
        if (Moudle_main.EvtGameStart != null)
            Moudle_main.EvtGameStart();
        go_Selcet.SetActive(false);

        if (Moudle_FileRead.FirstGiveMoney.Val == false)
        {
           // Debug.Log("第一次加钱100");
            MobileInterface.ChangePlayerScore(100);
           // Debug.Log(MobileInterface.GetPlayerScore());
            Moudle_FileRead.FirstGiveMoney.Val = true;
           // Debug.Log(Moudle_FileRead.FirstGiveMoney.Val);
        }
        bSelect = true;
        if (Moudle_main.EvtEveryDayReward != null && Moudle_scene.bEveryDayReward)
        {
            //if (Moudle_main.iState == 2)
            {
                Moudle_main.EvtEveryDayReward();
                Moudle_main.Singlton.go_Black.SetActive(true);
            }
            //bEveryDayReward = false;
        }
    }
    void beihai_Select()
    {
        ActionGame(0);
    }
    void beihaidao_Select()
    {
        ActionGame(1);
    }
    void bilu_Select()
    {
        ActionGame(2);
    }
    void niufenlan_Select()
    {
        ActionGame(3);
    }
	// Update is called once per frame
	void Update () 
    {
	
	}
}
