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
    public List<Quaternion> past_rotations;

    private bool returning = false;
    private float timer = 0;
    private int current_point = 0;

    void Start()
    {
        past_positions = new List<Vector3>
        {
            transform.position
        };
        past_rotations = new List<Quaternion>
        {
            transform.rotation
        };
    }

    void Update()
    {
        if (returning && past_positions.Count > 0)
        {
            // Cambia la rotación y va yendo de punto a punto
            if (Vector3.Distance(transform.position, past_positions[current_point]) > 0.1f)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, past_rotations[current_point], Time.deltaTime * return_speed);
                transform.position = Vector3.Lerp(transform.position, past_positions[current_point], Time.deltaTime * return_speed);
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
            if (timer > max_time / 100)
            {
                if (past_positions.Count > 0 && transform.position != past_positions[past_positions.Count - 1])
                {
                    if (past_positions.Count >= 100)
                    {
                        past_positions.RemoveAt(0);
                        past_rotations.RemoveAt(0);
                    }
                    past_positions.Add(transform.position);
                    past_rotations.Add(transform.rotation);
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
}
