using DbUp;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace DatabaseUpdater
{
    public static class DatabaseUpdater
    {
        public static void Update(string connectionString)
        {
            var upgrader =
                DeployChanges.To
                    .SqlDatabase(connectionString)
                    .WithScriptsEmbeddedInAssembly(Assembly.GetExecutingAssembly())
                    .LogToConsole()
                    .Build();

            var result = upgrader.PerformUpgrade();


        }
    }
}
