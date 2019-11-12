using UnityEngine;
using System.Collections.Generic;

public class Test_MemoryLeak : MonoBehaviour {
    private struct TypeNum
    {
        public System.Type T;
        public uint num;
    }

    public bool OutputNum_Material = false;

    private static int TypeNumComparison(TypeNum x, TypeNum y)
    {
        if (x.num > y.num)
            return 1;
        else if (x.num < y.num)
            return -1;
        else
            return 0;
    }

    void OutputAllObjectInfo()
    {
        UnityEngine.Object[] objs = FindObjectsOfType(typeof(UnityEngine.Object));
        Debug.Log("---------------------------------------------Object Counts = " + objs.Length);
        if (OutputNum_Material)
        {
            Debug.Log("材质对象名:");
            foreach (Object o in objs)
            {
                if (o is Material)
                {
                    Debug.Log(((Material)o).name);
                }
            }
        }

        Dictionary<System.Type, uint> typeNumDict = new Dictionary<System.Type, uint>();
        foreach (Object o in objs)
        {
            if (typeNumDict.ContainsKey(o.GetType()))
                typeNumDict[o.GetType()] = typeNumDict[o.GetType()] + 1;
            else
                typeNumDict.Add(o.GetType(), 1);


        }
        List<TypeNum> typelist = new List<TypeNum>();

        TypeNum tnTmp;
        foreach (KeyValuePair<System.Type, uint> typenum in typeNumDict)
        {

            tnTmp.T = typenum.Key;
            tnTmp.num = typenum.Value;
            typelist.Add(tnTmp);
        }

        typelist.Sort(TypeNumComparison);
        Debug.Log("类型排名:");
        foreach (TypeNum tn in typelist)
        {
            if (tn.num > 1)
                Debug.Log(tn.T + " : " + tn.num);
        }



    }
    void OnGUI()
    {

 
        if (GUI.Button(new Rect(100F, 0F, 100F, 50F), "输出记录"))
        {
            OutputAllObjectInfo();
        }

    }
}
