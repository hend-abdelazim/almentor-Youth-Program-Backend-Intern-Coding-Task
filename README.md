# Task Management REST API

Production-ready Task Management REST API built with ASP.NET Core 8, Entity Framework Core, SQL Server, and JWT Authentication.

---

## 1. Project Overview

This is a complete, production-quality Task Management System implemented as a REST API. It provides multi-user support with JWT authentication, per-user project and task isolation, filtering, sorting, pagination, search, and soft-delete functionality.

### Key Highlights
- Clean Architecture (Domain → Application → Infrastructure → API layers)
- JWT Authentication & Authorization with per-user ownership (IDOR protection)
- Entity Framework Core with SQL Server and Migrations
- Global Query Filters for soft deletes
- Advanced querying: filtering, sorting, pagination, case-insensitive search
- Consistent structured error responses with ProblemDetails
- 48 Unit Tests + 15 Integration Tests
- Postman collection and PowerShell convenience scripts

---

## 2. Features

✅ **Authentication**
- User registration with secure PBKDF2 password hashing
- JWT login with configurable token expiration
- Protected endpoints with 401/403 responses

✅ **Projects**
- CRUD operations for Projects
- Unique project name *per user*
- Paginated listing
- Soft-delete with automatic cascade to tasks

✅ **Tasks**
- CRUD operations for Tasks (nested under Projects)
- Cross-project task queries with project names included
- Status: `todo`, `in_progress`, `done`
- Priority: `low`, `medium`, `high`
- Due date validation (no past dates)
- Status transition logging (e.g. `done → todo`)

✅ **Filtering, Sorting, Pagination, Search**
- Filters: `status`, `priority`, `due_date_from`, `due_date_to`
- Sorting: `due_date`, `priority`, `created_at` (asc/desc)
- Pagination: `page` + `limit` with metadata (`totalCount`, `totalPages`)
- Case-insensitive partial search: `?q=` across title & description

✅ **Soft Deletes**
- `DeletedAt` nullable timestamps
- EF Core global query filters exclude deleted records
- Deleting a project automatically soft-deletes its tasks

✅ **Data Integrity**
- Unique constraints (username, email, project name per owner)
- Proper FK relationships, cascade delete behavior for Tasks
- Optimized indexes for owner lookups, status/priority/due-date filtering, search

---

## 3. Technology Stack

| Layer                | Technologies                                                                 |
|----------------------|-----------------------------------------------------------------------------|
| Runtime              | .NET 8, C# 12                                                               |
| Web Framework        | ASP.NET Core Web API, Controllers                                            |
| ORM                  | Entity Framework Core 8 (`Microsoft.EntityFrameworkCore.SqlServer`)         |
| Database             | Microsoft SQL Server (LocalDB / Express / Full)                             |
| Migrations           | EF Core CLI Migrations                                                       |
| Authentication       | JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`)                |
| Validation           | FluentValidation 11                                                          |
| Unit Tests           | xUnit + Moq                                                                  |
| Integration Tests    | xUnit + `WebApplicationFactory<Program>` + In-Memory EF Core provider       |
| Documentation        | Swagger / Swashbuckle 6.5, Postman Collection                                |

---

## 4. Architecture Overview

A classic **Clean Architecture** approach with clear separation of concerns.

```
Task Management REST API/
├── src/
│   ├── TaskManagement.Domain/          ← Core business entities, enums, interfaces
│   ├── TaskManagement.Application/     ← DTOs, validators, services, use cases
│   ├── TaskManagement.Infrastructure/  ← EF Core, repositories, auth, seed
│   └── TaskManagement.Api/             ← Controllers, middleware, Program.cs
├── tests/
│   ├── TaskManagement.UnitTests/       ← Validators, services, password hashing
│   └── TaskManagement.IntegrationTests/← HTTP-level WebApplicationFactory tests
├── postman/                             ← Collection + Environment
├── scripts/                             ← PowerShell convenience scripts
└── README.md
```

## 5. Project Structure

```
Domain/
├── Common/            BaseEntity
├── Entities/          User, Project, TaskItem
├── Enums/             TaskStatus (Todo/InProgress/Done), TaskPriority (Low/Medium/High)
├── Interfaces/        IUnitOfWork, IUserRepository, IProjectRepository, ITaskRepository
└── QueryParameters/   ProjectQueryParameters, TaskQueryParameters, PagedResult<T>

