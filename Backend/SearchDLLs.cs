namespace Backend
{
    public static class SearchDLLs
    {
        public static string[] SearchDLLsInDirectory(string[] dllNames, string directoryPath)
        {
            List<string> dllPaths = new List<string>();

            foreach (string dll in dllNames)
            {   
                string dllfilename = dll + ".dll";
                dllPaths.AddRange(Directory.GetFiles(directoryPath, dllfilename));
            }
            return dllPaths.ToArray();
        }
    }

}
