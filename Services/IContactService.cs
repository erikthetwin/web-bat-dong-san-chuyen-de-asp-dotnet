using webapp_demo.Models;

namespace webapp_demo.Services;

public interface IContactService
{
    Task<ContactRequest> CreateAsync(ContactRequest r);
    Task<List<ContactRequest>> GetForOwnerAsync(string ownerId);
    Task<List<ContactRequest>> GetForAdminAsync();
    Task<int> CountForOwnerAsync(string ownerId);
}