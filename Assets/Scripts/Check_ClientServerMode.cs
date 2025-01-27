using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Check_ClientServerMode : MonoBehaviour
{
    public ServerMessageController serverMessageController;
    public ClientMessageController clientMessageController;
   
    public void FindMessageController(/*string socketType*/)
    {
        /*if (socketType == "server")*/
            serverMessageController = FindObjectOfType<ServerMessageController>();
        /*if (socketType == "client")*/
            clientMessageController = FindObjectOfType<ClientMessageController>();
    }

}
