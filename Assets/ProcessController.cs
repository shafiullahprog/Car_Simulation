using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CurrentProcess { Assemble, Dismantle };
public class ProcessController : MonoBehaviour
{
    public CurrentProcess currentProcess = CurrentProcess.Dismantle;

    [HideInInspector]
    public List<Step> currentSteps;
    public int currentStepIndex = 0;

    protected virtual void SetAssembleProcess()
    {
        SetProcess(CurrentProcess.Assemble);
    }

    protected virtual void SetDismantleProcess()
    {
        SetProcess(CurrentProcess.Dismantle);
    }
    protected virtual void SelectStepList(List<Step> process)
    {
        currentSteps = process;
    }
    public void SetProcess(CurrentProcess newProcess)
    {
        if (currentProcess != newProcess)
        {
            currentProcess = newProcess;
            currentStepIndex = 0;
        }
        else
        {
            Debug.LogWarning("The process is already set");
        }
    }
}
