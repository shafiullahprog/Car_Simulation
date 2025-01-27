using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class Step
{
    public Transform objectToAnimate;
    public Animator objAnimator;
    public string AnimTrigger;

    public List<Transform> objectToEnable;
    public List<Transform> objectToDisable;
    public List<Outline> objectToHighlight;
}

public class StepController : ProcessController
{
    ServerMessageController serverMessageController;
    ClientMessageController clientMessageController;

    [Header("Steps")]
    [SerializeField] List<Step> dismantleSteps = new List<Step>();
    [SerializeField] List<Step> assemleSteps = new List<Step>();
     
    public UnityAction<int, bool> HighlightNextObjectEvent;
    public UnityAction<int, bool> DisablePreviousHighlightEvent;


    public UnityEvent OnAssemblyStep, OnDismantleStep;
    private void Start()
    {
        ClientMessageController.OnMessageReceive += ActionOnReceivedMessage;
        ServerMessageController.OnMessageReceive += ActionOnReceivedMessage;     
        ClientSocket.OnSocketAssigned += FindMessageController;
        HighlightNextObjectEvent += HighlightObjController;
        DisablePreviousHighlightEvent += HighlightObjController;
        SelectStepList(dismantleSteps);
    } 

    void FindMessageController(string socketType)
    {
        if(socketType == "server")
            serverMessageController = FindObjectOfType<ServerMessageController>();
        if(socketType == "client")
            clientMessageController = FindObjectOfType<ClientMessageController>();
    }

    void ActionOnReceivedMessage(string msg)
    {
        if(msg == "Dismantle")
        {
            SetDismantleProcess();
        }
        else if(msg == "Assemble")
        {
            SetAssembleProcess();
        }
    }

    public void SendProgressUpdate()
    {
        if (serverMessageController != null)
        {
            serverMessageController.SendMessageToClients(currentProcess.ToString());
        }
        else if (clientMessageController != null)
        {
            clientMessageController.SendMessageToServer(currentProcess.ToString());
        }
    }
    public void HighlightObjController(int stepIndex, bool val)
    {
        List<Outline> currentOutLineObj = currentSteps[stepIndex].objectToHighlight;
        foreach(Outline outline in currentOutLineObj)
        {
            outline.GetComponent<Outline>().enabled = val;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectObjectUnderMouse();
        }
    }
    private void DetectObjectUnderMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100))
        {
            MonitorSteps(hit.transform);
        }
    }
    private void MonitorSteps(Transform hitPart)
    {

        if (currentSteps.Count > 0)
        {    
            if (currentStepIndex < currentSteps.Count && hitPart == currentSteps[currentStepIndex].objectToAnimate)
            {
                DisablePreviousHighlightEvent?.Invoke(currentStepIndex - 1, false);
                HighlightNextObjectEvent?.Invoke(currentStepIndex, true);
                Step step = currentSteps[currentStepIndex];
                AnimationSteps(step.objAnimator, step.AnimTrigger);
                HandleCurrentStepsObjectActivation(currentSteps);
                currentStepIndex++;
            }
            else
            {
                Debug.Log("Wrong step");
            }
        }
        else
        {
            Debug.Log("No Steps found");
        }
    }
    private void AnimationSteps(Animator objToAnimate, string triggerName)
    {
        objToAnimate.SetTrigger(triggerName);
    } 
    protected override void SetAssembleProcess()
    {
        currentStepIndex = 0;
        UpdateAnimationForCurrentProcess();
        base.SetAssembleProcess();
        SelectStepList(assemleSteps);
        SkipStep();

        OnAssemblyStep?.Invoke();
    }
    protected override void SetDismantleProcess()
    {
        currentStepIndex = 0;
        UpdateAnimationForCurrentProcess();
        base.SetDismantleProcess();
        SelectStepList(dismantleSteps);
        SkipStep();

        OnDismantleStep?.Invoke();
    }
    protected override void SelectStepList(List<Step> process)
    {
        base.SelectStepList(process);
        SetInitialStateOfObjects(process);
    }
    private void UpdateAnimationForCurrentProcess()
    {
        foreach (var step in currentSteps)
        {
            
            if (step.objAnimator != null)
            {
                Debug.Log("Step name: " + step.objectToAnimate.name);
                string triggerName = step.AnimTrigger;
                step.objAnimator.ResetTrigger(triggerName);
            }
        }
    }
    private void HandleCurrentStepsObjectActivation(List<Step> currentSteps)
    {
        if (currentProcess == CurrentProcess.Dismantle)
        {
            if (currentStepIndex > 0)
            {
                foreach (Transform obj in currentSteps[currentStepIndex - 1].objectToDisable)
                {
                    if (obj != null)
                        obj.gameObject.SetActive(false);
                }
            }

            foreach (Transform obj in currentSteps[currentStepIndex].objectToEnable)
            {
                if (obj != null)
                    obj.gameObject.SetActive(true);
            }

            /*foreach(Outline outline in currentSteps[currentStepIndex].outlines)
           {
               if (outline != null)
               {
                   outline.GetComponent<Outline>().enabled = tru;
               }
           }*/
        }
        else if (currentProcess == CurrentProcess.Assemble)
        {
            if (currentStepIndex > 0)
            {
                foreach (Transform obj in currentSteps[currentStepIndex - 1].objectToEnable)
                {
                    if (obj != null)
                        obj.gameObject.SetActive(true);
                }
            }

            if (currentStepIndex < currentSteps.Count)
            {
                foreach (Transform obj in currentSteps[currentStepIndex].objectToDisable)
                {
                    if (obj != null) obj.gameObject.SetActive(false);
                }
            }
        }
    }
    private void SetInitialStateOfObjects(List<Step> currentStep)
    {
        foreach (Step step in currentStep)
        {
            Activatation(step.objectToDisable, true);
        }

        foreach(Step step in currentStep)
        {
            Activatation(step.objectToEnable, false);
        }
    }
    private void Activatation(List<Transform> objects, bool value)
    {
        foreach (Transform obj in objects)
        {
            obj.gameObject.SetActive(value);
        }
    }

    //UI Button call
    public void SkipStep()
    {
        /*if (toLastStep)
        {
            Debug.Log("Step count "+ (currentSteps.Count -1));
            if (currentStepIndex < currentSteps.Count - 1 && currentStepIndex == currentSteps.Count-1)
            {
                Debug.LogError("Second last");
                currentStepIndex = currentSteps.Count - 1;
                HandleCurrentStepsObjectActivation(currentSteps);
            }
            else
            {
                Debug.Log("Process Complete");
            }
        }
        else
        {*/
        if (currentStepIndex == 0)
        {
            HighlightNextObjectEvent?.Invoke(currentStepIndex, true);
            currentStepIndex = 1;
            HandleCurrentStepsObjectActivation(currentSteps);
        }
        else
        {
            Debug.Log("Not at the first step.");
        }
        //}
    }
}
