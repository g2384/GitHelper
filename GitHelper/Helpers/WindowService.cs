using System.Windows;

namespace GitHelper.Helpers
{
    public static class WindowService
    {
        public static void ShowWindow<T>(object dataContext) 
            where T : Window, new()
        {
            var window = new T
            {
                DataContext = dataContext,
                Owner = Application.Current.MainWindow
            };
            window.ShowDialog();
        }
    }
}
