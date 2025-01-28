using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : Check_ClientServerMode
{
    [SerializeField] StepController controller;
    [SerializeField] GameObject StartProcess, EndProcess;
    [SerializeField] GameObject TopLeftMenu;

    [SerializeField] Button startProcess;

    private void Start()
    {
        controller.OnFinalStepReached.AddListener(() =>
        {
            ProcessComplete(true);
        });
        startProcess.onClick.AddListener(StartMenu);
        ClientSocket.OnSocketAssigned += FindMessageController;
    }

    public void StartMenu()
    {
        StartProcess.SetActive(false);
        TopLeftMenu.SetActive(true);
        controller.SkipStep();
    }

    public void SendStartGameStatus(string message)
    {
        if (clientMessageController != null)
        {
            //Debug.Log("Send msg to server");
            clientMessageController.SendMessageToServer(message);
        }
        if (serverMessageController != null)
        {
            //Debug.Log("Send msg to client");
            serverMessageController.SendMessageToClients(message);
        }
    }

    public void ProcessComplete(bool value)
    {
        EndProcess.SetActive(value);
    }

    public void ResetSteps()
    {
        SceneManager.LoadScene(0);
    }
}
