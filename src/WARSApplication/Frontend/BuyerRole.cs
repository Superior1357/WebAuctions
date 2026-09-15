using Frontend.BuyerMode;

namespace Frontend;

public class BuyerRole(IViewController controller) : IView
{
    public string Title => "Buyer role";
    public Task<IViewResult> RunAsync() => controller.ShowViewAsync<ListAllSellersMode>();
}
