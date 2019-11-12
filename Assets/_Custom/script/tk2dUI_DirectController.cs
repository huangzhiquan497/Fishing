using UnityEngine;
using System.Collections;

public class tk2dUI_DirectController : tk2dUIBaseItemControl
{
    public float CircleRadius = 10F;
    [System.NonSerialized]
    public Vector3 Direction;
    [System.NonSerialized]
    public float Strength;//0~1
    private Vector3 offset = Vector3.zero; //offset on touch/click
    private bool isBtnActive = false; //if currently active


    private Vector3 mOriginPos;
    private Transform mTs;
    private Camera mUICamera;
    void OnEnable()
    {
        if (uiItem)
        {
            uiItem.OnDown += ButtonDown;
            uiItem.OnRelease += ButtonRelease;

            if (mTs == null)
            {
                mTs = transform;
                mOriginPos = mTs.position;
            }
            if (mUICamera == null)
            {
                mUICamera = tk2dUIManager.Instance.GetUICameraForControl(gameObject);
            }
        }
    }

    void OnDisable()
    {
        if (uiItem)
        {
            uiItem.OnDown -= ButtonDown;
            uiItem.OnRelease -= ButtonRelease;
        }

        if (isBtnActive)
        {
            if (tk2dUIManager.Instance != null)
            {
                tk2dUIManager.Instance.OnInputUpdate -= UpdateBtnPosition;
            }
            isBtnActive = false;
        }
    }

    private void UpdateBtnPosition()
    {
        mTs.position = CalculateNewPos();
        Direction = mTs.position - mOriginPos;
        Strength = Direction.magnitude / CircleRadius;
    }

    private Vector3 CalculateNewPos()
    {
        Vector2 pos = uiItem.Touch.position;

        Vector3 worldPos = mUICamera.ScreenToWorldPoint(new Vector3(pos.x, pos.y, mTs.position.z - mUICamera.transform.position.z));
        worldPos.z = mTs.position.z;
        worldPos += offset;

        Vector3 dir = worldPos - mOriginPos;
        float distance = dir.magnitude;
        if (distance  > CircleRadius)
        {
            worldPos = mOriginPos + dir / distance * CircleRadius;
        }
        return worldPos;
    }

    /// <summary>
    /// Set button to be down (drag can begin)
    /// </summary>
    public void ButtonDown()
    {
        if (!isBtnActive)
        {
            tk2dUIManager.Instance.OnInputUpdate += UpdateBtnPosition;
        }
        isBtnActive = true;
        offset = Vector3.zero;
        Vector3 newWorldPos = CalculateNewPos();
        offset = mTs.position - newWorldPos;
    }

    /// <summary>
    /// Set button release (so drag will stop)
    /// </summary>
    public void ButtonRelease()
    {
        if (isBtnActive)
        {
            tk2dUIManager.Instance.OnInputUpdate -= UpdateBtnPosition;
            mTs.position = mOriginPos;
            Direction = Vector3.zero;
            Strength = 0F;
        }
        isBtnActive = false;
    }
}
