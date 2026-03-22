using UnityEngine;

public class PlayermoveControler : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 4.0f;
    float hAxis;
    float vAxis;

    Vector3 moveVec;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");

        moveVec = new Vector3(hAxis, 0, vAxis);

        transform.position += moveVec * moveSpeed * Time.deltaTime;
    }
}
