using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Parkeasy
{
    /// <summary>
    /// Program Class.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Main method, runs on launch.
        /// </summary>
        /// <param name="args">String Array</param>
        public static void Main(string[] args)
        {
            //Passes args into BuildWebHost method.
            BuildWebHost(args).Run();
        }

        //BuildWebHost method with string array parameter, runs startup and builds system.
        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
