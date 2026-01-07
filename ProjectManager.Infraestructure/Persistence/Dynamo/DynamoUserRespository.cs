using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Enums;
using ProjectManager.Domain.Interfaces;
using System.Globalization;

namespace ProjectManager.Infraestructure.Persistence.Dynamo
{
    public sealed class DynamoUserRepository : IUserRepository
    {
        private readonly IAmazonDynamoDB _dynamoClient;
        private readonly string _usersTableName;
        private readonly string _usernameIndexName;

        public DynamoUserRepository(IAmazonDynamoDB dynamoClient, IConfiguration config)
        {
            _dynamoClient = dynamoClient ?? throw new ArgumentNullException(nameof(dynamoClient));
            _usersTableName = config["DynamoDB:UsersTableName"] ?? throw new InvalidOperationException("DynamoDB users table name is not configured.");
            _usernameIndexName = config["DynamoDB:UsersUsernameIndexName"] ?? "Username-index";
        }

        public async Task<User?> GetByUsernameAsync(string username, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                return null;
            }

            var request = new QueryRequest
            {
                TableName = _usersTableName,
                IndexName = _usernameIndexName,
                KeyConditionExpression = "#username = :username",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#username"] = "Username"
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":username"] = new AttributeValue { S = username }
                },
                Limit = 1,
                ConsistentRead = false
            };

            var response = await _dynamoClient.QueryAsync(request, ct);
            if (response.Items.Count == 0)
            {
                return null;
            }

            return Map(response.Items[0]);
        }

        private static User Map(IDictionary<string, AttributeValue> item)
        {
            return new User
            {
                Id = DynamoParseHelper.ParseInt(item, "Id"),
                Name = DynamoParseHelper.ParseString(item, "Name"),
                UserName = DynamoParseHelper.ParseString(item, "UserName"),
                PasswordHash = DynamoParseHelper.ParseString(item, "PasswordHash"),
                Email = DynamoParseHelper.ParseString(item, "Email"),
                Role = DynamoParseHelper.ParseRole(item, "Role")
            };
        }

       
    }
}