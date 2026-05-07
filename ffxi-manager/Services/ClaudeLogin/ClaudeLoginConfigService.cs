using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using FFXIManager.Models.ClaudeLogin;

namespace FFXIManager.Services.ClaudeLogin
{
    public class ClaudeLoginConfigService
    {
        private static readonly JsonSerializerOptions _opts = new() { WriteIndented = true };

        public string WindowerPath { get; set; } = "";

        public string ConfigFilePath => Path.Combine(WindowerPath, "config.json");

        public List<ClaudeCharacter> Load()
        {
            var result = new List<ClaudeCharacter>();
            if (!File.Exists(ConfigFilePath)) return result;

            try
            {
                var json = JsonNode.Parse(File.ReadAllText(ConfigFilePath));
                var accounts = json?["accounts"]?.AsArray();
                if (accounts == null) return result;

                foreach (var acc in accounts)
                {
                    if (acc == null) continue;
                    result.Add(new ClaudeCharacter
                    {
                        Name      = acc["name"]?.GetValue<string>() ?? "",
                        Password  = acc["password"]?.GetValue<string>() ?? "",
                        TotpSecret= acc["totpSecret"]?.GetValue<string>() ?? "",
                        Slot      = acc["slot"]?.GetValue<int>() ?? 1,
                        Args      = acc["args"]?.GetValue<string>() ?? "",
                        CharsBin  = acc["charsBin"]?.GetValue<string>() ?? "",
                    });
                }
            }
            catch { }

            return result;
        }

        public void Save(IEnumerable<ClaudeCharacter> characters, string clientRegion = "JP", int delay = 3000)
        {
            JsonNode? existing = null;
            if (File.Exists(ConfigFilePath))
            {
                try { existing = JsonNode.Parse(File.ReadAllText(ConfigFilePath)); } catch { }
            }

            var region = existing?["clientRegion"]?.GetValue<string>() ?? clientRegion;
            var d      = existing?["delay"]?.GetValue<int>() ?? delay;

            var accounts = new JsonArray();
            foreach (var c in characters)
            {
                accounts.Add(new JsonObject
                {
                    ["name"]       = c.Name,
                    ["password"]   = c.Password,
                    ["totpSecret"] = c.TotpSecret,
                    ["slot"]       = c.Slot,
                    ["args"]       = c.Args,
                    ["charsBin"]   = c.CharsBin,
                });
            }

            var root = new JsonObject
            {
                ["accounts"]     = accounts,
                ["clientRegion"] = region,
                ["delay"]        = d,
            };

            Directory.CreateDirectory(WindowerPath);
            File.WriteAllText(ConfigFilePath, root.ToJsonString(_opts));
        }

        public string GetPolDataPath()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\WOW6432Node\PlayOnline\SquareEnix\PlayOnlineViewer");
                var path = key?.GetValue("Path") as string;
                if (!string.IsNullOrEmpty(path))
                    return Path.Combine(path.TrimEnd('\\'), "usr", "all");
            }
            catch { }
            return @"C:\Program Files (x86)\PlayOnline\SquareEnix\PlayOnlineViewer\usr\all";
        }

        public List<string> GetAvailableCharsBins()
        {
            var polPath = GetPolDataPath();
            var bins = new List<string>();
            if (!Directory.Exists(polPath)) return bins;
            foreach (var f in Directory.GetFiles(polPath, "chars*.bin"))
                bins.Add(Path.GetFileNameWithoutExtension(f));
            bins.Sort();
            return bins;
        }

        public void CaptureCurrentBin(string binName)
        {
            var polPath = GetPolDataPath();
            var src = Path.Combine(polPath, "login_w.bin");
            var dst = Path.Combine(polPath, binName + ".bin");
            File.Copy(src, dst, overwrite: true);
        }
    }
}
