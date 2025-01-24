using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float cameraSpeed;

    private float original_Y = 0;
    
    void Start()
    {
        original_Y = transform.position.y;
    }

    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x, original_Y, target.position.z);

        transform.position = Vector3.Lerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
    }
}
