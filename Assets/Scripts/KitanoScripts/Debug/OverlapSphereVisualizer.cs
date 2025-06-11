using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class OverlapSphereVisualizer : MonoBehaviour
{
    public static OverlapSphereVisualizer Instance { get; private set; }

    [SerializeField] private Material sphereMaterial; // Sprites/Default + Alpha付き推奨
                                                      //[SerializeField] private GameObject labelPrefab;  // UI Textを含むプレハブ（任意）

    bool isVisualized = false; 


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        
    }

    public void ShowSphere(Vector3 center, float radius, float duration, string label = "OverlapSphere", Color? color = null)
    {
        isVisualized = SROptions.Current.isShowingAttackOverlapSphere;// SROptionsからのフラグ取得
        
        if(!isVisualized) return;

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Destroy(sphere.GetComponent<Collider>());

        sphere.transform.position = center;
        sphere.transform.localScale = Vector3.one * radius * 2f;

        Color baseColor = color ?? Color.green;
        baseColor.a = 0.3f;

        var mat = new Material(sphereMaterial);
        mat.color = baseColor;




        sphere.GetComponent<Renderer>().material = mat;

        if (!string.IsNullOrEmpty(label))//&& labelPrefab != null
        {
            //var labelObj = Instantiate(labelPrefab, FindObjectOfType<Canvas>().transform);
            //labelObj.GetComponent<Text>().text = label;

            //StartCoroutine(UpdateLabel(labelObj.GetComponent<RectTransform>(), sphere.transform));
            //Destroy(labelObj, duration);
        }

        Destroy(sphere, duration);
    }

    private IEnumerator UpdateLabel(RectTransform rect, Transform target)
    {
        while (target != null)
        {
            rect.position = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
            yield return null;
        }
    }
}
