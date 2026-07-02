using AfriChat.ViewModels;

namespace AfriChat.Views;

public partial class ChatsPage : ContentPage
{
    private readonly ChatsViewModel _vm;
    public ChatsPage(ChatsViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadCommand.Execute(null); }
}

public partial class ChatDetailPage : ContentPage
{
    private readonly ChatDetailViewModel _vm;
    public ChatDetailPage(ChatDetailViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadMessagesCommand.Execute(null); }
}

public partial class WalletPage : ContentPage
{
    private readonly WalletViewModel _vm;
    public WalletPage(WalletViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadCommand.Execute(null); }
}

public partial class MarketplacePage : ContentPage
{
    private readonly MarketplaceViewModel _vm;
    public MarketplacePage(MarketplaceViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadCommand.Execute(null); }
}

public partial class BorderAlertsPage : ContentPage
{
    private readonly BorderAlertsViewModel _vm;
    public BorderAlertsPage(BorderAlertsViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadCommand.Execute(null); }
}

public partial class SendMoneyPage : ContentPage
{
    private readonly SendMoneyViewModel _vm;
    public SendMoneyPage(SendMoneyViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadCommand.Execute(null); }
}

public partial class ProfilePage : ContentPage
{
    private readonly ProfileViewModel _vm;
    public ProfilePage(ProfileViewModel vm) { InitializeComponent(); BindingContext = _vm = vm; }
    protected override void OnAppearing() { base.OnAppearing(); _vm.LoadCommand.Execute(null); }
}

public partial class ProductDetailPage : ContentPage
{
    public ProductDetailPage() { Title = "Product"; }
}

public partial class OnboardingPage : ContentPage
{
    public OnboardingPage() { Title = "Welcome to AfriChat"; }
}
