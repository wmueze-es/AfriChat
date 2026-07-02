using System.Globalization;

namespace AfriChat.Converters;

public class IntToBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is int i && i > 0;
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => null;
}

public class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool b && !b;
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => null;
}

public class CreditEmojiConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is bool isCredit && isCredit ? "⬆️" : "⬇️";
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => null;
}

public class BoolToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isTrue = value is bool b && b;
        return parameter?.ToString() switch
        {
            "green" => isTrue ? Color.FromArgb("#087397") : Colors.Transparent,
            "text" => isTrue ? Colors.White : Color.FromArgb("#087397"),
            "invgreen" => !isTrue ? Color.FromArgb("#087397") : Colors.Transparent,
            "invtext" => !isTrue ? Colors.White : Color.FromArgb("#087397"),
            _ => Colors.Transparent
        };
    }
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => null;
}

public class MessageDirectionConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is AfriChat.Models.MessageDirection dir && dir == AfriChat.Models.MessageDirection.Outgoing
            ? LayoutOptions.End : LayoutOptions.Start;
    public object? ConvertBack(object? value, Type t, object? p, CultureInfo c) => null;
}
