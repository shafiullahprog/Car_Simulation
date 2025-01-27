using UnityEngine;

public class ClientMessageController : MessageController
{
    [SerializeField] ClientSocket clientSocket;
    string myIdentity = "Client", serverDestinationKey = "Server";

    private void Start()
    {
        clientSocket = GetComponent<ClientSocket>();
        ReceiveServerMessage();
    }

    //This receive a massage only from server
    public void ReceiveServerMessage()
    {
        ReceiveMessage(clientSocket, NameContainer.sendMessage, myIdentity);
    }

    //Client send msg to other clients
    public void SendMessageToServer(string msg)
    {
        Debug.Log("to client");
        SendMessage(serverDestinationKey, clientSocket, NameContainer.sendMessage, msg);
    }
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            SendMessageToServer("Message to server");
        }
    }
}