using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    private Rigidbody rb;
    private Vector2 moveDirection;

    // Dash state
    private bool isDashing = false;
    private float dashStartTime;

    // Attacking
    public UnityAction OnAttackTriggered;

    // Health
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
    
    // Dashing
    [SerializeField] private float dashSpeed = 15.0f;
    [SerializeField] private float dashDurationMs = 20.0f;
    public UnityAction OnDashTriggered;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        OnAttackTriggered += HandleAttack;
        _health = maxHealth;
        OnPlayerDeath += HandlePlayerDeath;
        OnDashTriggered += HandleDash;
    }

    // Update is called once per frame
    void Update()
    {
        // Check if dash duration has expired
        if (isDashing && (Time.time - dashStartTime) * 1000 >= dashDurationMs)
        {
            isDashing = false;
        }

        // Apply movement
        if (isDashing)
        {
            rb.linearVelocity = dashSpeed * new Vector3(moveDirection.x, 0, moveDirection.y);
        }
        else
        {
            rb.linearVelocity = moveSpeed * new Vector3(moveDirection.x, 0, moveDirection.y);
        }
    }

    void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();
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

    void OnDash(InputValue value)
    {
        var dashButton = value.Get<float>();
        var shouldDash = dashButton > 0.5f;

        if (shouldDash)
        {
            OnDashTriggered.Invoke();
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

    void HandleDash()
    {
        isDashing = true;
        dashStartTime = Time.time;
    }
}
