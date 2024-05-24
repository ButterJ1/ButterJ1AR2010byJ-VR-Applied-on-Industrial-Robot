using UnityEngine;

public class TouchFeedback : MonoBehaviour
{
    public string hapticObjectTag = "HapticObject";

    bool shouldVibrate = false;

    public bool GetShouldVibrate_Internal()
    {
        return shouldVibrate;
    }

    void Update()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 0.01f); // Adjust the sphere's radius as needed

        shouldVibrate = false;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(hapticObjectTag))
            {
                Rigidbody rigidbody = collider.GetComponent<Rigidbody>();
                if (rigidbody != null)
                {
                    float distance = Vector3.Distance(transform.position, rigidbody.transform.position);
                    float combinedRadius = collider.bounds.extents.magnitude + GetComponent<Collider>().bounds.extents.magnitude;

                    if (distance <= combinedRadius)
                    {
                        shouldVibrate = true;
                        break;
                    }
                }
            }
        }
        if (shouldVibrate)
        {
            OVRInput.SetControllerVibration(0.5f, 0.5f, OVRInput.Controller.RTouch);
            Debug.Log("Haptic feedback triggered!");
        } else
        {
            OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
        }
    }
}