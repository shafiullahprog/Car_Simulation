using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using System.IO;

class StreamingData
{
    public string ip;
    public string displayname;
}

public class ClientSocket : MonoBehaviour
{
    //Node js file name
    [Header("JS file path")]
    public string indexPath;

    public static string staticIP = "http://127.0.0.1:3000";
    string firstServerID = "";
    public string currentSocketType = "";
    string mySocketID = "";

    public SocketIOUnity socket;
    StreamingData streamingData; // in streaming asset baseurl IP should be of a server's IP (If you want to become a client)

    [SerializeField] GameObject clientPrefab, targetPosition;
    [SerializeField] TextMeshProUGUI textMeshProUGUI;

    public List<string> connectedClients = new List<string>();
    public List<GameObject> noofClients = new List<GameObject>();
    List<string> clientsDisplayName = new List<string>();

    public static Action<string> GetSocketType;
    public static Action<string> OnSocketAssigned;
    public static Action OnClientsDisconnected;

    #region RunServer
    string _dataPath = Application.streamingAssetsPath + "/ServerScript/";
    Process _buttonProcess;

    //Open node js using streaming asset for the server
    public string CommandProcess(string command, string workingDirectory)
    {
        ProcessStartInfo processInfo = new ProcessStartInfo();
        processInfo.FileName = Application.streamingAssetsPath + "\\nodeBin\\node.exe";
        processInfo.Arguments = _dataPath + indexPath;
        _buttonProcess = Process.Start(processInfo);
        return command;
    }
    #endregion

    #region Get Data from StreamingAssets/BaseUrl
    public string GetBaseURL()
    {
        string fullpath = Path.Combine(Application.streamingAssetsPath, "baseurl.txt");
        string dataToLoad = "";
        if (File.Exists(fullpath))
        {
            try
            {
                using FileStream stream = new FileStream(fullpath, FileMode.Open);
                using StreamReader reader = new StreamReader(stream);
                dataToLoad = reader.ReadToEnd().Trim();
            }
            catch (Exception e)
            {
                Debug.LogError("Error Occured. Path not Exist " + fullpath + "\n" + e);
            }
        }
        Debug.Log("data to load: "+ dataToLoad);
        return dataToLoad;
    }

    void StoreData()
    {
        streamingData = JsonUtility.FromJson<StreamingData>(GetBaseURL());
    }
    #endregion

    //Update client status when a client connects
    #region ClientStatus
    void ClientStatus()
    {
        socket.OnUnityThread(NameContainer.connectedClients, (response) =>
        {
            Debug.Log("Connected clients: " + response);
            connectedClients.Clear();
            string[] clientStatus = response.GetValue<string[]>();
            for (int i = 0; i < clientStatus.Length; i++)
            {
                Debug.Log("CLIENT NUM: " + clientStatus[i]);
                if (clientStatus[i] != mySocketID && !connectedClients.Contains(clientStatus[i]))
                {
                    connectedClients.Add(clientStatus[i]);
                }
            }
        });

        socket.OnUnityThread(NameContainer.displayname, (response) =>
        {
            RemoveClients();
            string[] clientStatus = response.GetValue<string[]>();
            for (int i = 0; i < clientStatus.Length; i++)
            {
                if (clientStatus[i] != streamingData.displayname && !clientsDisplayName.Contains(clientStatus[i]))
                {
                    clientsDisplayName.Add(clientStatus[i]);
                }
            }
            AddClient(clientsDisplayName);
        });
    }

    void AddClient(List<string> text)
    {
        if (clientPrefab != null && targetPosition != null)
        {
            for (int i = 0; i < text.Count; i++)
            {
                GameObject client = Instantiate(clientPrefab, targetPosition.transform);
                client.GetComponentInChildren<Text>().text = text[i];
                noofClients.Add(client);
            }
        }
    }

    void RemoveClients()
    {
        if (noofClients.Count > 0)
        {
            foreach (GameObject client in noofClients)
            {
                if (client != null)
                    Destroy(client);
            }
            noofClients.Clear();
            clientsDisplayName.Clear();
        }
    }
    #endregion

