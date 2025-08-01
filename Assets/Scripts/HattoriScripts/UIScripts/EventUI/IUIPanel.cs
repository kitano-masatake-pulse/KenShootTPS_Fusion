using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUIPanel
{
    /// <summary>
    /// パネルの初期化処理を行います。
    /// </summary>
    void Initialize();
    /// <summary>
    /// パネルのクリーンアップ処理を行います。
    /// </summary>
    void Cleanup();
    /// <summary>
    /// パネルの表示/非表示を設定します。
    /// </summary>
    /// <param name="visible">表示する場合はtrue、非表示にする場合はfalse。</param>
    void SetVisible(bool visible);
}
