namespace FileDeployment
{
    public class Watcher : WatcherBase
    {
        /// <summary>
        /// The watcher class
        /// </summary>
        /// <param name="watchLocation"> The directory you want to monitor</param>
        /// <param name="deployLocation"> The directory you want the files to be copied to</param>
        public Watcher(string watchLocation,string deployLocation) : base(watchLocation, deployLocation)
        {
        }
    }
}
