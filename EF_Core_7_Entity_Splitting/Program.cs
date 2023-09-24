

using Microsoft.EntityFrameworkCore;

Application10DbContext context = new();

#region Entity Splitting
//Veritabanındaki Birden fazla fiziksel tabloyu Entity Framework Core kısmında tek bir entity ile temsil etmemizi sağlayan bir özelliktir.
#endregion

#region Örnek

#region Veri Eklerken
Person person = new()
{
    Name = "Musa",
    Surname = "Yılmaz",
    City = "Ankara",
    Country = "Türkiye",
    PhoneNumber = "5458749658",
    PostCode = "0684",
    Street = "11.Cadde"
};

await context.Persons.AddAsync(person);
await context.SaveChangesAsync();
#endregion

#region Veri Okurken
person = await context.Persons.FindAsync(1);
Console.WriteLine();
#endregion

#endregion
public class Person
{
    #region Persons Tablosu
    public int Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }

    #endregion
    #region PhoneNumbers Tablosu
    public string? PhoneNumber { get; set; }
    #endregion
    #region Addresses Tablosu
    public string Street { get; set; }
    public string City { get; set; }
    public string? PostCode { get; set; }
    public string Country { get; set; }
    #endregion
}
class Application10DbContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entityBuilder =>
        {
            entityBuilder.ToTable("Persons")
                .SplitToTable("PhoneNumbers", tableBuilder =>
                {
                    tableBuilder.Property(person => person.Id).HasColumnName("PersonId");
                    tableBuilder.Property(person => person.PhoneNumber);
                })
                .SplitToTable("Addresses", tableBuilder =>
                {
                    tableBuilder.Property(person => person.Id).HasColumnName("PersonId");
                    tableBuilder.Property(person => person.Street);
                    tableBuilder.Property(person => person.City);
                    tableBuilder.Property(person => person.PostCode);
                    tableBuilder.Property(person => person.Country);

                });
        });
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=Application10DB; User Id=sa; Password=Annem+.-1966; TrustServerCertificate=True");
    }
}