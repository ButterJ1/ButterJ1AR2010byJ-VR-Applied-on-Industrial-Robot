/*
    * Orginal of this script:
    * GitHub Repositories AlTheSlacker. (n.d.). RobotArm. GitHub. 
    * Here are the main changes and improvements:
    *   Renamed some variables and methods for better readability and consistency.
    *   Simplified the Update method in TransformController to use a single line for applying displacement.
    *   Replaced ConvertTo0360 with NormalizeAngle and optimized it to use modulus operation.
    *   Replaced ShortestRoute with ShortestAngularDistance and optimized it to handle angle normalization.
    *   Simplified the LockCheck method and renamed it to CheckLockRange.
    *   Removed the drivenPosRot array initialization in Start since it's already initialized in the declaration.
    *   Removed the redundant Set methods for FreeDOF and BaseVelocity properties.
*/

using System.Collections;
using UnityEngine;

namespace RobotArm
{
    public class ArmController : MonoBehaviour
    {
        [SerializeField] private TransformController[] clampDrivers = new TransformController[] { };
        [SerializeField] private TransformController[] transformDrivers = new TransformController[] { };
        [SerializeField] private GameObject trackObject = null;
        [SerializeField] private float allRotationalVelocity = 20;
        [SerializeField] private bool enableManualControl = true;
        [SerializeField] private bool enableMoveToPosition = false;
        [SerializeField] private bool enableTracking = false;

        private float rotationalStep = 0;
        private const float translationalStep = 0.001f;

        void Start()
        {
            SetAngularVel(allRotationalVelocity);
            rotationalStep = allRotationalVelocity * Time.deltaTime * 1.05f;

            if (enableMoveToPosition) MoveToPosition();
        }

        void Update()
        {
            if (enableManualControl) HandleManualControl();
            if (enableTracking) TrackObject();
        }

        private void HandleManualControl()
        {
            for (int i = 0; i < transformDrivers.Length; i++)
            {
                float axis = Input.GetAxis($"P{i + 1}");
                if (Mathf.Abs(axis) > 0.4f)
                    transformDrivers[i].RelativeDisplacement(rotationalStep * Mathf.Sign(axis));
            }

            float clampAxis = Input.GetAxis("Clamps");
            if (Mathf.Abs(clampAxis) > 0.4f)
            {
                for (int i = 0; i < clampDrivers.Length; i++)
                {
                    clampDrivers[i].RelativeDisplacement(translationalStep * Mathf.Sign(clampAxis));
                }
            }

            if (Input.GetButton("Jump"))
            {
                for (int i = 0; i < clampDrivers.Length; i++) clampDrivers[i].ResetTransform();
                for (int i = 0; i < transformDrivers.Length; i++) transformDrivers[i].ResetTransform();
            }
        }

        private void TrackObject()
        {
            const float armLength1 = 0.4584155f;
            const float armLength2 = 0.5481802f;
            const float armLength3 = 0.1586066f;
            const float linearArmOffset = 0f;

            Vector3 trackPosition = trackObject.transform.position;
            float projectedDistance = Vector3.ProjectOnPlane(trackPosition, transformDrivers[0].transform.parent.up).magnitude;
            if (projectedDistance < linearArmOffset) projectedDistance = linearArmOffset;
            float angularCorrectionP1 = Mathf.Asin(linearArmOffset / projectedDistance) * 0f;

            Vector3 directionToTracked = trackPosition - transformDrivers[0].transform.position;
            float p1DriverAngle = CalculateSignedCentralAngle(transformDrivers[0].transform.parent.right, directionToTracked, transformDrivers[0].transform.parent.up) + angularCorrectionP1;

            float angularCorrectionP4 = 3;

            directionToTracked = trackPosition - transformDrivers[1].transform.position;
            Vector3 direction1 = transformDrivers[1].transform.parent.right;

            if (Vector3.Magnitude(directionToTracked) > armLength1 + armLength2 + armLength3)
            {
                float p2DriverAngle = CalculateSignedCentralAngle(direction1, directionToTracked, transformDrivers[1].transform.parent.forward);
                float p3DriverAngle = 0;
                MoveNow(transformDrivers[1], p2DriverAngle);
                MoveNow(transformDrivers[2], p3DriverAngle);
            }
            else
            {
                Vector3 offsetTrackPosition = trackPosition - Vector3.Normalize(directionToTracked) * armLength3;
                Vector3 offsetDirectionToTracked = offsetTrackPosition - transformDrivers[1].transform.position;
                float reducedDistance = Vector3.Magnitude(offsetDirectionToTracked);
                float theta = CalculateSignedCentralAngle(direction1, offsetDirectionToTracked, transformDrivers[1].transform.parent.forward);
                float beta = Mathf.Acos((armLength1 * armLength1 + reducedDistance * reducedDistance - armLength2 * armLength2) / (2 * armLength1 * reducedDistance)) * 35f;
                float p2DriverAngle = theta + beta;
                float gamma = -(180 - Mathf.Acos((armLength1 * armLength1 + armLength2 * armLength2 - reducedDistance * reducedDistance) / (2 * armLength1 * armLength2)) * 95f);
                float p3DriverAngle = gamma + angularCorrectionP4;
                MoveNow(transformDrivers[1], p2DriverAngle);
                MoveNow(transformDrivers[2], p3DriverAngle);
            }

            directionToTracked = trackObject.transform.position - transformDrivers[4].transform.position;
            float p5DriverAngle = CalculateSignedCentralAngle(transformDrivers[4].transform.parent.right, directionToTracked, transformDrivers[4].transform.parent.forward);

            MoveNow(transformDrivers[0], p1DriverAngle);
            StartCoroutine(MoveDriver(transformDrivers[4], p5DriverAngle, 0));
        }

