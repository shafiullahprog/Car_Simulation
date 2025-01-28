using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WrongStepMsg : MonoBehaviour
{
    CanvasGroup canvasGroup;
    [SerializeField] float time;

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        StepController.OnClickingWrongStep.AddListener(RevealUI);
    }

    void RevealUI()
    {
        canvasGroup.alpha = 1;
        Invoke("DisableUI",time);
    }

    void DisableUI()
    {
        Debug.Log("Dis");
        canvasGroup.alpha = 0;
    }
}
