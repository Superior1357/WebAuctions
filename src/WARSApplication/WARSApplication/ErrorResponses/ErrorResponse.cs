using System.Text.Json.Serialization;
using WARSApplication.ErrorResponses.BuyErrorResponses;
using WARSApplication.ErrorResponses.SellErrorResponses;

namespace WARSApplication.ErrorResponses;

public abstract record ErrorResponse(string Message);