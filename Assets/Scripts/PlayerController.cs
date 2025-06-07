using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI damageText;

    [Header("Movement Settings")]
    public float moveSpeed = 1f;

    [Header("Input Control")]
    public bool isPlayerControlled = false;

    [Header("AI Settings")]
    public Vector2 targetPosition = Vector2.zero;

    private Vector2 movementInput = Vector2.zero;

    // Amount of damage player got in this run
    public int Damage
    {
        get => int.TryParse(damageText.text, out var val) ? val : 0;
        set => damageText.text = value.ToString();
    }

    private void Start()
    {
        if (!isPlayerControlled)
            return;

        var playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Move"].performed += OnMoveInput;
        playerInput.actions["Move"].canceled += OnMoveInput;
    }

    private void OnMoveInput(InputAction.CallbackContext context)
    {
        movementInput = context.ReadValue<Vector2>().normalized;
    }

    private void Update()
    {
        if (isPlayerControlled)
        {
            MoveWithInput();
        }
        else
        {
            MoveTowardsTarget();
        }
    }

    // Handles movement when player input is active
    private void MoveWithInput()
    {
        Vector3 movement = new Vector3(movementInput.x, movementInput.y) * moveSpeed * Time.deltaTime;
        transform.position += movement;
    }

    // Handles automated movement towards a set position
    private void MoveTowardsTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;
        float distance = Vector2.Distance(currentPosition, targetPosition);

        if (distance > 0.01f)
        {
            transform.position = currentPosition + direction * moveSpeed * Time.deltaTime;
        }
        else
        {
            transform.position = targetPosition; // Snap to target when close
        }
    }
}
