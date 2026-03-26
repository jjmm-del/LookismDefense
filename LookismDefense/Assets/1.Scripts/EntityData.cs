using UnityEngine;

public abstract class EntityData : ScriptableObject
{
    [SerializeField] private string entityName; //이름
    [SerializeField] private float moveSpeed; //이동 속도
    [SerializeField] private GameObject prefab; //프리펩
    [SerializeField] private Sprite portraitIcon;

    public string EntityName => entityName;
    public float MoveSpeed => moveSpeed;
    public GameObject Prefab => prefab;
    public Sprite PortraitIcon => portraitIcon;
}