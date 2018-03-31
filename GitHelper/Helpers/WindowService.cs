using System.Windows;

namespace GitHelper.Helpers
{
    public static class WindowService
    {
        public static void ShowWindow<T>(object DataContext) 
            where T : Window, new()
        {
            var window = new T();
            window.DataContext = DataContext;
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }
    }
}
