using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5.0f;
    private Rigidbody rb;

    // Use to assign a callback for when attack is triggered
    public UnityAction OnAttackTriggered;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        OnAttackTriggered += HandleAttack;
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
}
