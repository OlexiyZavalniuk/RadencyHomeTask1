namespace Models
{
	public class Payer
	{
		public string Name { get; set; }
		public decimal Payment { get; set; }
		public DateTime Date { get; set; }
		public long Account_number { get; set; }
	}

	public class Service
	{
		public string Name { get; set; }
		public List<Payer> Payers { get; set; }
		public decimal Total { get; set; }
	}

	public class Data
	{
		public string City { get; set; }
		public List<Service> Services { get; set; }
		public decimal Total { get; set; }
	}
}