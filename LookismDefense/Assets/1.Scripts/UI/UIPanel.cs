using UnityEngine;

public abstract class UIPanel : UIBase
{
    public virtual void Close()
    {
        UIManager.Instance?.ClosePanel(this);
    }
}
