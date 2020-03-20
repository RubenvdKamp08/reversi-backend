using Microsoft.EntityFrameworkCore;
using ReversiBackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReversiBackEnd.DAL
{
    public class UserContext : DbContext
    {
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<Spel> Spels { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            string board = Spel.CreateBoard();
            string token = Spel.CreateGameToken();
            Spel spel = new Spel { GameId = 1, Omschrijving = "test", spelerWitToken = "11", spelerZwartToken = "12", bord = board, Token = token, aanDeBeurt = 1 };

            modelBuilder.Entity<Spel>().HasData(spel);
        }

    }
}
