using System.Collections.Generic;

namespace AuthServer.SsoInfrastructure.Services
{
    public interface IRealmTracker
    {
        void AddNewRealm(string address);
        IList<string> ReadVisitedRealms();
    }
}