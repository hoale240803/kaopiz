# Thunder Client Collection for Kaopiz Auth API

This directory contains the Thunder Client collection for testing the Kaopiz Auth API directly in VS Code.

## 📦 Collection Files

- **kaopiz-auth-api.json** - Thunder Client collection with all API endpoints and environments

## 🚀 Quick Setup

### 1. Install Thunder Client Extension
1. Open VS Code
2. Go to Extensions (Ctrl+Shift+X)
3. Search for "Thunder Client"
4. Install the extension by RangaV

### 2. Import Collection
1. Open Thunder Client in VS Code (Activity Bar → Thunder Client)
2. Click **Collections** tab
3. Click **Import** button (⚡ icon)
4. Select `kaopiz-auth-api.json`
5. Collection will be imported with all environments

### 3. Select Environment
1. In Thunder Client, click **Env** dropdown (top-right)
2. Select environment:
   - **Development** - For local testing
   - **Staging** - For staging environment
   - **Production** - For production testing

## 📋 Available Endpoints

### 🔐 Authentication Folder
- **Register User** - Create new user account
- **Login User** - Authenticate and get tokens  
- **Refresh Token** - Refresh expired access token
- **Logout User** - Logout and revoke tokens

### 👤 User Management Folder
- **Get Current User** - Retrieve user profile
- **Update User Profile** - Update user information

### 🏥 Health Checks Folder
- **API Health Check** - Check overall API health
- **Auth Service Health** - Check authentication service

## 🔄 Testing Workflow

### Recommended Testing Sequence:
```
1. Register User → 2. Login User → 3. Get Current User → 
4. Update Profile → 5. Refresh Token → 6. Logout User
```

### Automated Token Management
- **Login** automatically stores `accessToken` and `refreshToken`
- **Refresh** automatically updates tokens
- **Logout** automatically clears tokens
- All authenticated requests use stored tokens

## 📊 Environment Variables

### Development Environment
```json
{
  "baseUrl": "https://localhost:7001",
  "accessToken": "",
  "refreshToken": ""
}
```

### Staging Environment  
```json
{
  "baseUrl": "https://staging-api.kaopiz.com",
  "accessToken": "",
  "refreshToken": ""
}
```

### Production Environment
```json
{
  "baseUrl": "https://api.kaopiz.com", 
  "accessToken": "",
  "refreshToken": ""
}
```

## ✅ Automated Tests

Each request includes automated tests:

### Response Validation
- ✅ HTTP status code verification
- ✅ Success field validation  
- ✅ Response structure checks
- ✅ Data type validation

### Token Management
- ✅ Automatic token extraction and storage
- ✅ Token cleanup on logout
- ✅ Authorization header management

### Example Test Assertions
```javascript
// Status code test
res.status: 200

// JSON response test
json.success: true

// Data type test  
json.data.userId: string

// Environment variable update
json.data.accessToken → accessToken
```

## 🎯 Advanced Features

### Pre-Request Scripts
Thunder Client supports JavaScript for dynamic request handling:

```javascript
// Set dynamic timestamp
tc.setVar("timestamp", Date.now());

// Generate UUID
tc.setVar("requestId", tc.guid());
```

### Response Processing
Automatic variable extraction from responses:

```javascript
// Extract and store token
json.data.accessToken → accessToken

// Store user ID
json.data.user.id → userId
```

### Request Chaining
Requests automatically use variables from previous responses:
- Login stores tokens → Get User uses stored token
- Refresh updates tokens → Subsequent requests use new token

## 🔧 Customization

### Adding New Requests
1. Right-click on folder in Thunder Client
2. Select **New Request**
3. Configure method, URL, headers, body
4. Add tests and variable extraction

### Environment Management
1. Click **Env** tab in Thunder Client
2. Select environment to modify
3. Add/edit variables as needed
4. Variables are available as `{{variableName}}`

### Custom Tests
Add custom test assertions:
```javascript
// Custom status check
res.status == 201

// Custom JSON validation
json.data.email.includes("@")

// Custom header check
res.headers["content-type"].includes("json")
```

## 🚦 Error Scenarios

### Testing Error Conditions
1. **Invalid Credentials**
   - Modify login request with wrong password
   - Expect 400 status and error message

2. **Expired Token**
   - Use old/invalid access token
   - Expect 401 unauthorized

3. **Rate Limiting**
   - Send multiple login requests quickly
   - Expect 429 too many requests

## 📱 VS Code Integration Benefits

### Advantages of Thunder Client
- ✅ **Native VS Code** - No external application needed
- ✅ **Git Integration** - Collections can be version controlled
- ✅ **IntelliSense** - Auto-completion for variables
- ✅ **Workspace Integration** - Share collections with team
- ✅ **Dark/Light Theme** - Matches VS Code theme

### Team Collaboration
1. Commit collection to Git repository
2. Team members import collection in VS Code
3. Shared environment configurations
4. Consistent testing across team

## 📚 Additional Resources

- [Thunder Client Documentation](https://www.thunderclient.com/docs)
- [Thunder Client GitHub](https://github.com/rangav/thunder-client-support)
- [VS Code Extensions](https://marketplace.visualstudio.com/items?itemName=rangav.vscode-thunder-client)

---

For support or questions about this collection, please refer to the main [troubleshooting guide](../../operations/troubleshooting.md).