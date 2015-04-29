namespace FileDeployment
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var sysWatcher =
                new Watcher(
                    @"C:\Users\Jamie.Rees\Documents\Visual Studio 2012\Projects\HostedSignatures.Automation\HostedSignatures.Tests\bin\Debug\",
                    @"Y:\Builds\Hosted Signatures\MTM\HostedSignaturesSmokeTest\");
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
