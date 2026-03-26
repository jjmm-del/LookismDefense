using UnityEngine;

[CreateAssetMenu(menuName = "LookismDefense/DifficultySettings")]
public class DifficultyData : ScriptableObject
{
    [Header("Game Over Conditions")]
    [SerializeField] private int maxUnitCountLimits;//라인사 기준 (예: 80 마리)
    [SerializeField] private float bossTimeLimit; //보스 제한 시간
    [SerializeField] private int storyLimit; //정해진 라운드까지 뚫어야 하는 스토리
    
    [Header("Round Settings")]
    [SerializeField] private int maxRounds; // 난이도에 따른 라운드 수
    public int MaxUnitCountLimits => maxUnitCountLimits;
    public float BossTimeLimit => bossTimeLimit;
    public int StoryLimit => storyLimit;
    public int MaxRounds => maxRounds;

}
