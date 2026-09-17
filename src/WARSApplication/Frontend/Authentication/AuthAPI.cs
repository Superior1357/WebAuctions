using Refit;
using Backend;
using Backend.Data.SharedDataProperties;

namespace Frontend.Authentication;

public interface IAuthApi
{
    [Post("/register")]
    public Task RegisterAsync(RegisterRequest request);
    
    [Post("/login")]
    public Task<TokenResponse> LoginAsync(LoginRequest request);
}