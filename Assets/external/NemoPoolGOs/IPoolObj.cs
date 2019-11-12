using UnityEngine;
using System.Collections;

public interface IPoolObj {

    GameObject Prefab
    {
        get;
        set;

    }
    /// <summary>
    /// ????
    /// </summary>
     void On_Reuse(GameObject prefab);

    /// <summary>
    /// ????
    /// </summary>
      void On_Recycle();
}
