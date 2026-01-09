using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Configuration;
using ProjectManager.Domain.Entities;
using ProjectManager.Domain.Interfaces;
using System.Globalization;

namespace ProjectManager.Infraestructure.Persistence.Dynamo
{
    public sealed class DynamoProjectRepository : IProjectRepository
    {
        private readonly IAmazonDynamoDB _dynamoClient;
        private readonly string _projectsTableName;

        public DynamoProjectRepository(IAmazonDynamoDB dynamoClient, IConfiguration config)
        {
            _dynamoClient = dynamoClient ?? throw new ArgumentNullException(nameof(dynamoClient));
            _projectsTableName = config["DynamoDB:ProjectsTableName"] ?? throw new InvalidOperationException("DynamoDB projects table name is not configured.");
        }

        public async Task<IEnumerable<Project>> GetAsync(string? status, string? ownerId, CancellationToken cancellationToken = default)
        {
            var request = new ScanRequest
            {
                TableName = _projectsTableName
            };

            var filterExpressions = new List<string>();
            var attributeValues = new Dictionary<string, AttributeValue>();

            if (!string.IsNullOrWhiteSpace(status))
            {
                filterExpressions.Add("#status = :status");
                attributeValues[":status"] = new AttributeValue { S = status };
            }

            if (!string.IsNullOrWhiteSpace(ownerId))
            {
                filterExpressions.Add("#ownerId = :ownerId");
                attributeValues[":ownerId"] = new AttributeValue { S = ownerId };
            }

            if (filterExpressions.Count > 0)
            {
                request.FilterExpression = string.Join(" AND ", filterExpressions);
                request.ExpressionAttributeValues = attributeValues;
                request.ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#status"] = "Status",
                    ["#ownerId"] = "OwnerId"
                };
            }

            var response = await _dynamoClient.ScanAsync(request, cancellationToken);
            return response.Items.Select(Map);
        }

        public async Task<Project?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return null;
            }

            var request = new GetItemRequest
            {
                TableName = _projectsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["Id"] = new AttributeValue { S = id }
                }
            };

            var response = await _dynamoClient.GetItemAsync(request, cancellationToken);
            if (response.Item == null || response.Item.Count == 0)
            {
                return null;
            }

            return Map(response.Item);
        }

        public async Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var item = ToItem(project);
            var request = new PutItemRequest
            {
                TableName = _projectsTableName,
                Item = item,
                ConditionExpression = "attribute_not_exists(Id)"
            };

            await _dynamoClient.PutItemAsync(request, cancellationToken);
            return project;
        }

        public async Task<Project?> UpdateAsync(Project project, CancellationToken cancellationToken = default)
        {
            var updateRequest = new UpdateItemRequest
            {
                TableName = _projectsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["Id"] = new AttributeValue { S = project.Id }
                },
                UpdateExpression = "SET #name = :name, #description = :description, #ownerId = :ownerId, #status = :status, #updatedAtUtc = :updatedAtUtc",
                ExpressionAttributeNames = new Dictionary<string, string>
                {
                    ["#name"] = "Name",
                    ["#description"] = "Description",
                    ["#ownerId"] = "OwnerId",
                    ["#status"] = "Status",
                    ["#updatedAtUtc"] = "UpdatedAtUtc"
                },
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":name"] = new AttributeValue { S = project.Name },
                    [":description"] = new AttributeValue { S = project.Description },
                    [":ownerId"] = new AttributeValue { S = project.OwnerId },
                    [":status"] = new AttributeValue { S = project.Status },
                    [":updatedAtUtc"] = project.UpdatedAtUtc.HasValue
                        ? new AttributeValue { S = project.UpdatedAtUtc.Value.ToString("O", CultureInfo.InvariantCulture) }
                        : new AttributeValue { NULL = true }
                },
                ReturnValues = "ALL_NEW"
            };

            var response = await _dynamoClient.UpdateItemAsync(updateRequest, cancellationToken);
            if (response.Attributes == null || response.Attributes.Count == 0)
            {
                return null;
            }

            return Map(response.Attributes);
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            var request = new DeleteItemRequest
            {
                TableName = _projectsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    ["Id"] = new AttributeValue { S = id }
                }
            };

            var response = await _dynamoClient.DeleteItemAsync(request, cancellationToken);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }

        private static Project Map(IDictionary<string, AttributeValue> item)
        {
            return new Project
            {
                Id = item.TryGetValue("Id", out var id) ? id.S : string.Empty,
                Name = item.TryGetValue("Name", out var name) ? name.S : string.Empty,
                Description = item.TryGetValue("Description", out var description) ? description.S : string.Empty,
                OwnerId = item.TryGetValue("OwnerId", out var ownerId) ? ownerId.S : string.Empty,
                Status = item.TryGetValue("Status", out var status) ? status.S : string.Empty,
                CreatedAtUtc = item.TryGetValue("CreatedAtUtc", out var createdAt) && !string.IsNullOrWhiteSpace(createdAt.S)
                    ? DateTime.Parse(createdAt.S, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                    : DateTime.MinValue,
                UpdatedAtUtc = item.TryGetValue("UpdatedAtUtc", out var updatedAt) && !string.IsNullOrWhiteSpace(updatedAt.S)
                    ? DateTime.Parse(updatedAt.S, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal)
                    : null
            };
        }

        private static Dictionary<string, AttributeValue> ToItem(Project project)
        {
            return new Dictionary<string, AttributeValue>
            {
                ["Id"] = new AttributeValue { S = project.Id },
                ["Name"] = new AttributeValue { S = project.Name },
                ["Description"] = new AttributeValue { S = project.Description },
                ["OwnerId"] = new AttributeValue { S = project.OwnerId },
                ["Status"] = new AttributeValue { S = project.Status },
                ["CreatedAtUtc"] = new AttributeValue { S = project.CreatedAtUtc.ToString("O", CultureInfo.InvariantCulture) },
                ["UpdatedAtUtc"] = project.UpdatedAtUtc.HasValue
                    ? new AttributeValue { S = project.UpdatedAtUtc.Value.ToString("O", CultureInfo.InvariantCulture) }
                    : new AttributeValue { NULL = true }
            };
        }
    }
}