using System.Text.Json.Serialization;
using Backend.ErrorResponses.BuyErrorResponses;
using Backend.ErrorResponses.SellErrorResponses;

namespace Backend.ErrorResponses;

/// <summary>
/// Base class for all API error responses.
/// </summary>
/// <param name="Message"></param>
public abstract record ErrorResponse(string Message);