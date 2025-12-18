using UnityEngine;

public class CameraPingPong : MonoBehaviour
{
    public float speed = 2f;
    public float leftLimit = -10f;
    public float rightLimit = 10f;

    private int direction = 1;

    void Update()
    {
        transform.Translate(Vector3.right * direction * speed * Time.deltaTime);

        if (transform.position.x >= rightLimit)
            direction = -1;

        if (transform.position.x <= leftLimit)
            direction = 1;
    }
}
