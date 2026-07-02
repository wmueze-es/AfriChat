using System.Security.Cryptography;
using System.Text;
using AfriChat.Models;

namespace AfriChat.Services;

// ── Interfaces ────────────────────────────────────────────────────────

public interface IEncryptionService
{
    string Encrypt(string plainText, string recipientPublicKey);
    string Decrypt(string cipherText, string privateKey);
    (string PublicKey, string PrivateKey) GenerateKeyPair();
    string GenerateConversationKey();
    bool VerifySignature(string message, string signature, string publicKey);
}

public interface IMessagingService
{
    Task<List<Chat>> GetChatsAsync();
    Task<List<Message>> GetMessagesAsync(string chatId);
    Task<Message> SendMessageAsync(Message message);
    Task<Message> SendMoneyMessageAsync(string chatId, MoneyTransfer transfer);
    Task<Message> SendProductListingAsync(string chatId, ProductListing product);
    Task<Message> SendBorderAlertAsync(string chatId, BorderAlert alert);
    event EventHandler<Message> MessageReceived;
}

public interface IPaymentService
{
    Task<MoneyTransfer> SendMoneyAsync(string toUserId, decimal amount, string currency, string note);
    Task<MoneyTransfer> RequestMoneyAsync(string fromUserId, decimal amount, string currency, string note);
    Task<Wallet> GetWalletAsync();
    Task<List<ExchangeRate>> GetExchangeRatesAsync();
    Task<decimal> ConvertCurrencyAsync(decimal amount, string from, string to);
    Task<List<Currency>> GetSupportedCurrenciesAsync();
}

public interface IBorderAlertService
{
    Task<List<BorderAlert>> GetActiveAlertsAsync();
    Task<BorderAlert> ReportAlertAsync(BorderAlert alert);
    Task<bool> ConfirmAlertAsync(string alertId);
    Task<List<BorderAlert>> GetAlertsByBorderAsync(string borderName);
}

public interface IMarketplaceService
{
    Task<List<ProductListing>> GetListingsAsync(ProductCategory? category = null, string? country = null);
    Task<ProductListing> PostListingAsync(ProductListing listing);
    Task<List<TradeOpportunity>> GetTradeOpportunitiesAsync();
    Task<TradeOpportunity> PostOpportunityAsync(TradeOpportunity opportunity);
}

public interface IAuthService
{
    Task<User> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<User> LoginAsync(string phone, string otp);
    Task LogoutAsync();
}

// ── Implementations ───────────────────────────────────────────────────

/// <summary>
/// Signal-Protocol-inspired end-to-end encryption using AES-256-GCM + RSA key exchange.
/// In production replace with libsignal-protocol-dotnet.
/// </summary>
public class EncryptionService : IEncryptionService
{
    public (string PublicKey, string PrivateKey) GenerateKeyPair()
    {
        using var rsa = RSA.Create(2048);
        return (
            Convert.ToBase64String(rsa.ExportRSAPublicKey()),
            Convert.ToBase64String(rsa.ExportRSAPrivateKey())
        );
    }

    public string GenerateConversationKey()
    {
        var key = new byte[32];
        RandomNumberGenerator.Fill(key);
        return Convert.ToBase64String(key);
    }

    public string Encrypt(string plainText, string recipientPublicKey)
    {
        // 1. Generate ephemeral AES session key
        var sessionKey = new byte[32];
        var iv = new byte[12];
        RandomNumberGenerator.Fill(sessionKey);
        RandomNumberGenerator.Fill(iv);

        // 2. Encrypt message with AES-256-GCM
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        using var aes = new AesGcm(sessionKey, 16);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];
        aes.Encrypt(iv, plainBytes, cipherBytes, tag);

        // 3. Wrap session key with recipient RSA public key
        using var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(Convert.FromBase64String(recipientPublicKey), out _);
        var encryptedKey = rsa.Encrypt(sessionKey, RSAEncryptionPadding.OaepSHA256);