Application/
├── DTOs/
│   ├── Auth/          RegisterRequestDto, LoginRequestDto, AuthResponseDto
│   ├── Projects/      Create/Update/ProjectResponseDto
│   ├── Tasks/         Create/Update/TaskResponseDto
│   └── Common/        PagedResponseDto<T>
├── Exceptions/        NotFound, Forbidden, Unauthorized, Duplicate, Validation
├── Interfaces/        IAuthService, IProjectService, ITaskService, IJwtTokenService, IPasswordHasher
├── Mappings/          MappingExtensions (Entity → DTO, PagedResult → PagedResponse)
├── Services/          AuthService, ProjectService, TaskService
├── Validators/        FluentValidation validators for every request DTO
└── DependencyInjection.cs  → builder.Services.AddApplicationServices()

Infrastructure/
├── Auth/              JwtTokenService, PasswordHasher (PBKDF2 + salt), JwtSettings
├── Persistence/
│   ├── AppDbContext
│   ├── Configurations/  User/Project/TaskItem EF Core configs, indexes, filters
│   └── Migrations/      EF Core migration files
├── Repositories/      UserRepository, ProjectRepository, TaskRepository
├── UnitOfWork/        UnitOfWork (Implements IUnitOfWork)
├── Seed/              DatabaseSeeder (sample user, 2 projects, 6 tasks)
└── DependencyInjection.cs → builder.Services.AddInfrastructureServices(config)

Api/
├── Controllers/       AuthController, ProjectsController, TasksController
├── Middleware/        GlobalExceptionHandlerMiddleware
├── Program.cs         Swagger, CORS, Auth, DI, auto-migrate, seed
├── appsettings.json   Connection string, JWT settings, logging
└── appsettings.Development.json
```

---

## 6. Prerequisites

| Tool          | Minimum Version | Check with                 |
|---------------|-----------------|----------------------------|
| .NET SDK      | 8.0.x           | `dotnet --version`         |
| SQL Server    | LocalDB 2019+, Express, or Developer Edition | `sqlcmd` / SSMS |
| EF Core Tools | 8.0.0           | `dotnet ef --version`      |
| PowerShell    | 5.1 or 7+       | `$PSVersionTable.PSVersion` |

To install EF Core tools globally (if missing):
```powershell
dotnet tool install --global dotnet-ef
```

---

## 7. Installation & Configuration

### 7.1 Clone & Restore

```powershell
git clone <your-repo-url>
cd "Task Management REST API"
dotnet restore TaskManagement.sln
```

### 7.2 Connection String

Update **both** `appsettings.json` and `appsettings.Development.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
}
```

For a full SQL Server instance:
```
Server=.\SQLEXPRESS;Database=TaskManagementDb;Integrated Security=True;TrustServerCertificate=True;
Server=localhost;Database=TaskManagementDb;User Id=sa;Password=YourStrong!Passw0rd;TrustServerCertificate=True;
```

### 7.3 JWT Settings

```json
"JwtSettings": {
  "SecretKey": "CHANGE_ME_TO_AT_LEAST_32_CHARACTERS_SECRET_KEY_1234567890",
  "Issuer": "TaskManagement.Api",
  "Audience": "TaskManagement.Api.Clients",
  "ExpiresInMinutes": 60
}
```

⚠️ **Never commit real secrets to source control.** Use User Secrets in development:
```powershell
cd src/TaskManagement.Api
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:SecretKey" "YOUR_REAL_SECRET_HERE_AT_LEAST_32_BYTES"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Your-Connection-String"
```

---

## 8. Database Setup

### 8.1 Apply Existing Migrations (Recommended)

```powershell
# Using the convenience script:
.\scripts\migrate.ps1 -Apply -MigrationName InitialCreate

