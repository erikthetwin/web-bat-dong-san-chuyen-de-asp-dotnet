using Microsoft.EntityFrameworkCore;
using webapp_demo.Data;
using webapp_demo.Models;

namespace webapp_demo.Services;

public class ContactService : IContactService
{
    private readonly ApplicationDbContext _db;
    public ContactService(ApplicationDbContext db) => _db = db;

    public async Task<ContactRequest> CreateAsync(ContactRequest r)
    {
        _db.ContactRequests.Add(r);
        await _db.SaveChangesAsync();
        return r;
    }

    public async Task<List<ContactRequest>> GetForOwnerAsync(string ownerId) =>
        await _db.ContactRequests
            .Include(c => c.Property)
            .Where(c => c.Property!.OwnerId == ownerId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();

    public async Task<List<ContactRequest>> GetForAdminAsync() =>
        await _db.ContactRequests.Include(c => c.Property).OrderByDescending(c => c.CreatedAt).ToListAsync();

    public async Task<int> CountForOwnerAsync(string ownerId) =>
        await _db.ContactRequests.CountAsync(c => c.Property!.OwnerId == ownerId);
}