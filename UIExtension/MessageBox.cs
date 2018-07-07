using System.Windows;

namespace GitHelper.UIExtension
{
    public sealed class MessageBox
    {
        public static MessageBoxResult ShowError(string message)
        {
            return System.Windows.MessageBox.Show(message, "Error", MessageBoxButton.OK);
        }

        public static MessageBoxResult Show(string message)
        {
            return System.Windows.MessageBox.Show(message);
        }

        public static MessageBoxResult Show(string message, string caption)
        {
            return System.Windows.MessageBox.Show(message, caption);
        }
    }
}
