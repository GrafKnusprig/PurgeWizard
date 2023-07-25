using AdonisUI.Controls;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;

namespace PurgeWizard
{
    public class Context
    {
        private List<string> fileList = new List<string>();

        private List<string> patterns = new List<string>();

        private string baseFolder = string.Empty;

        [JsonProperty("FileList")]
        public List<string> FileList { get => fileList; set => fileList = value; }

        [JsonProperty("Patterns")]
        public List<string> Patterns { get => patterns; set => patterns = value; }

        [JsonProperty("BaseFolder")]
        public string BaseFolder { get => baseFolder; set => baseFolder = value; }

        public Context() { }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : AdonisWindow
    {
        private Context context = new Context();
        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

            Title = Title += " v" + fvi.FileVersion;
        }

        private void UpdateFilesList()
        {
            //remove duplicates and sort
            context.FileList.Sort();
            Tbx_fileList.ItemsSource = new List<string>(context.FileList.Distinct().ToList());
        }


        private void Btn_AddFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;
            if (dialog.ShowDialog() == true)
            {
                foreach (var filepath in dialog.FileNames)
                {
                    context.FileList.Add(Path.GetFileName(filepath));
                }
                UpdateFilesList();
            }
        }

        private void Btn_delete_Click(object sender, RoutedEventArgs e)
        {
            if (Tbx_fileList.SelectedIndex >= 0)
            {
                context.FileList.RemoveAt(Tbx_fileList.SelectedIndex);
                UpdateFilesList();
            }
        }

        private void Btn_deleteAll_Click(object sender, RoutedEventArgs e)
        {
            context.FileList = new List<string>();
            UpdateFilesList();
        }

        private void Btn_addPattern_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Tf_pattern.Text))
            {
                context.Patterns.Add(Tf_pattern.Text);
                Tf_pattern.Text = string.Empty;
                UpdatePatterns();
                Tf_pattern.Focus();
            }
        }

        private void UpdatePatterns()
        {
            context.Patterns.Sort();
            Tbx_patterns.ItemsSource = new List<string>(context.Patterns.Distinct().ToList());
        }

        private void Btn_delPattern_Click(object sender, RoutedEventArgs e)
        {
            if (Tbx_patterns.SelectedIndex >= 0)
            {
                context.Patterns.RemoveAt(Tbx_patterns.SelectedIndex);
                UpdatePatterns();
            }
        }

        private void Btn_delAllPattern_Click(object sender, RoutedEventArgs e)
        {
            context.Patterns = new List<string>();
            UpdatePatterns();
        }

        private void Btn_Purge_Click(object sender, RoutedEventArgs e)
        {
            var purgeDialog = new PurgeDialog(context);
            purgeDialog.ShowDialog();
        }

        private void Btn_openFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                context.BaseFolder = dialog.SelectedPath;
                UpdateBasePath();
            }
        }

        private void Btn_save_Click(object sender, RoutedEventArgs e)
        {
            var savedialog = new Microsoft.Win32.SaveFileDialog();
            savedialog.DefaultExt = "pwiz";
            savedialog.Filter = "PWIZ|*.pwiz";
            savedialog.AddExtension = true;
            savedialog.FileOk += FileOkEvent;
            if (savedialog.ShowDialog() == true)
            {
                var name = savedialog.FileName;
                SaveToXml(name);
            }
        }

        private void FileOkEvent(object? sender, CancelEventArgs e)
        {
            if (sender != null && !((Microsoft.Win32.SaveFileDialog)sender).FileName.EndsWith(".pwiz"))
            {
                System.Windows.MessageBox.Show("Please select a filename with the '.pwiz' extension");
                e.Cancel = true;
            }
        }

        private void SaveToXml(string path)
        {
            if (File.Exists(path))
            {
                try { File.Delete(path); }
                catch
                {
                    System.Windows.MessageBox.Show("Error. File could not be overwritten.");
                    return;
                }
            }

            var json = JsonConvert.SerializeObject(context, Newtonsoft.Json.Formatting.Indented);
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(json);
                writer.Flush();
            }
        }

        private void LoadFromXml(string path)
        {
            string json = File.ReadAllText(path);
            var c = JsonConvert.DeserializeObject<Context>(json);
            if (c != null)
                context = c;
            else
                System.Windows.MessageBox.Show("Error. File could not be loaded.");
        }

        private void Btn_load_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".pwiz";
            dialog.Filter = "PWIZ|*.pwiz";
            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileName.EndsWith("pwiz"))
                {
                    LoadFromXml(dialog.FileName);
                    UpdateAll();
                }
            }
        }

        private void LoadFilesFromTxt(string fileName)
        {
            var list = new List<string>();
            list.AddRange(File.ReadAllLines(fileName));
            foreach (var line in list)
            {
                try
                {
                    var filtered = line.Replace("\"", "");
                    context.FileList.Add(Path.GetFileName(filtered));
                }
                catch { }
            }
        }

        private void UpdateAll()
        {
            UpdateFilesList();
            UpdatePatterns();
            UpdateBasePath();
        }

        private void UpdateBasePath()
        {
            Tbx_baseFolder.Text = context.BaseFolder;
        }

        private void Btn_importFiles_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text|*.txt";
            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileName.EndsWith("txt"))
                {
                    LoadFilesFromTxt(dialog.FileName);
                    UpdateAll();
                }
            }
        }

        private void Btn_importPatterns_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.DefaultExt = ".txt";
            dialog.Filter = "Text|*.txt";
            if (dialog.ShowDialog() == true)
            {
                if (dialog.FileName.EndsWith("txt"))
                {
                    LoadPatternsFromTxt(dialog.FileName);
                    UpdateAll();
                }
            }
        }

        private void LoadPatternsFromTxt(string fileName)
        {
            context.Patterns.AddRange(File.ReadAllLines(fileName));
        }
    }
}
