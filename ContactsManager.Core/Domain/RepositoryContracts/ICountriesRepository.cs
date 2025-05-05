using Entities;

namespace RepositoryContracts
{
  /// <summary>
  /// Represent data access logic for mange Country entity
  /// </summary>
  public interface ICountriesRepository
  {
    /// <summary>
    /// Add a new country object to the data store
    /// </summary>
    /// <param name="country"> country object to add</param>
    /// <returns> country object after adding  it to data store</returns>
    Task<Country> AddCountry(Country country);
    /// <summary>
    /// return all country in data store
    /// </summary>
    /// <returns> return all country from the table</returns>
    Task<List<Country>> GetAllCountries();
    /// <summary>
    /// retuen country object besed on the given  country id ,otherwise its return null
    /// </summary>
    /// <param name="countryID">country id for searching </param>
    /// <returns>maching country or null</returns>
    Task<Country?> GetCountryByCountryId(Guid countryID);
    /// <summary>
    /// retuen country object besed on the given  country name ,otherwise its return null
    /// </summary>
    /// <param name="countryName">country name for searching </param>
    /// <returns>maching country or null</returns>
    Task<Country?> GetCountryByCountryName(string countryName);
  }
}
