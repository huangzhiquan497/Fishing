using UnityEngine;
using System.Collections;
using wazServer;
public class Module_Rank : MonoBehaviour
{
    public delegate void Event_RankShow();
    public static Event_RankShow EvtR;
    public GameObject Prefab_Rank;
    public static GameObject go_Rank;
    tk2dSprite Light;
    static tk2dUIItem ChangeName;
    // Use this for initialization
    void Start()
    {
        Moudle_main.EvtRank += Handle_GameRank;
    }
    void RankShow()
    {
        switch (Moudle_scene.GetRank())
        {
            case 1:
                go_Rank.transform.Find("scrollinglist/第一/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank()).ToString();
                go_Rank.transform.Find("scrollinglist/第二/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() + 1).ToString();
                go_Rank.transform.Find("scrollinglist/第三/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() + 2).ToString();

                go_Rank.transform.Find("scrollinglist/第一/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(0)).ToString();
                go_Rank.transform.Find("scrollinglist/第二/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(1)).ToString();
                go_Rank.transform.Find("scrollinglist/第三/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(2)).ToString();

                go_Rank.transform.Find("scrollinglist/第二/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(1));
                go_Rank.transform.Find("scrollinglist/第三/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(2));
                Light.transform.localPosition = new Vector3(-26, -31, -103);

                if (Moudle_FileRead.FirstChangeName.Val == false)
                {
                    setBtnActive(false);
                    go_Rank.transform.Find("scrollinglist/第一/玩家名").gameObject.SetActive(true);
                    go_Rank.transform.Find("scrollinglist/第一/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(0));
                }
                else
                {
                    setBtnActive(true);
                    go_Rank.transform.Find("scrollinglist/第一/玩家名").gameObject.SetActive(false);
                }
                break;
            case 2:
                go_Rank.transform.Find("scrollinglist/第一/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() - 1).ToString();
                go_Rank.transform.Find("scrollinglist//第二/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank()).ToString();
                go_Rank.transform.Find("scrollinglist/第三/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() + 1).ToString();
                go_Rank.transform.Find("scrollinglist/第四/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() + 2).ToString();

                go_Rank.transform.Find("scrollinglist/第一/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(0)).ToString();
                go_Rank.transform.Find("scrollinglist/第二/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(1)).ToString();
                go_Rank.transform.Find("scrollinglist/第三/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(2)).ToString();
                go_Rank.transform.Find("scrollinglist/第四/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(3)).ToString();

                go_Rank.transform.Find("scrollinglist/第一/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(0));
                go_Rank.transform.Find("scrollinglist/第三/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(2));
                go_Rank.transform.Find("scrollinglist/第四/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(3));
                Light.transform.localPosition = new Vector3(-26, -103, -103);
                if (Moudle_FileRead.FirstChangeName.Val == false)
                {
                    setBtnActive(false);
                    go_Rank.transform.Find("scrollinglist/第二/玩家名").gameObject.SetActive(true);
                    go_Rank.transform.Find("scrollinglist/第二/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(1));
                }
                else
                {
                    setBtnActive(true);
                    go_Rank.transform.Find("scrollinglist/第二/玩家名").gameObject.SetActive(false);
                }
                break;
            default:
                go_Rank.transform.Find("scrollinglist/第一/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() - 2).ToString();
                go_Rank.transform.Find("scrollinglist/第二/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() - 1).ToString();
                go_Rank.transform.Find("scrollinglist/第三/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank()).ToString();
                go_Rank.transform.Find("scrollinglist/第四/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() + 1).ToString();
                go_Rank.transform.Find("scrollinglist/第五/排名").GetComponent<tk2dTextMesh>().text = (Moudle_scene.GetRank() + 2).ToString();

                go_Rank.transform.Find("scrollinglist/第一/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(0)).ToString();
                go_Rank.transform.Find("scrollinglist/第二/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(1)).ToString();
                go_Rank.transform.Find("scrollinglist/第三/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(2)).ToString();
                go_Rank.transform.Find("scrollinglist/第四/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(3)).ToString();
                go_Rank.transform.Find("scrollinglist/第五/分数").GetComponent<tk2dTextMesh>().text = (Moudle_scene.getRankData(4)).ToString();

                go_Rank.transform.Find("scrollinglist/第一/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(0));
                go_Rank.transform.Find("scrollinglist/第二/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(1));
                go_Rank.transform.Find("scrollinglist/第四/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(3));
                go_Rank.transform.Find("scrollinglist/第五/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(4));
                Light.transform.localPosition = new Vector3(-26, -167, -103);
                if (Moudle_FileRead.FirstChangeName.Val == false)
                {
                    go_Rank.transform.Find("scrollinglist/第三/玩家名").gameObject.SetActive(true);
                    go_Rank.transform.Find("scrollinglist/第三/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(2));
                    setBtnActive(false);
                }
                else
                {
                    setBtnActive(true);
                    go_Rank.transform.Find("scrollinglist/第三/玩家名").gameObject.SetActive(false);
                }
                break;
        }
    }
    public static void setBtnActive(bool state)  //true 显示 false 隐藏
    {
        ChangeName.gameObject.SetActive(state);
    }
    void Handle_GameRank()
    {
        if (go_Rank==null)
        {
            go_Rank = Instantiate(Prefab_Rank) as GameObject;

            EvtR += RankShow;
            Light = go_Rank.transform.Find("back2").GetComponent<tk2dSprite>();
            ChangeName = go_Rank.transform.Find("button/改名").GetComponent<tk2dUIItem>();
            ChangeName.OnClick += SettingName;
           // Debug.Log("订阅");
            Light.transform.localPosition = new Vector3(-26, -167, -103);
            go_Rank.transform.Find("button/fanhui").GetComponent<tk2dUIItem>().OnClick += Rank_BackClick;
        }
        else
        {
            go_Rank.SetActive(true);
        }

    }
    void SettingName()
    {
        go_Rank.transform.Find("改名字").gameObject.SetActive(true);
        go_Rank.transform.Find("改名字/确定").GetComponent<tk2dUIItem>().OnClick += Tijiao;
        go_Rank.transform.Find("改名字/fanhui").GetComponent<tk2dUIItem>().OnClick += back_Click;
        StartCoroutine("guangbiao");
    }
    void back_Click()
    {
        go_Rank.transform.Find("改名字").gameObject.SetActive(false);

        StopCoroutine("guangbiao");
    }
    CS_RequestChangeName reqChangeName;

    public static void SetRank()
    {
        switch (Moudle_scene.GetRank())
        {
            case 1:
                if (Moudle_FileRead.FirstChangeName.Val == false)
                {
                    setBtnActive(false);
                    go_Rank.transform.Find("scrollinglist/第一/玩家名").gameObject.SetActive(true);
                    go_Rank.transform.Find("scrollinglist/第一/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(0));
                }
                else
                {
                    setBtnActive(true);
                    go_Rank.transform.Find("scrollinglist/第一/玩家名").gameObject.SetActive(false);
                }
                break;
            case 2:
                if (Moudle_FileRead.FirstChangeName.Val == false)
                {
                    setBtnActive(false);
                    go_Rank.transform.Find("scrollinglist/第二/玩家名").gameObject.SetActive(true);
                    go_Rank.transform.Find("scrollinglist/第二/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(1));
                }
                else
                {
                    setBtnActive(true);
                    go_Rank.transform.Find("scrollinglist/第二/玩家名").gameObject.SetActive(false);
                }
                break;
            default:
                if (Moudle_FileRead.FirstChangeName.Val == false)
                {
                    go_Rank.transform.Find("scrollinglist/第三/玩家名").gameObject.SetActive(true);
                    go_Rank.transform.Find("scrollinglist/第三/玩家名").GetComponent<TextMesh>().text = (Moudle_scene.getRankName(2));
                    setBtnActive(false);
                }
                else
                {
                    setBtnActive(true);
                    go_Rank.transform.Find("scrollinglist/第三/玩家名").gameObject.SetActive(false);
                }
                break;
        }
    }
    void Tijiao()
    {
        switch (Moudle_main.iState)
        {
            case 0:
                if (Moudle_main.EvtBackStart != null)
                    Moudle_main.EvtBackStart();
                break;
        }
        reqChangeName.Name = NameChange.getName();
       // Debug.Log(NameChange.getName());
        if (reqChangeName.Name != null && reqChangeName.Name != "")
        {
           // Debug.Log("名字不为空");

            Moudle_scene.mP.Send(Moudle_scene.session, reqChangeName);
        }
        if (reqChangeName.Name == null || reqChangeName.Name == "")
        {
          //  Debug.Log("名字为空");
        }
        
        //go_Rank.transform.FindChild("改名字/确定").GetComponent<tk2dUIItem>().OnClick -= Tijiao;
    }
    void Rank_BackClick()
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
        go_Rank.SetActive(false);
        Moudle_main.Singlton.go_Black.SetActive(false);
    }

    // Update is called once per frame
    IEnumerator guangbiao()
    {
        while (true)
        {
            string str=null;
            if (NameChange.getName() != null && NameChange.getName().Length >= 6)
            {
                str = NameChange.getName().Substring(0, 6);
                go_Rank.transform.Find("改名字/玩家名").GetComponent<TextMesh>().text = str;
            }
            if (NameChange.getName() != null && NameChange.getName().Length < 6)
            {
                go_Rank.transform.Find("改名字/玩家名").GetComponent<TextMesh>().text = NameChange.getName().Substring(0, NameChange.getName().Length);
            }
            yield return new WaitForSeconds(1F);
            if (NameChange.getName() != null && NameChange.getName().Length >= 6)
            {
                str = NameChange.getName().Substring(0, 6);
                go_Rank.transform.Find("改名字/玩家名").GetComponent<TextMesh>().text = str+"|";
            }
            if (NameChange.getName() != null && NameChange.getName().Length < 6)
            {
                go_Rank.transform.Find("改名字/玩家名").GetComponent<TextMesh>().text = NameChange.getName().Substring(0, NameChange.getName().Length) + "|";
            }
            yield return new WaitForSeconds(1F);
        }
    }
    void Update()
    {

    }
}
