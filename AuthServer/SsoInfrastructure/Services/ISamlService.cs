using System;
using System.IdentityModel.Services;

namespace AuthServer.SsoInfrastructure.Services
{
    public interface ISamlTokenService
    {
        SignInResponseMessage CreateResponseContainingToken(Uri currrentRequestUri);
    }
}