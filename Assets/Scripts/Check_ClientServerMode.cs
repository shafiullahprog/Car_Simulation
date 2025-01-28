using UnityEngine;

public class Check_ClientServerMode : MonoBehaviour
{
    protected ServerMessageController serverMessageController;
    protected ClientMessageController clientMessageController;
    public void FindMessageController(string socketType)
    {
        if (socketType == ActionStateSync.serverSocketType)
        {
            serverMessageController = FindObjectOfType<ServerMessageController>();
        }
        else
        {
            clientMessageController = FindObjectOfType<ClientMessageController>();
        }
    }
}
