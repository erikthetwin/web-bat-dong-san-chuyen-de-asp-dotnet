using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class ProfileViewModel
{
    [Required, StringLength(100), Display(Name = "Họ tên")]
    public string FullName { get; set; } = "";
    [StringLength(20), Display(Name = "Số điện thoại")]
    public string? Phone { get; set; }
    [StringLength(200)]
    public string? Address { get; set; }
    [StringLength(100), DataType(DataType.Password), Display(Name = "Mật khẩu mới")]
    public string? NewPassword { get; set; }
    [DataType(DataType.Password), Compare(nameof(NewPassword)), Display(Name = "Xác nhận mật khẩu mới")]
    public string? ConfirmNewPassword { get; set; }
}