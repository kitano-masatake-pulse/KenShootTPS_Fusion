// SimpleExplosionSphere.cs
using UnityEngine;
using System.Collections;

/// <summary>
/// 半透明のSphereプレハブをインスタンス化し、
/// 半径とアルファを時間変化させて消す簡易エフェクト。
/// - startRadius(A) → endRadius(C) を growTime(b) 秒で拡大
/// - アルファ: startAlpha(d) → endAlpha(e) を growTime(b) 秒で
/// - duration(f) のうち、終盤 fadeTime(g) 秒で 0 へフェードアウト
/// - ParticleSystemは使わない
/// </summary>
public class ExplosionSphereSpawner : MonoBehaviour
{
    // アニメ用パラメータ
    float startRadius, growTime, endRadius;
    float startAlpha, endAlpha, duration, fadeTime;

    // 参照
    MeshRenderer mr;
    MeshFilter mf;
    Material matInstance;

    // 変換用
    float meshRadiusLocal;      // Mesh.bounds 由来のローカル半径
    float childRelativeScale;   // 子のlossyScale / ルートlossyScale（非一様スケール対策）
    Color baseRGB;              // マテリアル元色のRGB（αは毎フレーム差し替え）

    // --------- 公開: 生成API ---------
    public  GameObject Spawn(
        GameObject spherePrefab, Vector3 position,
        float startRadius, float growTime, float endRadius,
        float startAlpha, float endAlpha,
        float duration, float fadeTime)
    {
        if (spherePrefab == null)
        {
            Debug.LogError("[SimpleExplosionSphere] spherePrefab が null です。");
            return null;
        }

        var go = Instantiate(spherePrefab, position, Quaternion.identity);
        go.name = "ExplosionSphere_NoPS";

        // アニメ制御用コンポーネントを付与
        var ctrl = go.AddComponent<ExplosionSphereSpawner>();
        ctrl.Initialize(startRadius, growTime, endRadius, startAlpha, endAlpha, duration, fadeTime);
        return go;
    }

    // 初期化
    void Initialize(
        float startRadius, float growTime, float endRadius,
        float startAlpha, float endAlpha,
        float duration, float fadeTime)
    {
        // パラメータ正規化
        this.duration = Mathf.Max(0.01f, duration);
        this.growTime = Mathf.Clamp(growTime, 0f, this.duration - 1e-4f);
        this.fadeTime = Mathf.Clamp(fadeTime, 0f, this.duration);
        this.startRadius = Mathf.Max(0f, startRadius);
        this.endRadius = Mathf.Max(0f, endRadius);
        this.startAlpha = Mathf.Clamp01(startAlpha);
        this.endAlpha = Mathf.Clamp01(endAlpha);

        // 必須コンポーネント
        mr = GetComponentInChildren<MeshRenderer>(true);
        mf = GetComponentInChildren<MeshFilter>(true);
        if (mr == null || mf == null || mf.sharedMesh == null)
        {
            Debug.LogError("[SimpleExplosionSphere] MeshRenderer / MeshFilter が見つかりません。");
            Destroy(gameObject);
            return;
        }

        // インスタンス化されたマテリアルを使用（sharedMaterialを直接書き換えない）
        matInstance = mr.material;

        // ベース色を取得（_BaseColor(URP) / _Color(Built-in) 両対応）
        if (matInstance.HasProperty("_BaseColor"))
            baseRGB = matInstance.GetColor("_BaseColor");
        else if (matInstance.HasProperty("_Color"))
            baseRGB = matInstance.GetColor("_Color");
        else
            baseRGB = Color.white;

        baseRGB.a = 1f; // αは毎フレーム上書きするのでRGBだけ使う

        // メッシュのローカル半径
        var ext = mf.sharedMesh.bounds.extents;
        meshRadiusLocal = Mathf.Max(ext.x, Mathf.Max(ext.y, ext.z));
        if (meshRadiusLocal <= 0f) meshRadiusLocal = 0.5f; // フォールバック

        // 子の相対スケール（子lossy / ルートlossy）を保存
        // これを考慮すると、プレハブ側にスケールが付いていても正しいワールド半径になる
        float rootLossy = transform.lossyScale.x;
        float childLossy = mr.transform.lossyScale.x;
        childRelativeScale = (rootLossy > 0f) ? (childLossy / rootLossy) : 1f;

        // 初期表示（半径= startRadius, α= startAlpha）
        ApplyScale(startRadius);
        ApplyAlpha(startAlpha);

        // アニメ開始
        StartCoroutine(Animate());
    }

    // 半径 → ルートlocalScaleへ変換（相対スケール考慮）
    void ApplyScale(float radius)
    {
        // 目標ワールド半径 R を満たすルートのlocalScale S:  R = meshRadiusLocal * (childRelativeScale * S)
        // → S = R / (meshRadiusLocal * childRelativeScale)
        float denom = meshRadiusLocal * Mathf.Max(1e-6f, childRelativeScale);
        float s = (denom > 0f) ? (radius / denom) : 1f;
        s = Mathf.Max(0f, s);
        transform.localScale = new Vector3(s, s, s);
    }

    // α適用（URP/Built-in両対応）
    void ApplyAlpha(float a)
    {
        a = Mathf.Clamp01(a);
        if (matInstance == null) return;

        if (matInstance.HasProperty("_BaseColor"))
        {
            var c = baseRGB; c.a = a;
            matInstance.SetColor("_BaseColor", c);
        }
        if (matInstance.HasProperty("_Color"))
        {
            var c = baseRGB; c.a = a;
            matInstance.SetColor("_Color", c);
        }
    }

    IEnumerator Animate()
    {
        float t = 0f;
        float holdEnd = Mathf.Max(0f, duration - fadeTime);

        while (t < duration)
        {
            // 現在時刻
            t += Time.deltaTime;
            float clamped = Mathf.Min(t, duration);

            // 半径
            float r;
            if (clamped <= growTime && growTime > 0f)
                r = Mathf.Lerp(startRadius, endRadius, clamped / growTime);
            else
                r = endRadius;
            ApplyScale(r);

            // アルファ
            float a;
            if (clamped <= growTime && growTime > 0f)
            {
                a = Mathf.Lerp(startAlpha, endAlpha, clamped / growTime);
            }
            else if (clamped <= holdEnd || fadeTime <= 0f)
            {
                a = endAlpha;
            }
            else
            {
                // 終盤フェード（endAlpha → 0）
                float x = (clamped - holdEnd) / Mathf.Max(1e-6f, fadeTime);
                a = Mathf.Lerp(endAlpha, 0f, x);
            }
            ApplyAlpha(a);

            yield return null;
        }

        // 念のため最終状態に揃える
        ApplyScale(endRadius);
        ApplyAlpha(0f);

        Destroy(gameObject);
    }
}
