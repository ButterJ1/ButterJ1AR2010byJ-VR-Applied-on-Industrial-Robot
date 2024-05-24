using UnityEngine;

public class ObjectController : MonoBehaviour
{
    public GameObject Anchor_point;
    public GameObject Anchor_point2;

    private bool positionLocked = false;
    private bool adjustmentLocked = false;

    void Update()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger) && !positionLocked && !adjustmentLocked)
        {
            GenerateObject(true);
        }

        if (OVRInput.GetDown(OVRInput.Button.SecondaryHandTrigger) && !positionLocked && !adjustmentLocked)
        {
            GenerateObject(false);
        }

        if (OVRInput.GetDown(OVRInput.Button.Three))
        {
            TogglePositionLock();
        }

        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) || OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
        {
            ToggleAdjustmentLock();
        }
    }

    private void GenerateObject(bool isLeft)
    {
        OVRPose objectPose = new OVRPose()
        {
            position = OVRInput.GetLocalControllerPosition(isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch),
            orientation = OVRInput.GetLocalControllerRotation(isLeft ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch)
        };

        OVRPose worldObjectPose = OVRExtensions.ToWorldSpacePose(objectPose);

        if (!isLeft)
        {
            Anchor_point.transform.position = worldObjectPose.position;
        }

        if (isLeft && !positionLocked)
        {
            Anchor_point2.transform.position = worldObjectPose.position;

            Vector3 lookDirection = Anchor_point2.transform.position - Anchor_point.transform.position;
            Quaternion rotateOne = Quaternion.LookRotation(lookDirection, Vector3.up);

            Quaternion additionalRotation = Quaternion.Euler(0f, 90f, 0f);
            Anchor_point.transform.rotation = rotateOne * additionalRotation;
        }
    }

    private void TogglePositionLock()
    {
        positionLocked = !positionLocked;
    }

    private void ToggleAdjustmentLock()
    {
        adjustmentLocked = !adjustmentLocked;
    }
}
