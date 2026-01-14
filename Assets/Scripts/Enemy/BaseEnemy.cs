using UnityEngine;

public class BaseEnemy : MonoBehaviour
{
    private float _health;
    private float _maxHealth;
    public float Health { get { return _health; } }
    private EnemySO _enemyData;
    public bool GoingToDie { get { return _health - DamageInFlight <= 0.0f; } }
    public float DamageInFlight { get; set; } = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _enemyData = GetComponent<EnemySOHolder>().EnemySOData;
        _maxHealth = _enemyData.Health;
        _health = _maxHealth;
    }

    public void TakeDamage(float damage)
    {
        _health -= damage;
        _health = Mathf.Max(0, _health);
        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        GameStateManager.GetInstance().Economy.AddMoney(_enemyData.Reward);
        Destroy(gameObject);
    }
}
