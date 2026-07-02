using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using AfriChat.Models;
using AfriChat.Services;
using System.Collections.ObjectModel;

namespace AfriChat.ViewModels;

// ── Base ──────────────────────────────────────────────────────────────
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _title = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    public bool IsNotBusy => !IsBusy;
}

// ── Chats ─────────────────────────────────────────────────────────────
public partial class ChatsViewModel : BaseViewModel
{
    private readonly IMessagingService _messaging;
    private readonly IAuthService _auth;

    [ObservableProperty] private ObservableCollection<Chat> _chats = new();
    [ObservableProperty] private ObservableCollection<Chat> _filteredChats = new();
    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private User? _currentUser;

    public ChatsViewModel(IMessagingService messaging, IAuthService auth)
    {
        _messaging = messaging;
        _auth = auth;
        Title = "AfriChat";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            CurrentUser = await _auth.GetCurrentUserAsync();
            var chats = await _messaging.GetChatsAsync();
            Chats = new ObservableCollection<Chat>(chats.OrderByDescending(c => c.UpdatedAt));
            FilteredChats = new ObservableCollection<Chat>(Chats);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public void Search(string query)
    {
        SearchQuery = query;
        if (string.IsNullOrWhiteSpace(query))
            FilteredChats = new ObservableCollection<Chat>(Chats);
        else
            FilteredChats = new ObservableCollection<Chat>(
                Chats.Where(c => c.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                                 (c.LastMessage?.Text.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)));
    }

    [RelayCommand]
    public async Task OpenChatAsync(Chat chat)
    {
        await Shell.Current.GoToAsync($"ChatDetailPage?chatId={chat.Id}&chatName={Uri.EscapeDataString(chat.Name)}");
    }

    [RelayCommand]
    public async Task NewChatAsync()
    {
        await Shell.Current.DisplayAlert("New Chat", "Contact search coming soon!", "OK");
    }
}

// ── Chat Detail ───────────────────────────────────────────────────────
[QueryProperty(nameof(ChatId), "chatId")]
[QueryProperty(nameof(ChatName), "chatName")]
public partial class ChatDetailViewModel : BaseViewModel
{
    private readonly IMessagingService _messaging;
    private readonly IPaymentService _payment;

    [ObservableProperty] private string _chatId = string.Empty;
    [ObservableProperty] private string _chatName = string.Empty;
    [ObservableProperty] private ObservableCollection<Message> _messages = new();
    [ObservableProperty] private string _messageText = string.Empty;
    [ObservableProperty] private bool _isEncrypted = true;

    public ChatDetailViewModel(IMessagingService messaging, IPaymentService payment)
    {
        _messaging = messaging;
        _payment = payment;
    }

    [RelayCommand]
    public async Task LoadMessagesAsync()
    {
        if (string.IsNullOrEmpty(ChatId)) return;
        IsBusy = true;
        try
        {
            var msgs = await _messaging.GetMessagesAsync(ChatId);
            Messages = new ObservableCollection<Message>(msgs);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText)) return;
        var msg = new Message
        {
            ChatId = ChatId, Text = MessageText,
            Direction = MessageDirection.Outgoing, Type = MessageType.Text
        };
        MessageText = string.Empty;
        var sent = await _messaging.SendMessageAsync(msg);
        Messages.Add(sent);
    }

    [RelayCommand]
    public async Task OpenSendMoneyAsync()
    {
        await Shell.Current.GoToAsync($"SendMoneyPage?chatId={ChatId}&chatName={Uri.EscapeDataString(ChatName)}");
    }

    [RelayCommand]
    public async Task ReportBorderAlertAsync()
    {
        await Shell.Current.GoToAsync("BorderAlertsPage");
    }
}

// ── Wallet ────────────────────────────────────────────────────────────
public partial class WalletViewModel : BaseViewModel
{
    private readonly IPaymentService _payment;

