using UnityEngine;

public interface IDamageable
{
    void TakeDamage(float damage, Color color, bool isEffect, bool isCrit);   
    void HandleHit(Shell shell, RaycastHit hit);
}
