using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EkkoUlt : MonoBehaviour
{
    [Header("Stats")]
    public float period_time;

    public List<Vector3> playerPositions;
    private Vector3 return_position;


    private bool mark_placed, return_to_pos;

    private float timer = 0;

    [Header("References")]
    public GameObject follower_model;
    public Transform returnMark;

    private int current_checkpoint = 0;
    void Start()
    {
        playerPositions.Add(transform.position);
        follower_model.transform.position = transform.position;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !mark_placed) { 
            StartCoroutine(WaitToReturn());
        }

        if (timer < 0.5f)
        {
            timer += Time.deltaTime;
        }
        else
        {
            if (playerPositions.Count > 100)
            {
                playerPositions.Clear();
                playerPositions.Add(transform.position);
            }
            if (playerPositions.Count > 0 && transform.position != playerPositions[playerPositions.Count - 1])
                playerPositions.Add(transform.position);
            timer = 0;
        }

        // Follower logic
        if (playerPositions.Count > 1 && current_checkpoint < playerPositions.Count)
        {
            if (Vector3.Distance(follower_model.transform.position, playerPositions[current_checkpoint]) <= 1)
            {
                ChangeFollowerObjective();
            }
            else
            {
                //follower_model.transform.position = Vector3.Lerp(follower_model.transform.position, playerPositions[current_checkpoint], (PlayerMovement.instance.speed * 0.0025f) * Time.deltaTime);
                follower_model.transform.Translate(follower_model.transform.forward * (PlayerMovement.instance.speed * 0.01f) * Time.deltaTime, Space.World);
            }
        }

        if (return_to_pos)
        {
            if (Vector3.Distance(transform.position, return_position) > 0.5f)
            {
                transform.position = Vector3.Lerp(transform.position, return_position, Time.deltaTime * 10);
            }
            else
            {
                return_to_pos = false;
            }
        }
    }

    public void ChangeFollowerObjective()
    {
        if (current_checkpoint < playerPositions.Count - 2)
            current_checkpoint++;


        Vector3 dir = playerPositions[current_checkpoint] - follower_model.transform.position;
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);
        follower_model.transform.rotation = rot;
    }

    public IEnumerator WaitToReturn()
    {
        mark_placed = true;

        // Activa los followers
        returnMark.transform.gameObject.SetActive(true);

        return_position = follower_model.transform.position;
        returnMark.position = return_position;

        yield return new WaitForSeconds(period_time);
        return_to_pos = true;

        // Desactiva los followers
        returnMark.transform.gameObject.SetActive(false);

        mark_placed = false;
    }
}
