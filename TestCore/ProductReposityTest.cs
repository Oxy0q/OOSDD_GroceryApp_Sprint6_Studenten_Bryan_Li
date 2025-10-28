using Grocery.Core.Data.Repositories;
using Grocery.Core.Models;
using NUnit.Framework;

namespace TestCore
{
    public class ProductRepositoryTests
    {
        [Test]
        public void AddProduct_PersistsInDatabase()
        {
            var repo = new ProductRepository();
            var product = new Product(0, "Test Product UC19", 5, DateOnly.FromDateTime(DateTime.Today.AddDays(10)), 1.23m);

            var added = repo.Add(product);

            Assert.That(added.Id, Is.GreaterThan(0));

            var fetched = repo.Get(added.Id);
            Assert.That(fetched, Is.Not.Null);
            Assert.That(fetched!.Name, Is.EqualTo("Test Product UC19"));
            Assert.That(fetched.Stock, Is.EqualTo(5));
            Assert.That(fetched.Price, Is.EqualTo(1.23m));
        }
    }
}