    [ObservableProperty] private Wallet? _wallet;
    [ObservableProperty] private ObservableCollection<ExchangeRate> _rates = new();
    [ObservableProperty] private ObservableCollection<Models.Currency> _currencies = new();
    [ObservableProperty] private decimal _convertAmount = 100;
    [ObservableProperty] private string _fromCurrency = "KES";
    [ObservableProperty] private string _toCurrency = "UGX";
    [ObservableProperty] private decimal _convertedAmount;
    [ObservableProperty] private string _balanceDisplay = "KES 0.00";

    public WalletViewModel(IPaymentService payment)
    {
        _payment = payment;
        Title = "Wallet";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            Wallet = await _payment.GetWalletAsync();
            BalanceDisplay = $"{Wallet.Currency} {Wallet.Balance:N2}";
            var rates = await _payment.GetExchangeRatesAsync();
            Rates = new ObservableCollection<ExchangeRate>(rates);
            var currencies = await _payment.GetSupportedCurrenciesAsync();
            Currencies = new ObservableCollection<Models.Currency>(currencies);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task ConvertAsync()
    {
        ConvertedAmount = await _payment.ConvertCurrencyAsync(ConvertAmount, FromCurrency, ToCurrency);
    }

    [RelayCommand]
    public async Task SendMoneyAsync()
    {
        await Shell.Current.GoToAsync("SendMoneyPage");
    }

    [RelayCommand]
    public async Task TopUpAsync()
    {
        await Shell.Current.DisplayAlert("Top Up", "Connect M-Pesa, Airtel Money, or bank account.", "OK");
    }
}

// ── Send Money ────────────────────────────────────────────────────────
[QueryProperty(nameof(ChatId), "chatId")]
[QueryProperty(nameof(RecipientName), "chatName")]
public partial class SendMoneyViewModel : BaseViewModel
{
    private readonly IPaymentService _payment;
    private readonly IMessagingService _messaging;

    [ObservableProperty] private string _chatId = string.Empty;
    [ObservableProperty] private string _recipientName = string.Empty;
    [ObservableProperty] private string _recipientId = string.Empty;
    [ObservableProperty] private decimal _amount;
    [ObservableProperty] private string _selectedCurrency = "KES";
    [ObservableProperty] private string _note = string.Empty;
    [ObservableProperty] private decimal _estimatedFee;
    [ObservableProperty] private decimal _convertedAmount;
    [ObservableProperty] private string _targetCurrency = "UGX";
    [ObservableProperty] private ObservableCollection<Models.Currency> _currencies = new();
    [ObservableProperty] private bool _isSuccess;

    public SendMoneyViewModel(IPaymentService payment, IMessagingService messaging)
    {
        _payment = payment;
        _messaging = messaging;
        Title = "Send Money";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        var currencies = await _payment.GetSupportedCurrenciesAsync();
        Currencies = new ObservableCollection<Models.Currency>(currencies);
    }

    partial void OnAmountChanged(decimal value)
    {
        EstimatedFee = Math.Round(value * 0.005m, 2);
    }

    [RelayCommand]
    public async Task SendAsync()
    {
        if (Amount <= 0) return;
        IsBusy = true;
        try
        {
            var transfer = await _payment.SendMoneyAsync(RecipientId, Amount, SelectedCurrency, Note);
            if (!string.IsNullOrEmpty(ChatId))
                await _messaging.SendMoneyMessageAsync(ChatId, transfer);
            IsSuccess = true;
            await Shell.Current.DisplayAlert("Sent! 🎉", $"{SelectedCurrency} {Amount:N2} sent successfully.\nRef: {transfer.Reference}", "Done");
            await Shell.Current.GoToAsync("..");
        }
        finally { IsBusy = false; }
    }
}

// ── Marketplace ───────────────────────────────────────────────────────
public partial class MarketplaceViewModel : BaseViewModel
{
    private readonly IMarketplaceService _marketplace;

    [ObservableProperty] private ObservableCollection<ProductListing> _products = new();
    [ObservableProperty] private ObservableCollection<TradeOpportunity> _opportunities = new();
    [ObservableProperty] private bool _showProducts = true;
    [ObservableProperty] private string _searchQuery = string.Empty;

