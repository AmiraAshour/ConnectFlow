using System;
using System.Collections.Generic;
using System.Text.Json;
using ContactsManager.Core.Domain.IdentityEnitities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Entities
{
  public class ApplicationDbContext : IdentityDbContext<ApplicationUser,ApplicationRole,Guid>
  {
    public virtual DbSet<Country> Countries { get; set; }
    public virtual DbSet<Person> Persons { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);

      modelBuilder.Entity<Country>().ToTable("Countries");
      modelBuilder.Entity<Person>().ToTable("Persons");

      string countriesJson = File.ReadAllText("countries.json");
      List<Country>? countries = JsonSerializer.Deserialize<List<Country>>(countriesJson);
      foreach (var country in countries)
        modelBuilder.Entity<Country>().HasData(country);

      string personsJson = File.ReadAllText("persons.json");
      List<Person>? persons = JsonSerializer.Deserialize<List<Person>>(personsJson);
      foreach (var person in persons)
        modelBuilder.Entity<Person>().HasData(person);

      //Fluent API
      modelBuilder.Entity<Person>().Property(x => x.TIN)
        .HasColumnName("TextIdentificationNumber")
        .HasColumnType("varchar(8)")
        .HasDefaultValue("ABCD1234");

      //modelBuilder.Entity<Person>().HasIndex(x => x.TIN).IsUnique();
      modelBuilder.Entity<Person>()
        .HasCheckConstraint("CHK_TIN", "len([TextIdentificationNumber]) = 8");

      //modelBuilder.Entity<Person>(entity=> entity
      //.HasOne<Country>(c=>c.Country)
      //.WithMany(p=>p.Persons)
      //.HasForeignKey(p=>p.CountryID));
    }
    public List<Person> sp_GetAllPersons()
    {
      return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
    }
    public int sp_InsertPerson(Person person)
    {
      SqlParameter[] sp = new SqlParameter[] {
      new SqlParameter("@PersonID",person.PersonID),
      new SqlParameter("@PersonName",person.PersonName),
      new SqlParameter("@Email",person.Email),
      new SqlParameter("@DateOfBirth",person.DateOfBirth),
      new SqlParameter("@Gender",person.Gender),
      new SqlParameter("@CountryID",person.CountryID),
      new SqlParameter("@Address",person.Address),
      new SqlParameter("@ReceiveNewsLetters",person.ReceiveNewsLetters),
      };
     return Database.ExecuteSqlRaw("EXECUTE [dbo].[InsertPerson] @PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", sp);

    }
  }
}
