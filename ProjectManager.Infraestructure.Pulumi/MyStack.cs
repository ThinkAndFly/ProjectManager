using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;
using Pulumi.Aws.Ecr;
using Pulumi.Aws.Rds;
using Pulumi.Docker;
using Pulumi.Docker.Inputs;

public class MyStack : Stack
{
    public Output<string> RepoUrl { get; }
    public Output<string> ImageName { get; }
    public Output<string> DbAddress { get; }
    public Output<string> DbEndpoint { get; }
    public Output<string> DbConnectionString { get; }

    public MyStack()
    {
        var appConfig = new Pulumi.Config("projectmanager");

        var appName = appConfig.Get("appName") ?? "projectmanager";
        var environment = appConfig.Get("environment") ?? Deployment.Instance.StackName;
        var dockerContext = appConfig.Get("dockerContext") ?? "../ProjectManager";
        var dbName = appConfig.Get("dbName") ?? "ProjectManagerDb";
        var dbUsername = appConfig.Require("dbUsername");
        var dbPassword = appConfig.RequireSecret("dbPassword");

        var repo = new Repository($"{appName}-{environment}-repo", new RepositoryArgs
        {
            Tags =
            {
                { "app", appName },
                { "env", environment }
            }
        });

        var authToken = Output.Create(GetAuthorizationToken.InvokeAsync());

        var image = new Image($"{appName}-{environment}-image", new ImageArgs
        {
            ImageName = repo.RepositoryUrl.Apply(url => $"{url}:latest"),
            Build = new DockerBuildArgs
            {
                Context = dockerContext
            },
            Registry = new RegistryArgs
            {
                Server = repo.RepositoryUrl,
                Username = authToken.Apply(t => t.UserName),
                Password = authToken.Apply(t => t.Password)
            }
        });

        var vpc = Output.Create(GetVpc.InvokeAsync(new GetVpcArgs { Default = true }));
        var subnetIds = vpc.Apply(v =>
            GetSubnets.InvokeAsync(new GetSubnetsArgs
            {
                Filters =
                {
                    new GetSubnetsFilterArgs
                    {
                        Name = "vpc-id",
                        Values = { v.Id }
                    }
                }
            })).Apply(s => s.Ids);

        var dbSecurityGroup = new SecurityGroup($"{appName}-{environment}-db-sg", new SecurityGroupArgs
        {
            VpcId = vpc.Apply(v => v.Id),
            Ingress =
            {
                new SecurityGroupIngressArgs
                {
                    Protocol = "tcp",
                    FromPort = 1433,
                    ToPort = 1433,
                    CidrBlocks = { vpc.Apply(v => v.CidrBlock) }
                }
            },
            Egress =
            {
                new SecurityGroupEgressArgs
                {
                    Protocol = "-1",
                    FromPort = 0,
                    ToPort = 0,
                    CidrBlocks = { "0.0.0.0/0" }
                }
            },
            Tags =
            {
                { "app", appName },
                { "env", environment }
            }
        });

        var dbSubnetGroup = new SubnetGroup($"{appName}-{environment}-db-subnets", new SubnetGroupArgs
        {
            SubnetIds = subnetIds,
            Tags =
            {
                { "app", appName },
                { "env", environment }
            }
        });

        var dbInstance = new Pulumi.Aws.Rds.Instance($"{appName}-{environment}-db", new Pulumi.Aws.Rds.InstanceArgs
        {
            AllocatedStorage = appConfig.GetInt32("dbAllocatedStorage") ?? 20,
            Engine = "sqlserver-ex",
            EngineVersion = "15.00",
            InstanceClass = appConfig.Get("dbInstanceClass") ?? "db.t3.micro",
            DbName = dbName,
            Username = dbUsername,
            Password = dbPassword,
            DbSubnetGroupName = dbSubnetGroup.Name,
            VpcSecurityGroupIds = { dbSecurityGroup.Id },
            PubliclyAccessible = false,
            StorageEncrypted = true,
            BackupRetentionPeriod = appConfig.GetInt32("dbBackupRetentionDays") ?? 7,
            DeletionProtection = appConfig.GetBoolean("dbDeletionProtection") ?? false,
            SkipFinalSnapshot = true,
            Tags =
            {
                { "app", appName },
                { "env", environment }
            }
        });

        var connectionString = Output.Tuple(dbInstance.Address, dbPassword).Apply(tuple =>
            $"Server={tuple.Item1};Database={dbName};User ID={dbUsername};Password={tuple.Item2};Encrypt=True;TrustServerCertificate=True;");

        RepoUrl = repo.RepositoryUrl;
        ImageName = image.ImageName;
        DbAddress = dbInstance.Address;
        DbEndpoint = dbInstance.Endpoint;
        DbConnectionString = Output.CreateSecret(connectionString);
    }
}
