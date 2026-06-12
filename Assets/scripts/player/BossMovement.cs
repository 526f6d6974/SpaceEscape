using UnityEngine;
using UnityEngine.InputSystem;

public class BossMovement : MonoBehaviour
{
    [Header("Controls")]
    [SerializeField] private InputAction horizontalOrbit;
    [SerializeField] private InputAction verticalFlight; // NEW: Replaces thrustAction!

    [Header("Orbital Settings")]
    public float orbitSpeed = 45f;
    public float orbitRadius = 35f;

    [Header("Flight Settings")]
    public float verticalSpeed = 15f; // NEW: Controls how fast you fly up/down

    private Transform bossCore;
    private Rigidbody rb;
    private float currentAngle = 0f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void OnEnable()
    {
        // 1. Wake up the New Input System controls
        horizontalOrbit.Enable();
        verticalFlight.Enable();

        if (rb != null)
        {
            rb.isKinematic = false;
            
            // --- CHANGE 1: TURN OFF GRAVITY ---
            // This ensures you hover perfectly in place when letting go of the keys!
            rb.useGravity = false; 

            // Completely erase any leftover falling momentum from the endless runner
            rb.linearVelocity = Vector3.zero; 

            // Unfreeze Z so you can orbit in 3D, keep X and Z rotation frozen so you don't tumble
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }

        // 2. Automatically find the Boss Core
        GameObject foundBoss = GameObject.Find("Boss_Core");
        if (foundBoss != null)
        {
            bossCore = foundBoss.transform;

            // 3. Calculate exact parking angle so orbit starts seamlessly
            Vector3 directionToPlayer = transform.position - bossCore.position;
            currentAngle = Mathf.Atan2(directionToPlayer.z, directionToPlayer.x) * Mathf.Rad2Deg;
        }
        else
        {
            Debug.LogError("Could not find Boss_Core!");
        }
    }

    void OnDisable()
    {
        horizontalOrbit.Disable();
        verticalFlight.Disable();
    }

    void Update()
    {
        if (bossCore == null) return;

        // --- HORIZONTAL INPUT (Left / Right Orbit) ---
        float horizontalInput = horizontalOrbit.ReadValue<float>();
        currentAngle += horizontalInput * orbitSpeed * Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (bossCore == null) return;

        // --- CHANGE 2: VERTICAL INPUT (W / S Keys) ---
        // Reads 1 if holding W, -1 if holding S, and 0 if letting go
        float verticalInput = verticalFlight.ReadValue<float>();

        // --- ORBIT MATH (X and Z Coordinates) ---
        float radians = currentAngle * Mathf.Deg2Rad;
        float xPos = bossCore.position.x + (Mathf.Cos(radians) * orbitRadius);
        float zPos = bossCore.position.z + (Mathf.Sin(radians) * orbitRadius);

        // --- CHANGE 3: HOLD POSITION & FLY UP/DOWN ---
        // We calculate the new Y position. Because gravity is off, if verticalInput is 0,
        // yPos stays exactly the same as it was last frame (perfect hover!).
        float yPos = rb.position.y + (verticalInput * verticalSpeed * Time.fixedDeltaTime);

        // Apply all 3 axes at once
        Vector3 newPosition = new Vector3(xPos, yPos, zPos);
        rb.MovePosition(newPosition);

        // Turn the ship to point along the curve of the circle
        rb.MoveRotation(Quaternion.Euler(0, -currentAngle, 0));
    }
}