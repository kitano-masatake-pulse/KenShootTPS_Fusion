using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickLimiter : MonoBehaviour
{
    [SerializeField]
    Button[] buttons;
    [SerializeField]
    CanvasGroup canvasGroup; 

    private bool _isClicked = false;

    private void Awake()
    {
        if (buttons == null || buttons.Length == 0)
        {
            buttons = GetComponentsInChildren<Button>(true);
        }
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }
        foreach (Button button in buttons)
        {
            button.onClick.AddListener(OnButtonClick);
        }
    }
    private void OnDestroy()
    {
        foreach (Button button in buttons)
        {
            button.onClick.RemoveListener(OnButtonClick);
        }
    }
    private void OnButtonClick()
    {
        if (_isClicked) return;
        _isClicked = true;
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        else {             
            foreach (Button button in buttons)
            {
                button.interactable = false;
            }
        }
    }
}


