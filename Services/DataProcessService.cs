using Models;
using System.Text.Json;

namespace Services
{
	public class DataProcessService : IDataProcessService
	{
		private List<Data> _data;
		private List<string> _invalidFiles;
		private int _errorLines;
		private int _totalLines;


		public DataProcessService()
		{
			_data = new List<Data>();
			_invalidFiles = new List<string>();
			_errorLines = 0;
			_totalLines = 0;
		}

		public void ProcessFile(string readFile, string logFile)
		{
			var fileInfo = new FileInfo(readFile);

			if (fileInfo.Extension != ".cvs" && fileInfo.Extension != ".txt")
			{
				_invalidFiles.Add(readFile);
				updateLog(logFile);
				return;
			}

			var skipFirstLine = fileInfo.Extension == ".cvs" ? true : false;

			foreach (var line in File.ReadLines(readFile))
			{


				// якщо формат .cvs, то пропускаємо перший рядок із заголовками
				if (skipFirstLine)
				{
					skipFirstLine = false;
					continue;
				}

				_totalLines++;
				processLine(line);
			}

			updateLog(logFile);
			_errorLines = 0;
			_totalLines = 0;
		}

		public void EndProcessing(string writeFile)
		{
			if (_data.Count > 0 )
			{
				saveResult(writeFile);
				_data.Clear();
			}
		}

		private void saveResult(string file)
		{
			string json = JsonSerializer.Serialize(_data);

			using var writer = new StreamWriter(file, false);
			writer.WriteLine(json);
		}

		private void updateLog(string file)
		{
			try
			{
				var lines = File.ReadLines(file).ToList();

				int parsed_files = int.Parse(lines[0].Split(' ')[1]);
				int parsed_lines = int.Parse(lines[1].Split(' ')[1]);
				int found_errors = int.Parse(lines[2].Split(' ')[1]);
				string invalid_files = lines[3].Split(' ')[1];

				var toWrite = string.Join("\n",
					$"parsed_files: {++parsed_files}",
					$"parsed_lines: {parsed_lines + _totalLines}",
					$"found_errors: {found_errors + _errorLines}",
					$"invalid_files: {string.Join(", ", _invalidFiles.ToArray())}");

				using var writer = new StreamWriter(file, false);
				writer.WriteLine(toWrite);
			}
			catch
			{
				var toWrite = string.Join("\n",
					$"parsed_files: 1",
					$"parsed_lines: {_totalLines}",
					$"found_errors: {_errorLines}",
					$"invalid_files: {string.Join(", ", _invalidFiles.ToArray())}");

				File.Create(file).Close();
				using var writer = new StreamWriter(file, false);
				writer.WriteLine(toWrite);
			}
		}

		private void processLine(string line)
		{
			try
			{
				var lineFields = line.Split(',');

				string city = lineFields[2].Split(',')[0][2..];
				string service = lineFields[8][1..];
				string name = $"{lineFields[0]}{lineFields[1]}";
				decimal payment = decimal.Parse(lineFields[5][1..]);
				var dateParts = lineFields[6][1..].Split('-');
				DateTime date = new DateTime(int.Parse(dateParts[0]), int.Parse(dateParts[2]), int.Parse(dateParts[1]));
				long account_number = long.Parse(lineFields[7][1..]);

				if (_data.FirstOrDefault(d => d.City == city && d.Services.FirstOrDefault(s => s.Name == service) != null) != null)
				{
					_data.First(d => d.City == city).Services.First(s => s.Name == service).Payers.Add(new Payer
					{
						Name = name,
						Payment = payment,
						Date = date,
						Account_number = account_number
					});
					_data.First(d => d.City == city).Services.First(s => s.Name == service).Total += payment;
					_data.First(d => d.City == city).Total += payment;
				}

				if (_data.FirstOrDefault(d => d.City == city && d.Services.FirstOrDefault(s => s.Name == service) == null) != null)
				{
					_data.First(d => d.City == city).Services.Add(new Service
					{
						Name = service,
						Total = payment,
						Payers = new List<Payer> { new Payer {
								Name = name,
								Payment = payment,
								Date = date,
								Account_number = account_number
							}}
					});
					_data.First(d => d.City == city).Total += payment;
				}

				if (_data.FirstOrDefault(d => d.City == city) == null)
				{
					_data.Add(new Data
					{
						City = city,
						Total = payment,
						Services = new List<Service> { new Service
							{
								Name = service,
								Total = payment,
								Payers = new List<Payer> {new Payer
								{
									Name = name,
									Payment = payment,
									Date = date,
									Account_number = account_number
								}}
							}}
					});
				}

			}
			catch
			{
				_errorLines++;
			}
		}
	}
}