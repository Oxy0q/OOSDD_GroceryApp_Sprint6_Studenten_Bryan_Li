using Grocery.Core.Data.Repositories;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = new List<GroceryListItem>();

        public GroceryListItemsRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL
                        )");

            EnsureSeedDataIfEmpty();
            // Populate the in-memory cache
            GetAll();
        }

        // Public CRUD surface 
        public List<GroceryListItem> GetAll()
        {
            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";
            var items = ReadItems(selectQuery);
            groceryListItems.Clear();
            groceryListItems.AddRange(items);
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE GroceryListId = @GroceryListId";
            return ReadItems(selectQuery, new SqliteParameter("@GroceryListId", id));
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            const string insertQuery = "INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(@GroceryListId, @ProductId, @Amount) RETURNING Id;";
            OpenConnection();
            using (var command = new SqliteCommand(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Amount", item.Amount);
                var scalar = command.ExecuteScalar();
                item.Id = Convert.ToInt32(scalar);
            }
            CloseConnection();

            groceryListItems.Add(item);
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            const string deleteQuery = "DELETE FROM GroceryListItem WHERE Id = @Id;";
            OpenConnection();
            using (var command = new SqliteCommand(deleteQuery, Connection))
            {
                command.Parameters.AddWithValue("@Id", item.Id);
                command.ExecuteNonQuery();
            }
            CloseConnection();

            var local = groceryListItems.FirstOrDefault(g => g.Id == item.Id);
            if (local != null) groceryListItems.Remove(local);
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            const string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = @Id";
            var items = ReadItems(selectQuery, new SqliteParameter("@Id", id));
            return items.FirstOrDefault();
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            const string updateQuery = "UPDATE GroceryListItem SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = @Id;";
            OpenConnection();
            using (var command = new SqliteCommand(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("@GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("@ProductId", item.ProductId);
                command.Parameters.AddWithValue("@Amount", item.Amount);
                command.Parameters.AddWithValue("@Id", item.Id);
                command.ExecuteNonQuery();
            }
            CloseConnection();

            var local = groceryListItems.FirstOrDefault(g => g.Id == item.Id);
            if (local != null)
            {
                local.GroceryListId = item.GroceryListId;
                local.ProductId = item.ProductId;
                local.Amount = item.Amount;
            }
            return item;
        }

        // ---------- Private helpers  ----------

        private List<GroceryListItem> ReadItems(string query, params SqliteParameter[] parameters)
        {
            var result = new List<GroceryListItem>();
            OpenConnection();
            using (var command = new SqliteCommand(query, Connection))
            {
                if (parameters != null && parameters.Length > 0)
                    command.Parameters.AddRange(parameters);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    result.Add(new GroceryListItem(id, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            return result;
        }

        private void EnsureSeedDataIfEmpty()
        {
            OpenConnection();
            using (var countCmd = new SqliteCommand("SELECT COUNT(1) FROM GroceryListItem", Connection))
            {
                var count = Convert.ToInt32(countCmd.ExecuteScalar());
                if (count == 0)
                {
                    var insertQueries = new List<string>
                    {
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 1, 3)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 2, 1)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 3, 4)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, 1, 2)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, 2, 5)"
                    };

                    InsertMultipleWithTransaction(insertQueries);
                }
            }
            CloseConnection();
        }
    }
}