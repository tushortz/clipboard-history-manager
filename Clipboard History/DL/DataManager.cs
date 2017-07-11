using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Clipboard_History.DL
{
    class DataManager
    {
        private readonly string _filepath;
        private readonly string _lastRunDate = ConfigurationManager.AppSettings["LastRunDate"];
        public DateTime LastRunDate {

            get
            {
                DateTime date;
                var isDate = DateTime.TryParse(_lastRunDate, out date);
                return isDate ? DateTime.Parse(_lastRunDate) : DateTime.Today;
            }
        }

        public DataManager()
        {
            var appDataFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var programName = typeof(Program).Assembly.ToString().Split(',')[0];
            var storageFolder = Path.Combine(appDataFolderPath, programName);

            Directory.CreateDirectory(storageFolder);
            _filepath = storageFolder + @"\today.txt";
        }

        public void StoreClipboardDataToFile()
        {
            if (Clipboard.ContainsText())
            {
                var clip = GetClipboardContent();
                var text = $"{clip.Time:HH:mm:ss}, {clip.Content}";

                var isPastDay = DateTime.Today.Date - LastRunDate >= TimeSpan.FromDays(1);

                using (var file = new StreamWriter(_filepath, !isPastDay))
                {
                    file.WriteLine(text);
                    UpdateLastRunDate();
                }
            }
        }

        private ClipObject GetClipboardContent()
        {
            var clipObject = new ClipObject
            {
                Content = Clipboard.GetText(),
                Time = DateTime.Now
            };

            return clipObject;
        }

        public List<ClipObject> GetTodayClipboardContents()
        {
            var clips = new List<ClipObject>();

            if (File.Exists(_filepath))
            {
                var data = File.ReadAllText(_filepath);
                var regex = new Regex(@"(\d+:\d+:\d+), (.*)?", RegexOptions.Compiled | RegexOptions.None);
                var matches = regex.Matches(data);

                foreach (Match match in matches)
                {
                    var groups = match.Groups;

                    clips.Add(new ClipObject
                    {
                        Time = DateTime.ParseExact(groups[1].Value, "HH:mm:ss", null),
                        Content = groups[2].Value
                    });
                }
            }

            return clips;
        }

        public void UpdateLastRunDate()
        {
            var config = ConfigurationManager.OpenExeConfiguration(Application.ExecutablePath);
            config.AppSettings.Settings["LastRunDate"].Value = DateTime.Today.ToString("dd-MM-yyyy");
            config.Save(ConfigurationSaveMode.Modified);
        }
    }
}
