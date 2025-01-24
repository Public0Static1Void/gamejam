using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float cameraSpeed;
    
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.position = Vector3.Lerp(transform.position, newPos, cameraSpeed * Time.deltaTime);
    }
}