# Or manually:
cd src/TaskManagement.Api
dotnet ef database update --startup-project . --project ../TaskManagement.Infrastructure --context AppDbContext
```

### 8.2 Create a New Migration

```powershell
.\scripts\migrate.ps1 -MigrationName AddNewFeature
# Or:
dotnet ef migrations add AddNewFeature --startup-project src/TaskManagement.Api --project src/TaskManagement.Infrastructure --context AppDbContext --output-dir Persistence/Migrations
```

Migrations are stored in `src/TaskManagement.Infrastructure/Persistence/Migrations/`.

---

## 9. Seed Data

When running the API in the **Development** environment, the application will:
1. Apply any pending migrations automatically
2. Seed demo data if the Users table is empty

### 9.1 Demo Credentials (Development Only)
| Field     | Value                |
|-----------|----------------------|
| Username  | `demouser`           |
| Email     | `demo@example.com`   |
| Password  | `DemoPass123!`       |

### 9.2 Seeded Sample Data
- 1 Demo User
- 2 Projects: **Website Redesign**, **Mobile App Development**
- 6 Tasks across both projects (various statuses, priorities, due dates)

### 9.3 Manual Seed Script

```powershell
.\scripts\seed.ps1
```
This applies migrations and then runs the API in Development mode (which triggers the seed logic). Press `Ctrl+C` once you see `Now listening on: http://...` to stop.

---

## 10. Running the Application

### 10.1 Convenience Script

```powershell
.\scripts\run.ps1
# HTTPS profile:
.\scripts\run.ps1 -LaunchProfile https
```

### 10.2 Manual

```powershell
dotnet build TaskManagement.sln -c Debug
dotnet run --project src/TaskManagement.Api/TaskManagement.Api.csproj --launch-profile http
# or
dotnet run --project src/TaskManagement.Api/TaskManagement.Api.csproj --launch-profile https
```

Default URLs:
| Profile  | URL                         |
|----------|-----------------------------|
| http     | http://localhost:5099       |
| https    | https://localhost:7069      |
| Swagger  | `/swagger` (Dev only)       |

---

## 11. API Documentation

All endpoints return JSON and consume JSON (`application/json`).

### 11.1 Authentication

> ⚠️ **All /api/projects and /api/tasks endpoints require a valid Bearer JWT token.**

---

#### 🔐 POST `/api/auth/register`
Register a new user account.

**Request Body:**
```json
{ "username": "john_doe", "email": "john@example.com", "password": "Pass123!", "confirmPassword": "Pass123!" }
```

| Field           | Rules                                |
|-----------------|--------------------------------------|
| `username`      | required, 3–50 chars                 |
| `email`         | required, valid email, unique        |
| `password`      | required, ≥6 chars                   |
| `confirmPassword` | must equal `password`              |

**Success (201 Created):**
```json
{ "token": "eyJhbGciOiJ...", "expiresAt": "2026-07-26T12:00:00Z", "userId": "GUID", "username": "john_doe", "email": "john@example.com" }
```

**Error Responses:** 422 (validation), 409 (duplicate username/email)

---

#### 🔐 POST `/api/auth/login`
Authenticate and obtain a JWT token.

**Request Body:**
```json
{ "usernameOrEmail": "john_doe", "password": "Pass123!" }
```

**Success (200 OK):** Same shape as register response.

**Error Responses:** 422 (validation), 401 (invalid credentials)

---

### 11.2 Projects (All require authentication)

Every project is scoped to its owner via JWT claims. Attempting to access another user's project returns **403 Forbidden**.

---

#### ✏️ POST `/api/projects`
Create a new project for the authenticated user.

**Body:**
```json
{ "name": "Website Redesign", "description": "Modernize the marketing site (optional)" }
```
| Field         | Rules                          |
|---------------|--------------------------------|
| `name`        | required, 1–200 chars, **unique per owner** |
| `description` | optional, ≤2000 chars         |

**Success (201 Created):**
```json
{ "id": "GUID", "name": "Website Redesign", "description": "...", "ownerId": "GUID", "createdAt": "...", "updatedAt": "..." }
```
Location header points to `GET /api/projects/{id}`.

**Errors:** 401, 409 (duplicate name), 422

---

#### 📄 GET `/api/projects?page=1&limit=10`
List projects for the authenticated user, paginated.

**Query Params:**
| Param   | Default | Range      |
|---------|---------|------------|
| `page`  | 1       | ≥1         |
| `limit` | 10      | 1–100      |

**Success (200 OK):**
```json
{
  "items": [ { "id": "...", "name": "...", "description": "...", "ownerId": "...", "createdAt": "...", "updatedAt": "..." } ],
  "page": 1, "limit": 10, "totalCount": 24, "totalPages": 3
}
```

---

#### 📄 GET `/api/projects/{id}`
Get a single project by ID. Returns **404** if not found, **403** if owned by another user.

