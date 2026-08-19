using System.ComponentModel.DataAnnotations;

namespace webapp_demo.Models;

public class PropertyFormViewModel
{
    public int Id { get; set; }

    [Required, StringLength(200), Display(Name = "Tiêu đề")]
    public string Title { get; set; } = "";

    [Required, Display(Name = "Mô tả")]
    public string Description { get; set; } = "";

    [Required, Range(100_000, 100_000_000_000, ErrorMessage = "Giá phải lớn hơn 0"), Display(Name = "Giá (VNĐ)")]
    public decimal Price { get; set; }

    [Required, Range(1, 100_000, ErrorMessage = "Diện tích phải lớn hơn 0"), Display(Name = "Diện tích (m²)")]
    public decimal Area { get; set; }

    [Range(0, 20), Display(Name = "Số phòng ngủ")]
    public int Bedrooms { get; set; }

    [Range(0, 20), Display(Name = "Số phòng tắm")]
    public int Bathrooms { get; set; }

    [Range(1, 50), Display(Name = "Số tầng")]
    public int Floors { get; set; } = 1;

    [Range(0, 100), Display(Name = "Chiều rộng mặt tiền (m)")]
    public decimal FacadeWidth { get; set; }

    [Required, Display(Name = "Quận/Huyện")]
    public string District { get; set; } = "";

    [Display(Name = "Phường/Xã")]
    public string? Ward { get; set; }

    [Display(Name = "Đường")]
    public string? Street { get; set; }

    [Required, StringLength(200), Display(Name = "Địa chỉ")]
    public string Address { get; set; } = "";

    [Display(Name = "Vĩ độ")]
    public double? Latitude { get; set; }

    [Display(Name = "Kinh độ")]
    public double? Longitude { get; set; }

    [Display(Name = "Cho thuê")]
    public bool IsForRent { get; set; }

    [Display(Name = "Số điện thoại liên hệ")]
    public string? ContactPhone { get; set; }

    [Required, Display(Name = "Loại bất động sản")]
    public int PropertyTypeId { get; set; }

    [Display(Name = "Hình ảnh (tối đa 5)")]
    public List<IFormFile>? Images { get; set; }
}