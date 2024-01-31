using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AnimatedPanelController : MonoBehaviour
{
    // アニメーター
    [SerializeField] private Animator _animator;
    // アニメーターコントローラーのレイヤー（通常０）
    [SerializeField] private int _layer;
    // アニメーターコントローラー内で定義したフラグ
    private static readonly int ParamIsOpen = Animator.StringToHash("IsOpen");
    public bool IsOpen => gameObject.activeSelf;
    public bool IsTransition { get; private set; }

    public void Open()
    {
        // 不正操作防止
        if (IsOpen || IsTransition) return;
        // パネルアクティブ
        gameObject.SetActive(true);
        // ISOpenフラグセット
        _animator.SetBool(ParamIsOpen, true);
        // アニメーション待機
        StartCoroutine(WaitAnimation("Shown"));
    }

    public void Close()
    {
        // 不正操作防止
        if (!IsOpen || IsTransition) return;
        // IsOpenフラグクリア
        _animator.SetBool(ParamIsOpen, false);
        // アニメーション待機、終了後パネル非アクティブ
        StartCoroutine(WaitAnimation("Hidden", () => gameObject.SetActive(false)));
    }

    private IEnumerator WaitAnimation(string stateName, UnityAction onCompleted = null)
    {
        IsTransition = true;

        yield return new WaitUntil(() =>
        {
            // ステート変化後、アニメーション終了までループ
            var state = _animator.GetCurrentAnimatorStateInfo(_layer);
            return state.IsName(stateName) && state.normalizedTime >= 1;
        });

        IsTransition = false;
        onCompleted?.Invoke();
    }
}
