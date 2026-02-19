using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 0.08f;
    [SerializeField] private LayerMask hitMask;

    private void OnEnable()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0)
            return;
        
        if (other.TryGetComponent<Health>(out var health))
            health.Damage(damage);
    }
}
