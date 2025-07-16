using UnityEngine;

public interface IEnemy
{
    void TakeDamage(int amount);
    bool PlayerInSight();
    Transform GetTransform();
}
