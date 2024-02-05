using UnityEngine;

public abstract class UIMenuBase : MonoBehaviour
{
    [SerializeField] protected GameObject _menuParent;

    public void ShowMenu()
    {
        _menuParent.SetActive(true);
    }

    public void HideMenu()
    {
        _menuParent.SetActive(false);
    }

}
