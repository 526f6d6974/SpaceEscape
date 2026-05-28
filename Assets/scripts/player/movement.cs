
using UnityEngine;
using UnityEngine.InputSystem;

public class movement : MonoBehaviour
{
    // this script is responsible for the movement of the player by processing the input from the player and applying the thrust force to the rb componement of the player
    [SerializeField] InputAction thrust;
    [SerializeField] float thrustForce = 1f;

    // this is for the rotation of the player 
    [SerializeField] InputAction rotation;
    [SerializeField] float rotationPower = 1f;
    Rigidbody rb;

    void OnEnable()
    {
        thrust.Enable();
        rotation.Enable();
    }
    void Start()
    {
        rb = GetComponent<Rigidbody>();//getting the rb componement for processing the thrust movement
    }


    void FixedUpdate()
    {
        process_thrust(thrustForce);
        process_rotation(rotationPower);
    }
    
    //processing the thrust movement by adding a relative force to the rb componement in the up direction multiplied by the thurst power
    public void process_thrust(float thurstpower)
    {
        if (thrust.IsPressed())
        {
            rb.AddRelativeForce(Vector3.up * thurstpower);
        }
    }

    //processing the rotation movement by adding a relative torque to the rb componement in the forward direction multiplied by the rotation power and the input value of the rotation action
    public void process_rotation(float rotationpower)
    {
        float rotation_input = rotation.ReadValue<float>();
        if (rotation_input < 0)
        {
            startrightthruster();
        }
        else if (rotation_input > 0)
        {
            startleftthruster();
        }
    }

    private void startleftthruster()
    {
        rkrotation(-rotationPower);
        
    }

    private void startrightthruster()
    {
        rkrotation(rotationPower);
       
    }

    private void rkrotation(float rotationthisframe)
    {
        rb.angularVelocity = Vector3.zero;
        transform.Rotate(Vector3.forward * rotationthisframe * Time.fixedDeltaTime,Space.World);
    }
    
    
    
}
