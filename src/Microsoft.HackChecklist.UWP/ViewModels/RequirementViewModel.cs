//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

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
        private ResponseStatus _status = ResponseStatus.None;

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
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public ObservableCollection<RequirementViewModel> Modules { get; set; }

        public Requirement ModelObject
        {
            get
            {
                if (Modules != null)
                {
                    _requirement.Modules = Modules.Select(x => x.ModelObject);
                }
                return _requirement;
            }
            set
            {
                if (value == null) return;
                _requirement = value;
                if (_requirement.Modules != null)
                {
                    Modules = new ObservableCollection<RequirementViewModel>(_requirement.Modules.Select(x => new RequirementViewModel(x)));
                }
            }
        }
    }
}
