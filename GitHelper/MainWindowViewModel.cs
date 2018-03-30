using GalaSoft.MvvmLight;
using GitHelper.Attributes;
using GitHelper.Interfaces;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace GitHelper
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ObservableCollection<ActionInfo> _actions;
        public ObservableCollection<ActionInfo> Actions
        {
            get => _actions;
            set
            {
                _actions = value;
                RaisePropertyChanged("Actions");
            }
        }

        private ActionInfo _selectedAction;
        public ActionInfo SelectedAction
        {
            get => _selectedAction;
            set
            {
                _selectedAction = value;
                DisplayFullInfo();
                RaisePropertyChanged("SelectedAction");
            }
        }

        public Configuration Config { get; set; }

        private static Logger _log = LogManager.GetCurrentClassLogger();

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

        public MainWindowViewModel()
        {
            Config = Configuration.GetConfiguration();
            var actions = new ObservableCollection<ActionInfo>();
            var types = GetTypesWith<GitHelperActionAttribute>(false).ToList();
            foreach (var type in types)
            {
                var instance = (IGitHelperActionMeta)Activator.CreateInstance(type);
                var actionInfo = new ActionInfo(instance, Config);
                actions.Add(actionInfo);
            }
            
            Actions = new ObservableCollection<ActionInfo>(actions.OrderBy(a => a.Title));
        }

        private void DisplayFullInfo()
        {
            if(SelectedAction == null)
            {
                return;
            }

            FullInfo = SelectedAction.GetFullInfo();
        }

        IEnumerable<Type> GetTypesWith<TAttribute>(bool inherit)
                              where TAttribute : Attribute
        {
            return from a in AppDomain.CurrentDomain.GetAssemblies()
                   from t in a.GetTypes()
                   where t.IsDefined(typeof(TAttribute), inherit)
                   select t;
        }
    }
}
