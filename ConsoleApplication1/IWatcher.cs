namespace FileDeployment
{
    using System.IO;

    public interface IWatcher
    {
        void StartWatch(string[] filters);
        void OnChanged(object source, FileSystemEventArgs e);
        void OnError(object source, ErrorEventArgs e);
        void OnRename(object source, RenamedEventArgs e);
    }
}
