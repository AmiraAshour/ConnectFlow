using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;

namespace Repositories
{
  public class CountriesRepositoey : ICountriesRepository
  {
    private readonly ApplicationDbContext _db;

    public CountriesRepositoey(ApplicationDbContext db)
    {
      _db = db;
    }
    public async Task<Country> AddCountry(Country country)
    {
      _db.Countries.Add(country);
      await _db.SaveChangesAsync();
      return country;
    }

    public async Task<List<Country>> GetAllCountries()
    {
      return await _db.Countries.ToListAsync();
    }

    public async Task<Country?> GetCountryByCountryId(Guid countryID)
    {
      return await _db.Countries.FirstOrDefaultAsync(x => x.CountryID == countryID);
    }

    public async Task<Country?> GetCountryByCountryName(string countryName)
    {
      return await _db.Countries.FirstOrDefaultAsync(x=>x.CountryName == countryName);
    }
  }
}
