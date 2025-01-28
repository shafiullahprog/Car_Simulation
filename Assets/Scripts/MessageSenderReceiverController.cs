using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageSenderReceiverController : Check_ClientServerMode
{
    [SerializeField] StepController stepController;
    [SerializeField] UIManager uiManager;
    private void Start()
    {
        ClientSocket.OnSocketAssigned += FindMessageController;
        MessageController.OnMessageReceive += ActionOnReceivedMessage;
        stepController.OnProcessChange.AddListener(SendProgressUpdate);
       // FindMessageController();
    }
    
    void ActionOnReceivedMessage(string msg)
    {
        if (msg == CurrentProcess.Dismantle.ToString())
        {
            stepController.SetDismantleProcess();
        }
        if (msg == CurrentProcess.Assemble.ToString())
        {
            stepController.SetAssembleProcess();
        }
        if(msg == ActionStateSync.StartMenu)
        {
            Debug.Log("StartMenu");
            uiManager.StartMenu();
        }
        if(msg == ActionStateSync.Restart)
        {
            uiManager.ResetSteps();
        }
    }

    public void SendProgressUpdate(string currentProcess)
    {
        if (serverMessageController != null)
        {
            serverMessageController.SendMessageToClients(currentProcess);
        }
        else if (clientMessageController != null)
        {
            clientMessageController.SendMessageToServer(currentProcess);
        }
    }

    private void OnDestroy()
    { 
        MessageController.OnMessageReceive -= ActionOnReceivedMessage;
    }

}