        // 4. Bundle: encryptedKey | iv | tag | ciphertext
        var bundle = Convert.ToBase64String(encryptedKey) + "." +
                     Convert.ToBase64String(iv) + "." +
                     Convert.ToBase64String(tag) + "." +
                     Convert.ToBase64String(cipherBytes);
        return bundle;
    }

    public string Decrypt(string cipherText, string privateKey)
    {
        var parts = cipherText.Split('.');
        if (parts.Length != 4) return "[Encrypted message]";

        var encryptedKey = Convert.FromBase64String(parts[0]);
        var iv = Convert.FromBase64String(parts[1]);
        var tag = Convert.FromBase64String(parts[2]);
        var cipherBytes = Convert.FromBase64String(parts[3]);

        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(Convert.FromBase64String(privateKey), out _);
        var sessionKey = rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);

        using var aes = new AesGcm(sessionKey, 16);
        var plainBytes = new byte[cipherBytes.Length];
        aes.Decrypt(iv, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }

    public bool VerifySignature(string message, string signature, string publicKey) => true; // Simplified
}

/// <summary>Mock messaging service — replace with WebSocket/SignalR in production.</summary>
public class MessagingService : IMessagingService
{
    public event EventHandler<Message>? MessageReceived;

    private readonly List<Chat> _chats = SeedData.Chats;
    private readonly Dictionary<string, List<Message>> _messages = SeedData.Messages;

    public Task<List<Chat>> GetChatsAsync() => Task.FromResult(_chats);

    public Task<List<Message>> GetMessagesAsync(string chatId) =>
        Task.FromResult(_messages.TryGetValue(chatId, out var msgs) ? msgs : new List<Message>());

    public Task<Message> SendMessageAsync(Message message)
    {
        message.Status = MessageStatus.Sent;
        if (!_messages.ContainsKey(message.ChatId))
            _messages[message.ChatId] = new();
        _messages[message.ChatId].Add(message);
        var chat = _chats.FirstOrDefault(c => c.Id == message.ChatId);
        if (chat != null) { chat.LastMessage = message; chat.UpdatedAt = DateTime.UtcNow; }
        return Task.FromResult(message);
    }

    public Task<Message> SendMoneyMessageAsync(string chatId, MoneyTransfer transfer)
    {
        var msg = new Message
        {
            ChatId = chatId, Type = MessageType.MoneyTransfer,
            Direction = MessageDirection.Outgoing, MoneyTransfer = transfer,
            Text = $"Sent {transfer.Currency} {transfer.Amount:N2} via AfriPay"
        };
        return SendMessageAsync(msg);
    }

    public Task<Message> SendProductListingAsync(string chatId, ProductListing product)
    {
        var msg = new Message
        {
            ChatId = chatId, Type = MessageType.ProductListing,
            Direction = MessageDirection.Outgoing, ProductListing = product,
            Text = $"Listed: {product.Title}"
        };
        return SendMessageAsync(msg);
    }

    public Task<Message> SendBorderAlertAsync(string chatId, BorderAlert alert)
    {
        var msg = new Message
        {
            ChatId = chatId, Type = MessageType.BorderAlert,
            Direction = MessageDirection.Outgoing, BorderAlert = alert,
            Text = $"⚠️ Border alert: {alert.BorderName}"
        };
        return SendMessageAsync(msg);
    }
}

public class PaymentService : IPaymentService
{
    private readonly Wallet _wallet = new()
    {
        UserId = "me", Balance = 45_230.00m, Currency = "KES",
        Transactions = SeedData.WalletTransactions
    };

