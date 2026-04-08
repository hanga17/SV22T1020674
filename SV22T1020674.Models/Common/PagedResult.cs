using System;
using System.Collections.Generic;

namespace SV22T1020674.Models.Common
{
    /// <summary>
    /// Lớp dùng để biểu diễn kết quả truy vấn/tìm kiếm dữ liệu dưới dạng phân trang
    /// </summary>
    /// <typeparam name="T">Kiểu của dữ liệu truy vấn được</typeparam>
    public class PagedResult<T> where T : class
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int RowCount { get; set; }
        public List<T> DataItems { get; set; } = new List<T>();

        // Tương thích code cũ
        public List<T> Items
        {
            get => DataItems;
            set => DataItems = value;
        }
        public int TotalRecords
        {
            get => RowCount;
            set => RowCount = value;
        }

        public int PageCount
        {
            get
            {
                if (PageSize == 0) return 1;
                return (int)Math.Ceiling((decimal)RowCount / PageSize);
            }
        }

        public bool HasPreviousPage => Page > 1;
        public bool HasNextPage => Page < PageCount;

        public List<PageItem> GetDisplayPages(int n = 5)
        {
            var result = new List<PageItem>();
            if (PageCount == 0) return result;

            n = n > 0 ? n : 5;

            int currentPage = Page;
            if (currentPage < 1) currentPage = 1;
            else if (currentPage > PageCount) currentPage = PageCount;

            int displayedPages = 2 * n + 1;
            int startPage = currentPage - n;
            int endPage = currentPage + n;

            if (startPage < 1)
            {
                endPage += (1 - startPage);
                startPage = 1;
            }

            if (endPage > PageCount)
            {
                startPage -= (endPage - PageCount);
                endPage = PageCount;
            }

            if (startPage < 1) startPage = 1;
            if (endPage - startPage + 1 > displayedPages)
                endPage = startPage + displayedPages - 1;

            // Trang đầu
            if (startPage > 1)
            {
                result.Add(new PageItem(1, currentPage == 1));
                if (startPage > 2) result.Add(new PageItem(0));
            }

            // Trang hiện tại và các trang lân cận
            for (int i = startPage; i <= endPage; i++)
            {
                result.Add(new PageItem(i, i == currentPage));
            }

            // Trang cuối
            if (endPage < PageCount)
            {
                if (endPage < PageCount - 1) result.Add(new PageItem(0));
                result.Add(new PageItem(PageCount, currentPage == PageCount));
            }

            return result;
        }
    }
}
