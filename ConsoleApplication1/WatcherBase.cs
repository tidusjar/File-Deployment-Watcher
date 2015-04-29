namespace FileDeployment
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Timers;

    public abstract class WatcherBase : IWatcher
    {
        private string WatchLocation { get; set; }
        private string DeployLocation { get; set; }
        public virtual List<FileSystemWatcher> SystemWatchers { get; set; }
        public virtual Dictionary<string, DateTime> ProcessedFiles { get; set; }
        private bool CurrentlyProcessing { get; set; }
        public virtual List<string> ToBeProcessed { get; set; }

        protected WatcherBase(string watchLocation, string deployLocation)
        {
            WatchLocation = watchLocation;
            DeployLocation = deployLocation;
        }

        /// <summary>
        /// Starts the watcher using the locations passed into the ctor
        /// </summary>
        /// <param name="filter">This is what file extensions you want to monitor, e.g. if you only want to monitor .Dll's then this would be: "*.dll"
        /// The default would be *.* (all files) if you want this then pass in an empty string array.</param>
        public virtual void StartWatch(string[] filter)
        {
            SystemWatchers = new List<FileSystemWatcher>();

            foreach (var f in filter)
            {
                var systemWatcher = new FileSystemWatcher(this.WatchLocation)
                {
                    EnableRaisingEvents = true,
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Attributes | NotifyFilters.Size,
                    Filter = f
                };
                SystemWatchers.Add(systemWatcher);

                ProcessedFiles = new Dictionary<string, DateTime>();
                ToBeProcessed = new List<string>();
                HookUpEvents();
                InitTimer();
            }



            //ProcessedFiles = new Dictionary<string, DateTime>();
            //ToBeProcessed = new List<string>();
            //HookUpEvents();
            //InitTimer();
        }

        public void OnChanged(object source, FileSystemEventArgs e)
        {
            if (!ProcessedFiles.ContainsKey(e.Name))
            {
                Console.WriteLine("{0} : Copying {1}", DateTime.Now, e.Name);
                if (File.Exists(string.Format("{0}{1}", DeployLocation, e.Name)))
                {
                    File.Delete(string.Format("{0}{1}", DeployLocation, e.Name));
                }

                Thread.Sleep(100);
                if (File.Exists(WatchLocation + e.Name))
                {
                    File.Copy(string.Format("{0}{1}", WatchLocation, e.Name),
                        string.Format("{0}{1}", DeployLocation, e.Name));

                    ProcessedFiles.Add(e.Name, File.GetLastWriteTime(WatchLocation + e.Name));
                }
                else
                {
                    Console.WriteLine("{0} : The file {1} does not exist. We cannot copy it.", DateTime.Now, e.Name);
                }
            }
            else
            {
                DateTime outDate;
                ProcessedFiles.TryGetValue(e.Name, out outDate);
                if (ProcessedFiles.ContainsKey(e.Name) && File.GetLastWriteTime(WatchLocation + e.Name) > outDate)
                {
                    AddToBeProcessed(e.Name);
                }
            }
        }

        public void OnError(object source, ErrorEventArgs e)
        {
            Console.WriteLine("Exception");

            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                Console.WriteLine(DateTime.Now.ToShortTimeString());
                Console.WriteLine("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message);
                Console.WriteLine("The file system watcher experienced an internal buffer overflow: " + e.GetException().StackTrace);
            }

            Console.WriteLine("{2} : Type: {0}. Message: {1}", e.GetException().GetType(), e.GetException().Message, DateTime.Now);
        }

        public void OnRename(object source, RenamedEventArgs e)
        {

            var wct = e.ChangeType;
            Console.WriteLine(DateTime.Now.ToShortTimeString());
            Console.WriteLine("File {0} {2} to {1}", e.OldFullPath, e.FullPath, wct);
        }

        /// <summary>
        /// Hookup all of the event handlers
        /// </summary>
        public virtual void HookUpEvents()
        {
            foreach (var watcher in SystemWatchers)
            {
                watcher.Changed += OnChanged;
                watcher.Created += OnChanged;
                watcher.Renamed += OnRename;
                watcher.Error += OnError;
            }
        }

        /// <summary>
        /// Clear the processing list and process the files that are queued
        /// </summary>
        public virtual void ClearProcessedList()
        {
            if (!CurrentlyProcessing && ProcessedFiles.Count > 0)
            {
                //We are now processing
                CurrentlyProcessing = true;

                Console.WriteLine("{0} : Clearing the List", DateTime.Now);
                ProcessedFiles.Clear();
                Console.WriteLine("{0} : Cleared the List", DateTime.Now);

                //Finished Processing
                CurrentlyProcessing = false;

                if (ToBeProcessed.Count > 0)
                {
                    Console.WriteLine("{0} : Copying files that have had changes since the last copy", DateTime.Now);
                    ToBeProcessed.ForEach(x => OnChanged(new object(), new FileSystemEventArgs(WatcherChangeTypes.Changed, WatchLocation, x.ToString(CultureInfo.DefaultThreadCurrentUICulture))));
                    ToBeProcessed.Clear();
                }
            }
        }

        private System.Timers.Timer _timer;
        /// <summary>
        /// The timer that we should clear down the processing list.
        /// </summary>
        public virtual void InitTimer()
        {
            _timer = new System.Timers.Timer(40000);
            _timer.Elapsed += ClearList;
            _timer.Start();
            GC.KeepAlive(_timer);
        }

        /// <summary>
        /// Clear the processing list in a new thread
        /// </summary>
        /// <param name="sender">This is an event handler</param>
        /// <param name="e">This is an event handler</param>
        public virtual async void ClearList(object sender, ElapsedEventArgs e)
        {
            await Task.Run(() => ClearProcessedList());
        }

        /// <summary>
        /// Add the files to the processing list
        /// </summary>
        /// <param name="file">the file name, we will check if the file has already be queued</param>
        public virtual void AddToBeProcessed(string file)
        {
            if (!ToBeProcessed.Contains(file))
            {
                ToBeProcessed.Add(file);
            }
        }
    }
}
