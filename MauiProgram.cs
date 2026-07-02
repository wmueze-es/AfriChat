using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using AfriChat.Services;
using AfriChat.ViewModels;
using AfriChat.Views;

namespace AfriChat;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("Poppins-Regular.ttf", "PoppinsRegular");
                fonts.AddFont("Poppins-Medium.ttf", "PoppinsMedium");
                fonts.AddFont("Poppins-SemiBold.ttf", "PoppinsSemiBold");
                fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
            });

        // Services
        builder.Services.AddSingleton<IMessagingService, MessagingService>();
        builder.Services.AddSingleton<IEncryptionService, EncryptionService>();
        builder.Services.AddSingleton<IPaymentService, PaymentService>();
        builder.Services.AddSingleton<IBorderAlertService, BorderAlertService>();
        builder.Services.AddSingleton<IMarketplaceService, MarketplaceService>();
        builder.Services.AddSingleton<IAuthService, AuthService>();

        // ViewModels
        builder.Services.AddTransient<ChatsViewModel>();
        builder.Services.AddTransient<ChatDetailViewModel>();
        builder.Services.AddTransient<WalletViewModel>();
        builder.Services.AddTransient<MarketplaceViewModel>();
        builder.Services.AddTransient<BorderAlertsViewModel>();
        builder.Services.AddTransient<SendMoneyViewModel>();
        builder.Services.AddTransient<ProfileViewModel>();

        // Views / Pages
        builder.Services.AddTransient<ChatsPage>();
        builder.Services.AddTransient<ChatDetailPage>();
        builder.Services.AddTransient<WalletPage>();
        builder.Services.AddTransient<MarketplacePage>();
        builder.Services.AddTransient<BorderAlertsPage>();
        builder.Services.AddTransient<SendMoneyPage>();
        builder.Services.AddTransient<ProductDetailPage>();
        builder.Services.AddTransient<ProfilePage>();
        builder.Services.AddTransient<OnboardingPage>();

#if DEBUG
        // builder.Logging.AddDebug(); // requires Microsoft.Extensions.Logging.Debug package
#endif
        return builder.Build();
    }
}
