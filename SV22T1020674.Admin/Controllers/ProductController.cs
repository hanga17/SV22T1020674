using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020674.BusinessLayers;
using SV22T1020674.Models.Catalog;

namespace SV22T1020674.Admin.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        // ================== DANH SÁCH + TÌM KIẾM ==================
        public async Task<IActionResult> Index(
    string keyword,
    int? categoryId,
    int? supplierId,
    decimal? minPrice,
    decimal? maxPrice,
    int page = 1) // ⚠️ PHẢI CÓ DÒNG NÀY
        {
            int pageSize = 50; // 👉 test cho dễ thấy

            // 👉 LƯU LẠI ĐỂ VIEW DÙNG
            ViewBag.Keyword = keyword;
            ViewBag.CategoryId = categoryId;
            ViewBag.SupplierId = supplierId;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.Page = page;

            var input = new ProductSearchInput()
            {
                Page = page, // ⚠️ QUAN TRỌNG NHẤT
                PageSize = pageSize,
                SearchValue = keyword ?? "",
                CategoryID = categoryId ?? 0,
                SupplierID = supplierId ?? 0,
                MinPrice = minPrice ?? 0,
                MaxPrice = maxPrice ?? 0
            };

            var result = await CatalogDataService.ListProductsAsync(input);

            // 👉 PHÂN TRANG
            ViewBag.RowCount = result.RowCount;
            ViewBag.PageCount = (int)Math.Ceiling((double)result.RowCount / pageSize);

            return View(result.DataItems);
        }

        // ================== TẠO MỚI ==================
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model)
        {
            await CatalogDataService.AddProductAsync(model);
            return RedirectToAction("Index");
        }

        // ================== CẬP NHẬT ==================
        public async Task<IActionResult> Edit(int id)
        {
            var data = await CatalogDataService.GetProductAsync(id);


            ViewBag.ProductID = id;
            ViewBag.Photos = await CatalogDataService.ListPhotosAsync(id);

            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile uploadPhoto)
        {
            // Nếu có chọn ảnh
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                // Đường dẫn lưu
                string path = Path.Combine(
    Directory.GetCurrentDirectory(),
    "..",
    "SV22T1020674.Shop",
    "wwwroot",
    "images",
    "products"
);

                // Nếu chưa có folder thì tạo
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                // Tạo tên file mới
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadPhoto.FileName);

                string fullPath = Path.Combine(path, fileName);

                // Lưu file
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                // GÁN VÀO MODEL
                model.Photo = fileName;
            }
            else
            {
                // Nếu KHÔNG chọn ảnh → giữ ảnh cũ
                var old = await CatalogDataService.GetProductAsync(model.ProductID);
                model.Photo = old?.Photo;
            }

            await CatalogDataService.UpdateProductAsync(model);

            return RedirectToAction("Index");
        }

        // ================== XÓA ==================
        // HIỂN THỊ TRANG XÁC NHẬN
        public async Task<IActionResult> Delete(int id = 0)
        {
            var data = await CatalogDataService.GetProductAsync(id);
            if (data == null)
                return RedirectToAction("Index");

            return View(data);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await CatalogDataService.GetProductAsync(id);

            if (await CatalogDataService.IsUsedProductAsync(id))
            {
                // Không xóa → chuyển thành ngừng bán
                product.IsSelling = false;
                await CatalogDataService.UpdateProductAsync(product);

                TempData["Message"] = "Sản phẩm đang được sử dụng → đã chuyển sang ngừng bán";
            }
            else
            {
                await CatalogDataService.DeleteProductAsync(id);
                TempData["Message"] = "Đã xóa sản phẩm";
            }

            return RedirectToAction("Index");
        }

        // ================== ẢNH ==================
        public async Task<IActionResult> Photos(int id = 0)
        {
            ViewBag.ProductID = id;
            var data = await CatalogDataService.ListPhotosAsync(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhoto(int productID, IFormFile uploadPhoto)
        {
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                // 📁 Lưu vào SHOP (để hiển thị được)
                string path = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "..",
                    "SV22T1020674.Shop",
                    "wwwroot",
                    "images",
                    "products"
                );

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadPhoto.FileName);
                string fullPath = Path.Combine(path, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                // 💾 Lưu DB
                ProductPhoto photo = new ProductPhoto()
                {
                    ProductID = productID,
                    Photo = fileName,
                    Description = ""
                };

                await CatalogDataService.AddPhotoAsync(photo);
            }

            return RedirectToAction("Photos", new { id = productID });
        }

        [HttpPost]
        public async Task<IActionResult> DeletePhoto(long id, int productId)
        {
            await CatalogDataService.DeletePhotoAsync(id);
            return RedirectToAction("Edit", new { id = productId });
        }

        // ================== THUỘC TÍNH ==================
        public async Task<IActionResult> Attributes(int id = 0)
        {
            ViewBag.ProductID = id;
            var data = await CatalogDataService.ListAttributesAsync(id);
            return View(data);
        }

        [HttpPost]
        public async Task<IActionResult> AddAttribute(ProductAttribute model)
        {
            await CatalogDataService.AddAttributeAsync(model);
            return RedirectToAction("Attributes", new { id = model.ProductID });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttribute(long id, int productId)
        {
            await CatalogDataService.DeleteAttributeAsync(id);
            return RedirectToAction("Attributes", new { id = productId });
        }
        [HttpPost]
        public async Task<IActionResult> UpdateAttribute(ProductAttribute model)
        {
            await CatalogDataService.UpdateAttributeAsync(model);
            return RedirectToAction("Attributes", new { id = model.ProductID });
        }
        // ===== FORM THÊM ẢNH =====
        public IActionResult CreatePhoto(int id)
        {
            ViewBag.ProductID = id;
            return View();
        }

        // ===== LƯU ẢNH =====
        [HttpPost]
        public async Task<IActionResult> CreatePhoto(ProductPhoto model, IFormFile uploadPhoto)
        {
            if (uploadPhoto != null && uploadPhoto.Length > 0)
            {
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/products");

                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadPhoto.FileName);

                string fullPath = Path.Combine(path, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await uploadPhoto.CopyToAsync(stream);
                }

                model.Photo = fileName;
            }

            await CatalogDataService.AddPhotoAsync(model);

            return RedirectToAction("Edit", new { id = model.ProductID });
        }
    }
}