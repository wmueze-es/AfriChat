using AfriChat.Views;

namespace AfriChat;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        RegisterRoutes();
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(ChatDetailPage), typeof(ChatDetailPage));
        Routing.RegisterRoute(nameof(SendMoneyPage), typeof(SendMoneyPage));
        Routing.RegisterRoute(nameof(ProductDetailPage), typeof(ProductDetailPage));
        Routing.RegisterRoute(nameof(OnboardingPage), typeof(OnboardingPage));
        Routing.RegisterRoute(nameof(BorderAlertsPage), typeof(BorderAlertsPage));
    }
}
