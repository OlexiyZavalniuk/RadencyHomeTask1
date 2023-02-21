using System.Configuration;

namespace Services
{
	public class WatcherService
	{
		private FileSystemWatcher? _watcher;
		private List<string> _processedFiles;
		private IDataProcessService _dataProcessService;

		public void Start()
		{
			if (!Directory.Exists(ConfigurationManager.AppSettings["Afolder"]) ||
				!Directory.Exists(ConfigurationManager.AppSettings["Bfolder"]))
			{
				Console.WriteLine("Configuration file fail");
				return;
			}

			_processedFiles = new List<string>();
			_dataProcessService = new DataProcessService();
			process();

			_watcher = new FileSystemWatcher(ConfigurationManager.AppSettings["Afolder"]);
			_watcher.Created += onCreated;

			_watcher.EnableRaisingEvents = true;
		}

		public void Stop()
		{
			_watcher?.Dispose();
		}

		private void onCreated(object sender, FileSystemEventArgs e)
		{
			process();
		}

		private void process()
		{
			var files = Directory.GetFiles(ConfigurationManager.AppSettings["Afolder"]);
			var resultFolder = $"{ConfigurationManager.AppSettings["Bfolder"]}\\{DateTime.Now.ToString("MM-dd-yyyy")}";
			if (!Directory.Exists(resultFolder))
			{
				Directory.CreateDirectory(resultFolder);
			}

			foreach (var file in files)
			{
				if (!_processedFiles.Contains(file))
				{
					_processedFiles.Add(file);
					_dataProcessService.ProcessFile(file, $"{resultFolder}\\meta.log");
				}
			}

			var i = Directory.EnumerateFiles(resultFolder, "output*").Count();
			_dataProcessService.EndProcessing($"{resultFolder}\\output{++i}.json");
		}
	}
}
