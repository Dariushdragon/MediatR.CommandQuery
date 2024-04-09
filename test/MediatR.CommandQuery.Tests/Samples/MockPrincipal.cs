using System.Security.Claims;
using System.Security.Principal;

namespace MediatR.CommandQuery.Tests.Samples;

public static class MockPrincipal
{
    static MockPrincipal()
    {
        Default = CreatePrincipal("test@mailinator.com", "Test User");
    }

    public static IPrincipal CreatePrincipal(string email, string name)
    {
        var claimsIdentity = new ClaimsIdentity("JWT", "sub", "role");
        claimsIdentity.AddClaim(new Claim("sub", email));
        claimsIdentity.AddClaim(new Claim("name", name));
        claimsIdentity.AddClaim(new Claim("email", email));

        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
        return claimsPrincipal;
    }


    public static IPrincipal Default { get; }

}
