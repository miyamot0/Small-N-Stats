using MahApps.Metro.Controls;
using Microsoft.Win32;
using System.IO;
using System.Windows.Documents;

namespace Small_N_Stats.View
{
    /// <summary>
    /// Interaction logic for OutputWindow.xaml
    /// </summary>
    public partial class OutputWindow : MetroWindow
    {
        public OutputWindow()
        {
            InitializeComponent();
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Properties.Settings.Default.Save();
        }

        private void saveLogs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            SaveFileDialog sd = new SaveFileDialog();
            sd.FileName = "Logs";
            sd.Filter = "Text file (*.txt)|*.txt|All files (*.*)|*.*";

            if (sd.ShowDialog() == true)
            {
                using (StreamWriter sw = new StreamWriter(sd.FileName))
                {
                    TextRange textRange = new TextRange(outputWindow.Document.ContentStart, outputWindow.Document.ContentEnd);
                    sw.Write(textRange.Text);
                }
            }
        }

        private void clearLogs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            outputWindow.Document.Blocks.Clear();
        }
    }
}
