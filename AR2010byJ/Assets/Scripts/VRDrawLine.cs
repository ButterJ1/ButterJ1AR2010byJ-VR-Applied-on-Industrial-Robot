using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VRDrawLine : MonoBehaviour
{
    public LineRenderer importedLineRenderer;
    public float followSpeed = 0.5f;
    public TextMeshProUGUI statusText;
    public TextMeshPro statusTextee;

    private bool shouldFollowLine = false;
    private List<Vector3> selectedPoints = new List<Vector3>();

    private LineRenderer currentLineRenderer;
    private List<Vector3> currentLinePoints = new List<Vector3>();
    private List<Vector3> importedLinePoints = new List<Vector3>();

    private float cosineSimilarity;
    private float euclideanDistance;

    //Diffrenent
    private bool isDrawingMode = false;
    private bool statusD = false;
    private bool press = false;
    private bool trigger = false;

    //button.two
    public string targetTag1 = "Tag1";
    public string targetTag2 = "Tag2"; 

    private MeshRenderer meshRenderer;
    private bool isMeshRendererEnabled = false;
    private int buttonPressCount = 0;
    private int link = 0;

    //Outside
    private TouchFeedback TouchFeedBack;
    private ColliderDetection ColiiderDectection;
    private HelloWorldScript HelloWorldScript;
    private GoogleSheet GoogleSheet;

    public GameObject followingObjectPrefab;
    private GameObject followingObject;
    private GameObject previousFollowingObject;

    public List<Vector3> GetSelectedPoints()
    {
        return selectedPoints;
    }

    public List<Vector3> GetImportedLinePoints()
    {
        return importedLinePoints;
    }

    public float GetCosineSimilarity()
    {
        return cosineSimilarity;
    }

    public float GetEuclideanDistance()
    {
        return euclideanDistance;
    }

    public bool GetPress()
    {
        return press;
    }


    void Start()
    {
        currentLineRenderer = gameObject.AddComponent<LineRenderer>();
        Material lineMaterial = new Material(Shader.Find("Legacy Shaders/Particles/Alpha Blended Premultiply"));

        currentLineRenderer.material = lineMaterial;
        currentLineRenderer.startWidth = 0.01f;
        currentLineRenderer.endWidth = 0.01f;

        if (importedLineRenderer != null)
        {
            importedLinePoints.Clear();

            for (int i = 0; i < importedLineRenderer.positionCount; i++)
            {
                Vector3 point;

                if (importedLineRenderer.useWorldSpace)
                {
                    point = currentLineRenderer.transform.InverseTransformPoint(importedLineRenderer.GetPosition(i));
                } else
                {
                    point = importedLineRenderer.GetPosition(i);
                }

                importedLinePoints.Add(point);
            }
        }

        statusText.text = "";

        meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            Debug.LogError("MeshRenderer component not found on the GameObject.");
            enabled = false;
        }

        TouchFeedBack = GetComponent<TouchFeedback>();
        ColiiderDectection = GetComponent<ColliderDetection>();
        HelloWorldScript = GetComponent<HelloWorldScript>();
        GoogleSheet = GetComponent<GoogleSheet>();

        if (previousFollowingObject != null)
        {
            previousFollowingObject.SetActive(false);
        }

    }

    void Update()
    {
        press = HelloWorldScript.GetisReady();

        if (link == 1)
        {
            if (OVRInput.GetDown(OVRInput.Button.Two))
            {
                ResetDrawing();
                trigger = false;
                link = 2;
            } else
            {
                statusText.text = "Please Press Button Two";
            }
        }

        if ((!ColiiderDectection.GetShouldVibrate() && !TouchFeedBack.GetShouldVibrate_Internal()) || (!trigger && link == 2))
        {
            if ((!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && !OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger)) || (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger)))
            {
                UpdateStatusText("Please press PrimaryIndexTrigger to start your experience!");
            }
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && !OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                if (isDrawingMode)
                {
                    if (shouldFollowLine)
                    {
                        if (press)
                        {
                            GoogleSheet.IsItReady(false);
                            UpdateStatusText("Following~");
                            FollowLine();
                        } else
                        {
                            UpdateStatusText("Please Press the Start Button");
                        }
                    } else
                    {
                        if (press)
                        {
                            if (shouldFollowLine)
                            {
                                UpdateStatusText("Following~");
                            } else {
                                StartCoroutine(UpdateStatusAfterDelay("Please Check your Data Sheets", 5f));
                            }
                        } else {
                            UpdateStatusText("Please Press the Start Button");
                        }
                    }
                } else
                {
                    UpdateStatusText("To Start Drawing Please Press SecondaryIndexTrigger!");
                }
            }
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger))
            {
                StartDrawingLine();
                //CreateFollowingObject();
            }
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                StopDrawingLine();
            }

            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && !statusD)
            {
                UpdateLine();
                UpdateFollowingObjectPosition();
                isDrawingMode = true;
                UpdateStatusText("Drawing~");
            }

            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.GetUp(OVRInput.Button.SecondaryIndexTrigger))
            {
                CompareLines();
            }

            if (!OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger))
            {
                if (isDrawingMode)
                {
                    StartCoroutine(UpdateStatusAfterDelay("Please Start Over Again!", 5f));
                } else
                {
                    UpdateStatusText("To Start Drawing Please Press PrimaryIndexTrigger!");
                }
            }
        } else if ((ColiiderDectection.GetShouldVibrate() || TouchFeedBack.GetShouldVibrate_Internal()) && link == 0)
        {
            shouldFollowLine = false;
            statusText.text = "Touching the barier";
            isDrawingMode = false;
            statusD = false;
            trigger = true;
            link = 1;
        }

        if (OVRInput.GetDown(OVRInput.Button.Four))
        {
            buttonPressCount++;

            if (buttonPressCount % 2 == 1)
            {
                isMeshRendererEnabled = !isMeshRendererEnabled;
                ToggleMeshRenderersWithTag(targetTag1);
                ToggleMeshRenderersWithTag(targetTag2);
            }
        }
    }

    void UpdateStatusText(string message)
    {
        statusText.text = message;
    }

    IEnumerator UpdateStatusAfterDelay(string message, float delay)
    {
        float elapsedTime = 0f;

        statusD = true;

        while (elapsedTime < delay && isDrawingMode)
        {
            if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger) && !OVRInput.Get(OVRInput.Button.SecondaryIndexTrigger) && isDrawingMode)
            {
                elapsedTime += Time.deltaTime;
                UpdateStatusText(message);

                if (elapsedTime >= delay)
                {
                    isDrawingMode = false;
                    statusD = false;
                    press = false;
                    yield break;
                }
            } else
            {
                UpdateStatusText("Interaction detected.");
            }
            yield return null;
        }
        isDrawingMode = false;
        statusD = false;
    }
 
    void FollowLine()
    {
        statusD = true;
        if (currentLinePoints.Count > 0)
        {
            transform.position = Vector3.MoveTowards(transform.position, currentLinePoints[0], followSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, currentLinePoints[0]) < 0.1f)
            {
                currentLinePoints.RemoveAt(0);
            }

            if (currentLinePoints.Count == 0)
            {
                shouldFollowLine = false;
                statusD = false;
                HelloWorldScript.SetIsReady(false);
                link = 0;
            }
        }
    }

    void StartDrawingLine()
    {
        shouldFollowLine = false;
        currentLinePoints.Clear();
    }

    void StopDrawingLine()
    {
        shouldFollowLine = true;
    }

    void UpdateLine()
    {
        Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Vector3 controllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward;
        Vector3 spawnPosition = controllerPosition + controllerDirection * 0.05f;

        currentLinePoints.Add(spawnPosition);
        UpdateLineRenderer(currentLineRenderer, currentLinePoints);
    }

    void UpdateLineRenderer(LineRenderer lineRenderer, List<Vector3> linePoints)
    {
        lineRenderer.positionCount = linePoints.Count;
        lineRenderer.SetPositions(linePoints.ToArray());
    }

    void UpdateFollowingObjectPosition()
    {
        if (followingObject != null)
        {
            Vector3 controllerPosition = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Vector3 controllerDirection = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch) * Vector3.forward;
            Vector3 triggerFrontPosition = controllerPosition + controllerDirection * 0.05f;

            followingObject.transform.position = triggerFrontPosition;
        }
    }

    void ToggleMeshRenderersWithTag(string tag)
    {
        GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(tag);

        foreach (GameObject obj in objectsWithTag)
        {
            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();

            if (renderer != null)
            {
                renderer.enabled = isMeshRendererEnabled;
            }
        }
    }

    
    void CompareLines()
    {
        List<Vector3> tempSelectedPoints = new List<Vector3>(); // Rename the local variable

        for (int i = 0; i < importedLinePoints.Count; i++)
        {
            Vector3 importedPoint = importedLinePoints[i];
            Vector3 closestPoint = FindClosestPoint(importedPoint, currentLinePoints);
            tempSelectedPoints.Add(closestPoint);
        }

        selectedPoints = tempSelectedPoints;

        cosineSimilarity = CalculateCosineSimilarity(selectedPoints, importedLinePoints);
        euclideanDistance = CalculateEuclideanDistance(selectedPoints, importedLinePoints);

        statusTextee.text = "Cosine Similarity: " + cosineSimilarity;
        statusTextee.text = "Euclidean Distance: " + euclideanDistance;
    }

    Vector3 FindClosestPoint(Vector3 targetPoint, List<Vector3> points)
    {
        Vector3 closestPoint = Vector3.zero;
        float minDistance = float.MaxValue;

        foreach (Vector3 point in points)
        {
            float distance = Vector3.Distance(targetPoint, point);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
    }

    float CalculateCosineSimilarity(List<Vector3> vectorA, List<Vector3> vectorB)
    {
        float dotProduct = 0;
        float magnitudeA = 0;
        float magnitudeB = 0;

        for (int i = 0; i < vectorA.Count; i++)
        {
            dotProduct += vectorA[i].x * vectorB[i].x + vectorA[i].y * vectorB[i].y + vectorA[i].z * vectorB[i].z;
            magnitudeA += Mathf.Pow(vectorA[i].x, 2) + Mathf.Pow(vectorA[i].y, 2) + Mathf.Pow(vectorA[i].z, 2);
            magnitudeB += Mathf.Pow(vectorB[i].x, 2) + Mathf.Pow(vectorB[i].y, 2) + Mathf.Pow(vectorB[i].z, 2);
        }

        magnitudeA = Mathf.Sqrt(magnitudeA);
        magnitudeB = Mathf.Sqrt(magnitudeB);

        if (magnitudeA == 0 || magnitudeB == 0)
            return 0; // Avoid division by zero

        return dotProduct / (magnitudeA * magnitudeB);
    }

    float CalculateEuclideanDistance(List<Vector3> vectorA, List<Vector3> vectorB)
    {
        float sum = 0;

        for (int i = 0; i < vectorA.Count; i++)
        {
            sum += Mathf.Pow(vectorA[i].x - vectorB[i].x, 2) + Mathf.Pow(vectorA[i].y - vectorB[i].y, 2) + Mathf.Pow(vectorA[i].z - vectorB[i].z, 2);
        }

        return Mathf.Sqrt(sum);
    }
    
    void ResetDrawing()
    {
        shouldFollowLine = false;
        currentLinePoints.Clear();
        statusText.text = "";
        currentLineRenderer.positionCount = 0;
    }
}
