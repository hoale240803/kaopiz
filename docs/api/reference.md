# API Reference

Complete reference for the Kaopiz Auth API endpoints.

## Base Information

- **Base URL**: `https://localhost:7001` (development)
- **API Version**: v1
- **Content-Type**: `application/json`
- **Authentication**: Bearer JWT Token

## Authentication Endpoints

### 🔐 Login
Authenticate a user and receive access tokens.

**Endpoint**: `POST /api/auth/login`

**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6",
    "expiresAt": "2025-08-21T12:15:00Z",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "roles": ["User"]
    }
  },
  "errors": []
}
```

**Error Response** (400 Bad Request):
```json
{
  "success": false,
  "message": "Invalid email or password",
  "data": null,
  "errors": [
    {
      "field": "credentials",
      "message": "Invalid email or password"
    }
  ]
}
```

**curl Example**:
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'
```

### 🔄 Refresh Token
Refresh an expired access token using a refresh token.

**Endpoint**: `POST /api/auth/refresh`

**Request Body**:
```json
{
  "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Token refreshed successfully",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "c9d0e1f2g3h4i5j6k7l8m9n0o1p2q3r4s5t6u7v8w9x0y1z2a3",
    "expiresAt": "2025-08-21T12:30:00Z"
  },
  "errors": []
}
```

**curl Example**:
```bash
curl -X POST "https://localhost:7001/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"
  }'
```

### 🚪 Logout
Logout user and invalidate refresh token.

**Endpoint**: `POST /api/auth/logout`

**Headers**:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Request Body**:
```json
{
  "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Logout successful",
  "data": null,
  "errors": []
}
```

**curl Example**:
```bash
curl -X POST "https://localhost:7001/api/auth/logout" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"
  }'
```

## User Management Endpoints

### 👤 Get Current User
Get the currently authenticated user's profile.

**Endpoint**: `GET /api/users/me`

**Headers**:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "User profile retrieved successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "roles": ["User"],
    "isActive": true,
    "createdAt": "2025-01-01T10:00:00Z",
    "lastLoginAt": "2025-08-21T10:00:00Z"
  },
  "errors": []
}
```

**curl Example**:
```bash
curl -X GET "https://localhost:7001/api/users/me" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
```

### ✏️ Update Profile
Update the current user's profile information.

**Endpoint**: `PUT /api/users/me`

**Headers**:
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Request Body**:
```json
{
  "firstName": "John",
  "lastName": "Smith"
}
```

**Response** (200 OK):
```json
{
  "success": true,
  "message": "Profile updated successfully",
  "data": {
    "id": "123e4567-e89b-12d3-a456-426614174000",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Smith",
    "fullName": "John Smith",
    "roles": ["User"],
    "isActive": true
  },
  "errors": []
}
```

**curl Example**:
```bash
curl -X PUT "https://localhost:7001/api/users/me" \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..." \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Smith"
  }'
```

## System Endpoints

### 🏥 Health Check
Check the health status of the API.

**Endpoint**: `GET /health`

**Response** (200 OK):
```json
{
  "status": "Healthy",
  "timestamp": "2025-08-21T10:00:00Z",
  "version": "1.0.0",
  "environment": "Development",
  "database": "Connected",
  "uptime": "02:15:30"
}
```

**curl Example**:
```bash
curl -X GET "https://localhost:7001/health"
```

## Response Format

All API responses follow a consistent format:

### Success Response
```json
{
  "success": true,
  "message": "Description of the operation",
  "data": {
    // Response data here
  },
  "errors": []
}
```

### Error Response
```json
{
  "success": false,
  "message": "Error description",
  "data": null,
  "errors": [
    {
      "field": "fieldName",
      "message": "Specific error message"
    }
  ]
}
```

## HTTP Status Codes

| Code | Description |
|------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 409 | Conflict |
| 422 | Validation Error |
| 429 | Too Many Requests |
| 500 | Internal Server Error |

## Rate Limiting

The API implements rate limiting to prevent abuse:

- **Login endpoint**: 5 requests per minute per IP
- **Other endpoints**: 100 requests per minute per user
- **Global**: 1000 requests per minute per IP

Rate limit headers are included in responses:
```
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 95
X-RateLimit-Reset: 1629884400
```

## Error Handling

### Validation Errors (422)
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    {
      "field": "email",
      "message": "Email is required"
    },
    {
      "field": "password",
      "message": "Password must be at least 8 characters"
    }
  ]
}
```

### Authentication Errors (401)
```json
{
  "success": false,
  "message": "Authentication required",
  "data": null,
  "errors": [
    {
      "field": "token",
      "message": "Invalid or expired token"
    }
  ]
}
```

## SDK Examples

### JavaScript/TypeScript
```typescript
interface LoginRequest {
  email: string;
  password: string;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: Array<{ field: string; message: string }>;
}

class KaopizAuthClient {
  private baseUrl = 'https://localhost:7001';
  private accessToken: string | null = null;

  async login(credentials: LoginRequest): Promise<ApiResponse<LoginResponse>> {
    const response = await fetch(`${this.baseUrl}/api/auth/login`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(credentials)
    });
    
    const result = await response.json();
    if (result.success) {
      this.accessToken = result.data.accessToken;
    }
    return result;
  }

  async getCurrentUser(): Promise<ApiResponse<User>> {
    const response = await fetch(`${this.baseUrl}/api/users/me`, {
      headers: { 'Authorization': `Bearer ${this.accessToken}` }
    });
    return response.json();
  }
}
```

### C# (.NET)
```csharp
public class KaopizAuthClient
{
    private readonly HttpClient _httpClient;
    private string? _accessToken;

    public KaopizAuthClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7001");
    }

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponse>>();
        
        if (result?.Success == true)
        {
            _accessToken = result.Data?.AccessToken;
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _accessToken);
        }
        
        return result;
    }

    public async Task<ApiResponse<User>> GetCurrentUserAsync()
    {
        var response = await _httpClient.GetAsync("/api/users/me");
        return await response.Content.ReadFromJsonAsync<ApiResponse<User>>();
    }
}
```

---

For more examples and detailed usage, see the [Usage Examples](./examples.md) guide.