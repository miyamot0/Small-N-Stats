using System;
using MahApps.Metro.Controls;
using Small_N_Stats.Interface;
using System.IO;
using unvell.ReoGrid.IO;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;

namespace Small_N_Stats
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, SpreadsheetInterface
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public void CallBackToMain()
        {
            System.Console.WriteLine("called back");
        }

        public void GainFocus()
        {
            spreadSheetView.Focus();
        }

        public bool NewFile()
        {
            spreadSheetView.Reset();
            Title = "Small n Stats - " + "New File";
            return false;
        }

        public string[] OpenFile()
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = "XLS Files|*.xls";
            openFileDialog1.Title = "Select an Excel File";

            if (openFileDialog1.ShowDialog() == true)
            {
                using (Stream myStream = openFileDialog1.OpenFile())
                {
                    spreadSheetView.Load(myStream, FileFormat.Excel2007);
                    Title = "Small n Stats - " + openFileDialog1.SafeFileName;
                }

                return new string[] { openFileDialog1.SafeFileName, Path.GetDirectoryName(openFileDialog1.FileName) };
            }

            return null;
        }

        public void SaveFile(string path, string title)
        {
            using (Stream myStream = new FileStream(Path.Combine(path, title), FileMode.Create))
            {
                var workbook = spreadSheetView;
                workbook.Save(myStream, FileFormat.Excel2007);
                Title = "Small n Stats - " + title;
            }
        }

        public string SaveFileWithDialog(string title)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.FileName = title;
            saveFileDialog1.Filter = "Excel file (*.xls)|*.xls|All files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (Stream myStream = saveFileDialog1.OpenFile())
                {
                    var workbook = spreadSheetView;
                    workbook.Save(myStream, FileFormat.Excel2007);
                    title = saveFileDialog1.SafeFileName;
                    Title = "Small n Stats - " + saveFileDialog1.SafeFileName;
                }

                return saveFileDialog1.SafeFileName;
            }
            else
            {
                return null;
            }

        }

        public string SaveFileAs(string title)
        {
            SaveFileDialog saveFileDialog1 = new SaveFileDialog();

            saveFileDialog1.FileName = title;
            saveFileDialog1.Filter = "Excel file (*.xls)|*.xls|All files (*.*)|*.*";

            if (saveFileDialog1.ShowDialog() == true)
            {
                using (Stream myStream = saveFileDialog1.OpenFile())
                {
                    var workbook = spreadSheetView;
                    workbook.Save(myStream, FileFormat.Excel2007);
                    title = saveFileDialog1.SafeFileName;
                    Title = "Small n Stats - " + saveFileDialog1.SafeFileName;
                }

                return title;
            }
            else
            {
                return null;
            }

        }

        public void ShutDown()
        {
            Close();
        }

        public void UpdateTitle(string _title)
        {
            Title = "Small n Stats - " + _title;
        }

        public List<double> ParseRange(string range)
        {
            List<double> mReturned = new List<double>();

            try
            {
                var rangeReturned = spreadSheetView.Worksheets[0].Ranges[range];

                spreadSheetView.Worksheets[0].IterateCells(rangeReturned, (row, col, cell) =>
                {
                    double num;
                    if (double.TryParse(cell.Data.ToString(), out num))
                    {
                        mReturned.Add(num);
                    }
                    return true;
                });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return mReturned;
        }
    }
}
