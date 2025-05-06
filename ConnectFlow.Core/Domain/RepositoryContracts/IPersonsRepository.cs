using Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RepositoryContracts
{
  /// <summary>
  /// Represent data access logic for mange person entity
  /// </summary>
  public interface IPersonsRepository
  {
    /// <summary>
    /// Add a new person object to data store
    /// </summary>
    /// <param name="person">person object to add </param>
    /// <returns>return person object after adding to the table</returns>
    Task<Person> AddPerson(Person person);
    /// <summary>
    /// return all persons in data store
    /// </summary>
    /// <returns> return person object from table</returns>
    Task<List<Person>> GetAllPersons();

    /// <summary>
    /// retuen person object besed on the given  person id ,otherwise its return null
    /// </summary>
    /// <param name="personID">person id for searching </param>
    /// <returns>maching person or null</returns>
    Task<Person?> GetPersonByPersonId(Guid personID);

    /// <summary>
    /// return all person object based on the given expression
    /// </summary>
    /// <param name="predicate">LINQ query to check</param>
    /// <returns>all matching persons </returns>
    Task<List<Person>> GetFilteredPersons(Expression<Func<Person,bool>>predicate);

    /// <summary>
    /// delete person based on perosn id and
    /// </summary>
    /// <param name="personID">perosn id for delete</param>
    /// <returns>return ture ,if delete is successful otherwise return false</returns>
    Task<bool> DeletePersonByPerosnID(Guid personID);

    /// <summary>
    /// update person details based on the person id
    /// </summary>
    /// <param name="person">person object to update </param>
    /// <returns>update perosn object</returns>
    Task<Person?>UpdatePerson(Person person);
  }
}
