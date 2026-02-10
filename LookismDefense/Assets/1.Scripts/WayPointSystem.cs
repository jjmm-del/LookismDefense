using UnityEngine;

public class WayPointSystem : MonoBehaviour
{
    //인스펙터에서 순서대로 위치(Trnasform)을 넣어줄 배열
    [SerializeField] private Transform[] waypoints;
    
    //외부에서 경로 정보를 가져갈 수 있게 프로퍼티 제공
    public Transform[] WayPoints => waypoints;

    //에디터 상에서 경로를 선으로 보여주는 디버그 기능
    private void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2) return;

        Gizmos.color = Color.red;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i + 1] != null)
            {
                Gizmos.DrawLine(waypoints[i].position, waypoints[i + 1].position);
            }
        }
    }
}
