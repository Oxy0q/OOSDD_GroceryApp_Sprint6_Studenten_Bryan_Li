using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class GroceryListItemsRepository : DatabaseConnection, IGroceryListItemsRepository
    {
        private readonly List<GroceryListItem> groceryListItems = [];

        public GroceryListItemsRepository()
        {
            CreateTable(@"CREATE TABLE IF NOT EXISTS GroceryListItem (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [GroceryListId] INTEGER NOT NULL,
                            [ProductId] INTEGER NOT NULL,
                            [Amount] INTEGER NOT NULL
                        )");

            // Seed if empty
            OpenConnection();
            using (var countCmd = new SqliteCommand("SELECT COUNT(1) FROM GroceryListItem", Connection))
            {
                var count = Convert.ToInt32(countCmd.ExecuteScalar());
                if (count == 0)
                {
                    List<string> insertQueries = [
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 1, 3)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 2, 1)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(1, 3, 4)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, 1, 2)",
                        @"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(2, 2, 5)"
                    ];
                    InsertMultipleWithTransaction(insertQueries);
                }
            }
            CloseConnection();
            GetAll();
        }

        public List<GroceryListItem> GetAll()
        {
            groceryListItems.Clear();
            string selectQuery = "SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    groceryListItems.Add(new(id, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            return groceryListItems;
        }

        public List<GroceryListItem> GetAllOnGroceryListId(int id)
        {
            // Query directly for performance
            List<GroceryListItem> result = [];
            string selectQuery = $"SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE GroceryListId = {id}";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int itemId = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    result.Add(new(itemId, groceryListId, productId, amount));
                }
            }
            CloseConnection();
            return result;
        }

        public GroceryListItem Add(GroceryListItem item)
        {
            string insertQuery = $"INSERT INTO GroceryListItem(GroceryListId, ProductId, Amount) VALUES(@GroceryListId, @ProductId, @Amount) Returning RowId;";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();
            groceryListItems.Add(item);
            return item;
        }

        public GroceryListItem? Delete(GroceryListItem item)
        {
            string deleteQuery = $"DELETE FROM GroceryListItem WHERE Id = {item.Id};";
            OpenConnection();
            Connection.ExecuteNonQuery(deleteQuery);
            CloseConnection();
            var local = groceryListItems.FirstOrDefault(g => g.Id == item.Id);
            if (local != null) groceryListItems.Remove(local);
            return item;
        }

        public GroceryListItem? Get(int id)
        {
            string selectQuery = $"SELECT Id, GroceryListId, ProductId, Amount FROM GroceryListItem WHERE Id = {id}";
            GroceryListItem? item = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    int itemId = reader.GetInt32(0);
                    int groceryListId = reader.GetInt32(1);
                    int productId = reader.GetInt32(2);
                    int amount = reader.GetInt32(3);
                    item = new(itemId, groceryListId, productId, amount);
                }
            }
            CloseConnection();
            return item;
        }

        public GroceryListItem? Update(GroceryListItem item)
        {
            string updateQuery = $"UPDATE GroceryListItem SET GroceryListId = @GroceryListId, ProductId = @ProductId, Amount = @Amount WHERE Id = {item.Id};";
            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("GroceryListId", item.GroceryListId);
                command.Parameters.AddWithValue("ProductId", item.ProductId);
                command.Parameters.AddWithValue("Amount", item.Amount);
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
    }
}