using System.Windows;

namespace GitHelper.UIExtension
{
    public static class Utility
    {
        public static bool? ShowDialog(Window window)
        {
            window.Owner = Application.Current.MainWindow;
            return window.ShowDialog();
        }
    }
}