    private readonly List<ExchangeRate> _rates = new()
    {
        new() { FromCurrency = "KES", ToCurrency = "UGX", Rate = 29.4m },
        new() { FromCurrency = "KES", ToCurrency = "NGN", Rate = 5.2m },
        new() { FromCurrency = "KES", ToCurrency = "GHS", Rate = 0.048m },
        new() { FromCurrency = "KES", ToCurrency = "ZAR", Rate = 0.137m },
        new() { FromCurrency = "KES", ToCurrency = "TZS", Rate = 17.8m },
        new() { FromCurrency = "KES", ToCurrency = "ETB", Rate = 0.39m },
        new() { FromCurrency = "KES", ToCurrency = "XOF", Rate = 3.1m },
        new() { FromCurrency = "USD", ToCurrency = "KES", Rate = 129.5m },
    };

    public Task<MoneyTransfer> SendMoneyAsync(string toUserId, decimal amount, string currency, string note)
    {
        var transfer = new MoneyTransfer
        {
            FromUserId = "me", ToUserId = toUserId, Amount = amount,
            Currency = currency, Note = note, Status = TransactionStatus.Completed,
            Reference = $"AC{DateTime.UtcNow:yyMMddHHmmss}", Fee = amount * 0.005m
        };
        _wallet.Balance -= amount;
        return Task.FromResult(transfer);
    }

    public Task<MoneyTransfer> RequestMoneyAsync(string fromUserId, decimal amount, string currency, string note)
        => Task.FromResult(new MoneyTransfer { ToUserId = "me", FromUserId = fromUserId, Amount = amount, Currency = currency, Note = note, Status = TransactionStatus.Pending });

    public Task<Wallet> GetWalletAsync() => Task.FromResult(_wallet);

    public Task<List<ExchangeRate>> GetExchangeRatesAsync() => Task.FromResult(_rates);

    public Task<decimal> ConvertCurrencyAsync(decimal amount, string from, string to)
    {
        var rate = _rates.FirstOrDefault(r => r.FromCurrency == from && r.ToCurrency == to);
        return Task.FromResult(rate != null ? amount * rate.Rate : amount);
    }

    public Task<List<Currency>> GetSupportedCurrenciesAsync() => Task.FromResult(SeedData.Currencies);
}

public class BorderAlertService : IBorderAlertService
{
    private readonly List<BorderAlert> _alerts = SeedData.BorderAlerts;

    public Task<List<BorderAlert>> GetActiveAlertsAsync() =>
        Task.FromResult(_alerts.Where(a => a.IsActive).OrderByDescending(a => a.ReportedAt).ToList());

    public Task<BorderAlert> ReportAlertAsync(BorderAlert alert)
    {
        _alerts.Add(alert);
        return Task.FromResult(alert);
    }

    public Task<bool> ConfirmAlertAsync(string alertId)
    {
        var alert = _alerts.FirstOrDefault(a => a.Id == alertId);
        if (alert != null) alert.ConfirmationCount++;
        return Task.FromResult(true);
    }

    public Task<List<BorderAlert>> GetAlertsByBorderAsync(string borderName) =>
        Task.FromResult(_alerts.Where(a => a.BorderName == borderName && a.IsActive).ToList());
}

public class MarketplaceService : IMarketplaceService
{
    private readonly List<ProductListing> _listings = SeedData.Products;
    private readonly List<TradeOpportunity> _opportunities = SeedData.TradeOpportunities;

    public Task<List<ProductListing>> GetListingsAsync(ProductCategory? category = null, string? country = null)
    {
        var query = _listings.AsEnumerable();
        if (category.HasValue) query = query.Where(p => p.Category == category.Value);
        if (!string.IsNullOrEmpty(country)) query = query.Where(p => p.SellerCountry == country);
        return Task.FromResult(query.ToList());
    }

    public Task<ProductListing> PostListingAsync(ProductListing listing)
    {
        _listings.Add(listing);
        return Task.FromResult(listing);
    }

    public Task<List<TradeOpportunity>> GetTradeOpportunitiesAsync() => Task.FromResult(_opportunities);

    public Task<TradeOpportunity> PostOpportunityAsync(TradeOpportunity opportunity)
    {
        _opportunities.Add(opportunity);
        return Task.FromResult(opportunity);
    }
}

