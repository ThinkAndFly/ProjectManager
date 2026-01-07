using Amazon.DynamoDBv2.Model;
using ProjectManager.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ProjectManager.Infraestructure.Persistence.Dynamo
{
    public static class DynamoParseHelper
    {
        public static int ParseInt(IDictionary<string, AttributeValue> item, string key)
        {
            if (!item.TryGetValue(key, out var attribute))
            {
                return 0;
            }

            var raw = attribute.N ?? attribute.S ?? string.Empty;
            return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : 0;
        }

        public static string ParseString(IDictionary<string, AttributeValue> item, string key)
        {
            if (item.TryGetValue(key, out var attribute) && attribute.S is not null)
            {
                return attribute.S;
            }

            return string.Empty;
        }

        public static RoleEnum ParseRole(IDictionary<string, AttributeValue> item, string key)
        {
            if (!item.TryGetValue(key, out var attribute) || string.IsNullOrWhiteSpace(attribute.S))
            {
                return RoleEnum.User;
            }

            return Enum.TryParse<RoleEnum>(attribute.S, true, out var parsedRole)
                ? parsedRole
                : RoleEnum.User;
        }
    }
}
