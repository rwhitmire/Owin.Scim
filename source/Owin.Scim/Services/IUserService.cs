﻿namespace Owin.Scim.Services
{
    using System.Threading.Tasks;

    using Microsoft.FSharp.Core;

    using Model.Users;

    public interface IUserService
    {
        Task<User> CreateUser(User user);

        Task<User> RetrieveUser(string userId);

        Task<User> UpdateUser(User user);

        Task<Unit> DeleteUser(string userId);
    }
}