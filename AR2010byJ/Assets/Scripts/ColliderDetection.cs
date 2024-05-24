using UnityEngine;

public class ColliderDetection : MonoBehaviour
{
    public string targetTag = "ColliderDetection"; 
    public float detectionDistance = 0.775f; 
    public AudioClip exitSound; 
    bool shouldVibrate = false;

    public bool GetShouldVibrate() 
    {
        return shouldVibrate;
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            float distance = Vector3.Distance(transform.position, other.transform.position);
            Debug.Log(distance);
            if (distance > detectionDistance)
            {
                shouldVibrate = true;
            } else {
                shouldVibrate = false;
            }
        }
    }
    void Update()
    {
        if (shouldVibrate)
        {
            Debug.Log("300!");
            OVRInput.SetControllerVibration(1f, 1f, OVRInput.Controller.RTouch);
        } else {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            Debug.Log("100!");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            shouldVibrate = false;
            Debug.Log("400!");
            if (exitSound != null)
            {
                AudioSource.PlayClipAtPoint(exitSound, transform.position);
            }
        }
    }
}
