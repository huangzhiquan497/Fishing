using UnityEngine;
using System.Collections;

public class Bs_4Reboot : MonoBehaviour {

    public tk2dTextMesh Text_RebootCountDown;
	// Use this for initialization
	IEnumerator Start () {
        float useTime = 3F;
        while (useTime > 0)
        {
            Text_RebootCountDown.text = ((int)useTime).ToString();
            Text_RebootCountDown.Commit();
            useTime = useTime - 1F;
            
            yield return new WaitForSeconds(1F);
        }

        //reboot
        win32Api.RebootSystem();
	}

}
