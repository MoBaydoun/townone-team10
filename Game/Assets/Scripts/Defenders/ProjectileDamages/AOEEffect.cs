using UnityEngine;

public class AOEEffect : BaseProjectileEffect
{
    [SerializeField] private float damageRadius;
    [SerializeField] private LayerMask targetLayerMask;
    
    public override void UpdateEffect(GameObject effectTarget, float baseDamage)
    {
        var raycastHits = Physics2D.CircleCastAll(effectTarget.transform.position, damageRadius, Vector2.one, 0.0f, targetLayerMask);

        foreach (var hit in raycastHits)
        {
            var hitGameObject = hit.collider.gameObject;

            var hitAttacker = hitGameObject.GetComponent<BaseAttacker>();
            
            hitAttacker.DealDamage(baseDamage);
        }

        IsDoneEffect = true;
    }
}
