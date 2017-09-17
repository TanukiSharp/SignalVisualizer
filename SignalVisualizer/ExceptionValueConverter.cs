using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Data;

namespace SignalVisualizer
{
    public class ExceptionValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var ex = value as Exception;

            if (ex == null)
                return null;

            return DumpExceptionRecursive(ex, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static string DumpExceptionRecursive(Exception ex, int level)
        {
            var sb = new StringBuilder();

            var indent = new string(' ', level * 4);

            sb.AppendLine(string.Format("{0}Type: {1}", indent, ex.GetType().FullName));
            sb.AppendLine(string.Format("{0}Message: {1}", indent, ex.Message));

            if (ex.Data.Keys.Count > 0)
            {
                sb.AppendLine(string.Format("{0}Data:", indent));
                foreach (var key in ex.Data.Keys)
                    sb.AppendLine(string.Format("{0}  Key: {0}, Value: {1}", key, ex.Data[key]));
            }

            if (ex is ReflectionTypeLoadException)
            {
                foreach (var loaderException in ((ReflectionTypeLoadException)ex).LoaderExceptions)
                    DumpExceptionRecursive(loaderException, level + 1);
            }
            else if (ex is AggregateException)
            {
                foreach (var innerException in ((AggregateException)ex).InnerExceptions)
                    DumpExceptionRecursive(innerException, level + 1);
            }

            if (ex.StackTrace != null)
            {
                sb.AppendLine(string.Format("{0}Stack:", indent));
                DumpStackTrace(level, ex.StackTrace, sb);
            }

            if (ex.InnerException != null)
            {
                sb.AppendLine();
                DumpExceptionRecursive(ex.InnerException, level + 1);
            }

            return sb.ToString();
        }

        private static void DumpStackTrace(int level, string stackTrace, StringBuilder sb)
        {
            var indent = new string(' ', 2 + level * 4);

            foreach (var line in stackTrace.Split('\n').Select(l => l.Trim()))
                sb.AppendLine(string.Format("{0}{1}", indent, line));
        }
    }
}
