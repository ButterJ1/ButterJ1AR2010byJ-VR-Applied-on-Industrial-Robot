using UnityEngine;

public class HelloWorldScript : MonoBehaviour
{
    private bool isReady = false;

    public bool GetisReady()
    {
        return isReady;
    }

    public void SetIsReady(bool value)
    {
        isReady = value;
    }

    public void SetHelloWorldText()
    {
        isReady = true;
    }
}
