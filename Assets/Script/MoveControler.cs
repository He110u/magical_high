using UnityEngine;

public class MoveControler : MonoBehaviour
{

    [SerializeField]
    float Movespeed;
    Vector3 forward, right;

    void Start()
    {
        forward = Camera.main.transform.forward;
        forward.y = 0;
        forward = Vector3.Normalize(forward);

        right = Quaternion.Euler(new Vector3(0, 90, 0)) * forward;
    }

    void Update()
    {
        if (Input.anyKey)
        {
            Move();
        }

    }

    void Move()
    {
        Vector3 RigthMovement = right * Movespeed * Time.smoothDeltaTime * Input.GetAxis("Horizontal");
        Vector3 ForwardMovement = forward * Movespeed * Time.smoothDeltaTime * Input.GetAxis("Vertical");
        Vector3 FinalMovement = ForwardMovement + RigthMovement;
        Vector3 direction = Vector3.Normalize(FinalMovement);


        if(direction != Vector3.zero)
        {
            transform.forward = direction;
            transform.position += FinalMovement;
        }
    }
}
