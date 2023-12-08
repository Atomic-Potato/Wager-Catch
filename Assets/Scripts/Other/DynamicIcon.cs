using UnityEngine;

public class DynamicIcon : MonoBehaviour
{
    [SerializeField] Sprite _catchersIcon;
    [SerializeField] Sprite _runnersIcon;
    [SerializeField] Sprite _nuteralIcon;

    [Space]
    [SerializeField] SpriteRenderer[] _iconRenderes;

    void Start()
    {
        Sprite icon; 
        switch (GameManager.Instance.PlayerTeam)
        {
            case TagsManager.Tag.Catcher:
                icon = _catchersIcon;
                break;
            case TagsManager.Tag.Runner :
                icon = _runnersIcon;
                break;
            default:
                icon = _nuteralIcon;
                break;
        }
        
        foreach (SpriteRenderer renderer in _iconRenderes)
            renderer.sprite = icon;
    }
}
