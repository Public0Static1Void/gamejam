using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ReturnScript : MonoBehaviour
{
    [Header("Stats")]
    public float return_speed;
    public float max_time = 3;
    public float explosion_rage;

    [Header("Positions record")]
    public List<Vector3> past_positions;
    public List<Vector2> past_rotations;
    public List<Quaternion> q_rotations;

    private bool returning = false;
    private float timer = 0;
    private int current_point = 0;

    private CameraRotation cameraRotation;

    void Start()
    {
        cameraRotation = Camera.main.GetComponent<CameraRotation>();

        past_positions = new List<Vector3>
        {
            transform.position
        };
        past_rotations = new List<Vector2>
        {
            new Vector2(cameraRotation.x, cameraRotation.y)
        };
        q_rotations = new List<Quaternion>
        {
            transform.rotation
        };
    }

    void Update()
    {
        if (returning && past_positions.Count > 0)
        {
            if (Vector3.Distance(transform.position, past_positions[current_point]) > 0.1f)
            {
                Vector3 dir = (past_positions[current_point] - transform.position).normalized;
                transform.Translate(dir * return_speed * Time.deltaTime, Space.World); /// Mueve al jugador en la dirección a su anterior posición
                
                cameraRotation.x = past_rotations[current_point].x; /// Cambia la rotación del script de la cámara, para que no haga cosas raras al acabar la transición
                cameraRotation.y = past_rotations[current_point].y;

                //transform.rotation = Quaternion.Euler(0, cameraRotation.x, 0);
                //Camera.main.transform.rotation = Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0);

                transform.rotation = Quaternion.Lerp(transform.rotation, q_rotations[current_point], Time.deltaTime * return_speed);
                Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, Quaternion.Euler(-cameraRotation.y, cameraRotation.x, 0), Time.deltaTime * return_speed * 0.75f);

                //transform.rotation = Quaternion.Lerp(transform.rotation, q_rotations[current_point], Time.deltaTime * return_speed * 0.1f);
            }
            else
            {
                if (current_point > 0)
                {
                    current_point--;
                }
                else
                {
                    // El jugador ha llegado al último punto
                    PlayerMovement.instance.canMove = true;
                    GetComponent<Rigidbody>().isKinematic = false;
                    returning = false;
                }
            }
        }
        else
        {
            if (timer > max_time / 10)
            {
                if (past_positions.Count > 0)
                {
                    if (past_positions.Count >= 10)
                    {
                        past_positions.RemoveAt(0);
                        past_rotations.RemoveAt(0);
                        q_rotations.RemoveAt(0);
                    }
                    past_positions.Add(transform.position);
                    past_rotations.Add(new Vector2(cameraRotation.x, cameraRotation.y));
                    q_rotations.Add(transform.rotation);
                }
                timer = 0;
            }
            else
            {
                timer += Time.deltaTime;
            }
        }   
    }

    public void ReturnToLastPosition(InputAction.CallbackContext con)
    {
        if (con.performed) // Se ha pulsado espacio
        {
            returning = true;
            GetComponent<Rigidbody>().isKinematic = true;
            PlayerMovement.instance.canMove = false;
            current_point = past_positions.Count - 1;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(past_positions[0], 1);
    }
}
