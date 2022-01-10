using System;

//[Serializable]
//public class ConnectionPayloadRoot
//{
//    public ConnectionPayload ConnectionPayload { get; set; }
//}

[Serializable]
public class ConnectionPayload
{
    public string userPassword { get; set; }
    public string userPlayerName { get; set; }
}
