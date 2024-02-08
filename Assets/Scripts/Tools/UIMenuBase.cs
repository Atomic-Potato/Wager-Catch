using UnityEngine;

public abstract class UIMenuBase : MonoBehaviour
{
    [SerializeField] protected GameObject _menuParent;

    public virtual void ShowMenu()
    {
        _menuParent.SetActive(true);
    }

    public virtual void HideMenu()
    {
        _menuParent.SetActive(false);
    }

}