public class AuthService : IAuthService
{
    private readonly User _currentUser = new()
    {
        Id = "me", Name = "Amara Diallo", Phone = "+254712345678",
        Country = "Kenya", AfriPayId = "@amarad", IsVerified = true, IsOnline = true
    };

    public Task<User> GetCurrentUserAsync() => Task.FromResult(_currentUser);
    public Task<bool> IsAuthenticatedAsync() => Task.FromResult(true);
    public Task<User> LoginAsync(string phone, string otp) => Task.FromResult(_currentUser);
    public Task LogoutAsync() => Task.CompletedTask;
}

// ── Seed Data ─────────────────────────────────────────────────────────
public static class SeedData
{
    public static List<Models.Currency> Currencies => new()
    {
        new() { Code="KES", Name="Kenyan Shilling", Flag="🇰🇪", Country="Kenya" },
        new() { Code="UGX", Name="Ugandan Shilling", Flag="🇺🇬", Country="Uganda" },
        new() { Code="NGN", Name="Nigerian Naira", Flag="🇳🇬", Country="Nigeria" },
        new() { Code="GHS", Name="Ghanaian Cedi", Flag="🇬🇭", Country="Ghana" },
        new() { Code="ZAR", Name="South African Rand", Flag="🇿🇦", Country="South Africa" },
        new() { Code="TZS", Name="Tanzanian Shilling", Flag="🇹🇿", Country="Tanzania" },
        new() { Code="ETB", Name="Ethiopian Birr", Flag="🇪🇹", Country="Ethiopia" },
        new() { Code="XOF", Name="West African CFA", Flag="🇸🇳", Country="Senegal" },
        new() { Code="USD", Name="US Dollar", Flag="🇺🇸", Country="International" },
    };

    public static List<Chat> Chats => new()
    {
        new() { Id="c1", Name="Kampala Market Group", Type=ChatType.Group, UnreadCount=12, UpdatedAt=DateTime.UtcNow.AddMinutes(-5), CategoryTag="market",
            LastMessage=new Message { Text="Fresh maize — 500kg available now", Timestamp=DateTime.UtcNow.AddMinutes(-5) } },
        new() { Id="c2", Name="Kofi Boateng", Type=ChatType.Direct, UnreadCount=3, UpdatedAt=DateTime.UtcNow.AddMinutes(-23),
            LastMessage=new Message { Text="Payment received, thanks!", Timestamp=DateTime.UtcNow.AddMinutes(-23) } },
        new() { Id="c3", Name="Beit Bridge Alerts", Type=ChatType.Channel, UnreadCount=1, UpdatedAt=DateTime.UtcNow.AddMinutes(-45), CategoryTag="border",
            LastMessage=new Message { Text="Heavy delays at ZA–ZW border", Timestamp=DateTime.UtcNow.AddMinutes(-45) } },
        new() { Id="c4", Name="Lagos Trade Network", Type=ChatType.Group, UnreadCount=0, UpdatedAt=DateTime.UtcNow.AddHours(-3),
            LastMessage=new Message { Text="New buyer for palm oil — 2 tonnes", Timestamp=DateTime.UtcNow.AddHours(-3) } },
        new() { Id="c5", Name="Fatima Mensah", Type=ChatType.Direct, UnreadCount=0, UpdatedAt=DateTime.UtcNow.AddDays(-1),
            LastMessage=new Message { Text="Can you deliver to Accra port?", Timestamp=DateTime.UtcNow.AddDays(-1) } },
        new() { Id="c6", Name="East Africa Logistics", Type=ChatType.Group, UnreadCount=0, UpdatedAt=DateTime.UtcNow.AddDays(-2),
            LastMessage=new Message { Text="Nairobi–Mombasa route open again ✅", Timestamp=DateTime.UtcNow.AddDays(-2) } },
    };

