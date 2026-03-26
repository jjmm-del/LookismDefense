using UnityEngine;

public static class SessionManager
{
    // 방 설정 데이터(나중에 멀티플레이 시 방장이 설정)
    public static int SelectedDifficultyIndex = -1; //선택한 난이도
    public static int MaxPlayerCount = 4; //최대 인원수
    public static bool isPasswordRoom = false; //비밀방 여부
    // 필요하다면 나중에 플레이어들의 닉네임 리스트 등도 여기에 저장합니다.
    // 비밀번호 설정?
}
