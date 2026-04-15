using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage);   
    void HandleHit(Shell shell, RaycastHit hit);
}
