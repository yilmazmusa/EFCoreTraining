
using Microsoft.EntityFrameworkCore;
using System.Reflection;

ApplicationDb9Context context = new();

#region EF Core 7 Öncesi Toplu Güncelleme

//var persons = await context.Persons.Where(p => p.PersonId > 5).ToListAsync();
//foreach (var person in persons)
//{
//    person.Name = $"{person.Name}...";
//}
//await context.SaveChangesAsync();
#endregion

#region EF Core 7 Öncesi Toplu Silme
//var persons = await context.Persons.Where(p => p.PersonId > 5).ToListAsync();
//context.RemoveRange(persons);
//await context.SaveChangesAsync();
#endregion


#region ExecuteUpdate -EF7 SONRASI

//await context.Persons.Where(p => p.PersonId > 5).ExecuteUpdateAsync(p => p.SetProperty(p => p.Name, v => v.Name + "YENİ")); //PersonId si 5 ten büyük olan Persons ların adının yanına yeni yazdık.
#endregion
#region ExecuteDelete - EF7 SONRASI

//await context.Persons.Where(p => p.Name.Contains("a")).ExecuteDeleteAsync(); // Person.Name(isminde) içerisinde a geçenleri sildik.

#endregion 

//ExecuteUpdate ve ExecuteDelete fonksiyonları ile bulk(toplu) veri güncelleme ve silme işlemleri gerçekleştirirken SaveChanges fonksiyonunu çağırmanız gerekmemektedir. Çünkü b fonksiyonlar adları üzerinde Execute... fonksiyonlarıdır. Yani direkt verittaanına fiziksel etkide bulunurlar.

//Eğer ki istyorsanız transaction kontrolünü ele alarak bu fonksiyonların işlevlerini de süreçte kontrol edebilirsiniz.

public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; }
}
class ApplicationDb9Context : DbContext
{
    public DbSet<Person> Persons { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=Application9DB; User Id=sa; Password=Annem+.-1966; TrustServerCertificate=True");
    }
}