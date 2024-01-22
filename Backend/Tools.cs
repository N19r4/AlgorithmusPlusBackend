namespace Backend
{
    public static class Tools
    {
        static bool IsFileLocked(FileInfo file)
        {
            try
            {
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
            FileInfo file = new FileInfo(path);
            bool unlocked = false;
            while (IsFileLocked(file))
            {
                unlocked = true;
            }
            return unlocked;
        }
    }

}