using AfriChat.Models;

namespace AfriChat.Controls;

/// <summary>
/// Custom control that renders different bubble types:
/// Text, MoneyTransfer card, ProductListing card, BorderAlert card.
/// </summary>
public class MessageBubbleView : ContentView
{
    public static readonly BindableProperty MessageProperty =
        BindableProperty.Create(nameof(Message), typeof(Message), typeof(MessageBubbleView),
            propertyChanged: (b, _, n) => ((MessageBubbleView)b).Build((Message)n));

    public Message? Message
    {
        get => (Message?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    private void Build(Message? msg)
    {
        if (msg == null) return;

        bool isOut = msg.IsOutgoing;
        var layout = new VerticalStackLayout { Spacing = 3, Margin = new Thickness(12, 3) };

        if (!isOut && !string.IsNullOrEmpty(msg.SenderName))
        {
            layout.Add(new Label
            {
                Text = msg.SenderName,
                FontFamily = "PoppinsMedium",
                FontSize = 11,
                TextColor = Color.FromArgb("#4F91A8")
            });
        }

        View bubble = msg.Type switch
        {
            MessageType.MoneyTransfer => BuildMoneyCard(msg),
            MessageType.ProductListing => BuildProductCard(msg),
            MessageType.BorderAlert => BuildAlertCard(msg),
            _ => BuildTextBubble(msg, isOut)
        };

        layout.Add(bubble);

        var timeRow = new HorizontalStackLayout { Spacing = 4 };
        timeRow.Add(new Label
        {
            Text = msg.TimeDisplay,
            FontSize = 10,
            TextColor = Color.FromArgb("#8DBFCA")
        });
        if (isOut)
        {
            timeRow.Add(new Label { Text = "🔒", FontSize = 9, TextColor = Color.FromArgb("#8DBFCA") });
        }
        layout.Add(timeRow);

        // Align outgoing right, incoming left
        Content = new Grid
        {
            Children =
            {
                new ContentView
                {
                    Content = layout,
                    HorizontalOptions = isOut ? LayoutOptions.End : LayoutOptions.Start
                }
            }
        };
    }

    private static Frame BuildTextBubble(Message msg, bool isOut)
    {
        var bgColor = isOut ? Color.FromArgb("#087397") : Color.FromArgb("#ECE6DB");
        var textColor = isOut ? Colors.White : Color.FromArgb("#102B35");

        return new Frame
        {
            BackgroundColor = bgColor,
            CornerRadius = 16,
            Padding = new Thickness(12, 8),
            HasShadow = false,
            BorderColor = isOut ? Colors.Transparent : Color.FromArgb("#D8D0C4"),
            MaximumWidthRequest = 280,
            Content = new Label
            {
                Text = msg.Text,
                TextColor = textColor,
                FontFamily = "PoppinsRegular",
                FontSize = 14,
                LineBreakMode = LineBreakMode.WordWrap
            }
        };
    }

    private static Frame BuildMoneyCard(Message msg)
    {
        var t = msg.MoneyTransfer!;
        return new Frame
        {
            BackgroundColor = Color.FromArgb("#0B759A"),
            CornerRadius = 16,
            Padding = new Thickness(14, 12),
            HasShadow = false,
            BorderColor = Colors.Transparent,
            MaximumWidthRequest = 260,
            Content = new VerticalStackLayout
            {
                Spacing = 6,
                Children =
                {
                    new Label { Text = "💸 AfriPay transfer", TextColor = Color.FromArgb("#ECE6DB"), FontSize = 11 },
                    new Label { Text = $"{t.Currency} {t.Amount:N0}", TextColor = Colors.White, FontFamily = "PoppinsSemiBold", FontSize = 24 },
                    new Label { Text = t.Note, TextColor = Color.FromArgb("#B7DCE2"), FontSize = 13 },
                    new BoxView { HeightRequest = 0.5, Color = Color.FromArgb("#087397") },
                    new Grid
                    {
                        ColumnDefinitions = new ColumnDefinitionCollection { new ColumnDefinition(GridLength.Star), new ColumnDefinition(GridLength.Star) },
                        Children =
                        {
                            new Label { Text = $"To: {t.ToUserId}", TextColor = Color.FromArgb("#B7DCE2"), FontSize = 11 }.Column(0),
                            new Label { Text = t.StatusDisplay, TextColor = Color.FromArgb("#B7DCE2"), FontSize = 11, HorizontalOptions = LayoutOptions.End }.Column(1)
                        }
                    }
                }
            }
        };
    }

    private static Frame BuildProductCard(Message msg)
    {
        var p = msg.ProductListing!;
        return new Frame
        {
            BackgroundColor = Color.FromArgb("#FFFFFF"),
            CornerRadius = 14,
            Padding = new Thickness(0),
            HasShadow = false,
            BorderColor = Color.FromArgb("#D8D0C4"),
            MaximumWidthRequest = 220,
            IsClippedToBounds = true,
            Content = new VerticalStackLayout
            {
                Children =
                {
                    new Frame
                    {
                        BackgroundColor = Color.FromArgb("#ECE6DB"),
                        HeightRequest = 90,
                        CornerRadius = 0,
                        HasShadow = false,
                        BorderColor = Colors.Transparent,
                        Content = new Label
                        {
                            Text = p.Emoji,
                            FontSize = 40,
                            HorizontalOptions = LayoutOptions.Center,
                            VerticalOptions = LayoutOptions.Center
                        }
                    },
                    new VerticalStackLayout
                    {
                        Padding = new Thickness(10, 8),
                        Spacing = 3,
                        Children =
                        {
                            new Label { Text = p.CategoryDisplay, FontSize = 10, TextColor = Color.FromArgb("#4F91A8"), FontFamily = "PoppinsMedium" },
                            new Label { Text = p.Title, FontFamily = "PoppinsMedium", FontSize = 13, TextColor = Color.FromArgb("#102B35") },
                            new Label { Text = p.PriceDisplay, FontFamily = "PoppinsSemiBold", FontSize = 14, TextColor = Color.FromArgb("#087397") },
                            new Label { Text = $"📍 {p.Location}", FontSize = 11, TextColor = Color.FromArgb("#8DBFCA") }
                        }
                    }
                }
            }
        };
    }

    private static Frame BuildAlertCard(Message msg)
    {
        var a = msg.BorderAlert!;
        return new Frame
        {
            BackgroundColor = Color.FromArgb("#B7DCE2"),
            CornerRadius = 12,
            Padding = new Thickness(12, 10),
            HasShadow = false,
            BorderColor = Color.FromArgb("#4F91A8"),
            MaximumWidthRequest = 280,
            Content = new VerticalStackLayout
            {
                Spacing = 5,
                Children =
                {
                    new Label { Text = $"⚠️ {a.BorderName}", FontFamily = "PoppinsSemiBold", FontSize = 13, TextColor = Color.FromArgb("#4F91A8") },
                    new Label { Text = $"{a.DelayIcon} {a.DelayType} · ~{a.EstimatedWait}", FontSize = 12, TextColor = Color.FromArgb("#2F6E84") },
                    new Label { Text = a.Description, FontSize = 12, TextColor = Color.FromArgb("#2F6E84"), MaxLines = 3 },
                    new Label { Text = $"✓ {a.ConfirmationCount} traders · {a.TimeAgo}", FontSize = 10, TextColor = Color.FromArgb("#4F91A8") }
                }
            }
        };
    }
}

// Helper extension for column assignment
public static class GridExtensions
{
    public static T Column<T>(this T view, int col) where T : View { Grid.SetColumn(view, col); return view; }
}
