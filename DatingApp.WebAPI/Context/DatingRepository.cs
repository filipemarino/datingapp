using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.WebAPI.Helpers;
using DatingApp.WebAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.WebAPI.Context {
    public class DatingRepository : IDatingRepository {
        private readonly DataContext _context;

        public DatingRepository (DataContext context) {
            _context = context;
        }

        public void Add<T> (T entity) where T : class {
            _context.Add(entity);
        }

        public void Delete<T> (T entity) where T : class {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id) {
            var user = await _context.Users.Include(p => p.Photos)
                                            .FirstOrDefaultAsync(e => e.Id == id);

            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams) {
            var users = _context.Users.Include(p => p.Photos).AsQueryable();

            users = users.Where(x => x.Id != userParams.UserId 
                                && x.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLikers.Contains(x.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLikees.Contains(x.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDoB = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDoB = DateTime.Today.AddYears(-userParams.MinAge);

                users = users.Where(x => x.DateOfBirth >= minDoB && x.DateOfBirth <= maxDoB);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(x => x.Created);
                        break;
                    default:
                        users = users.OrderByDescending(x => x.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            var user = await _context.Users.
                Include(x => x.Likers).
                Include(x => x.Likees).
                FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(x => x.LikeeId == id).Select(x => x.LikerId);
            }
            else
            {
                return user.Likees.Where(x => x.LikerId == id).Select(x => x.LikeeId);
            }
        }

        public async Task<bool> SaveAll() {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(e => e.Id == id);

            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            var photo = await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(x => x.IsMain);
            return photo;
        }

        public async Task<Like> GetLike(int userId, int recepientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(x => x.LikerId == userId && x.LikeeId == recepientId);
        }
    }
}