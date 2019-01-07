﻿namespace NTMiner.Vms {
    public class KernelHelpPageViewModel : ViewModelBase {
        private string _helpText;

        public KernelHelpPageViewModel() { }

        public string HelpText {
            get => _helpText;
            set {
                _helpText = value;
                OnPropertyChanged(nameof(HelpText));
            }
        }
    }
}
