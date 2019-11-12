using UnityEngine;
using System.Collections;
using wazServer;
public class Moudle_MainStart : MonoBehaviour
{
    public GameObject Prefab_startUI;
    public GameObject go_start;

    public GameObject Prefab_Quit;
    public GameObject go_Quit;

    // Use this for initialization
    void Awake()
    {
        //Moudle_main.Singlton.go_Black.SetActive(false);
    }
    void Start()
    {
        go_start = Instantiate(Prefab_startUI) as GameObject;
        go_start.transform.Find("buttoms/startbutton").GetComponent<tk2dUIItem>().OnClick += start_Click;
        go_start.transform.Find("buttoms/function").GetComponent<tk2dUIItem>().OnClick += Help_Click;
        go_start.transform.Find("buttoms/chengjiu").GetComponent<tk2dUIItem>().OnClick += Achievement_Click;
        go_start.transform.Find("buttoms/paihangbang").GetComponent<tk2dUIItem>().OnClick += Rank_Click;

        Moudle_main.EvtBackStart += Handle_GameBackStart;
        if (Moudle_main.EvtBackStart!=null)
            Moudle_main.EvtBackStart();

        go_Quit = Instantiate(Prefab_Quit) as GameObject;
        go_Quit.SetActive(false);

        go_Quit.transform.Find("yes").GetComponent<tk2dUIItem>().OnClick += YES_Click;
        go_Quit.transform.Find("no").GetComponent<tk2dUIItem>().OnClick += NO_Click;
       // go_start.transform.localScale = new Vector3(0.1f, 1, 1);
    }
    void Handle_GameBackStart()
    {
        switch (Moudle_main.iState)
        {
            case 0:
                {
                    go_start.SetActive(true);
                }
                break;
        }
    }
    void Help_Click()
    {
        wiipay.EvtLog("Game_GameStart_BtnHelp");
        Moudle_main.Singlton.go_Black.SetActive(true);
        switch (Moudle_main.iState)
        {
            case 0:
                {
                    go_start.SetActive(false);
                }
                break;
        }
        if (Moudle_main.EvtHelp != null)
            Moudle_main.EvtHelp();
    }

    void Achievement_Click()
    {
        wiipay.EvtLog("Game_GameStart_BtnAchievement");
        Moudle_main.Singlton.go_Black.SetActive(true);
        switch (Moudle_main.iState)
        {
            case 0:
                {
                    go_start.SetActive(false);
                }
                break;
        }
        if (Moudle_main.EvtAchievement != null)
            Moudle_main.EvtAchievement();
    }
    void Rank_Click()
    {
        wiipay.EvtLog("Game_GameStart_BtnRank");
        Moudle_main.Singlton.go_Black.SetActive(true);
        switch (Moudle_main.iState)
        {
            case 0:
                {
                    go_start.SetActive(false);
                }
                break;
        }
        CS_CommitScore iscore;
        iscore.score = MobileInterface.GetPlayerScore();
        Moudle_scene.mP.Send(Moudle_scene.session, iscore);

        CS_RequestRank rank;
        Moudle_scene.mP.Send(Moudle_scene.session, rank);
        if (Moudle_main.EvtRank != null)
            Moudle_main.EvtRank();
    }

    void OnDisable()
    {

    }
    void OnEnable()
    {

    }
    void start_Click()
    {
        switch (Moudle_main.iState)
        {
            case 0:
                {
                    go_start.SetActive(false);
                    Moudle_main.iState = 1;
                }
                break;
        }
        if (Moudle_main.EvtSceneSelect != null)
            Moudle_main.EvtSceneSelect();
    }

    void YES_Click()
    {
        Application.Quit();
    }

    void NO_Click()
    {
        go_Quit.SetActive(false);
        if (Moudle_main.iState==0)
        {
            go_start.SetActive(true);
        }
        Moudle_main.Singlton.go_Black.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
           // Application.Quit();
            Moudle_main.Singlton.go_Black.SetActive(true);
            go_start.SetActive(false);
            go_Quit.SetActive(true);
        }
    }
}
