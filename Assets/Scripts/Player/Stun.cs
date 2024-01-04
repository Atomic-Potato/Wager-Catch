using Pathfinding;
using UnityEngine;

public class Stun : MonoBehaviour
{
    [SerializeField, Min(0f)] float _nuteralStunTime = 2f;
    [SerializeField, Min(0f)] float _runnerStunTime = 1f;
    [SerializeField, Min(0f)] float _catcherStunTime = 3f;

    [Space]
    [SerializeField, Min(0f)] float _stunRange = 1f;
    [SerializeField] Transform _stunRangeOrigin;

    [Space]
    [SerializeField] GameObject _stunEffect;

    [Space, Header("GIZMOS")]
    [SerializeField] bool _isDrawStunRange;
    Vector2 _stunOrigin => _stunRangeOrigin != null ? (Vector2)_stunRangeOrigin.position : Vector2.zero;
    float _stunDuration;

    void OnDrawGizmos()
    {
        if (_isDrawStunRange)
        {
            
            Gizmos.DrawWireSphere(_stunOrigin, _stunRange);
        }
    }

    void Start()
    {
        switch(GameManager.Instance.OppositeTeam)
        {
            case TagsManager.Tag.Runner:
                _stunDuration = _runnerStunTime;
                break;
            case TagsManager.Tag.Catcher:
                _stunDuration = _catcherStunTime;
                break;
            default:
                _stunDuration = _nuteralStunTime;
                break;
        }
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Mouse1))
        {
            ShowStunEffect();
            RaycastHit2D[] hits = Physics2D.CircleCastAll(_stunOrigin, _stunRange, Vector2.zero); 
            foreach (RaycastHit2D hit in hits)
            {
                string tag = hit.collider.gameObject.tag;
                if (tag == GameManager.Instance.OppositeTeam.ToString())
                {
                    TeamPlayer player = hit.collider.gameObject.GetComponent<TeamPlayer>();
                    player.Sleep(_stunDuration);
                }
            }
        }
    }

    void ShowStunEffect()
    {
        Instantiate(_stunEffect, transform.position, Quaternion.identity);
    }
}
