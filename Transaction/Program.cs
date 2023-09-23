
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;
using System.Reflection.Metadata;
using System.Transactions;

ApplicationDb8Context context = new();

#region Transaction Nedir?
//Transaction, veritabanındaki kümülatif işlemleri atomik bir şekilde gerçekleştirmemizi sağlayan bir özelliktir.
//Bir transaction içerisindkei tüm işlemler commit edildiği taktirde veritabanına fiziksel olarak yansıtılacaktır. Ya da ROLLBACK edilirse tüm işlemler geri alınacak ve fiziksel olarak veritabanında herhangi bir verisel değişiklik durumu söz konusu olmayacaktır.
//Transaction'ın genel amacı veritabanındaki tutarlılık durumunu korumaktadır. Ya da bir başka deyişle verityabanındaki tutarsızlık durumlarına karşı önlem almaktır.
#endregion

#region Default Transaction Davranışı
//EF Core'da varsayılan olarak, yapılan tüm işlemler SaveChanges() fonksiyuyla veritabanına fiziksel olarak uygulanır.Hata alırsa arka planda kendi kendine o transaction ROLLBACK edilir. 
//Çünkü SaveChanges default olarak bir trasncationa sahiptir.
//Eğer ki bu süreçte bir problem/hata/başarısızlık durumu söz konusu olursa tüm işlemler geri alınır(rollback) ve işlemlerin hiçbiri veritabanına uygulanmaz.
//Böylece SaveChanges tüm işlemlerin ya tamamen başarılı olacağını ya da bir hata oluşursa veritabanını değiştirmeden işlemleri sonlandıracağını ifade etmektedir.
#endregion

#region Transaction Kontrolünü Manuel Sağlama

// IDbContextTransaction transaction= await context.Database.BeginTransactionAsync(); //BeginTransactionAsync() ile bir transaction başlattık onuda transactiona atadık sonrasında gerçekleşecek işlemler artık transactionda tutuluyor.Biz bu transactionu Commit() etmediğimiz sürece VERİTABANINA YANSIMAZ.Bunu da 31.satırda yaptık.


//Person p = new() { Name = "Musa" };
//await context.Persons.AddAsync(p);
//await context.SaveChangesAsync();
//await transaction.CommitAsync(); // transactionu burda Veritabanına Commit ettik.transaction Commit edilmediği sürece Veritabanında bir değişiklik yapmaz.
//await transaction.RollbackAsync(); // transactionu geri almak istiyorsak bu şekilde ROLLBACK yapabiliriz.

//Console.WriteLine();

//EF Core'da transaction kontrolü iradeli bir şekilde manuel sağlamak yani elde etmek istiyorsak eğer BeginTransactionAsync fonksiyonu çağrılmalıdır.

//Person p = new() { Name = "Abuzer" };
//await context.Persons.AddAsync(p);
//await context.SaveChangesAsync();

//await transaction.CommitAsync();
#endregion

#region **SAVEPOINTS**

//EF Core 5.0 sürümüyle gelmiştir.
//Veritabanı işlemleri sırasında bir hata meydana geliyorsa tüm transactionu değilde  şu noktaya kadar işlemleri geri al demek için Savepoints kullanılır.
//Savepoints, veritabanıu işlemleri sürecinde bir hata oluşursa veya başka bir nedenle yapılan işlemlerin geri alınması gerekiyorsa transaciton içerisinde dönüş yapılabilecek noktaları ifade eden bir özelliktir.
#region CreateSavepoint
//Transaction içerisinde geri dönüş noktası oluşturmamızı sağlayan bir fonksiyondur.
#endregion
#region RollbackToSavepoint
//Transacction içerisinde herhangi bir geri dönüş noktasına(Savepoint'e) rollback yapmamızı sağlayan fonksiyondur.
#endregion

//Savepoints özelliği bir transaction içerisinde istenildiği kadar kullanılabilir.

//IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();

//try
//{
//    Person? p11 = await context.Persons.FindAsync(11);
//    Person? p12 = await context.Persons.FindAsync(12);
//    context.Persons.RemoveRange(p11, p12);
//    await context.SaveChangesAsync(); // BAK KAYDETTİK AMA HALA VERİTABANINA COMMİT ETMEDİK

//    await transaction.CreateSavepointAsync("t1"); // Burda t1 anına geri dön diyoruz.

//    Person? p10 = await context.Persons.FindAsync(10);
//    context.Persons.Remove(p10);
//    await context.SaveChangesAsync();

//    await transaction.RollbackToSavepointAsync("t1"); //Diyoruz ki t1 anına geri dön t1 anında nasılsa(sadece p11 ve p13 silinmişti daha p10 silinmemişti) oraya dön diyoruz ve dönüyor.

//    await transaction.CommitAsync();
//}
//catch (Exception ex)
//{
//    Console.WriteLine(ex.Message);

//    throw;
//}
//finally
//{
//    Console.WriteLine("Commit edildi.");
//}

#endregion

#region TransactionScope  
//veritabanı işlemlerini bir grup olarak yapmamızı sağlayan bir sınıfıtr.
//ADO.NET ile de kullanılabilir.

//using TransactionScope transactionScope = new();
//Veritabanı işlemleri...
//..
//..
//transactionScope.Complete(); //Compote fonksiyonu yapılan veritabanı işlemlerinin commit edilmesini sağlar.
//Eğer ki rollback yapacaksanız complete fonksiyonunun tetiklenmemesi yeterlidir!

#region Complete

#endregion
#endregion

public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; }

    public ICollection<Order> Orders { get; set; }
}
public class Order
{
    public int OrderId { get; set; }
    public int PersonId { get; set; }
    public string Description { get; set; }
    public Person Person { get; set; }
}
class ApplicationDb8Context : DbContext
{
    public DbSet<Person> Persons { get; set; }
    public DbSet<Order> Orders { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=Application8DB; User Id=sa; Password=Annem+.-1966; TrustServerCertificate=True");
    }
}