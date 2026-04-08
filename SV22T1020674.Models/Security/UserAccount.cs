namespace SV22T1020674.Models.Security
{
    /// <summary>
    /// Thông tin tài khoản người dùng
    /// </summary>
    public class UserAccount
    {
        /// <summary>
        /// Mã tài khoản
        /// </summary>
        public string UserId { get; set; } = "";   // FIX: int thay vì string

        /// <summary>
        /// Tên đăng nhập
        /// </summary>
        public string UserName { get; set; } = "";

        /// <summary>
        /// Tên hiển thị (họ tên)
        /// </summary>
        public string DisplayName { get; set; } = "";

        /// <summary>
        /// Email
        /// </summary>
        public string Email { get; set; } = "";

        /// <summary>
        /// Ảnh đại diện
        /// </summary>
        public string Photo { get; set; } = "";

        /// <summary>
        /// Quyền
        /// </summary>
        public string RoleNames { get; set; } = "";
    }
}