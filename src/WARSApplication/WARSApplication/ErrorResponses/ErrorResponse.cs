using System.Text.Json.Serialization;
using WARSApplication.ErrorResponses.BuyErrorResponses;
using WARSApplication.ErrorResponses.SellErrorResponses;

namespace WARSApplication.ErrorResponses;

/// <summary>
/// Base class for all API error responses.
/// </summary>
/// <param name="Message"></param>
public abstract record ErrorResponse(string Message);