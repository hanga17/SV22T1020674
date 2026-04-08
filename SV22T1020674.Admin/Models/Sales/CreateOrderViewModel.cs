using SV22T1020674.Models.Catalog;

namespace SV22T1020674.Models.Sales
{
    public class CreateOrderViewModel
    {
        // Từ khóa tìm kiếm
        public string SearchValue { get; set; } = "";

        // Danh sách sản phẩm tìm được
        public List<Product> Products { get; set; } = new List<Product>();
    }
}