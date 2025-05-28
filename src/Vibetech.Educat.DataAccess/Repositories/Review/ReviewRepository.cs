using Microsoft.EntityFrameworkCore;
using Vibetech.Educat.DataAccess.Models;

namespace Vibetech.Educat.DataAccess.Repositories.Review;

public class ReviewRepository : BaseRepository<Models.Review>, IReviewRepository
{
    public ReviewRepository(EducatDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Models.Review>> GetByTeacherIdAsync(int teacherId)
    {
        return await _dbSet
            .Include(r => r.Teacher)
            .Include(r => r.Student)
            .Where(r => r.TeacherId == teacherId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.Review>> GetByStudentIdAsync(int studentId)
    {
        return await _dbSet
            .Include(r => r.Teacher)
            .Include(r => r.Student)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingByTeacherIdAsync(int teacherId)
    {
        var reviews = await _dbSet
            .Where(r => r.TeacherId == teacherId)
            .ToListAsync();
            
        if (!reviews.Any()) return 0;
        
        return reviews.Average(r => r.Rating);
    }
} 