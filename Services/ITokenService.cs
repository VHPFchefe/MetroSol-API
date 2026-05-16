using MetroSol.Core.Entities;

namespace MetroSol.API.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
}
