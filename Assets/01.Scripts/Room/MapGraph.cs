using System.Collections.Generic;
using UnityEngine;

public class MapGraph
{
    private readonly Dictionary<RoomID, List<RoomID>> _adjList = new();
    private readonly Dictionary<RoomID, Dictionary<RoomID, int>> _distanceTable = new();
    private readonly Dictionary<RoomID, Dictionary<RoomID, float>> _baseWeights = new();

    public const int UnreachableDistance = 999;

    public MapGraph()
    {
        InitializeGraph();
        PrecalculateDistances();
    }

    private void InitializeGraph()
    {
        foreach (RoomID room in System.Enum.GetValues(typeof(RoomID)))
        {
            if (room == RoomID.None) continue;
            _adjList[room] = new List<RoomID>();
            _baseWeights[room] = new Dictionary<RoomID, float>();
        }

        // --- B1층 (지하 구역) ---
        // 카페테리아: 정글스텝(메인), 엘리베이터B(전진), 로비(새 연결)
        SetEdge(RoomID.Cafeteria, RoomID.JungleStep, 50f);
        SetEdge(RoomID.Cafeteria, RoomID.Elevator_B, 30f);
        SetEdge(RoomID.Cafeteria, RoomID.Lobby, 20f);

        // 정글 스텝: 카페테리아(후퇴), 로비(전진), 엘리베이터B(전진), 계단B(전진)
        SetEdge(RoomID.JungleStep, RoomID.Cafeteria, 30f);
        SetEdge(RoomID.JungleStep, RoomID.Lobby, 30f);
        SetEdge(RoomID.JungleStep, RoomID.Elevator_B, 20f);
        SetEdge(RoomID.JungleStep, RoomID.Stair_B, 20f);

        // --- 1층 (중간 구역) ---
        // 로비: 정글스텝(후퇴), 계단B(전진), 엘리베이터B(전진), 카페테리아(후퇴)
        SetEdge(RoomID.Lobby, RoomID.JungleStep, 30f);
        SetEdge(RoomID.Lobby, RoomID.Stair_B, 30f);
        SetEdge(RoomID.Lobby, RoomID.Elevator_B, 20f);
        SetEdge(RoomID.Lobby, RoomID.Cafeteria, 20f);

        // 엘리베이터 B (B1~1층 연결부): 카페테리아, 로비, 계단B, 엘리베이터A(3층행)
        SetEdge(RoomID.Elevator_B, RoomID.Cafeteria, 30f);
        SetEdge(RoomID.Elevator_B, RoomID.Lobby, 30f);
        SetEdge(RoomID.Elevator_B, RoomID.Stair_B, 20f);
        SetEdge(RoomID.Elevator_B, RoomID.Elevator_A, 20f);

        // 계단 B (1층 계단): 정글스텝, 로비, 엘리베이터B, 계단A(3층행)
        SetEdge(RoomID.Stair_B, RoomID.JungleStep, 25f);
        SetEdge(RoomID.Stair_B, RoomID.Lobby, 25f);
        SetEdge(RoomID.Stair_B, RoomID.Elevator_B, 20f);
        SetEdge(RoomID.Stair_B, RoomID.Stair_A, 30f);

        // --- 3층 (상층 구역 - 오피스 인접) ---
        // 계단 A (3층 계단): 라운지, 엘리베이터A, 계단B(후퇴), 코칭룸(직통)
        SetEdge(RoomID.Stair_A, RoomID.Lounge, 40f);
        SetEdge(RoomID.Stair_A, RoomID.Elevator_A, 30f);
        SetEdge(RoomID.Stair_A, RoomID.Stair_B, 15f);
        SetEdge(RoomID.Stair_A, RoomID.CoachingRoom, 15f);

        // 엘리베이터 A (3층 엘리베이터): 계단A, 오른쪽 복도, 라운지, 엘리베이터B(후퇴)
        SetEdge(RoomID.Elevator_A, RoomID.Stair_A, 30f);
        SetEdge(RoomID.Elevator_A, RoomID.RightHallwayNearOffice, 30f);
        SetEdge(RoomID.Elevator_A, RoomID.Lounge, 20f);
        SetEdge(RoomID.Elevator_A, RoomID.Elevator_B, 20f);

        // 라운지 (허브): 코칭룸, 계단A, 왼쪽복도, 오른쪽복도, 엘리베이터A
        SetEdge(RoomID.Lounge, RoomID.CoachingRoom, 25f);
        SetEdge(RoomID.Lounge, RoomID.Stair_A, 25f);
        SetEdge(RoomID.Lounge, RoomID.LeftHallwayNearOffice, 20f);
        SetEdge(RoomID.Lounge, RoomID.RightHallwayNearOffice, 20f);
        SetEdge(RoomID.Lounge, RoomID.Elevator_A, 10f);

        // 코칭룸: 라운지, 왼쪽복도, 엘리베이터A, 계단A
        SetEdge(RoomID.CoachingRoom, RoomID.Lounge, 40f);
        SetEdge(RoomID.CoachingRoom, RoomID.LeftHallwayNearOffice, 30f);
        SetEdge(RoomID.CoachingRoom, RoomID.Elevator_A, 15f);
        SetEdge(RoomID.CoachingRoom, RoomID.Stair_A, 15f);

        // --- 오피스 경계 구역 ---
        // 왼쪽 복도: 오피스(최종), 오른쪽 복도(횡이동), 라운지(후퇴), 코칭룸(후퇴)
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.Office, 50f);
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.RightHallwayNearOffice, 20f);
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.Lounge, 10f);
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.CoachingRoom, 20f);

        // 오른쪽 복도: 오피스(최종), 왼쪽 복도(횡이동), 라운지(후퇴), 엘리베이터A(후퇴)
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.Office, 50f);
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.LeftHallwayNearOffice, 20f);
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.Lounge, 10f);
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.Elevator_A, 20f);
    }

    private void SetEdge(RoomID from, RoomID to, float baseWeight)
    {
        if (to == RoomID.None || to == from) return;

        if (!_adjList[from].Contains(to))
        {
            _adjList[from].Add(to);
        }

        // 양방향 가중치가 다를 수 있으므로 단방향으로 입력
        _baseWeights[from][to] = baseWeight;
    }

    public IReadOnlyList<RoomID> GetNeighbors(RoomID roomId)
    {
        if (_adjList.TryGetValue(roomId, out var neighbors))
            return neighbors;

        return System.Array.Empty<RoomID>();
    }

    public float GetBaseWeight(RoomID from, RoomID to)
    {
        if (_baseWeights.TryGetValue(from, out var targets) && targets.TryGetValue(to, out float weight))
        {
            return weight;
        }

        return 0f; // 연결되어 있지 않은 경우 0점
    }

    // BFS를 사용하여 모든 노드 쌍 간의 최단 거리를 캐싱
    private void PrecalculateDistances()
    {
        var allRooms = (RoomID[])System.Enum.GetValues(typeof(RoomID));

        foreach (RoomID startRoom in allRooms)
        {
            if (startRoom == RoomID.None) continue;

            _distanceTable[startRoom] = new Dictionary<RoomID, int>();

            Queue<RoomID> queue = new Queue<RoomID>();
            HashSet<RoomID> visited = new HashSet<RoomID>();

            queue.Enqueue(startRoom);
            visited.Add(startRoom);
            _distanceTable[startRoom][startRoom] = 0;

            while (queue.Count > 0)
            {
                RoomID current = queue.Dequeue();
                int currentDistance = _distanceTable[startRoom][current];

                foreach (RoomID neighbor in GetNeighbors(current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        _distanceTable[startRoom][neighbor] = currentDistance + 1;
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }
    }

    public int GetDistance(RoomID from, RoomID to)
    {
        if (_distanceTable.TryGetValue(from, out var targets) && targets.TryGetValue(to, out int dist))
        {
            return dist;
        }

        return UnreachableDistance;
    }
}