# Postman Collection for Kaopiz Auth API

This directory contains the Postman collection for testing the Kaopiz Auth API.

## 📦 Collection Files

- **kaopiz-auth-api.postman_collection.json** - Main collection with all API endpoints
- **kaopiz-auth-environments.json** - Environment configurations

## 🚀 Quick Setup

### 1. Import Collection
1. Open Postman
2. Click **Import** button
3. Select `kaopiz-auth-api.postman_collection.json`
4. Click **Import**

### 2. Import Environment
1. In Postman, click **Environments** tab
2. Click **Import** button
3. Select `kaopiz-auth-environments.json`
4. Select the appropriate environment (Development/Staging/Production)

### 3. Configure Variables
Update the environment variables:
- `baseUrl` - API base URL (e.g., `https://localhost:7001`)
- `accessToken` - Will be auto-populated after login
- `refreshToken` - Will be auto-populated after login

## 📋 Available Endpoints

### Authentication
- **POST** `/api/auth/register` - Register new user
- **POST** `/api/auth/login` - User login
- **POST** `/api/auth/refresh` - Refresh access token
- **POST** `/api/auth/logout` - User logout

### User Management
- **GET** `/api/users/me` - Get current user profile
- **PUT** `/api/users/me` - Update user profile

### Health Checks
- **GET** `/health` - API health check
- **GET** `/api/auth/health` - Auth service health

## 🔄 Testing Flow

### Recommended Testing Sequence:
1. **Register User** - Create a new account
2. **Login User** - Authenticate and get tokens
3. **Get Current User** - Verify authentication works
4. **Update Profile** - Test profile updates
5. **Refresh Token** - Test token refresh
6. **Logout User** - Clean logout

### Automated Tests
Each request includes automated tests that verify:
- ✅ Correct HTTP status codes
- ✅ Response structure validation
- ✅ Token management
- ✅ Data integrity

## 🎯 Test Scenarios

### Happy Path Testing
```
Register → Login → Get Profile → Update Profile → Refresh Token → Logout
```

### Error Handling Testing
- Invalid credentials
- Expired tokens
- Malformed requests
- Rate limiting

### Security Testing
- Token validation
- Authorization checks
- Input validation

## 📊 Environment Configurations

### Development
```json
{
  "baseUrl": "https://localhost:7001",
  "environment": "development"
}
```

### Staging
```json
{
  "baseUrl": "https://staging-api.kaopiz.com",
  "environment": "staging"
}
```

### Production
```json
{
  "baseUrl": "https://api.kaopiz.com",
  "environment": "production"
}
```

## 🔧 Advanced Usage

### Running Collection with Newman
```bash
# Install Newman CLI
npm install -g newman

# Run collection
newman run kaopiz-auth-api.postman_collection.json \
  -e development.postman_environment.json \
  --reporters html,cli \
  --reporter-html-export results.html
```

### CI/CD Integration
```yaml
# Example GitHub Actions step
- name: Run API Tests
  run: |
    newman run docs/tools/postman/kaopiz-auth-api.postman_collection.json \
      -e docs/tools/postman/environments/staging.json \
      --reporters junit \
      --reporter-junit-export test-results.xml
```

## 📚 Additional Resources

- [Postman Documentation](https://learning.postman.com/)
- [Newman CLI Guide](https://github.com/postmanlabs/newman)
- [API Documentation](../../api/reference.md)

---

For support or questions about this collection, please refer to the main [troubleshooting guide](../../operations/troubleshooting.md).