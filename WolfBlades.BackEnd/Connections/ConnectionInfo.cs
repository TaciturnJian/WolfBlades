using Fleck;

namespace WolfBlades.BackEnd.Connections
{
    public struct ConnectionInfo
    {
        public string IPAddress = string.Empty;
        public int Port = 0;
        public int LoginUserID = -1;
        public int UserAuthority = 0;
        public string LogPrefix = string.Empty;
        public int ID = -1;

        public ConnectionInfo()
        {
        }
    }
}
