using Microsoft.Threading;
using System;

namespace WebsiteCacher
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Application started. Creating modules.");

            var storage = new Storage("downloads/");
            var database = new DatabaseContext();
            var hashSolver = new HashSolver();

            var resourceManager = new ResourceManager(database, storage, hashSolver);
            var pageManager = new PageManager(database, resourceManager);
            var pageQueryManager = new PageQueryManager(database, pageManager);

            Console.WriteLine("Starting server.");

            // We are using asynchronout functions. The problem is that the database can be accessed only from a single
            // thread and therefore all functions must implicitly run in a single thread.
            // This is problem only for Console Application, Windows Forms and others have its own SynchronizationContext
            // https://devblogs.microsoft.com/pfxteam/await-synchronizationcontext-and-console-apps/

            AsyncPump.Run(async delegate
            {
                var s = new Server(8080, resourceManager, pageQueryManager);
                await s.Start();
            });

        }
    }
}
