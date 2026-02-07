using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyEntity : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;

    public EnemyData Data => enemyData;

    private EnemyMovement movement;

    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    //Spawner(WaveManager)가 적을 생성한 직후 호출하는 함수
    public void SetUp(EnemyData data, Transform[] path)
    {
        this.enemyData = data;
        
        //enemyMovement에게 "이 속도로, 이 길을 따라가라"고 명령
        if (movement != null)
        {
            movement.Initialize(data.MoveSpeed, path);
        }
        //체력 초기화 등 추가 로직 가능
        //currentHealth = data.MaxHealth;
    }
}
