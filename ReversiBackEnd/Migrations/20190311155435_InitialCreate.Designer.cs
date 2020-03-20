﻿// <auto-generated />
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ReversiBackEnd.DAL;

namespace ReversiBackEnd.Migrations
{
    [DbContext(typeof(UserContext))]
    [Migration("20190311155435_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.2-servicing-10034")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ReversiBackEnd.Models.Spel", b =>
                {
                    b.Property<int>("GameId")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Omschrijving");

                    b.Property<string>("Token");

                    b.Property<int>("aanDeBeurt");

                    b.Property<string>("bord");

                    b.Property<string>("spelerWitToken");

                    b.Property<string>("spelerZwartToken");

                    b.HasKey("GameId");

                    b.ToTable("Spels");

                    b.HasData(
                        new
                        {
                            GameId = 1,
                            Omschrijving = "test",
                            Token = "Hgju5pGMCUaYSE6GBW02dg==",
                            aanDeBeurt = 1,
                            bord = "[[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,1,2,0,0,0],[0,0,0,2,1,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0],[0,0,0,0,0,0,0,0]]",
                            spelerWitToken = "11",
                            spelerZwartToken = "12"
                        });
                });
#pragma warning restore 612, 618
        }
    }
}
