# 🌍 AfriChat — .NET MAUI iOS & Android App

Pan-African encrypted messaging, mobile money, marketplace, and border alerts — built with .NET MAUI targeting iOS 15+ and Android 7+.

---

## Project Structure

```
AfriChat/
├── AfriChat.csproj          # Multi-target: net9.0-android, net9.0-ios
├── MauiProgram.cs           # DI registration, fonts, plugins
├── App.xaml / AppShell.xaml # Application shell with bottom tab bar
│
├── Models/
│   └── Models.cs            # User, Chat, Message, MoneyTransfer,
│                             # ProductListing, BorderAlert, TradeOpportunity
│
├── Services/
│   └── Services.cs          # IEncryptionService  (AES-256-GCM + RSA)
│                             # IMessagingService   (WebSocket-ready)
│                             # IPaymentService     (AfriPay)
│                             # IBorderAlertService
│                             # IMarketplaceService
│                             # IAuthService
│                             # SeedData            (demo data)
│
├── ViewModels/
│   └── ViewModels.cs        # MVVM via CommunityToolkit.Mvvm
│                             # ChatsViewModel, ChatDetailViewModel,
│                             # WalletViewModel, SendMoneyViewModel,
│                             # MarketplaceViewModel, BorderAlertsViewModel,
│                             # ProfileViewModel
│
├── Views/
│   ├── ChatsPage.xaml           # Chat list with swipe-to-mute/delete
│   ├── ChatDetailPage.xaml      # Full conversation + feature bar
│   ├── WalletPage.xaml          # Balance, FX rates, currency converter
│   ├── SendMoneyPage.xaml       # Cross-border transfer flow
│   ├── MarketplacePage.xaml     # Products grid + trade opportunities
│   ├── BorderAlertsPage.xaml    # Live alerts + report form
│   ├── ProfilePage.xaml         # User profile + security settings
│   └── PageCodeBehinds.cs       # Minimal code-behind (OnAppearing hooks)
│
├── Controls/
│   └── MessageBubbleView.cs     # Custom control: Text / Money / Product / Alert bubbles
│
├── Converters/
│   └── Converters.cs            # IntToBool, InverseBool, CreditEmoji, BoolToColor
│
├── Resources/
│   └── Styles/
│       ├── Colors.xaml          # AfriGreen, AfriGold, AfriRed brand tokens
│       └── Styles.xaml          # Global styles (buttons, cards, labels)
│
└── Platforms/
    ├── Android/
    │   ├── MainActivity.cs
    │   ├── MainApplication.cs
    │   └── AndroidManifest.xml  # Camera, contacts, location, biometrics
    └── iOS/
        ├── AppDelegate.cs
        └── Info.plist           # NSUsageDescriptions, background modes
```

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 9.0+ |
| .NET MAUI workload | `dotnet workload install maui` |
| Xcode (iOS) | 15.0+ |
| Android SDK | API 24+ (Android 7.0) |
| Visual Studio 2022 / JetBrains Rider | Latest |

---

## Quick Start

```bash
# 1. Install MAUI workload
dotnet workload install maui

# 2. Restore packages
cd AfriChat
dotnet restore

# 3. Run on Android emulator
dotnet build -t:Run -f net9.0-android

# 4. Run on iOS simulator
dotnet build -t:Run -f net9.0-ios
```

### Fonts (required)
Download and place in `Resources/Fonts/`:
- [Poppins](https://fonts.google.com/specimen/Poppins) — Regular, Medium, SemiBold
- [Material Icons](https://fonts.google.com/icons) — Regular

---

## Key Features

### 🔒 End-to-End Encryption
- AES-256-GCM message encryption with ephemeral session keys
- RSA-2048 key exchange per conversation
- Keys generated on-device; server never sees plaintext
- In production: replace `EncryptionService` with `libsignal-protocol-dotnet`

### 💸 AfriPay — Cross-Border Money Transfer
- 9 currencies: KES, UGX, NGN, GHS, ZAR, TZS, ETB, XOF, USD
- 0.5% flat fee; live FX rates
- Biometric (Face ID / fingerprint) authorisation
- Transfer receipts embedded in chat as rich cards
- M-Pesa, Airtel Money, bank top-up integrations (plug in via `IPaymentService`)

### 🛒 Marketplace
- Product listings with category, price, quantity, location
- Trade opportunity board: buying, selling, partnerships, tenders
- Pan-African buyer-seller matching

### ⚠️ Border Watch
- Real-time crowdsourced delay reports
- Severity levels (Low → Critical) with color coding
- Covering: Beit Bridge, Malaba, Chirundu, Kazungula, Moyale, Nimule, Kasumbalesa
- Trader confirmation system to validate reports

### 💬 Messaging
- Group and direct chats
- Rich message types: text, money transfers, product cards, border alerts
- Swipe to mute / delete conversations
- Voice notes, image and document sharing ready

---

## Production Checklist

- [ ] Replace `EncryptionService` with Signal Protocol implementation
- [ ] Connect `MessagingService` to WebSocket / SignalR hub
- [ ] Connect `PaymentService` to Flutterwave, Chipper Cash, or MoMo API
- [ ] Integrate Firebase Cloud Messaging (push notifications)
- [ ] Add phone-number OTP authentication (Twilio / Africa's Talking)
- [ ] GDPR / POPIA compliance review
- [ ] App Store & Google Play submission
- [ ] Penetration testing for payment flows

---

## Architecture

```
View (XAML) ←→ ViewModel (CommunityToolkit.Mvvm) ←→ Service (Interface)
                                                         ↑
                                                    DI Container
                                               (MauiProgram.cs)
```

Pattern: **MVVM** with `[ObservableProperty]` and `[RelayCommand]` source generators.
Navigation: **Shell routing** with query parameters for deep links.
State: **ObservableCollection<T>** bound to CollectionView.
