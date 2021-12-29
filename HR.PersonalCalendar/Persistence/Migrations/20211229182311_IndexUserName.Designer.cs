﻿// <auto-generated />
using System;
using HR.PersonalCalendar.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HR.PersonalCalendar.Persistence.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20211229182311_IndexUserName")]
    partial class IndexUserName
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.13")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("HR.PersonalCalendar.Model.PersonalTimetable", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTimeOffset>("DateCreated")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("datetimeoffset")
                        .HasDefaultValueSql("sysdatetimeoffset()");

                    b.Property<DateTimeOffset?>("DateLastModified")
                        .HasColumnType("datetimeoffset");

                    b.Property<int?>("ElementId")
                        .HasColumnType("int");

                    b.Property<string>("ElementName")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<int>("ElementType")
                        .HasColumnType("int");

                    b.Property<string>("InstituteName")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<bool>("IsVisible")
                        .HasColumnType("bit");

                    b.Property<int?>("SchoolYearId")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("nvarchar(25)");

                    b.HasKey("Id");

                    b.HasIndex("UserName")
                        .HasDatabaseName("IX_PersonalTimetable_UserName");

                    b.ToTable("PersonalTimetable");
                });
#pragma warning restore 612, 618
        }
    }
}
