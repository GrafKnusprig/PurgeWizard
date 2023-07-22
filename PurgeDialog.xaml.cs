using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;

namespace PurgeWizard
{
    /// <summary>
    /// Interaction logic for PurgeDialoge.xaml
    /// </summary>
    public partial class PurgeDialog : Window
    {
        private Context context = new Context();
        private List<string> foundFiles = new List<string>();

        public PurgeDialog(Context context)
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            this.context = context;
            FindAllFiles();

        }

        private void FindAllFiles()
        {
            StringBuilder stringBuilder = new StringBuilder();
            foundFiles = new List<string>();

            if (string.IsNullOrEmpty(context.BaseFolder))
            {
                Txtbx_PurgeDialoge.Text = "Base folde path is empty.";
                return;
            }

            if (!Directory.Exists(context.BaseFolder))
            {
                Txtbx_PurgeDialoge.Text = "Base folder path " + context.BaseFolder + " does not exist.";
                return;
            }


            foreach (var file in context.FileList)
            {
                var fullPath = Path.Combine(context.BaseFolder, file);
                if (File.Exists(fullPath))
                {
                    stringBuilder.AppendLine("Found \"" + fullPath + "\"");
                    foundFiles.Add(fullPath);
                }
                else
                {
                    stringBuilder.AppendLine("Not Found \"" + fullPath + "\"");
                }
            }

            var patternFilesList = new List<string>();

            foreach (var pattern in context.Patterns)
            {
                var allFiles = Directory.GetFiles(context.BaseFolder, pattern);
                patternFilesList.AddRange(allFiles);
            }

            patternFilesList.Distinct().ToString();

            if (context.Patterns.Any())
                stringBuilder.AppendLine("Found pattern files:");

            foreach (var file in patternFilesList)
            {
                stringBuilder.AppendLine("\"" + file + "\"");
            }

            foundFiles.AddRange(patternFilesList);

            if (foundFiles.Count > 0)
            {
                Btn_doFinalPurge.IsEnabled = true;
                Txtbx_PurgeDialoge.Text = stringBuilder.ToString();
            }
            else
            {
                Txtbx_PurgeDialoge.Text = "No files found.";
            }

        }

        private void Btn_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Btn_doFinalPurge_Click(object sender, RoutedEventArgs e)
        {
            Txtbx_PurgeDialoge.Text = string.Empty;
            foreach (var file in foundFiles)
            {
                Txtbx_PurgeDialoge.Text += "Deleting " + "\"" + file + "\" ... ";
                try
                {
                    File.Delete(file);
                    Txtbx_PurgeDialoge.Text += "deleted.";
                }
                catch { Txtbx_PurgeDialoge.Text += "failed."; }

                Txtbx_PurgeDialoge.Text += Environment.NewLine;
            }
        }
    }
}
