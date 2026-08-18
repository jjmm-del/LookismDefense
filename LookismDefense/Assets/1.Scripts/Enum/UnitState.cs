using UnityEngine;

public enum UnitState
{
    Idle, // 대기( 사거리 내 적 자동 공격)
    Move, // 강제 이동(적 무시)
    Chase,
    Attack,
    Return,
    AttackMove, // 이동하며 적 발견 시 공격(어택땅)
    Hold // 위치 고정(움직이지 않고 사거리 내 적 공격)
}

public enum UnitAIState
{
    Idle,
    Chase,
    Attack,
    Return
}
