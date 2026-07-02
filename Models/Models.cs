namespace AfriChat.Models;

// ── User ──────────────────────────────────────────────────────────────
public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string AvatarUrl { get; set; } = string.Empty;
    public string AvatarInitials => Name.Length >= 2 ? Name[..2].ToUpper() : Name.ToUpper();
    public bool IsOnline { get; set; }
    public DateTime LastSeen { get; set; }
    public string AfriPayId { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
}

// ── Chat ──────────────────────────────────────────────────────────────
public enum ChatType { Direct, Group, Channel }

public class Chat
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public ChatType Type { get; set; }
    public List<User> Members { get; set; } = new();
    public Message? LastMessage { get; set; }
    public int UnreadCount { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsMuted { get; set; }
    public bool IsEncrypted { get; set; } = true;
    public string AvatarInitials => Name.Length >= 2 ? Name[..2].ToUpper() : Name.ToUpper();
    public string CategoryTag { get; set; } = string.Empty; // "market", "border", "trade"
}

// ── Message ───────────────────────────────────────────────────────────
public enum MessageType { Text, MoneyTransfer, ProductListing, BorderAlert, TradeOpportunity, Image, Document, VoiceNote }
public enum MessageStatus { Sent, Delivered, Read }
public enum MessageDirection { Outgoing, Incoming }

public class Message
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ChatId { get; set; } = string.Empty;
    public string SenderId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public MessageType Type { get; set; } = MessageType.Text;
    public MessageDirection Direction { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessageStatus Status { get; set; }
    public bool IsEncrypted { get; set; } = true;
    public bool IsDeleted { get; set; }
    public string? ReplyToMessageId { get; set; }

    // Rich message payloads
    public MoneyTransfer? MoneyTransfer { get; set; }
    public ProductListing? ProductListing { get; set; }
    public BorderAlert? BorderAlert { get; set; }
    public TradeOpportunity? TradeOpportunity { get; set; }

    public string TimeDisplay => Timestamp.ToLocalTime().ToString("HH:mm");
    public bool IsOutgoing => Direction == MessageDirection.Outgoing;
}

// ── AfriPay / Money Transfer ──────────────────────────────────────────
public enum TransactionStatus { Pending, Completed, Failed, Refunded }

public class MoneyTransfer
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string FromUserId { get; set; } = string.Empty;
    public string ToUserId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public decimal ReceivedAmount { get; set; }
    public string ReceivedCurrency { get; set; } = "KES";
    public decimal ExchangeRate { get; set; } = 1m;
    public string Note { get; set; } = string.Empty;
    public TransactionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Reference { get; set; } = string.Empty;
    public decimal Fee { get; set; }
    public string StatusDisplay => Status switch
    {
        TransactionStatus.Completed => "✅ Completed",
        TransactionStatus.Pending => "⏳ Processing",
        TransactionStatus.Failed => "❌ Failed",
        TransactionStatus.Refunded => "↩️ Refunded",
        _ => "Unknown"
    };
}

public class Wallet
{
    public string UserId { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public string Currency { get; set; } = "KES";
    public List<WalletTransaction> Transactions { get; set; } = new();
    public bool IsPinSet { get; set; }
    public bool IsBiometricEnabled { get; set; }
}

public class WalletTransaction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "KES";
    public bool IsCredit { get; set; }
    public DateTime Date { get; set; }
    public string Category { get; set; } = string.Empty;
    public string AmountDisplay => (IsCredit ? "+" : "-") + $"{Amount:N2} {Currency}";
    public Color AmountColor => IsCredit ? Color.FromArgb("#087397") : Color.FromArgb("#0B759A");
}

public class ExchangeRate
{
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// ── Marketplace / Products ────────────────────────────────────────────
public enum ProductCategory
{
    ProduceAndCrops, Livestock, TextilesAndClothing,
    Electronics, Machinery, ProcessedGoods, Services, Other
}

public class ProductListing
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string SellerId { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public string SellerCountry { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductCategory Category { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public string Unit { get; set; } = string.Empty;
    public string Quantity { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public List<string> ImageUrls { get; set; } = new();
    public bool IsAvailable { get; set; } = true;
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public string Emoji { get; set; } = "📦";
    public string PriceDisplay => $"{Currency} {Price:N2}/{Unit}";
    public string CategoryDisplay => Category switch
    {
        ProductCategory.ProduceAndCrops => "🌾 Produce & Crops",
        ProductCategory.Livestock => "🐄 Livestock",
        ProductCategory.TextilesAndClothing => "👗 Textiles",
        ProductCategory.Electronics => "📱 Electronics",
        ProductCategory.Machinery => "⚙️ Machinery",
        ProductCategory.ProcessedGoods => "🏭 Processed Goods",
        ProductCategory.Services => "🤝 Services",
        _ => "📦 Other"
    };
}

// ── Border Alerts ─────────────────────────────────────────────────────
public enum DelayType { DocumentChecks, TrafficCongestion, CustomsInspection, BorderClosed, Strike, TechnicalIssues }
public enum SeverityLevel { Low, Medium, High, Critical }

public class BorderAlert
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string BorderName { get; set; } = string.Empty;
    public string CountryA { get; set; } = string.Empty;
    public string CountryB { get; set; } = string.Empty;
    public DelayType DelayType { get; set; }
    public string EstimatedWait { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public SeverityLevel Severity { get; set; }
    public string ReportedBy { get; set; } = string.Empty;
    public int ConfirmationCount { get; set; }
    public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public string TimeAgo => (DateTime.UtcNow - ReportedAt).TotalMinutes < 60
        ? $"{(int)(DateTime.UtcNow - ReportedAt).TotalMinutes}m ago"
        : $"{(int)(DateTime.UtcNow - ReportedAt).TotalHours}h ago";
    public string SeverityColor => Severity switch
    {
        SeverityLevel.Critical => "#4F91A8",
        SeverityLevel.High => "#087397",
        SeverityLevel.Medium => "#93C7D0",
        SeverityLevel.Low => "#0B759A",
        _ => "#4F91A8"
    };
    public string DelayIcon => DelayType switch
    {
        DelayType.DocumentChecks => "📋",
        DelayType.TrafficCongestion => "🚛",
        DelayType.CustomsInspection => "🔍",
        DelayType.BorderClosed => "🚫",
        DelayType.Strike => "✊",
        DelayType.TechnicalIssues => "⚙️",
        _ => "⚠️"
    };
    public string CrossingDisplay => $"{CountryA} ↔ {CountryB}";
}

// ── Trade Opportunities ───────────────────────────────────────────────
public enum TradeType { Buying, Selling, Partnership, Investment, Tender }

public class TradeOpportunity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string PosterId { get; set; } = string.Empty;
    public string PosterName { get; set; } = string.Empty;
    public string PosterCountry { get; set; } = string.Empty;
    public TradeType Type { get; set; }
    public string Sector { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> TargetCountries { get; set; } = new();
    public DateTime PostedAt { get; set; } = DateTime.UtcNow;
    public string TypeDisplay => Type switch
    {
        TradeType.Buying => "🛒 Buying",
        TradeType.Selling => "💼 Selling",
        TradeType.Partnership => "🤝 Partnership",
        TradeType.Investment => "📈 Investment",
        TradeType.Tender => "📑 Tender",
        _ => "💡 Opportunity"
    };
}

// ── Currency ──────────────────────────────────────────────────────────
public class Currency
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Flag { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Display => $"{Flag} {Code}";
}
