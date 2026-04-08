using SV22T1020674.Models.Catalog;
using SV22T1020674.Models.Common;

namespace SV22T1020674.DataLayers.Interfaces
{
    public interface IProductRepository
    {
        Task<PagedResult<Product>> ListAsync(ProductSearchInput input);
        Task<Product?> GetAsync(int productID);
        Task<int> AddAsync(Product data);
        Task<bool> UpdateAsync(Product data);
        Task<bool> DeleteAsync(int productID);
        Task<bool> IsUsedAsync(int productID);

        Task<List<ProductAttribute>> ListAttributesAsync(int productID);
        Task<ProductAttribute?> GetAttributeAsync(long attributeID);
        Task<long> AddAttributeAsync(ProductAttribute data);
        Task<bool> UpdateAttributeAsync(ProductAttribute data);
        Task<bool> DeleteAttributeAsync(long attributeID);

        Task<List<ProductPhoto>> ListPhotosAsync(int productID);
        Task<ProductPhoto?> GetPhotoAsync(long photoID);
        Task<long> AddPhotoAsync(ProductPhoto data);
        Task<bool> UpdatePhotoAsync(ProductPhoto data);
        Task<bool> DeletePhotoAsync(long photoID);

        // ===== ADD =====
        Task<dynamic?> GetSellingProductAsync(int productID);
    }
}
