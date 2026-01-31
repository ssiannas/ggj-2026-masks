using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class FloatEvent : UnityEvent<float>
{
}

public class PlayerController : MonoBehaviour
{
    // Move
    [SerializeField] private float moveSpeed = 5.0f;

    // Attacking
    public UnityEvent OnAttackTriggered;

    // Health
    [SerializeField] private float maxHealth = 100.0f;
    public UnityEvent OnPlayerDeath;
    public FloatEvent OnHealthUpdated;

    // Dashing
    [SerializeField] private float dashSpeed = 15.0f;
    [SerializeField] private float dashDurationMs = 20.0f;
    public UnityEvent OnDashTriggered;
    private float _health;
    private float dashStartTime;

    // Dash state
    public bool isDashing;
    private Vector2 moveDirection;
    private Rigidbody rb;

    public bool isAlive => Health > 0;

    public float Health
    {
        get => _health;
        set
        {
            _health = value;
            OnHealthUpdated.Invoke(_health / maxHealth);
            if (_health < 0) OnPlayerDeath.Invoke();
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        OnAttackTriggered.AddListener(HandleAttack);
        _health = maxHealth;
        OnPlayerDeath.AddListener(HandlePlayerDeath);
        OnDashTriggered.AddListener(HandleDash);
    }

    // Update is called once per frame
    private void Update()
    {
        // Dead => no movement
        if (!isAlive)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        };

        // Check if dash duration has expired
        if (isDashing && (Time.time - dashStartTime) * 1000 >= dashDurationMs) isDashing = false;

        // Apply movement
        if (isDashing)
            rb.linearVelocity = dashSpeed * new Vector3(moveDirection.x, 0, moveDirection.y);
        else
            rb.linearVelocity = moveSpeed * new Vector3(moveDirection.x, 0, moveDirection.y);
    }

    private void OnMove(InputValue value)
    {
        moveDirection = value.Get<Vector2>();
    }

    private void OnAttack(InputValue value)
    {
        var attackButton = value.Get<float>();

        // Check if attack is triggered. Add additional conditions here.
        var shouldAttack = attackButton > 0.5f;

        if (shouldAttack) OnAttackTriggered.Invoke();
    }

    private void OnDash(InputValue value)
    {
        var dashButton = value.Get<float>();
        var shouldDash = dashButton > 0.5f;

        if (shouldDash) OnDashTriggered.Invoke();
    }

    private void OnCrouch(InputValue value)
    {
        var crouchButton = value.Get<float>();
        if (crouchButton > 0.5f) ApplyDamage(0.2f * maxHealth);
    }

    private void HandleAttack()
    {
        Debug.Log("Attacking!");
    }

    private void HandlePlayerDeath()
    {
        Debug.Log("Player Died!");
        isDashing = false;
    }

    public void ApplyDamage(float damage)
    {
        Health -= damage;
    }

    private void HandleDash()
    {
        isDashing = true;
        dashStartTime = Time.time;
    }
}