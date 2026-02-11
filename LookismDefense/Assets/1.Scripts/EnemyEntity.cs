using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyEntity : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    public EnemyData Data => enemyData;

    [SerializeField] private EnemyHealthBar healthBar;

    private EnemyMovement movement;
    // 실시간 체력 관리
    private float currentHealth;
    public float CurrentHealth => currentHealth;
    
    
    
    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    //Spawner(WaveManager)가 적을 생성한 직후 호출하는 함수
    public void Setup(EnemyData data, Transform[] path)
    {
        this.enemyData = data;

        currentHealth = data.MaxHealth;
        GameManager.Instance.RegisterEnemy(this);
        //enemyMovement에게 "이 속도로, 이 길을 따라가라"고 명령
        if (movement != null)
        {
            movement.Initialize(data.MoveSpeed, path);
        }
    }

    public void OnDamage(float damage)
    {
        //방어력 적용 공식
        float actualDamage = damage * Mathf.Ceil(1 - enemyData.Defense / (100 + enemyData.Defense)); 
        if(actualDamage < 1) actualDamage = 1; //최소데미지 1 보장

        currentHealth -= actualDamage;
        
        //UI 표시용(체력바 등 나중에 구현)
        if (healthBar != null)
        {
            healthBar.UpdateHealth(currentHealth, enemyData.MaxHealth);
        }
        Debug.Log($"{enemyData.EntityName}피격! 남은 체력: {currentHealth}");
        //(선택, 추가) 피격 효과음이나 이펙트 재생 위치
        //SoundManager.Instance.PlayHurtSound();
        
        if (currentHealth <= 0)
        {
            OnDeath();
        }
    }

    private void OnDeath()
    {
        //보상 지급(GameManager를 통해 골드 증가 등)
        //GameManager.Instance.AddGold(enemyData.DropGold);
        
        //매니저에게 죽었다고 알림
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UnRegisterEnemy(this);
        }

        Destroy(gameObject);
    }
}