---

#### ✏️ PUT `/api/projects/{id}`
Update project. Same body/validation as create. Returns 200 with updated project.

---

#### 🗑️ DELETE `/api/projects/{id}`
**Soft-delete** a project AND all its tasks (cascade soft-delete). Returns **204 No Content**.

Deleted records are invisible to normal queries but remain in the database (`DeletedAt` column is set).

---

### 11.3 Tasks (All require authentication)

Tasks always belong to exactly one Project. Authorization is enforced at Project level (OwnerId from JWT).

---

#### ✏️ POST `/api/projects/{projectId}/tasks`
Create a task under the specified project.

**Body:**
```json
{
  "title": "Build registration page",
  "description": "Optional description (≤5000 chars)",
  "status": "todo",
  "priority": "medium",
  "dueDate": "2026-12-31T00:00:00Z"
}
```

| Field         | Rules                                                                  |
|---------------|------------------------------------------------------------------------|
| `title`       | required, 1–500 chars                                                  |
| `description` | optional, ≤5000 chars                                                  |
| `status`      | `Todo` / `InProgress` / `Done` (default: `Todo`)                      |
| `priority`    | `Low` / `Medium` / `High` (default: `Medium`)                         |
| `dueDate`     | optional, **today or future** (UTC). Past dates return a 422.         |

**Success (201 Created):**
```json
{
  "id": "GUID", "projectId": "GUID", "projectName": "Website Redesign",
  "title": "...", "description": "...", "status": "todo", "priority": "medium",
  "dueDate": "2026-12-31T00:00:00Z", "createdAt": "...", "updatedAt": "..."
}
```

**Errors:** 401, 403 (not your project), 404 (no such project), 422

---

#### 📄 GET `/api/projects/{projectId}/tasks`
Get all tasks belonging to a specific project. Supports **filtering, sorting, pagination**.

Query params: `page`, `limit`, `status`, `priority`, `due_date_from`, `due_date_to`, `sort_by`, `sort_direction`.

---

#### 📄 GET `/api/tasks`
Get **all tasks across the authenticated user's projects**, including project names. Supports **everything**:
- Filtering: `status`, `priority`, `due_date_from`, `due_date_to`
- Sorting: `sort_by=duration|priority|created_at` + `sort_direction=asc|desc`
- Pagination: `page`, `limit`
- **Search**: `?q=` across title and description (case-insensitive, partial match)

Every task in `items` includes the `projectName` field (no N+1 queries — single SQL with JOIN).

---

#### 📄 GET `/api/tasks/{id}`
Single task by ID. 404 if missing, 403 if not your task.

---

#### ✏️ PUT `/api/tasks/{id}`
Update task. Same body/validation as create (note: `status`/`priority` required, nullable only on create).

⚠️ Unusual transitions like **`done → todo`** are allowed and logged at Warning level.

---

#### 🗑️ DELETE `/api/tasks/{id}`
Soft-delete a single task (204 No Content).

---

## 12. Filtering / Sorting / Pagination / Search Reference

Available on:
- `GET /api/tasks` (all features)
- `GET /api/projects/{id}/tasks` (filter + sort + pagination)
- `GET /api/projects` (pagination only)

### 12.1 Filters

| Query Parameter  | Type     | Example                          |
|------------------|----------|----------------------------------|
| `status`         | enum     | `?status=Todo`                   |
| `priority`       | enum     | `?priority=High`                 |
| `due_date_from`  | DateTime | `?due_date_from=2026-01-01`      |
| `due_date_to`    | DateTime | `?due_date_to=2026-12-31`        |

Combine freely: `GET /api/tasks?status=InProgress&priority=High&due_date_from=2026-07-01`

### 12.2 Sorting

| Query Parameter  | Allowed Values                                       |
|------------------|------------------------------------------------------|
| `sort_by`        | `due_date`, `priority`, `created_at`                 |
| `sort_direction` | `asc` (default) or `desc`                            |

Example: `GET /api/tasks?sort_by=due_date&sort_direction=asc`

### 12.3 Pagination

| Param   | Default | Min | Max |
|---------|---------|-----|-----|
| `page`  | 1       | 1   | —   |
| `limit` | 10      | 1   | 100 |

Every paginated response returns:
```json
{ "items": [...], "page": 1, "limit": 10, "totalCount": 47, "totalPages": 5 }
```

