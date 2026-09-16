using Refit;
using WARSApplication;
using WARSApplication.Data.SharedDataProperties;

namespace Frontend.Authentication;

public interface IAuthApi
{
    [Post("/register")]
    public Task RegisterAsync(RegisterRequest request);
    
    [Post("/login")]
    public Task<TokenResponse> LoginAsync(LoginRequest request);
}