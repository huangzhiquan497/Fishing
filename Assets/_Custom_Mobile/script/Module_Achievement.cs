using UnityEngine;
using System.Collections;

public class Module_Achievement : MonoBehaviour 
{
	public enum Achieve
	{
		LV5=1,
		LV10,
		LV20,
		LV30,
		LV40,
		Miss10,
		Miss20,
		Hit10,
		Hit20,
		SharkF,
		Fish20,
		Lantern50,
		Shark100,
		EveryFish,
		Gold500,
		Gold1000,
		Gold10000,
		Gold100000,
		Gold1000000
	}
    public GameObject Prefab_Achievement;
    static public GameObject go_Achievement;
	
    private int createOnce = 1;
	// Use this for initialization
	void Start ()
    {
        Moudle_main.EvtAchievement += Handle_GameAchievement;
	}
    void Handle_GameAchievement()
    {
        if (createOnce == 1)
        {
            go_Achievement = Instantiate(Prefab_Achievement) as GameObject;
            createOnce = 0;
        }
        else
        {
            go_Achievement.SetActive(true);
        }
        go_Achievement.transform.Find("fanhui").GetComponent<tk2dUIItem>().OnClick += Back_Click;
		
		if(Moudle_FileRead.BLV5.Val)
			set_show(Achieve.LV5);
		if(Moudle_FileRead.BLV10.Val)
			set_show(Achieve.LV10);
		if(Moudle_FileRead.BLV20.Val)
			set_show(Achieve.LV20);
		if(Moudle_FileRead.BLV30.Val)
			set_show(Achieve.LV30);
		if(Moudle_FileRead.BLV40.Val)
			set_show(Achieve.LV40);
		if(Moudle_FileRead.NotLuck10.Val)
			set_show(Achieve.Miss10);
		if(Moudle_FileRead.NotLuck20.Val)
			set_show(Achieve.Miss20);
		if(Moudle_FileRead.Luck10.Val)
			set_show(Achieve.Hit10);
		if(Moudle_FileRead.Luck20.Val)
			set_show(Achieve.Hit20);
		if(Moudle_FileRead.GameFirstShark.Val)
			set_show(Achieve.SharkF);
		if(Moudle_FileRead.CatchFish20.Val)
			set_show(Achieve.Fish20);
		if(Moudle_FileRead.CatchLantern50.Val)
			set_show(Achieve.Lantern50);
		if(Moudle_FileRead.CatchShark100.Val)
			set_show(Achieve.Shark100);
		if(Moudle_FileRead.CatchEveryFish.Val)
			set_show(Achieve.EveryFish);
		if(Moudle_FileRead.Gold500.Val)
			set_show(Achieve.Gold500);
		if(Moudle_FileRead.Gold1000.Val)
			set_show(Achieve.Gold1000);
		if(Moudle_FileRead.Gold10000.Val)
			set_show(Achieve.Gold10000);
		if(Moudle_FileRead.Gold100000.Val)
			set_show(Achieve.Gold100000);
		if(Moudle_FileRead.Gold1000000.Val)
			set_show(Achieve.Gold1000000);
		//if(
		//go_Achievement.transform.FindChild("scrollinglist/ScrollableArea/获得成就/Cup1/cup(y)").gameObject.SetActive(true);
    }
	
	public static void set_show(Achieve index)
	{
		string s = "scrollinglist/ScrollableArea/获得成就/Cup"+(int)index+"/cup(y)";
	    go_Achievement.transform.Find(s).gameObject.SetActive(true);
	}
    void Back_Click()
    {
        switch (Moudle_main.iState)
        {
            case 0:
                {
                    if (Moudle_main.EvtBackStart != null)
                        Moudle_main.EvtBackStart();
                }
                break;
            case 1:
                {
                    
                }
                break;
        }
        Moudle_main.Singlton.go_Black.SetActive(false);
        go_Achievement.SetActive(false);
    }
	// Update is called once per frame
	void Update ()
    {
		//string s = "scrollinglist/ScrollableArea/获得成就/Cup"+1.ToString()+"/cup(y)";
		//Debug.Log(s);
	}
}
