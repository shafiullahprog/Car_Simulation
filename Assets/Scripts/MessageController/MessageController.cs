using System;
using UnityEngine;
using UnityEngine.UI;

public class MessageController : MonoBehaviour
{
    public static Action<string> OnMessageReceive;
    string message;

    public void SendMessage(string destinationKey,ClientSocket clientSocket,string messageDestination, string message)
    {
        Client_Message client = new Client_Message();

        client.key = destinationKey;
        client.value = message;
        if (clientSocket.noofClients != null)
        {
            string jsonString = JsonUtility.ToJson(client);
            clientSocket.socket.Emit(messageDestination, jsonString);
        }
    }

    public void ReceiveMessage(ClientSocket clientSocket, string messageDestination, string myIdentity)
    {
        clientSocket.socket.OnUnityThread(messageDestination, (response) =>
        {
            string s = response.GetValue<string>();
            Client_Message client = JsonUtility.FromJson<Client_Message>(s);
           
            if (client.key == myIdentity)
            {
                Debug.Log("Msg received: " + s);
                OnMessageReceive?.Invoke(client.value);
                message = client.value;
            }
        });
    }

    public string GetURL()
    {
        if (message != "")
        {
            return message;
        }
        return "";
    }
}
