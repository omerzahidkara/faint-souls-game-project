
using UnityEngine;

public interface Warrior
{
    public void TakeDamage(int damage);
    public void Attack(int damage, Collider2D collider);

    public void Die();
}