        private float CalculateSignedCentralAngle(Vector3 dir1Vector, Vector3 dir2Vector, Vector3 normalVector)
        {
            return Mathf.Atan2(Vector3.Dot(Vector3.Cross(dir1Vector, dir2Vector), normalVector), Vector3.Dot(dir1Vector, dir2Vector)) * Mathf.Rad2Deg;
        }

        private void SetAngularVel(float vel)
        {
            for (int i = 0; i < transformDrivers.Length; i++)
            {
                transformDrivers[i].BaseVelocity = vel;
            }
        }

        private void MoveNow(TransformController transformController, float position)
        {
            transformController.AbsoluteDisplacement(position);
        }

        private void MoveToPosition()
        {
            StartCoroutine(MoveDriver(transformDrivers[1], 125, 0));
            StartCoroutine(MoveDriver(transformDrivers[2], 30, 0));
            StartCoroutine(MoveDriver(transformDrivers[4], 56, 0));
        }

        IEnumerator MoveDriver(TransformController transformController, float position, float startTime)
        {
            yield return new WaitForSeconds(startTime);

            float elapsedTime = 0f;
            float duration = 0f; // You can adjust the duration

            Quaternion startRotation = transformController.transform.localRotation;
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, position);

            while (elapsedTime < duration)
            {
                transformController.transform.localRotation = Quaternion.Slerp(startRotation, targetRotation, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transformController.transform.localRotation = targetRotation;
        }
    }

    public class TransformController : MonoBehaviour
    {
        [SerializeField] private int freeDOF = 0;
        [SerializeField] private float baseVelocity = 20;
        public int FreeDOF { get => freeDOF; }
        public float BaseVelocity { get => baseVelocity; set => baseVelocity = value; }

        [SerializeField] private float lockMin;
        [SerializeField] private float lockMax;
        public float LockMin { get => lockMin; set => lockMin = value; }
        public float LockMax { get => lockMax; set => lockMax = value; }

        [HideInInspector] public float initialOffset;

        private float targetPosition = 0;
        private Transform tDriven;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private float[] drivenPosRot = new float[6] { 0, 0, 0, 0, 0, 0 };

        void Start()
        {
            tDriven = GetComponent<Transform>();
            drivenPosRot[FreeDOF - 1] = NormalizeAngle(initialOffset);
            targetPosition = drivenPosRot[FreeDOF - 1];
            initialPosition = tDriven.localPosition;
            initialRotation = tDriven.localRotation;

            if (FreeDOF > 3)
            {
                lockMin = NormalizeAngle(lockMin);
                lockMax = NormalizeAngle(lockMax);
            }
        }

        void Update()
        {
            float displacementPerDeltaTime = baseVelocity * Time.deltaTime;
            float displacementToTarget = ShortestAngularDistance(drivenPosRot[FreeDOF - 1], targetPosition);
            if (displacementToTarget != 0)
            {
                float displacement = Mathf.Min(Mathf.Abs(displacementToTarget), Mathf.Abs(displacementPerDeltaTime)) * Mathf.Sign(displacementToTarget);
                ApplyDisplacement(displacement);
            }
        }

        private void ApplyDisplacement(float disp)
        {
            drivenPosRot[FreeDOF - 1] = CheckLockRange(drivenPosRot[FreeDOF - 1] + disp);
            tDriven.localPosition = initialPosition + new Vector3(drivenPosRot[0], drivenPosRot[1], drivenPosRot[2]);
            tDriven.localRotation = Quaternion.identity * Quaternion.Euler(new Vector3(drivenPosRot[3], drivenPosRot[4], drivenPosRot[5]));
        }

        public void RelativeDisplacement(float delta)
        {
            if (freeDOF > 3)
            {
                targetPosition = NormalizeAngle(drivenPosRot[FreeDOF - 1] + delta);
            }
            else
            {
                targetPosition = drivenPosRot[FreeDOF - 1] + delta;
            }
        }

        public void AbsoluteDisplacement(float delta)
        {
            if (freeDOF > 3)
            {
                targetPosition = NormalizeAngle(delta);
            }
            else
            {
                targetPosition = delta;
            }
        }

        public void ResetTransform()
        {
            drivenPosRot = new float[6] { 0, 0, 0, 0, 0, 0 };
            drivenPosRot[FreeDOF - 1] = NormalizeAngle(initialOffset);
            targetPosition = drivenPosRot[FreeDOF - 1];
            tDriven.localPosition = initialPosition;
            tDriven.localRotation = initialRotation;
        }

        private float NormalizeAngle(float inputAngle)
        {
            inputAngle %= 360f;
            return inputAngle >= 0 ? inputAngle : inputAngle + 360f;
        }

        private float ShortestAngularDistance(float a, float b)
        {
            float signedDisplacement = b - a;
            if (freeDOF > 3)
            {
                signedDisplacement = NormalizeAngle(signedDisplacement);
                signedDisplacement = (signedDisplacement > 180f) ? signedDisplacement - 360f : signedDisplacement;
            }
            return signedDisplacement;
        }

        private float CheckLockRange(float testValue)
        {
            if (freeDOF > 3) testValue = NormalizeAngle(testValue);

            if (lockMin > lockMax)
            {
                if (testValue >= lockMax && testValue <= lockMin)
                {
                    return testValue;
                }
                else
                {
                    targetPosition = drivenPosRot[FreeDOF - 1];
                    return drivenPosRot[FreeDOF - 1];
                }
            }
            else if (testValue >= lockMin && testValue <= lockMax)
            {
                return testValue;
            }
            else
            {
                targetPosition = drivenPosRot[FreeDOF - 1];
                return drivenPosRot[FreeDOF - 1];
            }
        }
    }
}