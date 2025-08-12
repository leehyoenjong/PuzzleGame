using System;
using UnityEngine;

public class MoveContoller : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] float moveTime = 0.25f;     // 이동 시간 (초)
    AnimationCurve moveCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    // 이동 상태 변수들
    Vector2 currentStartPos;
    Vector2 currentTargetPos;
    float currentMoveTime;
    float elapsedTime;
    bool isMoving = false;

    // 캐시된 컴포넌트
    RectTransform cachedTransform;
    public static event Action _complte_move;

    void Awake()
    {
        // 컴포넌트 캐싱으로 성능 최적화
        cachedTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 이동 처리
        if (isMoving == false)
        {
            return;
        }
        UpdateMovement();
    }

    /// <summary>
    /// 이동 시작 (Zero Allocation)
    /// </summary>
    void StartMovement(Vector2 targetPos)
    {
        if (Vector2.Distance(cachedTransform.anchoredPosition, targetPos) == 0)
        {
            return;
        }

        currentStartPos = cachedTransform.anchoredPosition;
        currentTargetPos = targetPos;
        currentMoveTime = moveTime;
        elapsedTime = 0f;
        isMoving = true;
    }

    /// <summary>
    /// 매 프레임 이동 업데이트 (Zero Allocation)
    /// </summary>
    void UpdateMovement()
    {
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / currentMoveTime;

        if (progress >= 1f)
        {
            // 이동 완료
            SetPosition(currentTargetPos);
            isMoving = false;
            OnMovementComplete();
            return;
        }

        // AnimationCurve를 사용한 부드러운 이동
        float curveValue = moveCurve.Evaluate(progress);
        Vector2 newPosition = Vector2.Lerp(currentStartPos, currentTargetPos, curveValue);
        SetPosition(newPosition);
    }

    /// <summary>
    /// 위치 설정 (Rigidbody2D 고려)
    /// </summary>
    void SetPosition(Vector2 position)
    {
        cachedTransform.anchoredPosition = position;
    }

    /// <summary>
    /// 이동 완료 시 호출되는 이벤트
    /// </summary>
    void OnMovementComplete()
    {
        _complte_move?.Invoke();
    }

    /// <summary>
    /// 외부에서 호출할 수 있는 이동 함수
    /// </summary>
    public void MoveTo(Vector2 position)
    {
        StartMovement(position);
    }

    /// <summary>
    /// 현재 이동 중인지 확인
    /// </summary>
    public bool IsMoving() => isMoving;
}