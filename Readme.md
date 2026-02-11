# 🌐 Translation Management Tool

A professional translation management system built with a **.NET 10** DDD-backend and a **Laravel 11** frontend. 

---

## 🛠 Tech Stack
- **Backend:** .NET 10, Entity Framework Core, OpenIddict (OIDC), PostgreSQL.
- **Frontend:** Laravel 11, Blade Templates, Bootstrap/Custom CSS.
- **Infrastructure:** Docker, Docker Compose.

---

## 🚀 Quick Start

### 1. Prerequisites
- [Docker Desktop](https://www.docker.com) installed and running.
- A terminal (Git Bash, PowerShell, or Linux Terminal).

### 2. Launch the Application
Use the provided automation script to build and start the environment:
```bash
# Start all services
./launch.sh up

# Stop all services
./launch.sh down

## 🔐 Authentication

Once the application is running, go to the frontend: [http://localhost:8000](http://localhost:8000)

**Default Admin Credentials:**
*   **Username:** `admin@test.com`
*   **Password:** `Password123!`

---

## 🛠 API Testing (Swagger)

Interactive API documentation and testing are available at:
[http://localhost:8080/swagger](http://localhost:8080/swagger)

**How to test protected routes:**
1.  Navigate to the **Account** section.
2.  Execute `POST /api/account/login` with the credentials above to establish a session.
3.  For **Translation** endpoints, click the **Authorize** button and provide your `access_token`.

---

## 🧪 Unit Tests

To execute the **NUnit** test suite, run the following command from the root directory:

**Run all tests**
```bash
dotnet test
