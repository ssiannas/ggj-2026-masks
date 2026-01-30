using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    private Rigidbody rb;

    // Use to assign a callback for when attack is triggered
    public UnityAction OnAttackTriggered;

    [SerializeField] private float maxHealth = 100.0f;
    private float _health;
    
    public UnityAction OnPlayerDeath;

    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            if (_health < 0)
            {
                OnPlayerDeath.Invoke();
            }
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        OnAttackTriggered += HandleAttack;
        _health = maxHealth;
        OnPlayerDeath += HandlePlayerDeath;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnMove(InputValue value)
    {
        var direction = value.Get<Vector2>();

        rb.linearVelocity = moveSpeed * new Vector3(direction.x, 0, direction.y);
    }

    void OnAttack(InputValue value)
    {
        var attackButton = value.Get<float>();
        
        // Check if attack is triggered. Add additional conditions here.
        var shouldAttack = attackButton > 0.5f;

        if (shouldAttack)
        {
            OnAttackTriggered.Invoke();
        }
    }

    void HandleAttack()
    {
        Debug.Log("Attacking!");
    }

    void HandlePlayerDeath()
    {
        Debug.Log("Player Died!");
    }

    public void ApplyDamage(float damage)
    {
        Health -= damage;
    }
}
