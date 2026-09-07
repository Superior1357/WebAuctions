using Frontend.BuyerMode;

namespace Frontend;

public class BuyerRole(IViewController controller) : IView
{
    public Task<IViewResult> RunAsync() => controller.ShowViewAsync<ListAllSellersMode>();
}