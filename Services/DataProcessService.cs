using Models;

namespace Services
{
	public class DataProcessService
	{
		private List<Models.Data> _data;
		private int _errorLines;
		private int _totalLines;


		public DataProcessService()
		{
			_data = new List<Models.Data>();
			_errorLines = 0;
			_totalLines = 0;
		}

		public void ProcessFile(string file)
		{
			var fileInfo = new FileInfo(file);
			var skipFirstLine = fileInfo.Extension == ".cvs" ? true : false;

			foreach (var line in File.ReadLines(file))
			{
				_totalLines++;

				// якщо формат .cvs, то пропускаємо перший рядок із заголовками
				if (skipFirstLine)
				{
					skipFirstLine = false;
					continue;
				}

				try
				{
					var lineFields = line.Split(',');

					string city = lineFields[2].Split(',')[0];
					string service = lineFields[6];
					string name = $"{lineFields[0]} {lineFields[1]}";
					decimal payment = decimal.Parse(lineFields[3]);
					DateTime date = DateTime.Parse(lineFields[4]);
					long account_number = long.Parse(lineFields[5]);

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
}