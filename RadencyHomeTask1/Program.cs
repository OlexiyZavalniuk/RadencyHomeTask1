var srv = new Services.WatcherService();

Console.WriteLine("Hello, dude!\n\'start\' to start processing\n\'stop\' to stop\n\'quit' to end work");

var entered = "";
do
{
	Console.Write("> ");
	entered = Console.ReadLine();

	if (entered == "start")
	{
		srv.Start();
		Console.WriteLine("Started.");
	}

	if (entered == "stop")
	{
		srv.Stop();
		Console.WriteLine("Stoped.");
	}

	if (entered != "start" && entered != "stop" && entered != "quit")
	{
		Console.WriteLine("Wrong command, dude!");
	}
}
while (entered != "quit");

Console.WriteLine("Goodbye, dude!");
