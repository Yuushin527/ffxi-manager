using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using FFXIManager.Models.ClaudeLogin;
using FFXIManager.Services.ClaudeLogin;
using FFXIManager.ViewModels.Base;

namespace FFXIManager.ViewModels.ClaudeLogin
{
    public class ClaudeLoginViewModel : ViewModelBase
    {
        private readonly ClaudeLoginConfigService _svc = new();
        private ClaudeCharacter? _selected;
        private string _windowerPath = "";
        private string _statusMessage = "";

        public ObservableCollection<ClaudeCharacter> Characters { get; } = [];
        public ObservableCollection<string> AvailableBins { get; } = [];

        public ClaudeCharacter? Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                OnPropertyChanged(nameof(Selected));
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public bool HasSelection => Selected != null;

        public string WindowerPath
        {
            get => _windowerPath;
            set
            {
                _windowerPath = value;
                _svc.WindowerPath = value;
                OnPropertyChanged(nameof(WindowerPath));
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public RelayCommand LoadCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand BrowseWindowerCommand { get; }
        public RelayCommand CaptureCurrentBinCommand { get; }

        public ClaudeLoginViewModel()
        {
            LoadCommand             = new RelayCommand(Load);
            SaveCommand             = new RelayCommand(Save);
            AddCommand              = new RelayCommand(Add);
            DeleteCommand           = new RelayCommand(Delete, () => HasSelection);
            BrowseWindowerCommand   = new RelayCommand(BrowseWindower);
            CaptureCurrentBinCommand= new RelayCommand(CaptureBin);

            // デフォルトパスを自動検出
            WindowerPath = DetectWindowerPath();
            Load();
        }

        private string DetectWindowerPath()
        {
            var candidates = new[]
            {
                @"D:\Google-ken1974.yuushin\FF11\シンボリックリンク\Windower",
                @"C:\Program Files (x86)\Windower4",
                @"C:\Windower4",
            };
            foreach (var p in candidates)
                if (File.Exists(Path.Combine(p, "Windower.exe"))) return p;
            return "";
        }

        private void Load()
        {
            Characters.Clear();
            foreach (var c in _svc.Load())
                Characters.Add(c);

            RefreshBins();
            StatusMessage = $"{Characters.Count} キャラクター読み込みました";
        }

        private void RefreshBins()
        {
            AvailableBins.Clear();
            foreach (var b in _svc.GetAvailableCharsBins())
                AvailableBins.Add(b);
        }

        private void Save()
        {
            try
            {
                _svc.Save(Characters);
                StatusMessage = "保存しました";
            }
            catch (Exception ex)
            {
                StatusMessage = $"保存エラー: {ex.Message}";
            }
        }

        private void Add()
        {
            var c = new ClaudeCharacter
            {
                Name     = "新しいキャラ",
                Slot     = 1,
                CharsBin = AvailableBins.FirstOrDefault() ?? "chars1",
            };
            Characters.Add(c);
            Selected = c;
        }

        private void Delete()
        {
            if (Selected == null) return;
            Characters.Remove(Selected);
            Selected = null;
        }

        private void BrowseWindower()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title  = "Windower.exe を選択",
                Filter = "Windower.exe|Windower.exe",
            };
            if (dialog.ShowDialog() == true)
                WindowerPath = Path.GetDirectoryName(dialog.FileName) ?? "";
        }

        private void CaptureBin()
        {
            var polPath = _svc.GetPolDataPath();
            var loginW  = Path.Combine(polPath, "login_w.bin");
            if (!File.Exists(loginW))
            {
                MessageBox.Show("login_w.bin が見つかりません。\nPOL を一度起動・終了してから実行してください。",
                    "ClaudeLogin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = _svc.GetAvailableCharsBins();
            int next = existing.Count + 1;
            var name = $"chars{next}";

            var result = MessageBox.Show(
                $"現在の login_w.bin を {name}.bin として保存します。\nよろしいですか？",
                "ClaudeLogin セットアップ", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                _svc.CaptureCurrentBin(name);
                RefreshBins();
                StatusMessage = $"{name}.bin を保存しました";
            }
            catch (Exception ex)
            {
                StatusMessage = $"エラー: {ex.Message}";
            }
        }
    }
}
