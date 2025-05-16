using TMPro;
using UnityEngine;

public class NameTagController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Transform target; // ì™è„ÇÃà íuÇí«Ç§

    private Camera mainCamera;

    public void SetName(string playerName)
    {
        nameText.text = playerName;
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera && target)
        {
            transform.position = new Vector3(target.position.x, target.position.y + 0.4f ,target.position.z);
            transform.rotation = Quaternion.LookRotation(transform.position - mainCamera.transform.position);
        }
    }
}