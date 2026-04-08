using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Common;
using SV22T1020674.Models.Sales;

namespace SV22T1020674.DataLayers.SQLServer
{
    public class OrderRepository : IOrderRepository
    {
        private readonly string _connectionString;

        public OrderRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        #region ORDER

        public async Task<int> AddAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Orders(CustomerID, OrderTime, DeliveryProvince, DeliveryAddress, EmployeeID, Status)
                VALUES(@CustomerID, GETDATE(), @DeliveryProvince, @DeliveryAddress, @EmployeeID, @Status);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Order data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Orders
                SET CustomerID = @CustomerID,
                    EmployeeID = @EmployeeID,
                    OrderTime = @OrderTime,
                    Status = @Status,
                    DeliveryProvince = @DeliveryProvince,
                    DeliveryAddress = @DeliveryAddress,
                    ShipperID = @ShipperID,
                    AcceptTime = @AcceptTime,
                    ShippedTime = @ShippedTime,
                    FinishedTime = @FinishedTime
                WHERE OrderID = @OrderID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                DELETE FROM OrderDetails WHERE OrderID = @OrderID;
                DELETE FROM Orders WHERE OrderID = @OrderID;";
            return await connection.ExecuteAsync(sql, new { OrderID = orderID }) > 0;
        }

        public async Task<OrderViewInfo?> GetAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT o.OrderID, o.CustomerID, o.EmployeeID, o.ShipperID,
                       o.OrderTime, o.Status, o.DeliveryProvince, o.DeliveryAddress,
                       c.CustomerName,
                       e.FullName AS EmployeeName,
                       s.ShipperName,
                       ISNULL(odTotals.TotalAmount,0) AS TotalAmount
                FROM Orders o
                LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
                LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
                LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
                LEFT JOIN (
                    SELECT OrderID, SUM(Quantity * SalePrice) AS TotalAmount
                    FROM OrderDetails
                    GROUP BY OrderID
                ) odTotals ON o.OrderID = odTotals.OrderID
                WHERE o.OrderID = @OrderID";
            return await connection.QueryFirstOrDefaultAsync<OrderViewInfo>(sql, new { OrderID = orderID });
        }

        public async Task<PagedResult<OrderViewInfo>> ListAsync(OrderSearchInput input)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
        SELECT o.OrderID, o.CustomerID, o.EmployeeID, o.ShipperID,
               o.OrderTime, o.Status, o.DeliveryProvince, o.DeliveryAddress,
               c.CustomerName, c.Phone AS CustomerPhone,
               e.FullName AS EmployeeName,
               s.ShipperName,
               ISNULL(odTotals.TotalAmount,0) AS TotalAmount
        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
        LEFT JOIN Employees e ON o.EmployeeID = e.EmployeeID
        LEFT JOIN Shippers s ON o.ShipperID = s.ShipperID
        LEFT JOIN (
            SELECT OrderID, SUM(Quantity * SalePrice) AS TotalAmount
            FROM OrderDetails
            GROUP BY OrderID
        ) odTotals ON o.OrderID = odTotals.OrderID
        WHERE 1=1
    ";

            // ===== FILTER =====
            if (input.Status != 0)
            {
                sql += " AND o.Status = @Status";
            }

            if (input.DateFrom != null)
            {
                sql += " AND o.OrderTime >= @DateFrom";
            }

            if (input.DateTo != null)
            {
                sql += " AND o.OrderTime <= @DateTo";
            }

            if (!string.IsNullOrWhiteSpace(input.SearchValue))
            {
                sql += " AND c.CustomerName LIKE @SearchValue";
            }

            // ===== ORDER + PAGING =====
            sql += @"
        ORDER BY o.OrderTime DESC
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY
    ";

            var parameters = new
            {
                Status = input.Status,
                DateFrom = input.DateFrom,
                DateTo = input.DateTo,
                SearchValue = "%" + input.SearchValue + "%",
                Offset = (input.Page - 1) * input.PageSize,
                PageSize = input.PageSize
            };

            var data = await connection.QueryAsync<OrderViewInfo>(sql, parameters);

            // ===== COUNT =====
            string countSql = @"
        SELECT COUNT(*)
        FROM Orders o
        LEFT JOIN Customers c ON o.CustomerID = c.CustomerID
        WHERE 1=1
    ";

            if (input.Status != 0)
                countSql += " AND o.Status = @Status";

            if (input.DateFrom != null)
                countSql += " AND o.OrderTime >= @DateFrom";

            if (input.DateTo != null)
                countSql += " AND o.OrderTime <= @DateTo";

            if (!string.IsNullOrWhiteSpace(input.SearchValue))
                countSql += " AND c.CustomerName LIKE @SearchValue";

            int count = await connection.ExecuteScalarAsync<int>(countSql, parameters);

            return new PagedResult<OrderViewInfo>
            {
                DataItems = data.ToList(),
                RowCount = count,
                Page = input.Page,
                PageSize = input.PageSize
            };
        }

        #endregion

        #region ORDER DETAIL

        public async Task<bool> AddDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO OrderDetails(OrderID, ProductID, Quantity, SalePrice)
                VALUES(@OrderID, @ProductID, @Quantity, @SalePrice)";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> UpdateDetailAsync(OrderDetail data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE OrderDetails
                SET Quantity = @Quantity,
                    SalePrice = @SalePrice
                WHERE OrderID = @OrderID AND ProductID = @ProductID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        public async Task<bool> DeleteDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM OrderDetails WHERE OrderID = @OrderID AND ProductID = @ProductID";
            return await connection.ExecuteAsync(sql, new { OrderID = orderID, ProductID = productID }) > 0;
        }

        public async Task<OrderDetailViewInfo?> GetDetailAsync(int orderID, int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT od.*, p.ProductName
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @OrderID AND od.ProductID = @ProductID";
            return await connection.QueryFirstOrDefaultAsync<OrderDetailViewInfo>(sql,
                new { OrderID = orderID, ProductID = productID });
        }

        public async Task<List<OrderDetailViewInfo>> ListDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT od.OrderID, od.ProductID, p.ProductName,
                       od.Quantity, od.SalePrice
                FROM OrderDetails od
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE od.OrderID = @orderID";
            var data = await connection.QueryAsync<OrderDetailViewInfo>(sql, new { orderID });
            return data.ToList();
        }

        #endregion

        #region CREATE ORDER & ORDER DETAIL

        public async Task<int> CreateOrderAsync(int customerID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Orders (CustomerID, OrderTime, Status)
                VALUES (@CustomerID, GETDATE(), 1);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, new { CustomerID = customerID });
        }

        public async Task<int> CreateOrderAsync(Order order)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Orders
                (CustomerID, EmployeeID, OrderTime, Status, DeliveryProvince, DeliveryAddress)
                VALUES
                (@CustomerID, @EmployeeID, @OrderTime, @Status, @DeliveryProvince, @DeliveryAddress);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, order);
        }

        public async Task AddOrderDetailAsync(OrderDetail detail)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO OrderDetails
                (OrderID, ProductID, Quantity, SalePrice)
                VALUES
                (@OrderID, @ProductID, @Quantity, @SalePrice);";
            await connection.ExecuteAsync(sql, detail);
        }

        #endregion

        #region CUSTOMER ORDER HISTORY

        public async Task<List<dynamic>> GetOrderHistoryAsync(int customerID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT o.OrderID, o.OrderTime, p.ProductName, od.SalePrice AS Price, od.Quantity
                FROM Orders o
                JOIN OrderDetails od ON o.OrderID = od.OrderID
                JOIN Products p ON od.ProductID = p.ProductID
                WHERE o.CustomerID = @CustomerID
                ORDER BY o.OrderID DESC";
            var data = await connection.QueryAsync(sql, new { CustomerID = customerID });
            return data.ToList();
        }

        public async Task<IEnumerable<dynamic>> GetOrdersByCustomerAsync(int customerID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT OrderID, OrderTime, Status
                FROM Orders
                WHERE CustomerID = @CustomerID
                ORDER BY OrderTime DESC";
            return await connection.QueryAsync(sql, new { CustomerID = customerID });
        }

        public async Task<dynamic?> GetOrderByIdAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Orders WHERE OrderID = @OrderID";
            return await connection.QueryFirstOrDefaultAsync(sql, new { OrderID = orderID });
        }

        public async Task<IEnumerable<dynamic>> GetOrderDetailsAsync(int orderID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                SELECT p.ProductName, d.Quantity, d.SalePrice
                FROM OrderDetails d
                JOIN Products p ON d.ProductID = p.ProductID
                WHERE d.OrderID = @OrderID";
            return await connection.QueryAsync(sql, new { OrderID = orderID });
        }

        #endregion
    }
}
