namespace Small_N_Stats.Interface
{
    public interface SpreadsheetInterface
    {
        void CallBackToMain();
        void UpdateTitle(string Title);
        void GainFocus();

        bool NewFile();

        void SaveFile(string path, string title);
        string SaveFileWithDialog(string title);
        string SaveFileAs(string title);

        string[] OpenFile();
        void ShutDown();
    }
}
