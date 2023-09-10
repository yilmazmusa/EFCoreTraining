using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using System.Reflection;

Test3DbContext context = new();

#region Connection Resiliency Nedir?
//EF Core üzerinde yapılan veritabanı çalışmaları sürecinde ister istemez veritabanı bağlantısında kopuşlar/kesintiler vs. meydana gelebilmektedir. 

//Connection Resiliency ile kopan bağlantıyı tekrar kurmak için gerekli tekrar bağlantı taleplerinde bulunabilir ve biryandan da execution strategy dediğimiz davranış modellerini belirleyerek bağlantıların kopması durumunda tekrar edecek olan sorguları baştan sona yeniden tetikleyebiliriz.
#endregion
#region EnableRetryOnFailure
//Uygulama sürecinde veritabanı bağlantısı koptuğu taktirde bu yapılandırma sayesinde bağlantıyı tekrardan kurmaya çalışabiliyirouz.

while (true)
{
    await Task.Delay(2000);
    var persons = await context.Persons.ToListAsync();
    persons.ForEach(p => Console.WriteLine(p.Name));
    Console.WriteLine("*******************");
}

#region MaxRetryCount
//Yeniden bağlantı sağlanması durumunun kaç kere gerçekleştirlecğeini bildirmektedir.
//Defualt değeri 6'dır.
#endregion
#region MaxRetryDelay
//Yeniden bağlantı sağlanması periyodunu bildirmektedir.
//Default değeri 30'dur.
#endregion
#endregion

#region Execution Strategies 
//Bağlantı koparsa sende yeniden bağlanmayı denersen bunun adına EF CORE da Execution Strategies denir.
//EF Core ile yapılan bir işlem sürecinde veritabanı bağlatısı koptuğu taktirde yeniden bağlantı denenirken yapılan davranışa/alınan aksiyona Execution Strategy denmektedir.

//Bu stratejiyi default dğerlerde kullanabieceğimiz gibi custom olarak da kendimize göre özelleştireibilir ve bağlantı koptuğu durumlarda istediğimiz aksiyonları alabiliriz.

#region Default Execution Strategy
//Eğer ki Connection Resiliency için EnableRetryOnFailure metodunu kullanıyorsak bu default execution stratgy karşılık gelecektir.
//MaxRetryCount : 6
//MaxRetryDelay : 30
//Default değerlerin kullanılabilmesi için EnableRetryOnFailure metodunun parametresis overload'ının kullanılması gerekmektedir.
#endregion
#region Custom Execution Strategy

#region Oluşturma

#endregion
#region Kullanma - ExecutionStrategy

while (true)
{
    await Task.Delay(2000);
    var persons = await context.Persons.ToListAsync();
    persons.ForEach(p => Console.WriteLine(p.Name));
    Console.WriteLine("*******************");
}
#endregion

#endregion
#region Bağlantı Koptuğu Anda Execute Edilmesi Gereken Tüm Çalışmaları Tekrar İşlemek
//EF Core ile yapılan çalışma sürecinde veritabanı bağlantısının kesildiği durumlarda, bazen bağlantının tekrardan kurulması tek başına yetmemekte, keszintinin olduğu çalışmanın da baştan tekrardan işlenmesi gerekebilmetkedir. İşte bu tarz durumlara karşılık EF Core Execute - ExecuteAsync fonksiyonunu bizlere sunmaktadır.

//Execute fonksiyonu, içerisine vermiş olduğumuz kodları commit edilene kadar işleyecektir. Eğer ki bağlantı kesilmesi meydana gelirse, bağlantının tekrardan kurulması durumunda Execute içerisindeki çalışmalar tekrar baştan işlenecek ve böylece yapılan işlemin tutarlılığı için gerekli çalışma sağlanmış olacaktır.

//var strategy = context.Database.CreateExecutionStrategy();
//await strategy.ExecuteAsync(async () =>
//{
//    using var transcation = await context.Database.BeginTransactionAsync();
//    await context.Persons.AddAsync(new() { Name = "Hilmi" });
//    await context.SaveChangesAsync();

//    await context.Persons.AddAsync(new Person() { Name = "Şuayip" });
//    await context.SaveChangesAsync();

//    await transcation.CommitAsync();
//});

#endregion
#region Execution Strategy Hangi Durumlarda Kullanılır?
//Veritabanının şifresi belirli periyotlarda otomatik olarak değişen uygulamalarda güncel şifreyle connection string'i sağlayacak bir operasyonu custom execution strategy belirleyerek gerçekleştitrebilirsiniz.
#endregion
#endregion

public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; }
}
class Test3DbContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        #region DEFAULT Execution Strategy

        optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=Test3DB; User Id=sa; Password=Annem+.-1966; TrustServerCertificate=True", builder => builder.EnableRetryOnFailure(
          maxRetryCount: 5, // 5 kere bağlanmayı dene
          maxRetryDelay: TimeSpan.FromSeconds(15), //15 saniyede bir bağlanmayı dene
          errorNumbersToAdd: new[] { 4060 }))
          .LogTo(
          filter: (eventId, level) => eventId == CoreEventId.ExecutionStrategyRetrying,
          logger: (evetData =>
          {
              Console.WriteLine($"Bağlantı tekrar kurulmaktadır.");
          })); // Bağlantı kesilirse direkt hata verme default ayarlarla DB ye tekrardan EnableRetryOnFailure ile  bağlanmaaya çalış diyoruz.Sonrasında da bu bağlantı kopukluğunun logunu tutuyoruz.Diyoruz ki önce dilter ile her şeyi loglama sadece yeniden bağlantı durumlarını logla diye filtreliyoruz sonra da logger ile logluyoruz.

        #endregion


        #region CUSTOM Execution Strategy

        optionsBuilder.UseSqlServer("Server=localhost\\sqlexpress;Database=Test3DB; User Id=sa; Password=Annem+.-1966; TrustServerCertificate=True", builder => builder.ExecutionStrategy(dependencies => new CustomExecutionStrategy(dependencies, maxRetryCount : 3, maxRetryDelay : TimeSpan.FromSeconds(15)))); // Bağlantı kesilirse direkt hata verme default ayarlarla DB ye tekrardan EnableRetryOnFailure ile  bağlanmaaya çalış diyoruz.Burda loglama yapmıyoruz çünkü burda CustomExecutionStrategy fonk ile ExecutionStrategy yapıyoruz bu fonk içinde log var zaten aşağıda 137. satır ve öncesi.

        #endregion

    }
}

class CustomExecutionStrategy : ExecutionStrategy
{
    public CustomExecutionStrategy(ExecutionStrategyDependencies dependencies, int maxRetryCount, TimeSpan maxRetryDelay) : base(dependencies, maxRetryCount, maxRetryDelay)
    {
    }

    public CustomExecutionStrategy(DbContext context, int maxRetryCount, TimeSpan maxRetryDelay) : base(context, maxRetryCount, maxRetryDelay)
    {
    }

    int retryCount = 0;
    protected override bool ShouldRetryOn(Exception exception)
    {
        //Yeniden bağlantı durumunun söz konusu olduğu anlarda yapılacak işlemler...
        Console.WriteLine($"#{++retryCount}. Bağlantı tekrar kuruluyor...");
        return true;
    }
}