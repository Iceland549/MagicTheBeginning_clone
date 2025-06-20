using System.Security.Claims;
using AutoMapper;
using AuthMicroservice.Application.DTOs;
using AuthMicroservice.Application.Interfaces;

namespace AuthMicroservice.Application.UseCases
{
    /// <summary>
    /// Retrieves the profile information of the currently authenticated user.
    /// </summary>
    public class GetProfileUseCase
    {
        private readonly IUserRepository _repo;
        private readonly IMapper _mapper;

        public GetProfileUseCase(IUserRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        /// <summary>
        /// Executes the profile retrieval by extracting user ID from the JWT claims.
        /// </summary>
        /// <param name="user">ClaimsPrincipal containing the authenticated user's claims.</param>
        /// <returns>DTO containing user profile and roles.</returns>
        public async Task<ProfileDto> ExecuteAsync(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? throw new InvalidOperationException("Missing or invalid token.");

            var entity = await _repo.GetByIdAsync(id)
                         ?? throw new KeyNotFoundException("User not found.");

            var roles = await _repo.GetRolesAsync(id);
            var dto = _mapper.Map<ProfileDto>(entity);
            dto.Roles = roles;

            return dto;
        }
    }
}