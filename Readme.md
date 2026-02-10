Translation Tool
A translation management system featuring a .NET 10 Backend (DDD Architecture) with OpenIddict and a Laravel Frontend.

🚀 How to Run
Use the control script in your terminal (Git Bash or Linux) to manage the application lifecycle(use root folder):
Start the Application
Builds images and starts all services (PostgreSQL, .NET API, Laravel):
bash
./launch.sh up

Stop the Application
Stops and removes all running containers:
bash
./launch.sh down
Use code with caution.

🔐 Authentication
Once the application is running, go to the frontend: http://localhost:8000
Default Admin Credentials:
Username: admin@test.com
Password: Password123!

🛠 API Testing (Swagger)
Interactive API documentation and testing are available at:
http://localhost:8080/swagger

How to test protected routes:
Navigate to the Account section.
Execute POST /api/account/login with the credentials above to establish a session.
For Translation endpoints, click the Authorize button and provide your access_token.

🧪 Unit Tests
To execute the NUnit test suite, run the following command from the root directory:
Run all tests
bash
dotnet test