using System.Windows;

namespace ExportCommitMessages
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ExportCommitMessageViewModel();
        }
    }
}
