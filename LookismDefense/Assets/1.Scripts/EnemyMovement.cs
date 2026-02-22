using System.Transactions;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float arrivalThreshold = 0.1f; //웨이포인트 도달 판정

    private float currentMoveSpeed; //받아온 이동 속도를 저장할 변수
    private Transform[] pathPoints; 
    private int currentPointIndex = 0;
    private bool isInitialized = false;

    //스턴 등으로 멈춰있는지 확인하는 변수
    private bool isStopped = false;

    //초기화
    public void Initialize(float speed, Transform[] path)
    {
        currentMoveSpeed = speed;
        pathPoints = path;
        currentPointIndex = 0;
        
        //시작 위치를 첫 번째 웨이포인트로 강제 이동
        if (pathPoints != null && pathPoints.Length > 0)
        {
            transform.position = pathPoints[0].position;
            isInitialized = true;
        }
    }

    private void Update()
    {
        if (!isInitialized || pathPoints == null || isStopped) return;

        MoveAlongPath();
    }

    private void MoveAlongPath()
    {
        //현재 목표 지점 가져오기
        if (currentPointIndex >= pathPoints.Length) return;
        
        Transform targetPoint = pathPoints[currentPointIndex];
        Vector3 direction = targetPoint.position - transform.position;
        
        //이동 (MoveTowards 사용)
        transform.position = Vector3.MoveTowards(transform.position, targetPoint.position, currentMoveSpeed * Time.deltaTime);
        
        //회전(적이 진행방향을 보게함)
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, rotationSpeed * Time.deltaTime);
        }
        //목표 지점 도달 체크
        if (Vector3.Distance(transform.position, targetPoint.position) < arrivalThreshold)
        {
            currentPointIndex++;
            
            //마지막 지점 도달 시 (라이프 차감 및 자폭)
            if (currentPointIndex >= pathPoints.Length)
            {
                currentPointIndex = 0;
            }
        }
    }
    
    // --- 외부 호출 함수(스턴, 이감) ---
    // 이동 멈춤 (스턴)
    public void StopMovement()
    {
        isStopped = true;
    }

    // 이동 재개 (스턴 풀림)
    public void ResumeMovement()
    {
        isStopped = false;
    }
    
    // 이감(느려짐)을 위한 속도 감소 함수
    public void SetSpeedMultiplier(float multiplier)
    {
        // currentMoveSpeed = baseMoveSpeed * multiplier ;
        // 이런식으로 확장
    }
}
