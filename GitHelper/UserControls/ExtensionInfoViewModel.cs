using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace GitHelper.UserControls
{
    public class ExtensionInfoViewModel:ViewModelBase
    {
        private string _fullInfo;
        public string FullInfo
        {
            get => _fullInfo;
            set
            {
                _fullInfo = value;
                RaisePropertyChanged("FullInfo");
            }
        }
    }
}
