using UnityEngine;

public class ServerMessageController : MessageController
{
    [SerializeField] ClientSocket clientSocket;

    string serverDestinationKey = "Server", clientDestinationKey = "Client";
    private void Start()
    {
        clientSocket = GetComponent<ClientSocket>();
        ReceiveClientMessage(); //events
    }

    //Server receives client msg
    void ReceiveClientMessage()
    {
        ReceiveMessage(clientSocket, NameContainer.sendMessage, serverDestinationKey);
    }

    //This func sends this content "send message to client" to all clients
    void SendMessageToClients()
    {
        Client_Message client = new Client_Message();

        client.key = clientDestinationKey;
        client.value = "send message to client";
        string jsonString = JsonUtility.ToJson(client);
        clientSocket.socket.Emit(NameContainer.sendMessage, jsonString);
    }

    public void SendMessageToClients(string msg)
    {
        SendMessage(clientDestinationKey, clientSocket, NameContainer.sendMessage, msg);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            //Debug.Log("Send msg to all clients");
            SendMessageToClients("Msg to client with common function");
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SendMessageToClients();
        }
    }
}
