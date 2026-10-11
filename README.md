# UserManagementApi

A simple user management REST API built with **ASP.NET Core Minimal API (.NET 10)**.
It supports full CRUD operations, input validation, request logging, and token-based authentication.
Developed with the help of GitHub Copilot.

## Features

- CRUD endpoints for managing users (GET, POST, PUT, DELETE)
- Validation: only valid user data is accepted (Name required, Email must be a valid format)
- Logging middleware: logs every request and response status code
- Authentication middleware: protects all endpoints with a bearer token
- In-memory data store (no database required)

## Project Structure

```
UserManagementApi/
├── Middleware/
│   ├── RequestLoggingMiddleware.cs
│   └── AuthenticationMiddleware.cs
├── Models/
│   └── User.cs
├── Program.cs
├── requests.http
└── README.md
```

## Getting Started

**Requirements:** .NET SDK 10 (check with `dotnet --version`)

```bash
git clone https://github.com/<your-username>/UserManagementApi.git
cd UserManagementApi
dotnet run
```

The console will show the URL the app is listening on, for example
`Now listening on: http://localhost:5000`.

## Authentication

Every endpoint except `GET /` requires this header:

```
Authorization: Bearer secret-token
```

Requests without a valid token receive `401 Unauthorized`.

> Note: the token is hard-coded for demonstration purposes only. A real application
> should load secrets from configuration and use a proper scheme such as JWT.

## Endpoints

| Method | Path          | Description        | Success | Errors        |
|--------|---------------|--------------------|---------|---------------|
| GET    | `/`           | Health check       | 200     | -             |
| GET    | `/users`      | Get all users      | 200     | 401           |
| GET    | `/users/{id}` | Get a user by id   | 200     | 401, 404      |
| POST   | `/users`      | Create a user      | 201     | 400, 401      |
| PUT    | `/users/{id}` | Update a user      | 200     | 400, 401, 404 |
| DELETE | `/users/{id}` | Delete a user      | 200     | 401, 404      |

### Request body (POST / PUT)

```json
{
  "name": "Charlie",
  "email": "charlie@example.com"
}
```

### Validation rules

- `name`: required, maximum 100 characters
- `email`: required, must be a valid email format

Invalid input returns `400 Bad Request` with details, for example:

```json
{
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Name": ["Name is required"],
    "Email": ["Email format is invalid"]
  }
}
```

## Testing the API

Open `requests.http` in VS Code with the **REST Client** extension and click
"Send Request". Update the `@host` variable to match your port.

Or use curl:

```bash
curl -H "Authorization: Bearer secret-token" http://localhost:5000/users
```

## Middleware

The pipeline order is important:

1. `RequestLoggingMiddleware` logs the method, path, and response status code
2. `AuthenticationMiddleware` rejects requests without a valid token
3. Endpoint handlers process the request