    public MarketplaceViewModel(IMarketplaceService marketplace)
    {
        _marketplace = marketplace;
        Title = "Marketplace";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var products = await _marketplace.GetListingsAsync();
            Products = new ObservableCollection<ProductListing>(products);
            var opps = await _marketplace.GetTradeOpportunitiesAsync();
            Opportunities = new ObservableCollection<TradeOpportunity>(opps);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task OpenProductAsync(ProductListing product)
    {
        await Shell.Current.GoToAsync($"ProductDetailPage?productId={product.Id}");
    }

    [RelayCommand]
    public async Task PostListingAsync()
    {
        await Shell.Current.DisplayAlert("Post Listing", "Create a new product listing.", "OK");
    }

    [RelayCommand]
    public void ToggleView()
    {
        ShowProducts = !ShowProducts;
    }
}

// ── Border Alerts ─────────────────────────────────────────────────────
public partial class BorderAlertsViewModel : BaseViewModel
{
    private readonly IBorderAlertService _borderService;
    private readonly IMessagingService _messaging;

    [ObservableProperty] private ObservableCollection<BorderAlert> _alerts = new();
    [ObservableProperty] private BorderAlert? _selectedBorderPost;

    // New alert form
    [ObservableProperty] private string _selectedBorder = "Beit Bridge";
    [ObservableProperty] private string _selectedDelayType = "Traffic / Congestion";
    [ObservableProperty] private string _selectedWaitTime = "3–6 hours";
    [ObservableProperty] private string _alertDescription = string.Empty;

    public List<string> BorderPosts => new() { "Beit Bridge (ZA–ZW)", "Malaba (UG–KE)", "Chirundu (ZM–ZW)", "Kazungula (ZM–BW)", "Moyale (KE–ET)", "Nimule (SS–UG)", "Kasumbalesa (ZM–CD)", "Other" };
    public List<string> DelayTypes => new() { "Document checks", "Traffic / Congestion", "Customs inspection", "Border closed", "Strike / Protest", "Technical issues" };
    public List<string> WaitTimes => new() { "Under 1 hour", "1–3 hours", "3–6 hours", "6–12 hours", "12+ hours", "Border closed" };

    public BorderAlertsViewModel(IBorderAlertService borderService, IMessagingService messaging)
    {
        _borderService = borderService;
        _messaging = messaging;
        Title = "Border Watch";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var alerts = await _borderService.GetActiveAlertsAsync();
            Alerts = new ObservableCollection<BorderAlert>(alerts);
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task SubmitAlertAsync()
    {
        if (string.IsNullOrWhiteSpace(AlertDescription)) return;
        IsBusy = true;
        try
        {
            var alert = new BorderAlert
            {
                BorderName = SelectedBorder,
                DelayType = DelayType.TrafficCongestion,
                EstimatedWait = SelectedWaitTime,
                Description = AlertDescription,
                Severity = SeverityLevel.Medium,
                ReportedBy = "You",
                ConfirmationCount = 1
            };
            var saved = await _borderService.ReportAlertAsync(alert);
            Alerts.Insert(0, saved);
            AlertDescription = string.Empty;
            await Shell.Current.DisplayAlert("Alert Submitted", "Your border report has been shared with traders nearby.", "OK");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    public async Task ConfirmAlertAsync(BorderAlert alert)
    {
        await _borderService.ConfirmAlertAsync(alert.Id);
        alert.ConfirmationCount++;
    }
}

// ── Profile ───────────────────────────────────────────────────────────
public partial class ProfileViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly IPaymentService _payment;

    [ObservableProperty] private User? _user;
    [ObservableProperty] private Wallet? _wallet;

    public ProfileViewModel(IAuthService auth, IPaymentService payment)
    {
        _auth = auth;
        _payment = payment;
        Title = "Profile";
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        User = await _auth.GetCurrentUserAsync();
        Wallet = await _payment.GetWalletAsync();
    }

    [RelayCommand]
    public async Task EditProfileAsync()
    {
        await Shell.Current.DisplayAlert("Edit Profile", "Profile editing coming soon!", "OK");
    }
}