### 12.4 Search

Only on `GET /api/tasks`: `?q=<search term>`

- Case-insensitive
- Partial (contains) match across **title AND description**
- Combinable with filters/sort/pagination: `?q=backend&status=Todo&page=1&limit=5`

---

## 13. Validation Rules

| Rule                                                         | HTTP Code |
|--------------------------------------------------------------|-----------|
| Empty / too-long task title                                  | 422       |
| Empty project name                                           | 422       |
| Project name already exists for THIS user                    | 409       |
| Username already exists                                      | 409       |
| Email already exists                                         | 409       |
| Register: password mismatch                                  | 422       |
| Due date in the past (create/update task)                    | 422       |
| Invalid status/priority enum value                           | 422/400   |
| Request to resource that doesn't exist (project, task)       | 404       |
| Request to another user's resource                           | 403       |
| No token / expired / invalid JWT                             | 401       |
| Wrong credentials on login                                   | 401       |

---

## 14. Authentication & Authorization

### 14.1 How it works
1. User registers → password hashed with **PBKDF2 (100,000 iters, SHA-256, 16-byte salt)**.
2. User logs in → JWT issued with claims: `sub`, `uid`, `name`, `email`, `jti`.
3. Every protected controller extracts `uid` from claims (NOT from client payload) → **IDOR safe**.
4. Services always compare entity's `OwnerId` / `Project.OwnerId` with the JWT user ID → returns 403 on mismatch.

### 14.2 Correct Usage

After login/register, include the token in every request header:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
```

Swagger: Click **Authorize** → type `Bearer <token>` and confirm.

---

## 15. Soft Deletes

Both `Projects` and `Tasks` tables have a nullable `DeletedAt` column.

- **EF Core global query filters** automatically exclude rows where `DeletedAt IS NOT NULL` from all queries.
- `DELETE /api/projects/{id}` sets `Projects.DeletedAt` AND calls `SoftDeleteByProjectIdAsync` to cascade to tasks.
- Deleted records are preserved in the database for compliance/auditing.
- Raw SQL or `.IgnoreQueryFilters()` can retrieve them (used in the integration test Flow 1 to confirm the cascade).

---

## 16. Database Schema

### 16.1 Users Table

| Column         | Type         | Constraints                                  |
|----------------|--------------|----------------------------------------------|
| `Id`           | UNIQUEIDENTIFIER | PK, clustered                            |
| `Username`     | NVARCHAR(50) | NOT NULL, UNIQUE INDEX `IX_Users_Username`  |
| `Email`        | NVARCHAR(256)| NOT NULL, UNIQUE INDEX `IX_Users_Email`     |
| `PasswordHash` | NVARCHAR(MAX)| NOT NULL                                     |
| `CreatedAt`    | DATETIME2    | NOT NULL                                     |

### 16.2 Projects Table

| Column        | Type         | Constraints                                                             |
|---------------|--------------|-------------------------------------------------------------------------|
| `Id`          | UNIQUEIDENTIFIER | PK                                                                  |
| `Name`        | NVARCHAR(200)| NOT NULL                                                                |
| `Description` | NVARCHAR(2000)| NULLABLE                                                                |
| `OwnerId`     | UNIQUEIDENTIFIER | FK → Users.Id, ON DELETE RESTRICT, INDEX `IX_Projects_OwnerId`    |
| `CreatedAt`   | DATETIME2    | NOT NULL                                                                |
| `UpdatedAt`   | DATETIME2    | NOT NULL                                                                |
| `DeletedAt`   | DATETIME2    | NULLABLE (soft delete)                                                  |
| **Constraint**| —            | UNIQUE INDEX `IX_Projects_OwnerId_Name (OwnerId, Name)` WHERE `DeletedAt IS NULL` (duplicate name protection) |

### 16.3 Tasks Table

| Column        | Type            | Constraints                                                  |
|---------------|-----------------|--------------------------------------------------------------|
| `Id`          | UNIQUEIDENTIFIER| PK                                                           |
| `ProjectId`   | UNIQUEIDENTIFIER| FK → Projects.Id, ON DELETE CASCADE, INDEX `IX_Tasks_ProjectId` |
| `Title`       | NVARCHAR(500)   | NOT NULL                                                     |
| `Description` | NVARCHAR(5000)  | NULLABLE                                                     |
| `Status`      | NVARCHAR(20)    | NOT NULL, INDEX `IX_Tasks_Status`                           |
| `Priority`    | NVARCHAR(20)    | NOT NULL, INDEX `IX_Tasks_Priority`                         |
| `DueDate`     | DATETIME2       | NULLABLE, INDEX `IX_Tasks_DueDate`                           |
| `CreatedAt`   | DATETIME2       | NOT NULL, INDEX `IX_Tasks_CreatedAt`                        |
| `UpdatedAt`   | DATETIME2       | NOT NULL                                                     |
| `DeletedAt`   | DATETIME2       | NULLABLE (soft delete)                                       |
| Search support| —               | INDEX `IX_Tasks_Search (Title, Description)`                |

### 16.4 Relationships
```
User 1───∞ Project 1───∞ TaskItem
         (OwnerId)         (ProjectId)
