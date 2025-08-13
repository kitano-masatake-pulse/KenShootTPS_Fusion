using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAudioEmitter : MonoBehaviour,IPointerEnterHandler,IPointerDownHandler,ISubmitHandler,ICancelHandler
{
    [Header("UI SFX Keys")]
    public string hoverKey = "se_sentaku";
    public string clickKey = "se_confirm_new";
    public string submitKey = "se_confirm_new";
    public string cancelKey = "se_cancel";
    [SerializeField]
    private float hoverVolume = 0.3f;

    public void OnPointerEnter(PointerEventData eventData)
    {
        AudioManager.Instance.PlaySound(hoverKey,SoundCategory.System,0.01f,hoverVolume);
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
