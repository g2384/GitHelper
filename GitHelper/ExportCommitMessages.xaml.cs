using System.Windows;
using GitHelper.ViewModel;

namespace GitHelper
{
    /// <summary>
    /// Interaction logic for ExportCommitMessages.xaml
    /// </summary>
    public partial class ExportCommitMessages : Window
    {
        public ExportCommitMessages(BranchInfo branch, Configuration config)
        {
            InitializeComponent();
            DataContext = new ExportCommitMessageViewModel(branch, config);
        }
    }
}