```

- `User → Project`: Restrict delete (prevent deleting a user with projects)
- `Project → TaskItem`: Cascade hard delete at DB level, but soft delete is handled by the app layer to preserve task rows.

---

## 17. Error Handling

A centralized **GlobalExceptionHandlerMiddleware** catches all exceptions and returns consistent `application/problem+json` per RFC 7807:

```json
{
  "type": null,
  "title": "Conflict",
  "status": 409,
  "detail": "Project with Name 'Existing' already exists.",
  "instance": "/api/projects"
}
```

### 17.1 HTTP Status Code Map

| Exception Type                 | Status | Title                |
|--------------------------------|--------|----------------------|
| `NotFoundException`            | 404    | Not Found            |
| `ForbiddenAccessException`     | 403    | Forbidden            |
| `UnauthorizedException`        | 401    | Unauthorized         |
| `DuplicateEntityException`     | 409    | Conflict             |
| `Application.ValidationException` | 422 | Validation Error     |
| `FluentValidation.ValidationException` | 422 | Validation Failed (includes per-field errors array) |
| Anything else                  | 500    | Internal Server Error |

⚠️ Production-style 500 responses DO NOT include stack traces or sensitive details.

---

## 18. Testing

Total: **63 tests (48 Unit + 15 Integration)**.

### 18.1 Run Tests

```powershell
# All tests
.\scripts\test.ps1
dotnet test TaskManagement.sln

# Only unit tests
.\scripts\test.ps1 -Type unit

# Only integration tests
.\scripts\test.ps1 -Type integration
```

### 18.2 Unit Tests Coverage

| File                              | What's tested                                                   |
|-----------------------------------|-----------------------------------------------------------------|
| `TaskValidationTests`             | Empty/long/valid task titles                                    |
| `DueDateValidationTests`          | Null/today/future/past due dates on create/update task          |
| `StatusValidationTests`           | Valid Todo/InProgress/Done, invalid enum                       |
| `PriorityValidationTests`         | Low/Medium/High valid                                           |
| `StatusTransitionTests`           | All 6 status-direction transitions (including done→todo warning logged) |
| `ProjectValidationTests`          | Empty/long name, long description, valid                       |
| `AuthValidationTests`             | Valid, short username, invalid email, short password, mismatch |
| `PasswordHasherTests`             | Different hashes per same input, correct verify, no plaintext, various inputs |

### 18.3 Integration Tests Coverage (InMemory DB + WebApplicationFactory)

Flow 1 (Lifecycle + Soft Delete):
- Register → Login → Create Project → Create Task → Mark Done → Delete Project → verify 404, verify not in lists, verify DB rows still exist with `DeletedAt != null`.

Flow 2 (Filtering):
- Creates 8 tasks across 3 statuses × 3 priorities → asserts correct counts and values for Todo, Done, High, Low, and combined InProgress+High filters.

Flow 3 (Search + Pagination + Sorting):
- Case-insensitive BACKEND/backend/BaCkEnD count matches
- Searches by title ("design") and description ("endpoints")
- Page 1/2/3/4 item counts, totalCount=7, totalPages=3
- Sort priority asc/desc order and due_date asc order

**Edge case integration tests (10+ scenarios):**
- Duplicate project name → 409
- Past due date → validation error (400/422)
- Invalid project ID → 404 across get/update/delete
- Invalid task ID → 404 across get/update/delete
- Unauthorized (no JWT) on every project/task endpoint → 401
- Access another user's project/task/create task/update/delete → 403
- `GET /api/tasks` includes `projectName` field
- Invalid registration input → 422
- Duplicate username on register → 409
- Wrong login credentials → 401
- Invalid priority/status enum values → 400/422

---

## 19. Postman

### 19.1 Import Postman Files

1. Open Postman
2. **File → Import**
3. Select both files from the `postman/` folder:
   - `TaskManagement.postman_collection.json`
   - `TaskManagement.postman_environment.json`
4. Confirm import.

### 19.2 Postman Configuration

1. From the environment dropdown (top-right), select **Task Management Dev Environment**.
2. Click the **eye / quick edit** icon next to it.
3. Verify/set `baseUrl` to match your running API (e.g. `https://localhost:7069` or `http://localhost:5099`).
4. Save.

