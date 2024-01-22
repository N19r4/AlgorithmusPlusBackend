namespace Backend
{
    public static class Tools
    {
        static bool IsFileLocked(FileInfo file)
        {
            try
            {
                Console.WriteLine("Trying to open file: " + file.FullName);
                using (FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                Console.WriteLine("Error: File is locked: " + file.FullName);
                return true;
            }

            //file is not locked
            return false;
        }
        //     waitForUnlockedFile(FileInfo file){
        // var unlocked = False
        // while(isFileLocked(file){}
        // unlocked=true
        // return unlocked
        // }
        public static bool WaitForUnlockedFile(string path)
        {
            //write line with name of the file saying waiting for file to be unlocked
            Console.WriteLine("Waiting for file to be unlocked: " + path);
            FileInfo file = new FileInfo(path);
            bool unlocked = false;
            if (!File.Exists(path))
            {
                unlocked = true;
                return unlocked;
            }
            while (IsFileLocked(file))
            {
                //wait 200ms
                System.Threading.Thread.Sleep(200);
            }
            unlocked = true;
            Console.WriteLine("File unlocked: " + path);
            return unlocked;
        }
    }

}