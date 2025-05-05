using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
  public class PersonsRepository : IPersonsRepository
  {
    private readonly ApplicationDbContext _db;
    private readonly ILogger<IPersonsRepository> _logger;

    public PersonsRepository(ApplicationDbContext db,ILogger<IPersonsRepository>logger)
    {
      _db = db;
      _logger = logger;
    }
    public async Task<Person> AddPerson(Person person)
    {
      _db.Persons.Add(person);
      await _db.SaveChangesAsync();
      return person;
    }

    public async Task<bool> DeletePersonByPerosnID(Guid personID)
    {
      _db.Persons.RemoveRange(_db.Persons.Where(x=>x.PersonID==personID));
      int rowsDelete= await _db.SaveChangesAsync();
      return rowsDelete>0;
    }

    public async Task<List<Person>> GetAllPersons()
    {
      _logger.LogInformation("GetAllPersons method of PersonRepository");

      return await _db.Persons.Include("Country").ToListAsync();
    }

    public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
    {
      _logger.LogInformation("GetFilterPerson method of PersonsRepository");

      return await _db.Persons.Include("Country").Where(predicate).ToListAsync();

    }

    public async Task<Person?> GetPersonByPersonId(Guid personID)
    {
      return await _db.Persons.Include("Country").FirstOrDefaultAsync(x=>x.PersonID==personID);

    }

    public async Task<Person?> UpdatePerson(Person person)
    {
     Person?  personMatch =await _db.Persons.FirstOrDefaultAsync(x=>x.PersonID == person.PersonID);
      if (personMatch == null) 
        return null;

      personMatch.Gender=person.Gender;
      personMatch.Address=person.Address;
      personMatch.Country=person.Country;
      personMatch.CountryID=person.CountryID;
      personMatch.DateOfBirth=person.DateOfBirth;
      personMatch.Email=person.Email;
      personMatch.ReceiveNewsLetters=person.ReceiveNewsLetters;
      int rowsUpdate= await _db.SaveChangesAsync();
      return personMatch;

    }
  }
}
