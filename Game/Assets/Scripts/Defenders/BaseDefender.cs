using FMODUnity;
using UnityEngine;

public class BaseDefender : BaseUnit
{
    [SerializeField] private float aggroRadius = 5.0f;
    // The distance to lose aggro should always be larger than the aggro radius.
    [SerializeField] private float loseAggroDistance = 6.0f;
    [SerializeField] private LayerMask targetLayerMask;
    [SerializeField] private float timeBetweenTargetingChecks = 1.0f;
    
    private BaseDefenderShoot baseDefenderShoot;
    private Timer _targetingCheckTimer;
    private bool _isTargetingEnemy;
    private GameObject _currentTarget;
    private SpriteRenderer _spriteRenderer;

    public override void Awake()
    {
        base.Awake();
        
        baseDefenderShoot = GetComponent<BaseDefenderShoot>();
        _targetingCheckTimer = new Timer(timeBetweenTargetingChecks);
        baseDefenderShoot = GetComponent<BaseDefenderShoot>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        RuntimeManager.PlayOneShot("event:/SFX/Tower_Destroy", transform.position);

    }

    private void Update()
    {
        if (_currentTarget)
        {
            Vector3 targ = _currentTarget.transform.position;
            targ.z = 0f;
 
            Vector3 objectPos = transform.position;
            targ.x = targ.x - objectPos.x;
            targ.y = targ.y - objectPos.y;
 
            float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle + 180));

            transform.GetChild(0).transform.rotation = Quaternion.identity;
        }
        
        UpdateTargetChecks();
        UpdateShooting();
    }

    /*
     * Every so often, updates the current target for this defender.
     * If not targeting, checks for enemies to target.
     * Else, sees if the currently targeted enemy is still in range.
     */
    private void UpdateTargetChecks()
    {
        if (!_targetingCheckTimer.IsFinished())
        {
            UpdateTargetedEnemy();
            _targetingCheckTimer.Reset();
        }
        else
        {
            _targetingCheckTimer.UpdateTime();
        }
    }

    /**
     * If not targeting, checks for enemies within range.
     * Else, checks if the targeted enemy is still in range.
     */
    private void UpdateTargetedEnemy()
    {
        if (_isTargetingEnemy)
        {
            // Check to make sure current target still exists
            if (_currentTarget == null)
            {
                // Set is targeted enemy to false since previous target no longer exists
                _isTargetingEnemy = false;
                return;
            }
            // See if targeted enemy is still in range.
            if (Vector2.Distance(transform.position, _currentTarget.transform.position) <= loseAggroDistance) return;

            _isTargetingEnemy = false;
        }

        if (_isTargetingEnemy) return;
        
        // Try finding an enemy within range.
        var hitResult = Physics2D.CircleCast(transform.position, aggroRadius, Vector2.one, 0.0f, targetLayerMask);
        var hitCollider = hitResult.collider;

        if (!hitCollider) return;
        
        _currentTarget = hitCollider.gameObject;
        _isTargetingEnemy = true;
    }

    private void UpdateShooting()
    {
        if (!_isTargetingEnemy) return;

        baseDefenderShoot.UpdateShoot(_currentTarget);
    }
}