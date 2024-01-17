using System.Collections;
using TMPro;
using UnityEngine;
using Ability;

[RequireComponent (typeof (Collider2D))]
public class Doorway : MonoBehaviour
{
    [SerializeField, Min(0)] int _price = 20; 
    [SerializeField, Min(0)] float _activationTime = 2f;
    [SerializeField, Min(0)] float _impulseRadius = 2f;
    [SerializeField] LayerMask _impulsedLayers;
    [SerializeField] bool _isDrawImpulseRadiusGizmos;

    [Space, SerializeField] GameObject _obstacle;
    [SerializeField] BoxCollider2D _obstacleCollider;
    [SerializeField] TMP_Text _priceText;
    [SerializeField] Canvas _canvas;

    bool _isInGame => GameManager.Instance.CurrentGameState == GameManager.GameState.InGame;
    bool _isActive = true;

    void OnDrawGizmos()
    {
        if (_isDrawImpulseRadiusGizmos)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, _impulseRadius);
        }
    }

    void Awake()
    {
        GameManager.Instance.PlayerSpawnBroadcaster.AddListener(Deactivate);
        GameManager.Instance.PlayerDespawnBroadcaster.AddListener(Activate);
        AbilitiesManager.Instance.AddAbilitySelectionBroadCasterListener(Deactivate);
        AbilitiesManager.Instance.AddAbilityRemovedBroadcasterListener(Activate);

        _canvas.worldCamera = Camera.main;
        _obstacle.SetActive(false);
        _priceText.text = _price + "$";
    }
    
    void OnMouseOver()
    {
        if (!_isInGame || !_isActive)
            return;

        ShowPrice();
        if (Input.GetMouseButton(0) && !_obstacle.activeSelf && GameManager.Instance.Balance <= _price)
            StartCoroutine(ActivateObstacle());
    }

    void OnMouseExit()
    {
        if (!_isInGame)
            return;

        HidePrice();
    }

    void ShowPrice()
    {
        if (_obstacle.activeSelf)
            return;

        _priceText.enabled = true;
        _priceText.color = ColorsManager.GetPriceColor(_price);
    }

    void HidePrice()
    {
        _priceText.enabled = false;
    }

    IEnumerator ActivateObstacle()
    {
        _obstacle.SetActive(true);
        Bounds bounds = new Bounds(_obstacleCollider.bounds.center, _obstacleCollider.bounds.size);

        HidePrice();
        GameManager.Instance.DeductBalance(_price);
        GameManager.ImpulseObjectsInPorximity(transform.position, _impulsedLayers);
        UpdateGrid();
        TeamsManager.Instance.ForceUpdateAllPlayersPaths();
        GuardsManager.Instance.ForceUpdateAllGuardsPaths();
        
        yield return new WaitForSeconds(_activationTime);

        _obstacle.SetActive(false);
        UpdateGrid();
        GuardsManager.Instance.ForceUpdateAllGuardsPaths();

        void UpdateGrid()
        {
            Pathfinding.Grid.Instance.UpdateGridSection(bounds.min, bounds.max, false, true);
        }
    }

    void Activate()
    {
        if (GameManager.Instance.PlayerInstance != null)
            return;
        _isActive = true;
    }

    void Deactivate()
    {
        _isActive = false;
    }
}
