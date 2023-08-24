namespace WolfBlades.BackEnd.Connections;

public class ConnectionStorageInfo : IDataStorage<ConnectionInfo>
{
    public string IPAddress = string.Empty;
    public int LoginUserID = -1;
    public int Port;

    public void ReadFrom(ConnectionInfo data)
    {
        IPAddress = data.IPAddress;
        Port = data.Port;
        LoginUserID = data.LoginUserID;
    }

    public void WriteTo(ref ConnectionInfo data)
    {
        data.IPAddress = IPAddress;
        data.Port = Port;
        data.LoginUserID = LoginUserID;
    }
}