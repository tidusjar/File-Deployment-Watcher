namespace FileDeployment
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var sysWatcher =
                new Watcher(
                    @"C:\WatchLocation\",
                    @"C:\DeployLocation\");
            Console.WriteLine("The Watcher has started");
            
            var filters = new string[]
            {
                "*.dll",
                "*.txt"
            };

            sysWatcher.StartWatch(filters);

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }
    }
}
