
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

ApplicationDb9Context context = new();

#region EF Core Select Sorgularını Güçlendirme Teknikleri

#region IQueryable - IEnumerable Farkı

//IQueryable, bu arayüz üzerinde yapılan işlemler direkt generate edilecek olan sorguya yansıtılacaktır.
//IEnumerable, bu arayüz üzerinde yapılan işlemler temel sorgu neticesinde gelen ve in-memorye yüklenen instance'lar üzerinde gerçekleştirilir. Yani sorguya yansıtılmaz.

//IQueryable ile yapılan sorgulama çalışmalarında sql sorguyu hedef verileri elde edecek şekilde generate edilecekken, IEnumerable ile yapılan sorgulama çalışmalarında sql daha geniş verileri getirebilecek şekilde execute edilerek hedef veriler in-memory'de ayıklanır.

//IQueryable hedef verileri getirirken, hedef verilerden daha fazlasını getirip in-memory'de ayıklar.

// NOT: IQueryable ve IEnumerable davranışlar olarak aralarında farklar barındırlasalrda her ikiside Deterred Execution(GECİKMELİ ÇALIŞTIRMA) davranışı sergiler.Yani her iki arayüz üzerindende oluışturulan işlemi Execute edebilmek için .TolistAsync() gibi fonksiyonları veya ForEacah gibi tetikleyici işlemleri gerçekleştirmemiz gerekmektedir.


#region IQueryable

//var persons = await context.Persons.Where(p => p.Name.Contains("a"))
//               .Take(3)
//               .ToListAsync();

//var persons1 = await context.Persons.Where(p => p.Name.Contains("a"))
//               .Where(p => p.PersonId > 3)
//               .Take(3)
//               .Skip(3)
//               .ToListAsync();

//Console.WriteLine();

#endregion
// IEnumerable türde sorguyu IQueryable' a çevirir.
#region IEnumerable

//var persons =  context.Persons.Where(p => p.Name.Contains("a"))
//               .AsEnumerable()
//               .Take(3)
//               .ToList(); //Burda sorguya where i ekler ama take i eklemez .ünkü öncesinde sorgu  AsEnumerable() ile IEnumerable türüne çekildi.
#endregion

#region AsQueryable
// IQueryable türde sorguyu IEnumerable' e çevirir.

#endregion
#region AsEnumerable

#endregion
#endregion

#region Yalnızca İhtiyaç Olan Kolonları Listeleyin - Select

//var persons = await context.Persons.Select(p => new
//{
//    Name = p.Name
//}).ToListAsync();

//Console.WriteLine();

#endregion

#region Result'ı Limitleyin - Take

//var persons = await context.Persons.Take(100).ToListAsync(); //TOP 100 YANİ

#endregion

#region Join Sorgularında Eager Loading Sürecinde Verileri Filtreleyin

//var persons = await context.Persons.Include(p => p.Orders
//                                                .Where(O => O.OrderId % 2 == 0)
//                                                .OrderByDescending(o => o.OrderId)
//                                                .Take(2))
//                                                .ToListAsync();


#endregion

#region Şartlara Bağlı Join Yapılacaksa Eğer Explicit Loading Kullanın
//var person = await context.Persons.Include(p => p.Orders).FirstOrDefaultAsync(p => p.PersonId == 1); //Yanlış Niye yanlış gelen Personlardan Name i Ayşe olanların Order larını getirmek istiyoruz.Bunun için en baştan iki tabloyu bağlamak sonra Person.name i Ayşe olanların Orderlarını getirmek mantıksız ve maliyetli.O yüzden önce Personları getiririz.Sonra bu Personlardan Name i Ayşe olan varsa(İf ile kontrol edip) onun Orderlarını getiririz.
//var person = await context.Persons.FirstOrDefaultAsync(p => p.PersonId == 1);     // Doğru

//if (person.Name == "Ayşe")
//{
//    //Orderları getir.
//   var personOrders =  context.Persons.Entry(person).Collection(p => p.Orders).LoadAsync();

//}

//Console.WriteLine(person.Name);
#endregion

#region Lazy Loading Kullanırken Dikkatli Olun!
#region Riskli Durum
//Yapılmaması gereken bir durum
//var persons = await context.Persons.ToListAsync();

//foreach (var person in persons)
//{
//    foreach (var order in person.Orders)
//    {
//        Console.WriteLine($"{person.Name} - {order.OrderId}");
//    }
//    Console.WriteLine("***********");
//}
#endregion

 #region İdeal Durum

//var persons = await context.Persons.Select( p => new {p.Name, p.Orders}).ToListAsync();

//foreach (var person in persons)
//{
//    foreach (var order in person.Orders)
//    {
//        Console.WriteLine($"{ person.Name} - { order.OrderId}");
//    }
//    Console.WriteLine("+++++++++++++++++++++++++++");
//}
#endregion

#endregion

#region İhtiyaç Noktalarında Ham SQL Kullanın - FromSql

#endregion

#region Asenkron Fonksiyonları Tercih Edin

#endregion

#endregion

public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; }

    public virtual ICollection<Order> Orders { get; set; }
}
public class Order
{
    public int OrderId { get; set; }
    public int PersonId { get; set; }
    public string Description { get; set; }

    public virtual Person Person { get; set; }
}
class ApplicationDb9Context : DbContext
{
    public DbSet<Person> Persons { get; set; }
    public DbSet<Order> Orders { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());


    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=Application9DB; User Id=sa; Password=Annem+.-1966; TrustServerCertificate=True")
            .UseLazyLoadingProxies();
    }
}