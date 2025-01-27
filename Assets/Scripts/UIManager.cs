using UnityEngine;
using UnityEngine.UI;

public class UIManager : Check_ClientServerMode
{
    [SerializeField] StepController controller;
    [SerializeField] ClientSocket clientSocket;
    [SerializeField] GameObject StartProcess, EndProcess;
    [SerializeField] GameObject TopLeftMenu;

    [SerializeField] Button startProcess;

    private void Start()
    {
        Invoke("FindMessageController",1f);
        controller.OnFinalStepReached.AddListener( () =>
        {
            ProcessComplete(true);
        });
        startProcess.onClick.AddListener(StartMenu);
    }

    public void StartMenu()
    {
        StartProcess.SetActive(false);
        TopLeftMenu.SetActive(true);
        controller.SkipStep();
        //SendStatus();
    }

    public void SendStartGameStatus()
    {
        if (clientMessageController != null)
        {
            Debug.Log("Send msg to server");
            clientMessageController.SendMessageToServer(ActionStateSync.StartMenu);
        }
        if (serverMessageController != null)
        {
            Debug.Log("Send msg to client");
            serverMessageController.SendMessageToClients(ActionStateSync.StartMenu);
        }
    }

    public void ProcessComplete(bool value)
    {
        EndProcess.SetActive(value);
    }
}
