using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ReversiBackEnd.DAL;
using ReversiBackEnd.Models;

namespace ReversiBackEnd
{
    public class Program
    {        

        public static void Main(string[] args)
        {
            string board = Spel.CreateBoard();
            string token = Spel.CreateGameToken();
            Spel spel = new Spel { Omschrijving = "test", spelerWitToken = "11", spelerZwartToken = "12", bord = board, Token = token, aanDeBeurt = 1 };
            
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>();
    }
}
