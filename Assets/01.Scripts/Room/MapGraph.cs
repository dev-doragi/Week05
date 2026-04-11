using System.Collections.Generic;
using UnityEngine;

public class MapGraph
{
    private readonly Dictionary<RoomID, List<RoomID>> _adjList = new Dictionary<RoomID, List<RoomID>>();
    private readonly Dictionary<RoomID, Dictionary<RoomID, int>> _distanceTable = new Dictionary<RoomID, Dictionary<RoomID, int>>();

    public MapGraph()
    {
        InitializeGraph();
        PrecalculateDistances();
    }

    public IReadOnlyList<RoomID> GetNeighbors(RoomID room)
    {
        if (_adjList.TryGetValue(room, out List<RoomID> neighbors))
            return neighbors;

        return System.Array.Empty<RoomID>();
    }

    public bool IsConnected(RoomID from, RoomID to)
    {
        if (_adjList.TryGetValue(from, out List<RoomID> neighbors) == false)
            return false;

        return neighbors.Contains(to);
    }

    public int GetDistance(RoomID from, RoomID to)
    {
        if (_distanceTable.TryGetValue(from, out Dictionary<RoomID, int> distances) == false)
            return int.MaxValue;

        if (distances.TryGetValue(to, out int distance) == false)
            return int.MaxValue;

        return distance;
    }

    private void InitializeGraph()
    {
        foreach (RoomID room in System.Enum.GetValues(typeof(RoomID)))
        {
            if (room == RoomID.None)
                continue;

            _adjList[room] = new List<RoomID>();
        }

        // B1F
        AddBidirectionalEdge(RoomID.B1F_Cafeteria, RoomID.B1F_JungleStepLower);
        AddBidirectionalEdge(RoomID.B1F_JungleStepLower, RoomID.B1F_Stair);
        AddBidirectionalEdge(RoomID.B1F_Stair, RoomID.B1F_Cafe);
        AddBidirectionalEdge(RoomID.B1F_Cafe, RoomID.B1F_Cafeteria);

        // 1F
        AddBidirectionalEdge(RoomID.F1_Elevator, RoomID.F1_JungleStepUpper);
        AddBidirectionalEdge(RoomID.F1_JungleStepUpper, RoomID.F1_Stair);
        AddBidirectionalEdge(RoomID.F1_Stair, RoomID.F1_Lobby);
        AddBidirectionalEdge(RoomID.F1_Lobby, RoomID.F1_Elevator);

        // 3F
        AddBidirectionalEdge(RoomID.F3_Elevator, RoomID.F3_Lounge);
        AddBidirectionalEdge(RoomID.F3_Lounge, RoomID.F3_CoachingRoom);
        AddBidirectionalEdge(RoomID.F3_CoachingRoom, RoomID.F3_Hallway);
        AddBidirectionalEdge(RoomID.F3_Hallway, RoomID.F3_Elevator);

        AddBidirectionalEdge(RoomID.B1F_Stair, RoomID.F1_Stair);
        AddBidirectionalEdge(RoomID.F1_Elevator, RoomID.F3_Elevator);

        AddBidirectionalEdge(RoomID.F3_CoachingRoom, RoomID.Office);
        AddBidirectionalEdge(RoomID.F3_Hallway, RoomID.Office);
    }

    private void AddBidirectionalEdge(RoomID a, RoomID b)
    {
        AddEdge(a, b);
        AddEdge(b, a);
    }

    private void AddEdge(RoomID from, RoomID to)
    {
        if (_adjList.TryGetValue(from, out List<RoomID> neighbors) == false)
            return;

        if (neighbors.Contains(to))
            return;

        neighbors.Add(to);
    }

    private void PrecalculateDistances()
    {
        foreach (RoomID start in _adjList.Keys)
        {
            _distanceTable[start] = CalculateDistancesFrom(start);
        }
    }

    private Dictionary<RoomID, int> CalculateDistancesFrom(RoomID start)
    {
        Dictionary<RoomID, int> distances = new Dictionary<RoomID, int>();
        Queue<RoomID> queue = new Queue<RoomID>();

        foreach (RoomID room in _adjList.Keys)
            distances[room] = int.MaxValue;

        distances[start] = 0;
        queue.Enqueue(start);

        while (queue.Count > 0)
        {
            RoomID current = queue.Dequeue();
            int nextDistance = distances[current] + 1;

            foreach (RoomID neighbor in _adjList[current])
            {
                if (distances[neighbor] <= nextDistance)
                    continue;

                distances[neighbor] = nextDistance;
                queue.Enqueue(neighbor);
            }
        }

        return distances;
    }
}