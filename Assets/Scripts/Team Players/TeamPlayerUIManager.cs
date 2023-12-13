using Pathfinding;
using UnityEngine;
using UnityEngine.UI;
public class TeamPlayerUIManager : MonoBehaviour
{
    [SerializeField] TeamPlayer _teamPlayer;
    
    [Space, Header("Stamina Bar")]
    [SerializeField] RectTransform _staminaBarTransform;
    [SerializeField] Image _staminaBarImage;
    [SerializeField] Color _sprintingColor = new Color(0f, 1f, 0f, .5f);
    [SerializeField] Color _staminaRecoveryColor = new Color(1f, 0f, 0f, .5f);


    [Space, Header("Stun Bar")]
    [SerializeField] GameObject _stunBar;
    [SerializeField] Canvas _canvas;

    void Start()
    {
        _canvas.worldCamera = Camera.main;
    }

    void Update()
    {
        UpdateStaminaBar();
    }

    void UpdateStaminaBar()
    {
        float stamina = _teamPlayer.GetStaminaPercentage();
        if (stamina >= 1f)
        {
            HideStamina();
            return;
        }

        _staminaBarImage.color = _teamPlayer.IsSprinting ? _sprintingColor : _staminaRecoveryColor;
        _staminaBarTransform.localScale = new Vector3(stamina, _staminaBarTransform.localScale.y, _staminaBarTransform.localScale.y);

        void HideStamina()
        {
            if (_staminaBarTransform.localScale.x != 0f)
                _staminaBarTransform.localScale = new Vector3(0f, _staminaBarTransform.localScale.y, _staminaBarTransform.localScale.y);
        }
    }

    void UpdateStunBar()
    {

    }
}
