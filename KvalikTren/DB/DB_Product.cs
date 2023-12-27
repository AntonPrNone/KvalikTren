using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using KvalikTren.Model;
using System.Linq;

public static partial class DB_Interaction
{
    public static List<Product> GetProduct()
    {
        OpenConnection();

        List<Product> productsList = new List<Product>();

        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = _connection;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = "SELECT * FROM Products_table";

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Product products = new Product
                        {
                            ID_Products = Convert.ToInt32(reader["ID_Products"]),
                            Name = reader["Name"].ToString(),
                            Count = Convert.ToInt32(reader["Count"]),
                            Type = reader["Type"].ToString()
                        };

                        productsList.Add(products);
                    }

                    return productsList;
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении данных продукции: {ex.Message}");
                return null;
            }
        }
    }

    public static (bool success, string errorMessage) AddProduct(string name, int count, string type)
    {
        OpenConnection();

        using (SqlTransaction transaction = _connection.BeginTransaction())
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _connection;
                cmd.Transaction = transaction;
                cmd.CommandType = CommandType.Text;

                try
                {
                    cmd.CommandText = "INSERT INTO Products_table (Name, Count, Type) VALUES (@Name, @Count, @Type)";
                    cmd.Parameters.AddWithValue("@Name", name);
                    cmd.Parameters.AddWithValue("@Count", count);
                    cmd.Parameters.AddWithValue("@Type", type);

                    cmd.ExecuteNonQuery();
                    transaction.Commit();
                    return (true, null);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Ошибка при добавлении продукции: {ex.Message}");
                    return (false, "Произошла ошибка при добавлении продукции");
                }
            }
        }
    }

    public static (bool success, string errorMessage) UpdateProducts(DataTable dataTable)
    {
        OpenConnection();

        try
        {
            // Удаляем продукцию, которых нет в переданном DataTable
            DeleteNonExistingProducts(dataTable);

            // Обновляем существующую продукцию
            foreach (DataRow row in dataTable.Rows)
            {
                int id = (int)row["ID"];
                string name = (string)row["Наименование"];
                int count = (int)row["Кол-во"];
                string type = (string)row["Тип"];

                if (ProductsExists(id))
                {
                    UpdateProducts(id, name, count, type);
                }
            }

            // Добавляем новую продукцию
            foreach (DataRow row in dataTable.Rows)
            {
                int id = (int)row["ID"];
                string name = (string)row["Наименование"];
                int count = (int)row["Кол-во"];
                string type = (string)row["Тип"];

                if (!ProductsExists(id))
                {
                    AddProduct(name, count, type);
                }
            }

            return (true, null);
        }

        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при обновлении данных продукции: {ex.Message}");
            return (false, "Произошла ошибка при обновлении данных продукции");
        }
    }

    private static void UpdateProducts(int id, string name, int count, string type)
    {
        OpenConnection();

        using (SqlTransaction transaction = _connection.BeginTransaction())
        {
            using (SqlCommand cmd = new SqlCommand())
            {
                cmd.Connection = _connection;
                cmd.Transaction = transaction;
                cmd.CommandType = CommandType.Text;

                try
                {
                    // Проверяем, существует ли запись с указанным ID в базе данных
                    cmd.CommandText = "SELECT COUNT(*) FROM Products_table WHERE ID_Products = @ID";
                    cmd.Parameters.AddWithValue("@ID", id);

                    int countReturn = (int)cmd.ExecuteScalar();

                    if (countReturn > 0)
                    {
                        // Если запись существует, обновляем ее данные
                        cmd.Parameters.Clear();
                        cmd.CommandText = "UPDATE Products_table SET Name = @Name, Count = @Count, Type = @Type WHERE ID_Products = @ID";
                        cmd.Parameters.AddWithValue("@ID", id);
                        cmd.Parameters.AddWithValue("@Name", name);
                        cmd.Parameters.AddWithValue("@Count", count);
                        cmd.Parameters.AddWithValue("@Type", type);

                        cmd.ExecuteNonQuery();
                        transaction.Commit();
                    }

                    else
                    {
                        transaction.Rollback();
                        Console.WriteLine($"Запись с ID {id} не найдена при обновлении продукции.");
                    }
                }

                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine($"Ошибка при обновлении данных продукции: {ex.Message}");
                }
            }
        }
    }

    private static bool ProductsExists(int id)
    {
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = _connection;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = "IF EXISTS (SELECT 1 FROM Products_table WHERE ID_Products = @ID) SELECT 1 ELSE SELECT 0";
            cmd.Parameters.AddWithValue("@ID", id);

            return (int)cmd.ExecuteScalar() > 0;
        }
    }

    private static void DeleteNonExistingProducts(DataTable dataTable)
    {
        List<int> existingStudentIds = GetProductIds();

        // Определяем продукцию, которых нет в переданном DataTable
        List<int> nonExistingStudentIds = existingStudentIds.Except(dataTable.AsEnumerable().Select(row => row.Field<int>("ID"))).ToList();

        foreach (int id in nonExistingStudentIds)
        {
            // Удаляем продукцию из базы данных
            DeleteProduct(id);
        }
    }

    private static List<int> GetProductIds()
    {
        List<int> productIds = new List<int>();

        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = _connection;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = "SELECT ID_Products FROM Products_table";

            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    productIds.Add(Convert.ToInt32(reader["ID_Products"]));
                }
            }
        }

        return productIds;
    }

    private static void DeleteProduct(int id)
    {
        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = _connection;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = "DELETE FROM Products_table WHERE ID_Products = @ID";
            cmd.Parameters.AddWithValue("@ID", id);

            cmd.ExecuteNonQuery();
        }
    }
}