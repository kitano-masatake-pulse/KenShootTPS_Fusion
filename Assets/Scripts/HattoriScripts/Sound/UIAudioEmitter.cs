using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAudioEmitter : MonoBehaviour,IPointerEnterHandler,IPointerDownHandler,ISubmitHandler,ICancelHandler
{
    [Header("UI SFX Keys")]
    public string hoverKey = "se_cursorChange";
    public string clickKey = "se_confirm";
    public string submitKey = "se_confirm";
    public string cancelKey = "se_cancel";

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(hoverKey,SoundCategory.System,0.1f);
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(clickKey, SoundCategory.System);
    }
    public void OnSubmit(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySound(submitKey, SoundCategory.System);
    }
    public void OnCancel(BaseEventData eventData)
    {
        AudioManager.Instance.PlaySound(cancelKey, SoundCategory.System);
    }
}
