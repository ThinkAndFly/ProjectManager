using Pulumi;
using Pulumi.Aws.Ec2;
using Pulumi.Aws.Ec2.Inputs;
using Pulumi.Aws.Rds;

class MyStack : Stack
{
    public MyStack()
    {
        // Example: security group (very simplified)
        var sg = new SecurityGroup("db-sg", new SecurityGroupArgs
        {
            Description = "Security group for SQL Server RDS",
            Ingress =
            {
                new SecurityGroupIngressArgs
                {
                    FromPort = 1433,
                    ToPort = 1433,
                    Protocol = "tcp",
                    CidrBlocks = { "0.0.0.0/0" } // demo only
                }
            }
        });

        var db = new Pulumi.Aws.Rds.Instance("projects-db", new Pulumi.Aws.Rds.InstanceArgs
        {
            Engine = "sqlserver-ex",
            InstanceClass = "db.t3.micro",
            AllocatedStorage = 20,
            Username = "sa_user",
            Password = "SomeStrongPassword1!",
            VpcSecurityGroupIds = { sg.Id },
            SkipFinalSnapshot = true,
        });

        this.DbEndpoint = db.Endpoint;
    }

    [Output]
    public Output<string> DbEndpoint { get; set; }
}