using Microsoft.EntityFrameworkCore;
using QuestionaireApi.Interfaces;
using QuestionaireApi.Models.Database;
using QuestionaireApi.Models.Dto;

namespace QuestionaireApi.Services
{
    public class RoleService(QuestionaireDbContext context) : IRoleService
    {
        public async Task<List<Role>> GetRoles()
        {
            try
            {
                return await context.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while retrieving roles.", ex);
            }
        }
        
        public async Task<Role?> GetRoleById(int id)
        {
            try
            {
                return await context.Roles
                    .Include(r => r.Users)
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while retrieving role with ID {id}.", ex);
            }
        }
        
        public async Task CreateRole(RoleDto role)
        {
            try
            {
                context.Roles.Add(new Role
                {
                    Name = role.Name
                });
                
                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while adding the new role.", ex);
            }
        }
        
        public async Task<bool> UpdateRole(int id, RoleDto updatedRole)
        {
            try
            {
                Role? existingRole = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);

                if (existingRole == null) return false;

                existingRole.Name = updatedRole.Name;

                context.Roles.Update(existingRole);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while updating the role with ID {id}.", ex);
            }
        }
        
        public async Task<bool> DeleteRole(int id)
        {
            try
            {
                Role? role = await context.Roles.FirstOrDefaultAsync(r => r.Id == id);

                if (role == null) return false;

                context.Roles.Remove(role);
                await context.SaveChangesAsync();
                
                return true;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"An error occurred while deleting the role with ID {id}.", ex);
            }
        }
    }
}
