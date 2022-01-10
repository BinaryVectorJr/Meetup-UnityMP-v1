public struct PlayerData
{
    public string playerName { get; private set; }

    public PlayerData(string _playerName)
    {
        playerName = _playerName;
    }
}
