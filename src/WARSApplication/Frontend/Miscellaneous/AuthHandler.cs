using System.Net.Http.Headers;
namespace Frontend.Miscellaneous;

/// <summary>
/// Authentication handler for the http client.
/// Attaches the Jwt token to the request header.
/// </summary>
/// <param name="tokenBearer">the Jwt token bearer</param>
public class AuthHandler(ITokenBearer tokenBearer) : DelegatingHandler
{
    /// <summary>
    /// Attaches the Jwt token to the request header.
    /// </summary>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = tokenBearer.Token;

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
        
        return await base.SendAsync(request, cancellationToken);
    }
}