    public static Dictionary<string, List<Message>> Messages => new()
    {
        ["c1"] = new()
        {
            new() { Id="m1", ChatId="c1", SenderName="Kofi Boateng", Direction=MessageDirection.Incoming, Text="Good morning traders! Who has maize stock today?" },
            new() { Id="m2", ChatId="c1", SenderName="Grace Osei", Direction=MessageDirection.Incoming, Text="I have 500kg dry maize at Nakasero market. UGX 1,200/kg.",
                Type=MessageType.ProductListing, ProductListing=new ProductListing { Title="Dry Maize — Grade A", Price=1200, Currency="UGX", Unit="kg", Quantity="500kg", Location="Nakasero, Kampala", Emoji="🌽", SellerName="Grace Osei", SellerCountry="Uganda", Category=ProductCategory.ProduceAndCrops } },
            new() { Id="m3", ChatId="c1", Direction=MessageDirection.Outgoing, Text="Very interested! Can you do bulk pricing for 200kg?" },
            new() { Id="m4", ChatId="c1", SenderName="Grace Osei", Direction=MessageDirection.Incoming, Text="Yes — UGX 1,050/kg for 200kg+. Delivery within Kampala." },
            new() { Id="m5", ChatId="c1", Direction=MessageDirection.Outgoing, Type=MessageType.MoneyTransfer,
                MoneyTransfer=new MoneyTransfer { Amount=210000, Currency="UGX", Note="200kg maize deposit", Status=TransactionStatus.Completed, Reference="AC240702091523" } },
            new() { Id="m6", ChatId="c1", SenderName="Grace Osei", Direction=MessageDirection.Incoming, Text="✅ Payment received! Delivery confirmed for 2pm. See you then." },
            new() { Id="m7", ChatId="c1", SenderName="David Nkosi", Direction=MessageDirection.Incoming, Type=MessageType.BorderAlert,
                BorderAlert=new BorderAlert { BorderName="Malaba Border", CountryA="Uganda", CountryB="Kenya", DelayType=DelayType.DocumentChecks, EstimatedWait="6 hours", Severity=SeverityLevel.High, Description="Document checks causing heavy delays for all commercial vehicles.", ConfirmationCount=8, ReportedBy="David Nkosi" } },
            new() { Id="m8", ChatId="c1", Direction=MessageDirection.Outgoing, Text="Thanks David! @everyone — consider routing via Busia for urgent shipments today." },
        }
    };

    public static List<BorderAlert> BorderAlerts => new()
    {
        new() { BorderName="Beit Bridge", CountryA="South Africa", CountryB="Zimbabwe", DelayType=DelayType.TrafficCongestion, EstimatedWait="8+ hours", Severity=SeverityLevel.Critical, Description="Severe congestion. Commercial vehicles backing up 15km on SA side.", ConfirmationCount=34, ReportedBy="Tendai M.", ReportedAt=DateTime.UtcNow.AddHours(-1) },
        new() { BorderName="Malaba Border", CountryA="Uganda", CountryB="Kenya", DelayType=DelayType.DocumentChecks, EstimatedWait="6 hours", Severity=SeverityLevel.High, Description="New documentation requirements for electronics imports. Customs officers checking all manifests.", ConfirmationCount=18, ReportedBy="David Nkosi", ReportedAt=DateTime.UtcNow.AddMinutes(-45) },
        new() { BorderName="Chirundu", CountryA="Zambia", CountryB="Zimbabwe", DelayType=DelayType.CustomsInspection, EstimatedWait="3 hours", Severity=SeverityLevel.Medium, Description="Random inspection campaign in effect until end of week.", ConfirmationCount=9, ReportedBy="Chisomo K.", ReportedAt=DateTime.UtcNow.AddHours(-2) },
        new() { BorderName="Moyale", CountryA="Kenya", CountryB="Ethiopia", DelayType=DelayType.TechnicalIssues, EstimatedWait="2 hours", Severity=SeverityLevel.Low, Description="ASYCUDA system intermittently down. Manual processing ongoing.", ConfirmationCount=5, ReportedBy="Biruk T.", ReportedAt=DateTime.UtcNow.AddHours(-3) },
    };

