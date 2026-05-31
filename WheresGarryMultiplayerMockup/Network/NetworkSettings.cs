using System;
using System.Collections.Generic;
using System.Text;

namespace WheresGarryMultiplayerMockup.Network
{
    [Serializable]
    public struct NetworkSettings
    {
        public ushort port;
        public ushort maxConnections;
        public string username;
    }
}
