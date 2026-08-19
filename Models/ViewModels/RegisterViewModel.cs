using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class RegisterViewModel
{
    [Required, EmailAddress, Display(Name = "Email")]
    public string Email { get; set; } = "";
    [Required, StringLength(100), Display(Name = "Họ tên")]
    public string FullName { get; set; } = "";
    [Required, StringLength(20), Display(Name = "Số điện thoại")]
    public string Phone { get; set; } = "";
    [StringLength(200)]
    public string? Address { get; set; }
    [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password), Display(Name = "Mật khẩu")]
    public string Password { get; set; } = "";
    [DataType(DataType.Password), Compare(nameof(Password)), Display(Name = "Xác nhận mật khẩu")]
    public string ConfirmPassword { get; set; } = "";
    [Display(Name = "Vai trò")]
    public string Role { get; set; } = "Buyer";
}