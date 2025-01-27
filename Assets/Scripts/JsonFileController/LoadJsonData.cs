using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class DataInfo
{
    public int id;
    public string Domain_name;
    public string Sub_Domain_name;
    public string Technology_name;
    public string Asset_type;
    public string Content_name;
    public string Content_path;
}

[System.Serializable]
public class DataListWrapper
{
    public string success;
    public List<DataInfo> data;
}

public class LoadJsonData : MonoBehaviour
{
    [SerializeField] string URL;
    [SerializeField] GameObject parentObject;
    [SerializeField] GameObject prefab;

    public List<DataInfo> dataInfo = new List<DataInfo>();
    void Awake()
    {
        //StartCoroutine(GetJsonData(URL));
    }
    IEnumerator GetJsonData(string Url)
    {
        using (UnityWebRequest www = UnityWebRequest.Get(Url))
        {
            yield return www.SendWebRequest();
            if(www.result != UnityWebRequest.Result.Success) 
            {
                Debug.Log("Error: "+ www.error);
            }
            string jsonString = www.downloadHandler.text;
            StoreData(jsonString);
        }
    } 

    public void StoreData(string jsonString)
    {
        DataListWrapper wrapper = JsonUtility.FromJson<DataListWrapper>(jsonString);
        dataInfo = wrapper.data;
        Debug.Log("JSon loaded");

        PopulateList();
    }

    public void PopulateList()
    {
        foreach (DataInfo dataInfo in dataInfo)
        {
            GameObject listElement = Instantiate(prefab, parentObject.transform);
            listElement.GetComponentInChildren<Text>().text = dataInfo.Asset_type;
            SendJsonDataToClient(listElement, dataInfo.Content_path);
        }
    }
    void SendJsonDataToClient(GameObject gameObject, string message)
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() =>
        {
            ServerMessageController serverMessageController = FindObjectOfType<ServerMessageController>(true);

            /*if (serverMessageController != null)
                serverMessageController.SendMessageToSelectedClient(message);*/
        });
    }
}
