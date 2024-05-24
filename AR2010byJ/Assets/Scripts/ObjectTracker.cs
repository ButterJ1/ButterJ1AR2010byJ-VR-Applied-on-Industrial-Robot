using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectTracker : MonoBehaviour
{
    public List<Transform> targets;
    private int currentTargetIndex = 0;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            CycleTarget();
        }
        TrackCurrentTarget();
    }

    void TrackCurrentTarget()
    {
        if (targets.Count == 0)
        {
            Debug.LogWarning("No targets assigned. Add targets to the 'targets' list in the inspector.");
            return;
        }

        Transform currentTarget = targets[currentTargetIndex];

        Vector3 directionToTarget = currentTarget.position - transform.position;
        Quaternion rotationToTarget = Quaternion.LookRotation(directionToTarget, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotationToTarget, Time.deltaTime * 5f);

        float moveSpeed = 3f;
        transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, Time.deltaTime * moveSpeed);
    }

    void CycleTarget()
    {
        currentTargetIndex = (currentTargetIndex + 1) % targets.Count;
    }
}
