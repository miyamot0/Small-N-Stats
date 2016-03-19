using Small_N_Stats.Interface;
using Small_N_Stats.View;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Small_N_Stats.ViewModel
{
    class MainWindowViewModel : ViewModelBase, OutputWindowInterface
    {
        public MainWindow MainWindow { get; set; }
        public SpreadsheetInterface _interface { get; set; }

        public RelayCommand FileNewCommand { get; set; }
        public RelayCommand FileOpenCommand { get; set; }
        public RelayCommand FileSaveCommand { get; set; }
        public RelayCommand FileSaveAsCommand { get; set; }
        public RelayCommand FileCloseCommand { get; set; }

        public RelayCommand FileSaveNoDialogCommand { get; set; }

        public RelayCommand ViewLoadedCommand { get; set; }
        public RelayCommand ViewClosingCommand { get; set; }

        /* Menu Items */

        public RelayCommand DiscountingWindowCommand { get; set; }
        public RelayCommand EscalationWindowCommand { get; set; }
        public RelayCommand TheilSenWindowCommand { get; set; }
        public RelayCommand OmnibusTauWindowCommand { get; set; }

        /* End Menu Items */

        OutputWindow mOutput;
        RichTextBox mTextBox;
        FlowDocument fd;

        bool isOutputShown;

        bool haveFileLoaded = false;
        string title = "New File";
        string path = "";

        /* For Demo Purposes */

        public RelayCommand TestCommand { get; set; }

        /* ^^^ ^^^ ^^^ */

        public MainWindowViewModel()
        {
            FileNewCommand = new RelayCommand(param => CreateNewFile(), param => true);
            FileOpenCommand = new RelayCommand(param => OpenFile(), param => true);
            FileSaveCommand = new RelayCommand(param => SaveFile(), param => true);
            FileSaveAsCommand = new RelayCommand(param => SaveFileAs(), param => true);
            FileCloseCommand = new RelayCommand(param => CloseProgram(), param => true);

            FileSaveNoDialogCommand = new RelayCommand(param => SaveFileWithoutDialog(), param => true);

            ViewLoadedCommand = new RelayCommand(param => ViewLoaded(), param => true);
            ViewClosingCommand = new RelayCommand(param => ViewClosed(), param => true);

            /* Menu Items */

            DiscountingWindowCommand = new RelayCommand(param => OpenDiscountingWindow(), param => true);
            EscalationWindowCommand = new RelayCommand(param => OpenEscalationWindow(), param => true);
            TheilSenWindowCommand = new RelayCommand(param => OpenTheilSenWindow(), param => true);
            OmnibusTauWindowCommand = new RelayCommand(param => OpenOmnibusTauWindow(), param => true);

            /* End Menu Items */

        }

        private void OpenOmnibusTauWindow()
        {
            var mContext = new OmnibusTauViewModel();
            mContext.mWindow = MainWindow;
            mContext.mInterface = this;

            var mWin = new OmnibusTauWindow();
            mWin.DataContext = mContext;
            mWin.Owner = MainWindow;
            mWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mWin.Topmost = true;

            mContext.windowRef = mWin;

            mWin.Show();
        }

        private void OpenTheilSenWindow()
        {
            var mContext = new TheilSenViewModel();
            mContext.mWindow = MainWindow;
            mContext.mInterface = this;

            var mWin = new TheilSenWindow();
            mWin.DataContext = mContext;
            mWin.Owner = MainWindow;
            mWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mWin.Topmost = true;

            mContext.windowRef = mWin;

            mWin.Show();
        }

        private void OpenDiscountingWindow()
        {

            var mContext = new DiscountingViewModel();
            mContext.mWindow = MainWindow;
            mContext.mInterface = this;

            var mWin = new DiscountingWindow();
            mWin.DataContext = mContext;
            mWin.Owner = MainWindow;
            mWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mWin.Topmost = true;

            mContext.windowRef = mWin;

            mWin.Show();
        }

        private void OpenEscalationWindow()
        {
            var mContext = new EscalationViewModel();
            mContext.mWindow = MainWindow;
            mContext.mInterface = this;

            var mWin = new EscalationWindow();
            mWin.DataContext = mContext;
            mWin.Owner = MainWindow;
            mWin.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mWin.Topmost = true;

            mContext.windowRef = mWin;

            mWin.Show();
        }

        private void CreateNewFile()
        {
            title = "New File";
            haveFileLoaded = _interface.NewFile();
        }

        private void SaveFile()
        {
            if (haveFileLoaded)
            {
                SaveFileWithoutDialog();
            }
            else
            {
                title = _interface.SaveFileWithDialog(title);

                if (title != null)
                {
                    haveFileLoaded = true;
                }

            }
        }

        private void SaveFileAs()
        {
            title = _interface.SaveFileAs(title);

            if (title != null)
            {
                haveFileLoaded = true;
            }
        }

        private void SaveFileWithoutDialog()
        {
            if (haveFileLoaded)
            {
                _interface.SaveFile(path, title);
            }
        }

        private void OpenFile()
        {
            string[] returned = _interface.OpenFile();

            if (returned != null)
            {
                title = returned[0];
                path = returned[1];
            }
        }

        private void CloseProgram()
        {
            _interface.ShutDown();
        }

        private void ViewLoaded()
        {
            _interface.GainFocus();
            mOutput = new OutputWindow();
            mOutput.Owner = MainWindow;
            mOutput.Closed += setWindowToFalse;
            mOutput.Show();
            isOutputShown = true;

            SendMessageToOutput("---------------------------------------------------");
            SendMessageToOutput("Loadin core libraries...");
            SendMessageToOutput("Core libraries loaded.");
        }

        private void ViewClosed()
        {
            Properties.Settings.Default.Save();
        }

        public void ParagraphReporting(Paragraph results)
        {
            if (isOutputShown)
            {
                mOutput.Activate();
                mTextBox = mOutput.outputWindow;
                fd = mOutput.outputWindow.Document;
            }
            else
            {
                mOutput = new OutputWindow();
                mOutput.Owner = MainWindow;
                mOutput.Closed += setWindowToFalse;
                mOutput.Show();
                isOutputShown = true;

                RichTextBox mTextBox = mOutput.outputWindow;
                FlowDocument fd = mOutput.outputWindow.Document;
            }

            fd.Blocks.Add(results);
            mOutput.outputWindow.ScrollToEnd();
            mOutput.Scroller.ScrollToEnd();
        }

        private void CallBack()
        {
            _interface.CallBackToMain();
            _interface.UpdateTitle(title);
        }

        private void setWindowToFalse(object sender, EventArgs e)
        {
            ((Window)sender).Closed -= setWindowToFalse;
            isOutputShown = false;
            MainWindow.Activate();
        }

        public void SendMessageToOutput(string message)
        {
            Paragraph para = new Paragraph();
            para.Inlines.Add(message);
            ParagraphReporting(para);
        }
    }
}
