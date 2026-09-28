using System;
using System.Collections.Generic;
using System.IO;
using MySql.Data.MySqlClient;
using luxury_cars.Models;

namespace luxury_cars
{
    public class DatabaseHelper
    {
        private const string ConnectionString =
            "Server=localhost;" +
            "Database=quizapp;" +
            "User ID=root;" +
            "Password=1662;" +
            "Port=3306;";

        // ── Test connection ──────────────────────────────
        public static bool TestConnection()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                return true;
            }
            catch { return false; }
        }

        // ── Validate login (old) ─────────────────────────
        public static string? ValidateLogin(string username, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT username FROM users WHERE username=@u AND password=@p", conn);
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@p", password);
                return cmd.ExecuteScalar()?.ToString();
            }
            catch { return null; }
        }

        // ── Map reader row to Car object ─────────────────
        private static Car MapCar(MySqlDataReader reader) => new Car
        {
            Id = reader.GetInt32("id"),
            ModelNumber = reader.GetString("model_number"),
            Brand = reader.GetString("brand"),
            Name = reader.GetString("name"),
            Year = reader.GetInt32("year"),
            Color = reader.GetString("color"),
            Price = reader.GetDecimal("price"),
            Stock = reader.GetInt32("stock"),
            Status = reader.GetString("status"),
            ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path"))
                                ? null
                                : reader.GetString("image_path"),
            CreatedAt = reader.GetDateTime("created_at").ToString("dd MMM yyyy")
        };

        // ── Get all cars ─────────────────────────────────
        public static List<Car> GetAllCars()
        {
            var list = new List<Car>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, model_number, brand, name, year, color, " +
                    "price, stock, status, image_path, created_at " +
                    "FROM cars ORDER BY created_at DESC", conn);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapCar(reader));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return list;
        }

        // ── Search cars ──────────────────────────────────
        public static List<Car> SearchCars(string keyword)
        {
            var list = new List<Car>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT id, model_number, brand, name, year, color, " +
                    "price, stock, status, image_path, created_at " +
                    "FROM cars WHERE " +
                    "brand LIKE @k OR name LIKE @k OR " +
                    "model_number LIKE @k OR color LIKE @k " +
                    "ORDER BY created_at DESC", conn);
                cmd.Parameters.AddWithValue("@k", $"%{keyword}%");
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(MapCar(reader));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return list;
        }

        // ── Add new car ──────────────────────────────────
        public static bool AddCar(Car car)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "INSERT INTO cars " +
                    "(model_number, brand, name, year, color, price, stock, status, image_path) " +
                    "VALUES (@m, @b, @n, @y, @c, @p, @s, @st, @img)", conn);
                cmd.Parameters.AddWithValue("@m", car.ModelNumber);
                cmd.Parameters.AddWithValue("@b", car.Brand);
                cmd.Parameters.AddWithValue("@n", car.Name);
                cmd.Parameters.AddWithValue("@y", car.Year);
                cmd.Parameters.AddWithValue("@c", car.Color);
                cmd.Parameters.AddWithValue("@p", car.Price);
                cmd.Parameters.AddWithValue("@s", car.Stock);
                cmd.Parameters.AddWithValue("@st", car.Stock > 0 ? "Available" : "Out of Stock");
                cmd.Parameters.AddWithValue("@img",
                    string.IsNullOrWhiteSpace(car.ImagePath)
                        ? (object)DBNull.Value
                        : car.ImagePath);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1062)
                    throw new Exception("Model number already exists!");
                throw;
            }
        }

        // ── Update image path ────────────────────────────
        public static bool UpdateCarImage(int carId, string? imagePath)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "UPDATE cars SET image_path = @img WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@img",
                    string.IsNullOrWhiteSpace(imagePath)
                        ? (object)DBNull.Value
                        : imagePath);
                cmd.Parameters.AddWithValue("@id", carId);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); return false; }
        }

        // ── Delete car ───────────────────────────────────
        public static bool DeleteCar(int id)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand("DELETE FROM cars WHERE id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                return cmd.ExecuteNonQuery() > 0;
            }
            catch (MySqlException ex)
            {
                if (ex.Number == 1451)
                    throw new Exception("Cannot delete this car — it has sales records.");
                throw new Exception($"Delete failed: {ex.Message}");
            }
        }

        // ── Get stats ────────────────────────────────────
        public static (int total, int available, int outOfStock) GetStats()
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT SUM(stock) as total, " +
                    "SUM(CASE WHEN status='Available' THEN stock ELSE 0 END) as avail, " +
                    "SUM(CASE WHEN status='Out of Stock' THEN 1 ELSE 0 END) as oos " +
                    "FROM cars", conn);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return (
                        reader.IsDBNull(reader.GetOrdinal("total")) ? 0 : Convert.ToInt32(reader["total"]),
                        reader.IsDBNull(reader.GetOrdinal("avail")) ? 0 : Convert.ToInt32(reader["avail"]),
                        reader.IsDBNull(reader.GetOrdinal("oos")) ? 0 : Convert.ToInt32(reader["oos"])
                    );
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return (0, 0, 0);
        }

        // ── Process full sale via stored procedure ────────
        public static (bool success, int saleId, string message)
            ProcessSale(SaleRecord sale)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("ProcessCarSale", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("p_customer_name", sale.CustomerName);
                cmd.Parameters.AddWithValue("p_customer_phone", sale.CustomerPhone);
                cmd.Parameters.AddWithValue("p_customer_email", sale.CustomerEmail);
                cmd.Parameters.AddWithValue("p_customer_address", sale.CustomerAddress);
                cmd.Parameters.AddWithValue("p_customer_idproof", sale.IdProof);
                cmd.Parameters.AddWithValue("p_car_id", sale.CarId);
                cmd.Parameters.AddWithValue("p_sale_price", sale.SalePrice);
                cmd.Parameters.AddWithValue("p_amount_paid", sale.AmountPaid);
                cmd.Parameters.AddWithValue("p_payment_type", sale.PaymentType);
                cmd.Parameters.AddWithValue("p_emi_months", sale.EmiMonths);
                cmd.Parameters.AddWithValue("p_delivery_date", sale.DeliveryDate.ToString("yyyy-MM-dd"));
                cmd.Parameters.AddWithValue("p_notes", sale.Notes);

                var saleIdParam = new MySqlParameter("p_sale_id", MySqlDbType.Int32)
                { Direction = System.Data.ParameterDirection.Output };
                var custIdParam = new MySqlParameter("p_customer_id", MySqlDbType.Int32)
                { Direction = System.Data.ParameterDirection.Output };
                var msgParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                { Direction = System.Data.ParameterDirection.Output };

                cmd.Parameters.Add(saleIdParam);
                cmd.Parameters.Add(custIdParam);
                cmd.Parameters.Add(msgParam);
                cmd.ExecuteNonQuery();

                int saleId = Convert.ToInt32(saleIdParam.Value);
                string message = msgParam.Value?.ToString() ?? "";
                return (saleId > 0, saleId, message);
            }
            catch (Exception ex) { return (false, 0, $"Error: {ex.Message}"); }
        }

        // ── Sell one car ─────────────────────────────────
        public static (bool success, string message) SellCar(
            int id, string brand, string name)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var checkCmd = new MySqlCommand(
                    "SELECT stock FROM cars WHERE id = @id", conn);
                checkCmd.Parameters.AddWithValue("@id", id);
                int currentStock = Convert.ToInt32(checkCmd.ExecuteScalar());
                if (currentStock <= 0)
                    return (false, $"{brand} {name} is already out of stock!");

                var updateCmd = new MySqlCommand(
                    "UPDATE cars SET stock = stock - 1, " +
                    "status = CASE WHEN stock - 1 <= 0 THEN 'Out of Stock' ELSE 'Available' END " +
                    "WHERE id = @id", conn);
                updateCmd.Parameters.AddWithValue("@id", id);
                int rows = updateCmd.ExecuteNonQuery();

                if (rows > 0)
                {
                    int newStock = currentStock - 1;
                    return (true, newStock == 0
                        ? $"{brand} {name} sold! Now OUT OF STOCK."
                        : $"{brand} {name} sold! Stock remaining: {newStock}");
                }
                return (false, "Update failed.");
            }
            catch (Exception ex) { return (false, $"Error: {ex.Message}"); }
        }

        // ── Helper: ensure CarImages folder exists ────────
        public static string EnsureCarImagesFolder()
        {
            string folder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "CarImages");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            return folder;
        }

        // ── RBAC: Validate login via stored procedure ─────
        public static (int userId, int roleId, string roleName)?
            ValidateUserLogin(string username, string password)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("ValidateUserLogin", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_username", username);
                cmd.Parameters.AddWithValue("p_password", password);

                var userIdParam = new MySqlParameter("p_user_id", MySqlDbType.Int32)
                { Direction = System.Data.ParameterDirection.Output };
                var roleIdParam = new MySqlParameter("p_role_id", MySqlDbType.Int32)
                { Direction = System.Data.ParameterDirection.Output };
                var roleNameParam = new MySqlParameter("p_role_name", MySqlDbType.VarChar, 50)
                { Direction = System.Data.ParameterDirection.Output };

                cmd.Parameters.Add(userIdParam);
                cmd.Parameters.Add(roleIdParam);
                cmd.Parameters.Add(roleNameParam);
                cmd.ExecuteNonQuery();

                if (userIdParam.Value == null || userIdParam.Value == DBNull.Value)
                    return null;

                int userId = Convert.ToInt32(userIdParam.Value);
                int roleId = Convert.ToInt32(roleIdParam.Value);
                string roleName = roleNameParam.Value?.ToString() ?? "";

                if (userId == 0) return null;
                return (userId, roleId, roleName);
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); return null; }
        }

        // ── RBAC: Get permissions for a role ─────────────
        public static List<string> GetRolePermissions(int roleId)
        {
            var list = new List<string>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("GetRolePermissions", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_role_id", roleId);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add(reader.GetString("permission_name"));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return list;
        }

        // ── RBAC: Get all users ───────────────────────────
        public static List<(int id, string username, string roleName, int roleId)>
            GetAllUsers()
        {
            var list = new List<(int, string, string, int)>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("GetAllUsers", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                    list.Add((
                        reader.GetInt32("id"),
                        reader.GetString("username"),
                        reader.GetString("role_name"),
                        reader.GetInt32("role_id")
                    ));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return list;
        }

        // ── RBAC: Add new user ────────────────────────────
        public static (bool success, string message, int userId) AddUser(
            string username, string password, int roleId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("AddUser", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_username", username);
                cmd.Parameters.AddWithValue("p_password", password);
                cmd.Parameters.AddWithValue("p_role_id", roleId);

                var userIdParam = new MySqlParameter("p_user_id", MySqlDbType.Int32)
                { Direction = System.Data.ParameterDirection.Output };
                var msgParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                { Direction = System.Data.ParameterDirection.Output };
                cmd.Parameters.Add(userIdParam);
                cmd.Parameters.Add(msgParam);
                cmd.ExecuteNonQuery();

                int userId = Convert.ToInt32(userIdParam.Value);
                string message = msgParam.Value?.ToString() ?? "";
                return (userId > 0, message, userId);
            }
            catch (Exception ex) { return (false, ex.Message, 0); }
        }

        // ── RBAC: Update user role ────────────────────────
        public static bool UpdateUserRole(int userId, int roleId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("UpdateUserRole", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_user_id", userId);
                cmd.Parameters.AddWithValue("p_role_id", roleId);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); return false; }
        }

        // ── RBAC: Delete user ─────────────────────────────
        public static (bool success, string message) DeleteUser(int userId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("DeleteUser", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_user_id", userId);
                var msgParam = new MySqlParameter("p_message", MySqlDbType.VarChar, 255)
                { Direction = System.Data.ParameterDirection.Output };
                cmd.Parameters.Add(msgParam);
                cmd.ExecuteNonQuery();
                string message = msgParam.Value?.ToString() ?? "";
                return (true, message);
            }
            catch (Exception ex) { return (false, ex.Message); }
        }

        // ── Report: Get sales (daily/weekly/yearly) ───────
        public static List<SaleReportRow> GetSalesReport(string filter)
        {
            var list = new List<SaleReportRow>();
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("GetSalesReport", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_filter", filter);
                var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new SaleReportRow
                    {
                        SaleId = reader.GetInt32("sale_id"),
                        SaleDate = reader.GetDateTime("sale_date")
                                              .ToString("dd MMM yyyy HH:mm"),
                        CustomerName = reader.GetString("customer_name"),
                        CustomerPhone = reader.GetString("customer_phone"),
                        ModelNumber = reader.GetString("model_number"),
                        Brand = reader.GetString("brand"),
                        CarName = reader.GetString("car_name"),
                        Year = reader.GetInt32("year"),
                        Color = reader.GetString("color"),
                        PaymentType = reader.GetString("payment_type"),
                        EmiMonths = reader.IsDBNull(reader.GetOrdinal("emi_months"))
                                              ? 0 : reader.GetInt32("emi_months"),
                        SalePrice = reader.GetDecimal("sale_price"),
                        AmountPaid = reader.GetDecimal("amount_paid"),
                        DueAmount = reader.GetDecimal("due_amount"),
                        DeliveryDate = reader.GetDateTime("delivery_date")
                                              .ToString("dd MMM yyyy"),
                        Notes = reader.IsDBNull(reader.GetOrdinal("notes"))
                                              ? "" : reader.GetString("notes")
                    });
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return list;
        }

        // ── Report: Get summary totals ────────────────────
        public static (int totalSales, decimal totalRevenue, decimal totalDue)
            GetReportSummary(string filter)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                using var cmd = new MySqlCommand("GetReportSummary", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("p_filter", filter);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return (
                        reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                        reader.IsDBNull(1) ? 0 : reader.GetDecimal(1),
                        reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                    );
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return (0, 0, 0);
        }

        // ── Check trigger log after user insert ──────────
        public static (bool triggered, string username, DateTime triggeredAt)
            CheckUserTrigger(int userId)
        {
            try
            {
                using var conn = new MySqlConnection(ConnectionString);
                conn.Open();
                var cmd = new MySqlCommand(
                    "SELECT username, triggered_at FROM user_audit_log " +
                    "WHERE user_id = @id ORDER BY triggered_at DESC LIMIT 1", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                var reader = cmd.ExecuteReader();
                if (reader.Read())
                    return (true,
                            reader.GetString("username"),
                            reader.GetDateTime("triggered_at"));
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }
            return (false, "", DateTime.MinValue);
        }
    }
}