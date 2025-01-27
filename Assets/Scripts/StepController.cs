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
    [Header("Steps")]
    [SerializeField] List<Step> dismantleSteps = new List<Step>();
    [SerializeField] List<Step> assemleSteps = new List<Step>();
     
    public UnityAction<int, bool> HighlightNextObjectEvent;
    public UnityAction<int, bool> DisablePreviousHighlightEvent;

    [HideInInspector] public UnityEvent OnGamestart = new UnityEvent();
    [HideInInspector] public UnityEvent<string> OnProcessChange = new UnityEvent<string>();
    [HideInInspector] public UnityEvent OnFinalStepReached;
    [HideInInspector] public UnityEvent OnClickingWrongStep;

    private void Start()
    {
        HighlightNextObjectEvent += HighlightObjController;
        DisablePreviousHighlightEvent += HighlightObjController;
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
    }
    private void MonitorSteps(Transform hitPart)
    {
        if (currentSteps.Count > 0)
        {    
            if (currentStepIndex < currentSteps.Count && hitPart == currentSteps[currentStepIndex].objectToAnimate)
            {
                Step step = currentSteps[currentStepIndex];
                
                AnimationSteps(step.objAnimator, step.AnimTrigger);
                HandleCurrentStepsObjectActivation(currentSteps);
                
                DisablePreviousHighlightEvent?.Invoke(currentStepIndex - 1, false);
                HighlightNextObjectEvent?.Invoke(currentStepIndex, true);
                
                currentStepIndex++;

                if(currentStepIndex == currentSteps.Count)
                {
                    OnFinalStepReached?.Invoke();
                }
            }
            else
            {
                OnClickingWrongStep?.Invoke();
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
    public override void SetAssembleProcess()
    {
        currentStepIndex = 0;
        UpdateAnimationForCurrentProcess();
        base.SetAssembleProcess();
        SelectStepList(assemleSteps);
        SkipStep();

        //OnProcessChange?.Invoke(currentProcess.ToString());
    }
    public override void SetDismantleProcess()
    {
        currentStepIndex = 0;
        UpdateAnimationForCurrentProcess();
        base.SetDismantleProcess();
        SelectStepList(dismantleSteps);
        SkipStep();

        //OnProcessChange?.Invoke(currentProcess.ToString());
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
            //SendProcessUpdateToClient();
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
    public void SendProcessUpdateToClient(string msg)
    {
        OnProcessChange?.Invoke(msg);
    }
}
