using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestMoveScripts : MonoBehaviour
{
    [SerializeField] GameObject player;
    [SerializeField] List<GameObject> porters = new List<GameObject>();
    int movePositionIndex;
    [SerializeField] float moveSpeed;
    float location;
    private void Start()
    {
        movePositionIndex = 0;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            movePositionIndex++;
            if (movePositionIndex >= porters.Count)
            {
                movePositionIndex = 0;
            }
        }
        OnMove(porters[movePositionIndex].transform.position);
    }
    private void OnMove(Vector3 targetPos)
    {
        if (targetPos == player.transform.position) return;

        Vector3 dir = targetPos - player.transform.position;
        player.transform.position += dir * moveSpeed * Time.deltaTime;
    }
}
