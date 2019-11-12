using UnityEngine;
using System.Collections;

public class shop : MonoBehaviour {

    public GameObject Prefab_Shop;
    public GameObject go_shop;
    int create = 1;
    float ScaleX = 0.8f;
    float ScaleY = 0.8f;

    public static int select = -1;  //玩家选择充值哪一项

    private float startTime = 0;
	// Use this for initialization
	void Start () 
    {
        Moudle_main.EvtShop += Handle_Shop;
	}
    void Handle_Shop()
    {
        if(create==1)
        {
            go_shop = Instantiate(Prefab_Shop) as GameObject;
            create = 0;
            go_shop.transform.Find("购买").GetComponent<tk2dUIItem>().OnClick += Buy_Click;
            go_shop.transform.Find("fanhui").GetComponent<tk2dUIItem>().OnClick += Quit_Click;

            go_shop.transform.Find("2元").GetComponent<tk2dUIItem>().OnClick += two_Click;
            go_shop.transform.Find("6元").GetComponent<tk2dUIItem>().OnClick += six_Click;
            go_shop.transform.Find("20元").GetComponent<tk2dUIItem>().OnClick += ershi_Click;
            go_shop.transform.Find("50元").GetComponent<tk2dUIItem>().OnClick += wushi_Click;
            go_shop.transform.Find("100元").GetComponent<tk2dUIItem>().OnClick += yibai_Click;
            go_shop.transform.Find("500元").GetComponent<tk2dUIItem>().OnClick += wubai_Click;
        }
        else
        {
            go_shop.SetActive(true);
        }

    }
    IEnumerator change(string son)
    {
        reset();

        Transform tr = go_shop.transform.Find(son).GetComponent<Transform>();

        while (startTime + 0.2f > Time.time)
        {
            tr.localScale = new Vector3(ScaleX, ScaleY, 0);
            ScaleX += 0.2f / (0.2f / Time.deltaTime);
            ScaleY += 0.2f / (0.2f / Time.deltaTime);
            yield return 0;
        }
        tr.localScale = new Vector3(1, 1, 0);
    }
    void reset()
    {

        ScaleX = ScaleY = 0.8f;
        go_shop.transform.Find("2元").GetComponent<Transform>().localScale = new Vector3(ScaleX, ScaleY, 0);
        go_shop.transform.Find("6元").GetComponent<Transform>().localScale = new Vector3(ScaleX, ScaleY, 0);
        go_shop.transform.Find("20元").GetComponent<Transform>().localScale = new Vector3(ScaleX, ScaleY, 0);
        go_shop.transform.Find("50元").GetComponent<Transform>().localScale = new Vector3(ScaleX, ScaleY, 0);
        go_shop.transform.Find("100元").GetComponent<Transform>().localScale = new Vector3(ScaleX, ScaleY, 0);
        go_shop.transform.Find("500元").GetComponent<Transform>().localScale = new Vector3(ScaleX, ScaleY, 0);
    }
    void two_Click()
    {
        startTime = Time.time;
        select = 0;
        StopCoroutine("change");

        StartCoroutine("change", "2元");
    }

    void six_Click()
    {
        startTime = Time.time;
        select = 1;
        StopCoroutine("change");

        StartCoroutine("change", "6元");
    }
    void ershi_Click()
    {
        startTime = Time.time;
        select = 2;
        StopCoroutine("change");

        StartCoroutine("change", "20元");
    }
    void wushi_Click()
    {
        startTime = Time.time;
        select = 3;
        StopCoroutine("change");

        StartCoroutine("change", "50元");
    }
    void yibai_Click()
    {
        startTime = Time.time;
        select = 4;
        StopCoroutine("change");

        StartCoroutine("change", "100元");
    }
    void wubai_Click()
    {
        startTime = Time.time;
        select = 5;
        StopCoroutine("change");

        StartCoroutine("change", "500元");
    }
    void Buy_Click()
    {
        reset();
        if (select != -1)
        {
            //Debug.Log("order" + select);
           // wiipay.Order(select);

            mmpay.Order(select);
            Moudle_main.Singlton.go_Black.SetActive(false);
            go_shop.SetActive(false);
        }
    }
    void Quit_Click()
    {
        reset();
        Moudle_main.Singlton.go_Black.SetActive(false);
        go_shop.SetActive(false);
        select = -1;
    }
	// Update is called once per frame
	void Update ()
    {
       // Debug.Log(go_shop);

	}
}