### 19.3 Running Requests

1. Run **Authentication → Login** (edit the body to match your user, or use the seed credentials):
   ```json
   { "usernameOrEmail": "demouser", "password": "DemoPass123!" }
   ```
2. Postman test script auto-stores the returned `token` into the collection variable and the environment.
3. Run **Projects → Create Project** → script stores `projectId` var.
4. Run **Tasks → Create Task** → script stores `taskId` var.
5. All filtering, sorting, search endpoints are under the *Filtering, Sorting & Search* folder.

### 19.4 Postman Test Scripts

Every key request includes `pm.test(...)` that checks:
- HTTP status codes (200 / 201 / 204 where expected)
- Pagination metadata structure
- Auto-extraction of token, projectId, taskId into variables

---

## 20. Design Decisions

### 20.1 Why Clean Architecture?
Keeps EF Core / SQL Server / JWT implementation details (Infrastructure) isolated from the business rules (Application) and domain model (Domain). Allowed me to swap the database in tests from SQL Server → InMemory without touching any service code.

### 20.2 Why Soft-Deletes via Global Query Filters?
EF Core global query filters are the cleanest way to guarantee deleted rows never leak into normal queries. They are applied **automatically** to every LINQ query on `AppDbContext`, so it's impossible for a developer to "forget" a `Where(x => x.DeletedAt == null)` check.

### 20.3 How Are N+1 Queries Avoided?
- Task queries use `.Include(t => t.Project)` to eagerly load the project name in a **single JOIN**.
- `GetAllByOwnerIdAsync` uses one LINQ query with `Where(t => t.Project.OwnerId == ownerId)` — that's one trip to the DB for any number of tasks.
- No lazy loading proxies enabled.

### 20.4 How Is Validation Handled?
Two-tiered:
1. **FluentValidation** runs in controllers before any business logic (model validation, enum values, due-date rules, string lengths).
2. **Application-level rules** in the services layer: duplicate names, ownership checks (403), not-found (404), past due date guard.

### 20.5 How Are Ownership & IDOR Prevented?
- Controllers **never read OwnerId from the request body**.
- User ID is always extracted from JWT claims: `User.FindFirstValue("uid")`.
- Every service method accepts both `(entityId, userId)` and loads the entity + project owner via `Include` before doing any operation. If `OwnerId != userId` → **403**.
- Global `/api/tasks` listing is automatically scoped via `t.Project.OwnerId == userId` filter — even if a user guesses a task GUID, they can only see/act on their own.

---

## 21. Quick Start (Under 10 Minutes)

```powershell
# 1. Open PowerShell
cd "Task Management REST API"

# 2. Verify tools
dotnet --version   # 8.0.x
dotnet ef --version # 8.0.x

# 3. Restore
dotnet restore TaskManagement.sln

# 4. Edit appsettings.Development.json if needed (LocalDB default usually works)
notepad src\TaskManagement.Api\appsettings.Development.json

# 5. Create + apply database, then seed
.\scripts\migrate.ps1 -Apply -MigrationName TempSeedRun  # if needed
# OR just:
.\scripts\seed.ps1    # (Ctrl+C once API is listening)

# 6. Run tests
.\scripts\test.ps1 -Type all   # → 63/63 passed

# 7. Run API
.\scripts\run.ps1

# 8. Browser opens at Swagger UI:
#    http://localhost:5099/swagger
# 9. Login via Auth/Login endpoint (demouser / DemoPass123!)
# 10. Copy the token → Swagger "Authorize" button → Bearer <token>
# 11. Explore!
```

---
