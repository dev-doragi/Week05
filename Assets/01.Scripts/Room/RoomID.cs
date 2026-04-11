public enum RoomID
{
    None = 0,

    // B1F: Cafeteria <-> JungleStepLower <-> Cafe <-> Stair <-> Cafeteria
    B1F_Cafeteria,
    B1F_JungleStepLower,
    B1F_Cafe,
    B1F_Stair,

    // 1F: Elevator <-> JungleStepUpper <-> Lobby <-> Stair <-> Elevator
    F1_Elevator,
    F1_JungleStepUpper,
    F1_Lobby,
    F1_Stair,

    // 3F: Elevator <-> Lounge <-> CoachingRoom <-> Hallway <-> Elevator
    F3_Elevator,
    F3_Lounge,
    F3_CoachingRoom,
    F3_Hallway,

    // Office: F3_CoachingRoom, F3_Hallway 와 연결된 특수 목적지
    Office
}