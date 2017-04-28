using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.HackChecklist.Models;
using Microsoft.HackChecklist.Models.Contracts;
using Microsoft.HackChecklist.Models.Enums;
using Microsoft.HackChecklist.UWP.ViewModels.Base;

namespace Microsoft.HackChecklist.UWP.ViewModels
{
    public class RequirementViewModel : ViewModelBase, ISoftware
    {
        private Requirement _requirement;

        public RequirementViewModel(Requirement requirement)
        {
            ModelObject = requirement;
        }

        public string Name
        {
            get => _requirement.Name;
            set
            {
                _requirement.Name = value;
                OnPropertyChanged(nameof(Name));
            }
        }

        public string AdditionalInformation
        {
            get => _requirement.AdditionalInformation;
            set
            {
                _requirement.AdditionalInformation = value;
                OnPropertyChanged(nameof(AdditionalInformation));
            }
        }

        public CheckType CheckType
        {
            get => _requirement.CheckType;
            set
            {
                _requirement.CheckType = value;
                OnPropertyChanged(nameof(CheckType));
            }
        }

        public string InstallationRegistryKey
        {
            get => _requirement.InstallationRegistryKey;
            set
            {
                _requirement.InstallationRegistryKey = value;
                OnPropertyChanged(nameof(InstallationRegistryKey));
            }
        }

        public string InstallationRegistryValue
        {
            get => _requirement.InstallationRegistryValue;
            set
            {
                _requirement.InstallationRegistryValue = value;
                OnPropertyChanged(nameof(InstallationRegistryValue));
            }
        }

        public string InstallationRegistryExpectedValue
        {
            get => _requirement.InstallationRegistryExpectedValue;
            set
            {
                _requirement.InstallationRegistryExpectedValue = value;
                OnPropertyChanged(nameof(InstallationRegistryExpectedValue));
            }
        }

        public bool IsOptional
        {
            get => _requirement.IsOptional;
            set
            {
                _requirement.IsOptional = value;
                OnPropertyChanged(nameof(IsOptional));
            }
        }

        public string InstallationNotes
        {
            get => _requirement.InstallationNotes;
            set
            {
                _requirement.InstallationNotes = value;
                OnPropertyChanged(nameof(InstallationNotes));
            }
        }

        public ResponseStatus Status 
        {
            get => _requirement.Status;
            set
            {
                _requirement.Status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ObservableCollection<Software> Modules { get; set; }

        public Requirement ModelObject
        {
            get
            {
                if (Modules != null)
                {
                    _requirement.Modules = Modules.ToList();
                }
                return _requirement;
            }
            set
            {
                if (value == null) return;
                _requirement = value;
                if (_requirement.Modules != null)
                {
                    Modules = new ObservableCollection<Software>(_requirement.Modules);
                }
            }
        }
    }
}