    private void Awake()
    {
        GetSocketType += SetMessageController;
        Initialize();
    }

    //Start the setup
    private void Initialize()
    {
        StoreData();
        StartServer();
        ConnectToServer();
        AssignType();
        ClientStatus();
        EventHandler();
    }

    //if staticIP is similar to my streamingdata IP then open nodejs
    void StartServer()
    {
        if (streamingData.ip == staticIP)
        {
            CommandProcess(indexPath, _dataPath);
            Debug.Log("Server open terminal");
        }
        else
        {
            Debug.Log("Client cannot open terminal");
        }
    }

    //connect to server
    void ConnectToServer()
    {
        var ip = new Uri(streamingData.ip);
        socket = new SocketIOUnity(ip);
        socket.Connect();
    }

    //if IP and staticIP are same, send msg to socket to make this device as Server
    void EventHandler()
    {
        socket.OnConnected += (sender, e) =>
        {
            Debug.Log("Connected");
            socket.Emit(NameContainer.displayname, streamingData.displayname);
            if(streamingData.ip == staticIP)
            {
                socket.Emit(NameContainer.firstServer);
            }
        };
    }

    void AssignType()
    {
        socket.OnUnityThread(NameContainer.connection, (response) =>
        {
            mySocketID = socket.Id;
            if (!string.IsNullOrEmpty(mySocketID))
                textMeshProUGUI.text = "My ID: " + mySocketID;
        });

        //Assign as a client or a server
        socket.OnUnityThread(NameContainer.socketType, (response) =>
        {
            string SocketType = response.GetValue<string>();
            Debug.Log("Assign as " + SocketType);
            GetSocketType?.Invoke(SocketType);
        });

        socket.OnUnityThread(NameContainer.firstServer, (response) =>
        {
            if (response.GetValue<string>() != "")
            {
                firstServerID = response.GetValue<string>();
            }
        });

        //Disconnect all the clients if a server disconnect
        socket.OnUnityThread(NameContainer.disconnectAllClients, (response)=> 
        {
            OnClientsDisconnected?.Invoke();
            Debug.Log("On server disconnected: "+ response);
        });
    }
    public string GetMySocketID()
    {
        return mySocketID;
    }

    //Based on the socket type add/update a script to send and recieve msg
    public void SetMessageController(string socketType)
    {
        if (socketType == NameContainer.serverSocketName)
        {
            currentSocketType = socketType;
            
            if (!GetComponent<ServerMessageController>())
            {
                this.AddComponent<ServerMessageController>();
            }
            if (GetComponent<ClientMessageController>())
            {
                socket.Off(NameContainer.sendMessage);
                Destroy(GetComponent<ClientMessageController>());
            }
            OnSocketAssigned?.Invoke(socketType);
        }
        else
        {
            currentSocketType = socketType;
            
            if (!GetComponent<ClientMessageController>())
            {
                this.AddComponent<ClientMessageController>();
            }
            if (GetComponent<ServerMessageController>())
            {
                socket.Off(NameContainer.sendMessage);
                Destroy(GetComponent<ServerMessageController>());
            }
            OnSocketAssigned?.Invoke(socketType);
        }
    }

    //Disconnect server in a sequence
    void DisconnectServer()
    {
        socket.Emit(NameContainer.removename, streamingData.displayname);
        Debug.Log("send disconnect message 1");
        socket.Disconnect();
        socket.Dispose();
        KillProcess();
    }

    //Kill node js
    void KillProcess()
    {
        if (streamingData.ip == staticIP)
        {
            Debug.Log("kill node js");
            if (_buttonProcess != null && !_buttonProcess.HasExited)
            {
                Debug.Log("kill node js 1");
                _buttonProcess.Kill();
                _buttonProcess.Dispose();
            }
        }
    }
    private void OnDisable()
    {
        GetSocketType -= SetMessageController;
    }
    private void OnApplicationQuit()
    {
        DisconnectServer();
    }
    
}
