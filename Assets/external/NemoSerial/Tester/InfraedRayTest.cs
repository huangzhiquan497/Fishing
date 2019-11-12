using UnityEngine;
using System.Collections.Generic;

public class InfraedRayTest : MonoBehaviour {
    public GameObject Prefab_HitCurosr;
    private ArcadeIO_InfraedRayZhang mAIO;
    GameObject preCreateGO;
    private float mDestroyHitCursorTime = 0.5F;
    public Camera cam;
    void Awake()
    {
        mAIO = GetComponent<ArcadeIO_InfraedRayZhang>();
        mAIO.HandleTouchScreen += HandleTouchScreen;
    }

    void Start()
    {
        GenerateAdjustFrame();
    }

    void HandleTouchScreen(float x,float y)
    {
        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(x, y, cam.nearClipPlane));
        if (preCreateGO != null)
            Destroy(preCreateGO);
        preCreateGO = Instantiate(Prefab_HitCurosr) as GameObject;
        TextMesh tm = preCreateGO.GetComponentInChildren<TextMesh>();
        if (tm != null)
            tm.text = string.Format("({0},{1})"
                , Mathf.RoundToInt(x * (mAIO.HoriRayNum+1)), Mathf.RoundToInt(y * (mAIO.VertRayNum+1)));
        worldPos.z = 1F;
        preCreateGO.transform.position = worldPos;
        mDestroyHitCursorTime = 0.5F;
        //Debug.Log(new Vector3(x, y, 0.1F));
    }


    void GenerateAdjustFrame()
    {
        
        Vector2 screenDim = new Vector2(3.2F, 2F);
        Vector2 perTileOffset = new Vector2(screenDim.x / (mAIO.HoriRayNum+1), screenDim.y / (mAIO.VertRayNum+1));
        Vector2 localOffset = new Vector2(perTileOffset.x * 0.5F*(mAIO.HoriRayNum-1), perTileOffset.y * 0.5F*(mAIO.VertRayNum-1));
        float lineWidth = 3.2F/500F;
        for (int i = 0; i != mAIO.HoriRayNum;++i )
        {
            //;//
            GameObject lineGo = new GameObject("frameLine");
            LineRenderer lr = lineGo.AddComponent<LineRenderer>();
            float curX = perTileOffset.x * i - localOffset.x;
            lr.SetPosition(0, new Vector3(curX, screenDim.y * 0.5F, 20F));
            lr.SetPosition(1, new Vector3(curX, -screenDim.y * 0.5F, 20F));
            lr.SetWidth(lineWidth, lineWidth);
            
            lr.useWorldSpace = false;
            lineGo.transform.parent = transform;
        }

        for (int i = 0; i != mAIO.VertRayNum; ++i)
        {
            //;//
            GameObject lineGo = new GameObject("frameLine");
            LineRenderer lr = lineGo.AddComponent<LineRenderer>();
            float curY = perTileOffset.y * i - localOffset.y;
            lr.SetPosition(0, new Vector3(-screenDim.x * 0.5f, curY, 20F));
            lr.SetPosition(1, new Vector3(screenDim.x * 0.5f, curY, 20F));
            lr.SetWidth(lineWidth, lineWidth);
            lr.useWorldSpace = false;
            lineGo.transform.parent = transform;
        }
    }
	// Update is called once per frame
	void Update () {
        mDestroyHitCursorTime -= Time.deltaTime;
        if (mDestroyHitCursorTime < 0F && preCreateGO != null)
        {
            Destroy(preCreateGO);
            preCreateGO = null;
        }

	}

    void OnGUI()
    {

    }
}
