Welcome to Project Manager.

This solution exposes a secure API to perform CRUD operations of projects. It is structured using Clean Architecture and the database engine of choice is SqlServer.

• Security:
- The authentication token is secured using a pair of RSA keys that are provided in the repository for practicality.
- Only the password hash is stored in the database.

• Infrastructure:
- The solution is fully deployable with docker.
- The solution uses Entity Framework Core to handle migrations and to create and initialize the database.
- The solution contains a Pulumi project to set up AWS RDS for the database and AWS ECR for the docker containers.

• Testing:
- There is a Test project in the solution that contains unit tests for both Project and Security services.
- The API includes a health endpoint to check its status.

• Running the solution:
- Docker Desktop needs to be installed.
- Using bash in ./ProjectManager run the following command "docker compose up --build" to run the application.
- Once the containers are running, the API can be accessed with Postman.
- A Postman collection is included in the solution ProjectManager.postman_collection.json to facilitate testing the API.

• Misc:
The following test user is created when the database is initialized. 
{ 
	"username":"testuser", 
	"password":"Test@123" 
}