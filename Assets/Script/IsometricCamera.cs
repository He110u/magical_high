using UnityEngine;

public class IsometricCamera : MonoBehaviour
{
    [SerializeField]
    float offsetY;
    public GameObject player;

    void Update()
    {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + offsetY, player.transform.position.z);
    }
}
