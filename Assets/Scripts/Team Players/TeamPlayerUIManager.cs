using System.Linq.Expressions;
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
    [SerializeField] RectTransform _sleepBarTransform;

    [Space]
    [SerializeField] Canvas _canvas;

    bool _isUpdateSleepBar;
    float _sleepTimer;

    void Awake()
    {
        // Hiding the sleep bar
        _sleepBarTransform.localScale = new Vector3(0f, _sleepBarTransform.localScale.y, _sleepBarTransform.localScale.y);
    }

    void Start()
    {
        _canvas.worldCamera = Camera.main;
        if (!_teamPlayer.IsCanSprint)
            _staminaBarImage.enabled = false;
        _teamPlayer.SleepEvent.AddListener(StartSleepBarUpdate);
    }

    void Update()
    {
        if (_staminaBarTransform)
            UpdateStaminaBar();
        if (_sleepBarTransform)
            UpdateSleepBar();
    }

    void UpdateStaminaBar()
    {
        if (!_teamPlayer.IsCanSprint)
            return;

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

    void UpdateSleepBar()
    {
        if (!_isUpdateSleepBar)
            return;
        
        _sleepTimer -= Time.deltaTime;
        float sleepPerecentage = _sleepTimer / _teamPlayer.CurrentSleepTime;
        
        if (sleepPerecentage <= 0)
        {
            HideSleepBar();
            _isUpdateSleepBar = false;
        }

        _sleepBarTransform.localScale = new Vector3(sleepPerecentage, _sleepBarTransform.localScale.y, _sleepBarTransform.localScale.y);

        void HideSleepBar()
        {
            if (_sleepBarTransform.localScale.x != 0f)
                _sleepBarTransform.localScale = new Vector3(0f, _sleepBarTransform.localScale.y, _sleepBarTransform.localScale.y);
        }
    }

    void StartSleepBarUpdate()
    {
        _isUpdateSleepBar = true;
        _sleepTimer = _teamPlayer.CurrentSleepTime;
    }
}
