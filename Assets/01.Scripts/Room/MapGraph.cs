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
        // 모든 RoomID에 대해 컬렉션 초기화
        foreach (RoomID room in System.Enum.GetValues(typeof(RoomID)))
        {
            if (room == RoomID.None) continue;

            _adjList[room] = new List<RoomID>();
            _baseWeights[room] = new Dictionary<RoomID, float>();
        }

        // 기존 확률을 100분율 가중치(BaseWeight)로 변환하여 연결
        // 예: roll < 0.65f -> 65f, 나머지 -> 35f

        // Cam 8 (b1층 카페테리아)
        SetEdge(RoomID.Cafeteria, RoomID.JungleStep, 65f);
        SetEdge(RoomID.Cafeteria, RoomID.Elevator_B, 35f);

        // Cam 7 (정글 스텝)
        SetEdge(RoomID.JungleStep, RoomID.Cafeteria, 70f);
        SetEdge(RoomID.JungleStep, RoomID.Lobby, 30f);
        SetEdge(RoomID.JungleStep, RoomID.Elevator_B, 20f);


        // Cam 5 (1층 로비)
        SetEdge(RoomID.Lobby, RoomID.JungleStep, 45f);
        SetEdge(RoomID.Lobby, RoomID.Stair_B, 35f); // 0.8 - 0.45 = 0.35
        SetEdge(RoomID.Lobby, RoomID.Elevator_B, 20f);

        // Cam 4 (3층 계단)
        SetEdge(RoomID.Stair_A, RoomID.Lounge, 60f);
        SetEdge(RoomID.Stair_A, RoomID.Elevator_A, 40f);
        SetEdge(RoomID.Stair_B, RoomID.Stair_A, 15f);

        // Cam 6 (1층 계단)
        SetEdge(RoomID.Stair_B, RoomID.JungleStep, 40f);
        SetEdge(RoomID.Stair_B, RoomID.Lobby, 35f); // 0.75 - 0.4 = 0.35
        SetEdge(RoomID.Stair_B, RoomID.Stair_A, 25f);

        // Cam 3 (라운지)
        SetEdge(RoomID.Lounge, RoomID.CoachingRoom, 35f);
        SetEdge(RoomID.Lounge, RoomID.Stair_A, 35f); // 0.7 - 0.35 = 0.35
        SetEdge(RoomID.Lounge, RoomID.LeftHallwayNearOffice, 15f); // 0.85 - 0.7 = 0.15
        //SetEdge(RoomID.Lounge, RoomID.RightHallwayNearOffice, 15f);

        // Cam 1 (코칭룸)
        SetEdge(RoomID.CoachingRoom, RoomID.Lounge, 65f); // 원래 50% + 15% 중복 할당되어 있던 것 합산
        SetEdge(RoomID.CoachingRoom, RoomID.LeftHallwayNearOffice, 20f); // 0.7 - 0.5 = 0.2
        SetEdge(RoomID.CoachingRoom, RoomID.Elevator_A, 15f);

        // Cam 2 (3층 엘리베이터 앞)
        SetEdge(RoomID.Elevator_A, RoomID.Stair_A, 50f);
        SetEdge(RoomID.Elevator_A, RoomID.RightHallwayNearOffice, 20f); // 0.7 - 0.5 = 0.2
        SetEdge(RoomID.Elevator_A, RoomID.Lounge, 15f); // 0.85 - 0.7 = 0.15
        SetEdge(RoomID.Elevator_A, RoomID.Elevator_B, 15f);

        // Cam 9 (b1층 엘리베이터)
        SetEdge(RoomID.Elevator_B, RoomID.Cafeteria, 60f);
        SetEdge(RoomID.Elevator_B, RoomID.Elevator_A, 20f); // 0.8 - 0.6 = 0.2
        SetEdge(RoomID.Elevator_B, RoomID.Stair_B, 20f);

        // Cam 1A (오피스 왼쪽 복도)
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.Office, 65f);
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.RightHallwayNearOffice, 20f); // 0.85 - 0.65 = 0.2
        SetEdge(RoomID.LeftHallwayNearOffice, RoomID.Lounge, 15f);

        // Cam 2A (오피스 오른쪽 복도)
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.Office, 65f);
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.LeftHallwayNearOffice, 20f); // 0.85 - 0.65 = 0.2
        SetEdge(RoomID.RightHallwayNearOffice, RoomID.Lounge, 15f);
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