    public static List<ProductListing> Products => new()
    {
        new() { Title="Arabica Coffee Beans", Description="Premium grade washed Arabica from Mount Kenya region. Consistent quality, 60kg bags.", Price=320, Currency="USD", Unit="60kg bag", Quantity="200 bags", Location="Nairobi, Kenya", SellerName="James Mwangi", SellerCountry="Kenya", Category=ProductCategory.ProduceAndCrops, Emoji="☕" },
        new() { Title="Ankara Fabric Rolls", Description="High-quality 100% cotton Ankara prints. 12 yards per roll, 80+ designs available.", Price=45, Currency="USD", Unit="roll", Quantity="500 rolls", Location="Lagos, Nigeria", SellerName="Chidi Okafor", SellerCountry="Nigeria", Category=ProductCategory.TextilesAndClothing, Emoji="🎨" },
        new() { Title="Shea Butter (Raw)", Description="Unrefined grade A shea butter from Tamale region. Certified organic.", Price=2.8m, Currency="USD", Unit="kg", Quantity="2,000 kg", Location="Tamale, Ghana", SellerName="Abena Asante", SellerCountry="Ghana", Category=ProductCategory.ProduceAndCrops, Emoji="🧴" },
        new() { Title="Sesame Seeds", Description="White hulled sesame seeds, moisture <6%, FFA <1%. Export quality.", Price=1.45m, Currency="USD", Unit="kg", Quantity="50 tonnes", Location="Khartoum, Sudan", SellerName="Omar Hassan", SellerCountry="Sudan", Category=ProductCategory.ProduceAndCrops, Emoji="🌿" },
        new() { Title="Solar Panel Installation", Description="Residential and commercial solar systems 5kW–100kW. Full installation and maintenance.", Price=850, Currency="USD", Unit="kW installed", Quantity="Open", Location="Johannesburg, SA", SellerName="Thabo Dlamini", SellerCountry="South Africa", Category=ProductCategory.Services, Emoji="☀️" },
    };

    public static List<TradeOpportunity> TradeOpportunities => new()
    {
        new() { PosterName="Rashid Al-Amin", PosterCountry="Tanzania", Type=TradeType.Buying, Sector="Agriculture", Title="Looking for 100 tonnes of sisal fibre", Description="Established textile manufacturer seeking consistent supply of raw sisal. Long-term contract available.", TargetCountries=new(){"Kenya","Tanzania","Mozambique"} },
        new() { PosterName="Marie Kouassi", PosterCountry="Ivory Coast", Type=TradeType.Partnership, Sector="Logistics", Title="Co-invest in cross-border cold chain", Description="Seeking logistics partner for refrigerated transport network connecting ECOWAS countries.", TargetCountries=new(){"Ghana","Senegal","Nigeria"} },
    };

    public static List<WalletTransaction> WalletTransactions => new()
    {
        new() { Description="Received from Kofi Boateng", Amount=15000, Currency="KES", IsCredit=true, Date=DateTime.Today, Category="Payment" },
        new() { Description="Sent to Grace Osei", Amount=210000, Currency="UGX", IsCredit=false, Date=DateTime.Today.AddDays(-1), Category="Transfer" },
        new() { Description="Market sale — coffee", Amount=41600, Currency="KES", IsCredit=true, Date=DateTime.Today.AddDays(-2), Category="Sales" },
        new() { Description="Sent to David Nkosi", Amount=5000, Currency="KES", IsCredit=false, Date=DateTime.Today.AddDays(-3), Category="Transfer" },
        new() { Description="Top up from M-Pesa", Amount=10000, Currency="KES", IsCredit=true, Date=DateTime.Today.AddDays(-5), Category="Top-up" },
    };
}
