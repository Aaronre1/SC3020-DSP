// See https://aka.ms/new-console-template for more information
using SC3020_DSP.Application;

var experiment = new Experiments();
var choices = new[] { 1, 6 };

while (true)
{
    Console.WriteLine("=========================== SC3020 - Project 1 ==========================");
    foreach (var c in choices)
    {
        switch (c)
        {
            case 1:
                Console.WriteLine("1. Experiment 1 - Seed NBA Data");
                break;
            case 2:
                Console.WriteLine("2. Experiment 2 - Build B+ Tree");
                break;
            case 3:
                Console.WriteLine("3. Experiment 3 - Retrieve 'FG_PCT_home' == 0.5");
                break;
            case 4:
                Console.WriteLine("4. Experiment 4 - Retrieve 'FG_PCT_home' BETWEEN 0.6 to 1 (inclusive)");
                break;
            case 6:
                Console.WriteLine("6. Exit");
                break;
        }
    }

    Console.WriteLine("Enter an option: ");
    var line = Console.ReadLine();
    if (int.TryParse(line, out var choice))
    {
        if (!choices.Contains(choice))
        {
            Console.WriteLine("Invalid choice");
            continue;
        }

        switch (choice)
        {
            case 1:
                experiment.Initialize();
                choices = new[] { 2, 6 };
                break;
            case 2:
                experiment.BuildBPlusTree();
                choices = new[] { 3, 4, 5, 6 };
                break;
            case 3:
                experiment.FindRecords(0.5M);
                break;
            case 4:
                experiment.FindRecords(0.6M, 1M);
                break;
            case 6:
                return;
            default:
                Console.WriteLine("Invalid choice");
                break;
        }
    }
}

