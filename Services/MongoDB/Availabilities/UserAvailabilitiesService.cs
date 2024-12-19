﻿using MongoDB.Driver;
using Project_A_Server.Interfaces;
using Project_A_Server.Models.Availabilities;
using System.Security.Cryptography;

namespace Project_A_Server.Services.MongoDB.Availabilities
{
    public class UserAvailabilitiesService
    {
        private readonly IGenericRepository<UserAvailabilities> _repository;

        public UserAvailabilitiesService(IGenericRepository<UserAvailabilities> repository)
        {
            _repository = repository;
        }

        public async Task<List<UserAvailabilities>> GetAllAsync() => 
            await _repository.GetAllAsync();

        public async Task<UserAvailabilities?> GetByUIDAsync(string uid) =>
            await _repository.GetByUIDAsync(uid);

        public async Task CreateAsync(UserAvailabilities newAvailability) =>
            await _repository.CreateAsync(newAvailability);

        public async Task UpdateAsync(string id, UserAvailabilities updatedAvailability) =>
            await _repository.UpdateAsync(id, updatedAvailability);

        public async Task RemoveAsync(string id) =>
            await _repository.DeleteByObjectIdAsync(id);

        public async Task AddAvailabilityAsync(string uid, string aid)
        {
            var filter = Builders<UserAvailabilities>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserAvailabilities>.Update.AddToSet(x => x.Availabilities, aid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"User with ID {uid} not found.");
        }
        public async Task RemoveAvailabilityAsync(string uid, string aid)
        {
            var filter = Builders<UserAvailabilities>.Filter.Eq(x => x.UID, uid);
            var update = Builders<UserAvailabilities>.Update.Pull(x => x.Availabilities, aid);

            UpdateResult result = await _repository.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0)
                throw new InvalidOperationException($"User with ID {uid} not found.");
        }
    }
}
