using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SetupUI : MonoBehaviour
{
    [SerializeField] GameObject JsonDataList, loadJsonFileData;

    //Action to invoke for as server/client assigned
    private void Awake()
    {
        ClientSocket.OnSocketAssigned += OnDataLoaded;
    }

    private void OnDisable()
    {
        ClientSocket.OnSocketAssigned -= OnDataLoaded;
    }

    //UI on off controller based on the type of this socket types(client/server)
    void OnDataLoaded(string socketType)
    {
        if (socketType == NameContainer.serverSocketName)
        {
            //setup server
            OnOffSystem(true, false);
        }
        else
        {
            //setup client
            OnOffSystem(false, true);
        }
    }
    public void OnOffSystem(bool a, bool b)
    {
        if (JsonDataList != null && loadJsonFileData != null)
        {
            JsonDataList.SetActive(a);
            loadJsonFileData.SetActive(b);
        }
    }
}
