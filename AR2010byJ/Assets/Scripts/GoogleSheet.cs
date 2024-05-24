using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class GoogleSheet : MonoBehaviour
{
    private VRDrawLine vrDrawLine;
    private bool hasUploaded = false;

    public TextMeshPro feedBack;

    public void IsItReady(bool value)
    {
        hasUploaded = value;
    }

    void Start()
    {
        vrDrawLine = GetComponent<VRDrawLine>();
    }

    void Update()
    {        
        if (!hasUploaded)
        {
            StringBuilder formData = new StringBuilder();

            formData.Append("Hello World!");
            formData.Append("\n");
            formData.Append("Hello World!");
            formData.Append("\n");
            formData.Append("Hello World!");

            WWWForm form = new WWWForm();
            form.AddField("formData", formData.ToString());

            Debug.Log("Formatted Form Data: " + formData.ToString());

            StartCoroutine(Upload(form));
            hasUploaded = true;
        }
        
    }

   IEnumerator Upload(WWWForm form)
   {
       using (UnityWebRequest www = UnityWebRequest.Post("https://script.google.com/macros/s/AKfycbziZSAvYJAg1oQ4KFuWznLmDY0B-NV4Sg3TThgVfKXLmm5tArIIpolkf1GGacqDj30Jxg/exec", form))
       {
           yield return www.SendWebRequest();

           if (www.isNetworkError || www.isHttpError)
           {
               Debug.Log(www.error);
           } else
           {
               Debug.Log("Form upload complete!");
               feedBack.text = "Form upload complete!";
           }
       }
   }
}