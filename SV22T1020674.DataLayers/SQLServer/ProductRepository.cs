using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020674.DataLayers.Interfaces;
using SV22T1020674.Models.Catalog;
using SV22T1020674.Models.Common;

namespace SV22T1020674.DataLayers.SQLServer
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        // ======================== PRODUCT ========================

        public async Task<int> AddAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO Products (ProductName, ProductDescription, SupplierID, CategoryID, Unit, Price, Photo, IsSelling)
                VALUES (@ProductName, @ProductDescription, @SupplierID, @CategoryID, @Unit, @Price, @Photo, @IsSelling);
                SELECT CAST(SCOPE_IDENTITY() as int);";
            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> DeleteAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM Products WHERE ProductID = @ProductID";
            return await connection.ExecuteAsync(sql, new { ProductID = productID }) > 0;
        }

        public async Task<Product?> GetAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM Products WHERE ProductID = @ProductID";
            return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { ProductID = productID });
        }

        public async Task<bool> IsUsedAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                IF EXISTS(SELECT * FROM OrderDetails WHERE ProductID = @ProductID)
                    SELECT 1
                ELSE
                    SELECT 0";
            return await connection.ExecuteScalarAsync<bool>(sql, new { ProductID = productID });
        }

        public async Task<PagedResult<Product>> ListAsync(ProductSearchInput input)
        {
            var result = new PagedResult<Product>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string searchValue = string.IsNullOrWhiteSpace(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = new SqlConnection(_connectionString);

            // ===== COUNT =====
            string sqlCount = @"
        SELECT COUNT(*) 
        FROM Products 
        WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
        AND (@MinPrice = 0 OR Price >= @MinPrice)
        AND (@MaxPrice = 0 OR Price <= @MaxPrice)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(sqlCount, new
            {
                SearchValue = searchValue,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice
            });

            // ===== LIST =====
            string sqlList = @"
        SELECT *
        FROM Products
        WHERE (@SearchValue = N'' OR ProductName LIKE @SearchValue)
        AND (@MinPrice = 0 OR Price >= @MinPrice)
        AND (@MaxPrice = 0 OR Price <= @MaxPrice)
        ORDER BY ProductName
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var data = await connection.QueryAsync<Product>(sqlList, new
            {
                SearchValue = searchValue,
                MinPrice = input.MinPrice,
                MaxPrice = input.MaxPrice,
                Offset = input.Offset,
                PageSize = input.PageSize
            });

            result.DataItems = data.ToList();

            return result;
        }

        public async Task<bool> UpdateAsync(Product data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE Products
                SET ProductName = @ProductName,
                    ProductDescription = @ProductDescription,
                    SupplierID = @SupplierID,
                    CategoryID = @CategoryID,
                    Unit = @Unit,
                    Price = @Price,
                    Photo = @Photo,
                    IsSelling = @IsSelling
                WHERE ProductID = @ProductID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // ======================== ATTRIBUTES ========================

        public async Task<long> AddAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO ProductAttributes (ProductID, AttributeName, AttributeValue, DisplayOrder)
                VALUES (@ProductID, @AttributeName, @AttributeValue, @DisplayOrder);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> DeleteAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM ProductAttributes WHERE AttributeID = @AttributeID";
            return await connection.ExecuteAsync(sql, new { AttributeID = attributeID }) > 0;
        }

        public async Task<ProductAttribute?> GetAttributeAsync(long attributeID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductAttributes WHERE AttributeID = @AttributeID";
            return await connection.QueryFirstOrDefaultAsync<ProductAttribute>(sql, new { AttributeID = attributeID });
        }

        public async Task<List<ProductAttribute>> ListAttributesAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductAttributes WHERE ProductID = @ProductID ORDER BY DisplayOrder";
            var data = await connection.QueryAsync<ProductAttribute>(sql, new { ProductID = productID });
            return data.ToList();
        }

        public async Task<bool> UpdateAttributeAsync(ProductAttribute data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE ProductAttributes 
                SET AttributeName = @AttributeName, AttributeValue = @AttributeValue, DisplayOrder = @DisplayOrder
                WHERE AttributeID = @AttributeID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // ======================== PHOTOS ========================

        public async Task<long> AddPhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                INSERT INTO ProductPhotos (ProductID, Photo, Description, DisplayOrder, IsHidden)
                VALUES (@ProductID, @Photo, @Description, @DisplayOrder, @IsHidden);
                SELECT CAST(SCOPE_IDENTITY() as bigint);";
            return await connection.ExecuteScalarAsync<long>(sql, data);
        }

        public async Task<bool> DeletePhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"DELETE FROM ProductPhotos WHERE PhotoID = @PhotoID";
            return await connection.ExecuteAsync(sql, new { PhotoID = photoID }) > 0;
        }

        public async Task<ProductPhoto?> GetPhotoAsync(long photoID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductPhotos WHERE PhotoID = @PhotoID";
            return await connection.QueryFirstOrDefaultAsync<ProductPhoto>(sql, new { PhotoID = photoID });
        }

        public async Task<List<ProductPhoto>> ListPhotosAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"SELECT * FROM ProductPhotos WHERE ProductID = @ProductID ORDER BY DisplayOrder";
            var data = await connection.QueryAsync<ProductPhoto>(sql, new { ProductID = productID });
            return data.ToList();
        }

        public async Task<bool> UpdatePhotoAsync(ProductPhoto data)
        {
            using var connection = new SqlConnection(_connectionString);
            string sql = @"
                UPDATE ProductPhotos 
                SET Photo = @Photo, Description = @Description, DisplayOrder = @DisplayOrder, IsHidden = @IsHidden
                WHERE PhotoID = @PhotoID";
            return await connection.ExecuteAsync(sql, data) > 0;
        }

        // ===== SQL CHUYỂN TỪ CONTROLLER =====
        public async Task<dynamic?> GetSellingProductAsync(int productID)
        {
            using var connection = new SqlConnection(_connectionString);

            string sql = @"
                SELECT *
                FROM Products
                WHERE ProductID = @id AND IsSelling = 1";

            return await connection.QueryFirstOrDefaultAsync(sql, new { id = productID });
        }
    }
}
