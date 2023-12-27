using System;
using System.Data.SqlClient;
using System.Data;
using KvalikTren.Model;

public static partial class DB_Interaction
{
    public static (bool success, string errorMessage) CheckLogin(string login, string password)
    {
        OpenConnection();

        using (SqlCommand cmd = new SqlCommand())
        {
            cmd.Connection = _connection;
            cmd.CommandType = CommandType.Text;

            cmd.CommandText = "SELECT Password, Role FROM User_table WHERE Login = @Login";
            cmd.Parameters.AddWithValue("@Login", login);

            try
            {
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string storedPassword = reader["Password"].ToString();

                        if (storedPassword == password)
                        {
                            Console.WriteLine("Вход выполнен успешно");

                            User.Login = login;
                            User.Password = password;
                            User.Role = reader["Role"].ToString();

                            return (true, null);
                        }

                        else
                        {
                            Console.WriteLine("Неверный пароль");
                            return (false, "Неверный пароль");
                        }
                    }

                    else
                    {
                        Console.WriteLine("Пользователь не найден");
                        return (false, "Пользователь не найден");
                    }
                }
            }

            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке входа: {ex.Message}");
                return (false, "Произошла ошибка");
            }
        }
    }
}