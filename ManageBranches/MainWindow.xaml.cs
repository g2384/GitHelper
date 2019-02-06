using GitHelper.Extension;
using GitHelper.Extension.Attributes;

namespace ManageBranches
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    [RequireConfig(false)]
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new ManageBranchesViewModel(Configuration.GetConfiguration());
        }
    }
}
