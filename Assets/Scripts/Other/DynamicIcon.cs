using UnityEngine;
using UnityEngine.UI;

public class DynamicIcon : MonoBehaviour
{
    [SerializeField] bool _isUseOppositeTeamIcon;
    [SerializeField] Sprite _catchersIcon;
    [SerializeField] Sprite _runnersIcon;
    [SerializeField] Sprite _nuteralIcon;

    [Space]
    [SerializeField] Image[] _iconRenderes;

    void Start()
    {
        Sprite icon; 
        TagsManager.Tag team = _isUseOppositeTeamIcon ? GameManager.Instance.OppositeTeam : GameManager.Instance.PlayerTeam_TAG;
        switch (team)
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
        
        foreach (Image renderer in _iconRenderes)
            renderer.sprite = icon;
    }
}
