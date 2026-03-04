using UnityEngine;

[RequireComponent(typeof(EnemyMovement))]
public class EnemyEntity : MonoBehaviour
{
    [SerializeField] private EnemyData enemyData;
    public EnemyData Data => enemyData;

    [SerializeField] private EnemyHealthBar healthBar;
    [SerializeField] private GameObject selectionIndicator;

    private EnemyMovement movement;
    // 실시간 체력 관리
    private float currentHealth;
    private float currentDefense;
    private float armorBreakTimer = 0f;
    private bool isArmorBroken = false;
    private float stunTimer = 0f;
    private bool isStunned = false;
    
    
    
    public float CurrentHealth => currentHealth;
    
    
    
    private void Awake()
    {
        movement = GetComponent<EnemyMovement>();
    }

    private void Update()
    {
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                if (movement != null)
                {
                    movement.ResumeMovement();
                }
            }
        }

        if (isArmorBroken)
        {
            armorBreakTimer -= Time.deltaTime;
            if (armorBreakTimer <= 0)
            {
                isArmorBroken = false;
                currentDefense = enemyData.Defense;
            }
        }
    }
    //Spawner(WaveManager)가 적을 생성한 직후 호출하는 함수
    public void Setup(EnemyData data, Transform[] path)
    {
        this.enemyData = data;

        currentHealth = data.MaxHealth;
        currentDefense = data.Defense;
        if (data.Type == EnemyType.Normal)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemy(this);
            }
        }
        //enemyMovement에게 "이 속도로, 이 길을 따라가라"고 명령
        if (movement != null)
        {
            movement.Initialize(data.MoveSpeed, path);
        }
    }

    public void ApplyStun(float duration)
    {
        //이미 스턴 중이라면 더 긴 시간으로 갱신
        if (duration > stunTimer)
        {
            stunTimer = duration;
            isStunned = true;
            if (movement != null)
            {
                movement.StopMovement();
            }
        }
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        if (movement != null)
        {
            movement.ApplySlow(slowPercent, duration);
        }
    }

    public void ApplyArmorBreak(float reduceAmount, float duration)
    {
        //이미 방깎이 걸려있을 때 갱신하거나 중첩하는 로직
        //여기서는 가장 높은 방깎 수치로 덮어 씌우는 방식을 씁니다. -> 동일 캐릭이면 중첩안되지만 서로 다른종류 일때는 중첩하게
        float targetDefense = enemyData.Defense - reduceAmount;
        if (targetDefense < 0)
        {
            targetDefense = 0; //방어력이 마이너스가 되지않게 방어
        }

        if (targetDefense < currentDefense)
        {
            currentDefense = targetDefense;
        }
        armorBreakTimer = duration; // 지속시간 갱신
        isArmorBroken = true;
    }
    public void OnDamage(float damage)
    {
        //방어력 적용 공식
        float actualDamage = damage * Mathf.Ceil(1 - currentDefense / (100 + currentDefense)); 
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
            switch(Data.Type)
            {
                case EnemyType.Story:
                    GameManager.Instance.AdvanceStory(Data.StoryRewards);
                    break;
                case EnemyType.Boss:
                    GameManager.Instance.BossDefeated();
                    break;
                case EnemyType.Mission:
                    //GameManager.Instance.ClearMission(int missionLevel, reward);
                    break;
                case EnemyType.Normal:
                    GameManager.Instance.UnRegisterEnemy(this);
                    break;
            }
        }

        Destroy(gameObject);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionIndicator != null)
        {
            selectionIndicator.SetActive(isSelected);
        }
    }
}
