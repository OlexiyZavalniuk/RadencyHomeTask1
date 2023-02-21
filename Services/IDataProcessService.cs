namespace Services
{
	internal interface IDataProcessService
	{
		public void ProcessFile(string readFile, string logFile);

		public void EndProcessing(string writeFile);
	}
}
