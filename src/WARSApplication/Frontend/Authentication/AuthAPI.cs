using Refit;
using Backend;

namespace Frontend.Authentication;

public interface IAuthApi
{
    [Post("/register")]
    public Task RegisterAsync(RegisterRequest request);
    
    [Post("/login")]
    public Task<TokenResponse> LoginAsync(LoginRequest request);
}