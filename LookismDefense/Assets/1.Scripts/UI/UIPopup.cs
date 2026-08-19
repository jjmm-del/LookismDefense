using UnityEngine;

public class UIPopup : UIBase
{
    public virtual void Close()
    {
        UIManager.Instance.ClosePopup(this);
    }
}
