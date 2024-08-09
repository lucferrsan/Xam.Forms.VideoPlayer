using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Maui.VideoPlayer
{
    public class VideoSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sourceString && !string.IsNullOrWhiteSpace(sourceString))
            {
                return Uri.TryCreate(sourceString, UriKind.Absolute, out Uri uri) && uri.Scheme != "file"
                    ? VideoSource.FromUri(sourceString)
                    : VideoSource.FromResource(sourceString);
            }

            throw new InvalidOperationException("Cannot convert null or whitespace to VideoSource");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
