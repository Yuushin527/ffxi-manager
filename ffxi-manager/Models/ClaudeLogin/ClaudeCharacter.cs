using System.ComponentModel;

namespace FFXIManager.Models.ClaudeLogin
{
    public class ClaudeCharacter : INotifyPropertyChanged
    {
        private string _name = "";
        private string _password = "";
        private string _totpSecret = "";
        private int _slot = 1;
        private string _charsBin = "chars1";
        private string _args = "";

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(nameof(Password)); }
        }

        public string TotpSecret
        {
            get => _totpSecret;
            set { _totpSecret = value; OnPropertyChanged(nameof(TotpSecret)); }
        }

        public int Slot
        {
            get => _slot;
            set { _slot = value; OnPropertyChanged(nameof(Slot)); }
        }

        public string CharsBin
        {
            get => _charsBin;
            set { _charsBin = value; OnPropertyChanged(nameof(CharsBin)); }
        }

        public string Args
        {
            get => _args;
            set { _args = value; OnPropertyChanged(nameof(Args)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public ClaudeCharacter Clone() => new()
        {
            Name = Name, Password = Password, TotpSecret = TotpSecret,
            Slot = Slot, CharsBin = CharsBin, Args = Args
        };
    }
}
