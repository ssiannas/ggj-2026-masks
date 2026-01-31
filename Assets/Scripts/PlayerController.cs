using System;
using ggj_2026_masks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[Serializable]
public class FloatEvent : UnityEvent<float>
{
}

public class PlayerController : MonoBehaviour
{
    // UI
    [SerializeField] private PlayerUIController _playerUIController;

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

    // Dash state
    public bool isDashing;
    private float _health;
    private float dashStartTime;
    private Vector2 moveDirection;
    private Rigidbody rb;
    
    // Interactions
    [SerializeField] private InteractionController _interactionController;

    // Attacking controller
    private PlayerAttackingController _attackingController;

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
        if (_playerUIController is not null) OnHealthUpdated.AddListener(_playerUIController.SetHealthPercentage);
        _interactionController = GetComponent<InteractionController>();
        _attackingController = GetComponent<PlayerAttackingController>();
    }

    // Update is called once per frame
    private void Update()
    {
        // Dead => no movement
        if (!isAlive)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

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

    private void OnTestDamage(InputValue value)
    {
        var testDamageButton = value.Get<float>();
        if (testDamageButton > 0.5f) ApplyDamage(0.2f * maxHealth);
    }

    private void OnInteract(InputValue value)
    {
        var interactButton = value.Get<float>();
        if (interactButton > 0.5f)
        {
            Debug.Log("Player pressed interact.");
            _interactionController.TryInteract();
        };
    }

    private void HandleAttack()
    {
        Debug.Log($"Handling attack: {gameObject.name}");
        _attackingController?.Attack();
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