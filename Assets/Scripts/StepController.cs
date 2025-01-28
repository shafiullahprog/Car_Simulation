using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

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

    ClientMessageController controller;
    ServerMessageController serverController;

    [Header("Steps")]
    [SerializeField] List<Step> dismantleSteps = new List<Step>();
    [SerializeField] List<Step> assemleSteps = new List<Step>();
     
    public UnityAction<int, bool> HighlightNextObjectEvent;
    public UnityAction<int, bool> DisablePreviousHighlightEvent;

    [HideInInspector] public UnityEvent OnGamestart = new UnityEvent();
    [HideInInspector] public UnityEvent OnFinalStepReached = new UnityEvent();

    [HideInInspector] public UnityEvent<string> OnProcessChange = new UnityEvent<string>();

    [HideInInspector] public static UnityEvent OnClickingWrongStep = new UnityEvent();


    private void Start()
    {
        HighlightNextObjectEvent += HighlightObjController;
        DisablePreviousHighlightEvent += HighlightObjController;
        MessageController.OnMessageReceive += ProcessStepMessage;
        SelectStepList(dismantleSteps);
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
        else
        {
            if (!EventSystem.current.IsPointerOverGameObject())
            {
                OnClickingWrongStep?.Invoke();
                SendStepMessage("wrongStep", currentStepIndex);
            }
        }
    }
    private void MonitorSteps(Transform hitPart)
    {
        if (currentSteps.Count > 0)
        {    
            if (currentStepIndex < currentSteps.Count && hitPart == currentSteps[currentStepIndex].objectToAnimate)
            {
                PerformStep();
                SendStepMessage("currentStep", currentStepIndex);
            }
            else
            {
                OnClickingWrongStep?.Invoke();
                SendStepMessage("wrongStep", currentStepIndex);
            }
        }
        else
        {
            Debug.Log("No Steps found");
        }
    }

    private void PerformStep()
    {
        Step step = currentSteps[currentStepIndex];

        AnimationSteps(step.objAnimator, step.AnimTrigger);
        HandleCurrentStepsObjectActivation(currentSteps);

        DisablePreviousHighlightEvent?.Invoke(currentStepIndex - 1, false);
        HighlightNextObjectEvent?.Invoke(currentStepIndex, true);

        currentStepIndex++;

        if (currentStepIndex == currentSteps.Count)
        {
            OnFinalStepReached?.Invoke();
        }
    }

    private void AnimationSteps(Animator objToAnimate, string triggerName)
    {
        objToAnimate.SetTrigger(triggerName);
    } 
    public override void SetAssembleProcess()
    {
        currentStepIndex = 0;
        UpdateAnimationForCurrentProcess();
        base.SetAssembleProcess();
        SelectStepList(assemleSteps);
        SkipStep();
    }
    public override void SetDismantleProcess()
    {
        currentStepIndex = 0;
        UpdateAnimationForCurrentProcess();
        base.SetDismantleProcess();
        SelectStepList(dismantleSteps);
        SkipStep();
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
                //Debug.Log("Step name: " + step.objectToAnimate.name);
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
        }
        else if (currentProcess == CurrentProcess.Assemble)
        {
            if (currentStepIndex >= 0)
            {
                foreach (Transform obj in currentSteps[currentStepIndex].objectToEnable)
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
            if(obj !=null)
                obj.gameObject.SetActive(value);
        }
    }

    //UI Button call
    public void SkipStep()
    {
        if (currentStepIndex == 0)
        {
            //SendProcessUpdateToClient();
            HighlightNextObjectEvent?.Invoke(currentStepIndex, true);
            if (currentProcess == CurrentProcess.Dismantle)
            {
                currentStepIndex = 1;
                HandleCurrentStepsObjectActivation(currentSteps);
            }
            else
            {
                HandleCurrentStepsObjectActivation(currentSteps);
                currentStepIndex = 1;
            }
        }
        else
        {
            Debug.Log("Not at the first step.");
        }
    }

    public void SendProcessUpdate(string msg)
    {
        OnProcessChange?.Invoke(msg);
    }

    private void SendStepMessage(string actionType, int stepIndex)
    {
        string message = $"{actionType}:{stepIndex}";
        controller = FindObjectOfType<ClientMessageController>();
        if (controller != null)
        {
            controller.SendMessageToServer(message);
        }

        serverController = FindObjectOfType<ServerMessageController>();
        if (serverController != null)
        {
            serverController.SendMessageToClients(message);
        }
    }

    private void ProcessStepMessage(string message)
    {
        string[] parts = message.Split(':');
        if (parts.Length != 2) return;

        string actionType = parts[0];
        int stepIndex = int.Parse(parts[1]);

        switch (actionType)
        {
            case "currentStep":
                PerformStep();
                break;
            case "wrongStep":
                OnClickingWrongStep?.Invoke();
                break;
        }
    }

    private void OnDestroy()
    {
        HighlightNextObjectEvent -= HighlightObjController;
        DisablePreviousHighlightEvent -= HighlightObjController;
        MessageController.OnMessageReceive -= ProcessStepMessage;
    }
}
