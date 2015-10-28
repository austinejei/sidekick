using System.Security.Claims;

namespace AuthServer.SsoInfrastructure.Factories
{
    public interface IClaimsPrincipalFactory
    {
        ClaimsPrincipal Create(string userName);
    }
}