using GalaSoft.MvvmLight;

namespace GitHelper.UserControls
{
    public class ExtensionInfoViewModel:ViewModelBase
    {
        private string _fullInfo;
        public string FullInfo
        {
            get => _fullInfo;
            set => Set(ref _fullInfo, value); 
        }
    }
}
