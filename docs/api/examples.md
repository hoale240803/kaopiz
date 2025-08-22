# API Usage Examples

This document provides practical examples of using the Kaopiz Auth API with various tools and programming languages.

## 🔧 curl Examples

### Complete Authentication Flow

#### 1. Login
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "john.doe@example.com",
    "password": "SecurePass123!"
  }' \
  | jq '.'
```

**Response**:
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjNlNDU2Ny1lODliLTEyZDMtYTQ1Ni00MjY2MTQxNzQwMDAiLCJlbWFpbCI6ImpvaG4uZG9lQGV4YW1wbGUuY29tIiwicm9sZSI6IlVzZXIiLCJpYXQiOjE2Mjk4ODQ0MDAsImV4cCI6MTYyOTg4NTMwMH0.signature",
    "refreshToken": "b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6",
    "expiresAt": "2025-08-21T12:15:00Z",
    "user": {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "email": "john.doe@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "fullName": "John Doe",
      "roles": ["User"]
    }
  },
  "errors": []
}
```

#### 2. Use Access Token for Protected Endpoints
```bash
# Store the token for convenience
ACCESS_TOKEN="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."

# Get current user profile
curl -X GET "https://localhost:7001/api/users/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  | jq '.'
```

#### 3. Refresh Token When Expired
```bash
REFRESH_TOKEN="b8c9d4e5f6g7h8i9j0k1l2m3n4o5p6q7r8s9t0u1v2w3x4y5z6"

curl -X POST "https://localhost:7001/api/auth/refresh" \
  -H "Content-Type: application/json" \
  -d "{
    \"refreshToken\": \"$REFRESH_TOKEN\"
  }" \
  | jq '.'
```

#### 4. Update User Profile
```bash
curl -X PUT "https://localhost:7001/api/users/me" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Smith"
  }' \
  | jq '.'
```

#### 5. Logout
```bash
curl -X POST "https://localhost:7001/api/auth/logout" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{
    \"refreshToken\": \"$REFRESH_TOKEN\"
  }" \
  | jq '.'
```

### Error Handling Examples

#### Invalid Credentials
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "invalid@example.com",
    "password": "wrongpassword"
  }' \
  | jq '.'
```

**Response** (400 Bad Request):
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

#### Validation Errors
```bash
curl -X POST "https://localhost:7001/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "not-an-email",
    "password": "123"
  }' \
  | jq '.'
```

**Response** (422 Validation Error):
```json
{
  "success": false,
  "message": "Validation failed",
  "data": null,
  "errors": [
    {
      "field": "email",
      "message": "Invalid email format"
    },
    {
      "field": "password",
      "message": "Password must be at least 8 characters"
    }
  ]
}
```

#### Unauthorized Access
```bash
curl -X GET "https://localhost:7001/api/users/me" \
  -H "Authorization: Bearer invalid_token" \
  | jq '.'
```

**Response** (401 Unauthorized):
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

## 📜 Shell Script Example

Complete authentication flow in a shell script:

```bash
#!/bin/bash

# Configuration
API_BASE="https://localhost:7001"
EMAIL="john.doe@example.com"
PASSWORD="SecurePass123!"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${YELLOW}Kaopiz Auth API Test Script${NC}"
echo "=================================="

# Function to make API calls with error handling
make_api_call() {
    local method=$1
    local endpoint=$2
    local data=$3
    local auth_header=$4
    
    echo -e "\n${YELLOW}Making $method request to $endpoint${NC}"
    
    if [ -n "$auth_header" ]; then
        response=$(curl -s -w "%{http_code}" -X "$method" "$API_BASE$endpoint" \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $auth_header" \
            -d "$data")
    else
        response=$(curl -s -w "%{http_code}" -X "$method" "$API_BASE$endpoint" \
            -H "Content-Type: application/json" \
            -d "$data")
    fi
    
    http_code="${response: -3}"
    response_body="${response%???}"
    
    if [ "$http_code" -ge 200 ] && [ "$http_code" -lt 300 ]; then
        echo -e "${GREEN}✓ Success ($http_code)${NC}"
        echo "$response_body" | jq '.'
        return 0
    else
        echo -e "${RED}✗ Error ($http_code)${NC}"
        echo "$response_body" | jq '.'
        return 1
    fi
}

# 1. Health Check
echo -e "\n${YELLOW}1. Health Check${NC}"
make_api_call "GET" "/health" "" ""

# 2. Login
echo -e "\n${YELLOW}2. Login${NC}"
login_data="{\"email\":\"$EMAIL\",\"password\":\"$PASSWORD\"}"
login_response=$(make_api_call "POST" "/api/auth/login" "$login_data" "")

if [ $? -eq 0 ]; then
    # Extract tokens
    ACCESS_TOKEN=$(echo "$login_response" | jq -r '.data.accessToken // empty')
    REFRESH_TOKEN=$(echo "$login_response" | jq -r '.data.refreshToken // empty')
    
    if [ -n "$ACCESS_TOKEN" ] && [ "$ACCESS_TOKEN" != "null" ]; then
        echo -e "${GREEN}✓ Access token obtained${NC}"
        
        # 3. Get Current User
        echo -e "\n${YELLOW}3. Get Current User Profile${NC}"
        make_api_call "GET" "/api/users/me" "" "$ACCESS_TOKEN"
        
        # 4. Update Profile
        echo -e "\n${YELLOW}4. Update User Profile${NC}"
        update_data='{"firstName":"John","lastName":"Updated"}'
        make_api_call "PUT" "/api/users/me" "$update_data" "$ACCESS_TOKEN"
        
        # 5. Refresh Token
        if [ -n "$REFRESH_TOKEN" ] && [ "$REFRESH_TOKEN" != "null" ]; then
            echo -e "\n${YELLOW}5. Refresh Token${NC}"
            refresh_data="{\"refreshToken\":\"$REFRESH_TOKEN\"}"
            refresh_response=$(make_api_call "POST" "/api/auth/refresh" "$refresh_data" "")
            
            if [ $? -eq 0 ]; then
                NEW_ACCESS_TOKEN=$(echo "$refresh_response" | jq -r '.data.accessToken // empty')
                NEW_REFRESH_TOKEN=$(echo "$refresh_response" | jq -r '.data.refreshToken // empty')
                
                # 6. Logout
                echo -e "\n${YELLOW}6. Logout${NC}"
                logout_data="{\"refreshToken\":\"$NEW_REFRESH_TOKEN\"}"
                make_api_call "POST" "/api/auth/logout" "$logout_data" "$NEW_ACCESS_TOKEN"
            fi
        fi
    else
        echo -e "${RED}✗ Failed to extract access token${NC}"
    fi
else
    echo -e "${RED}✗ Login failed${NC}"
fi

echo -e "\n${YELLOW}Test completed!${NC}"
```

## 🐍 Python Examples

### Using requests library

```python
import requests
import json
from typing import Optional, Dict, Any

class KaopizAuthClient:
    def __init__(self, base_url: str = "https://localhost:7001"):
        self.base_url = base_url
        self.access_token: Optional[str] = None
        self.refresh_token: Optional[str] = None
        self.session = requests.Session()
        
    def _make_request(self, method: str, endpoint: str, data: Optional[Dict] = None, 
                     auth_required: bool = False) -> Dict[Any, Any]:
        url = f"{self.base_url}{endpoint}"
        headers = {"Content-Type": "application/json"}
        
        if auth_required and self.access_token:
            headers["Authorization"] = f"Bearer {self.access_token}"
            
        response = self.session.request(method, url, json=data, headers=headers)
        return response.json()
    
    def login(self, email: str, password: str) -> Dict[Any, Any]:
        """Login and store tokens"""
        data = {"email": email, "password": password}
        result = self._make_request("POST", "/api/auth/login", data)
        
        if result.get("success") and result.get("data"):
            self.access_token = result["data"].get("accessToken")
            self.refresh_token = result["data"].get("refreshToken")
            
        return result
    
    def get_current_user(self) -> Dict[Any, Any]:
        """Get current user profile"""
        return self._make_request("GET", "/api/users/me", auth_required=True)
    
    def update_profile(self, first_name: str, last_name: str) -> Dict[Any, Any]:
        """Update user profile"""
        data = {"firstName": first_name, "lastName": last_name}
        return self._make_request("PUT", "/api/users/me", data, auth_required=True)
    
    def refresh_access_token(self) -> Dict[Any, Any]:
        """Refresh access token"""
        if not self.refresh_token:
            return {"success": False, "message": "No refresh token available"}
            
        data = {"refreshToken": self.refresh_token}
        result = self._make_request("POST", "/api/auth/refresh", data)
        
        if result.get("success") and result.get("data"):
            self.access_token = result["data"].get("accessToken")
            self.refresh_token = result["data"].get("refreshToken")
            
        return result
    
    def logout(self) -> Dict[Any, Any]:
        """Logout and clear tokens"""
        if not self.refresh_token:
            return {"success": False, "message": "No refresh token available"}
            
        data = {"refreshToken": self.refresh_token}
        result = self._make_request("POST", "/api/auth/logout", data, auth_required=True)
        
        if result.get("success"):
            self.access_token = None
            self.refresh_token = None
            
        return result

# Usage example
def main():
    client = KaopizAuthClient()
    
    # Login
    print("Logging in...")
    login_result = client.login("john.doe@example.com", "SecurePass123!")
    print(json.dumps(login_result, indent=2))
    
    if login_result.get("success"):
        # Get user profile
        print("\nGetting user profile...")
        user_result = client.get_current_user()
        print(json.dumps(user_result, indent=2))
        
        # Update profile
        print("\nUpdating profile...")
        update_result = client.update_profile("John", "Updated")
        print(json.dumps(update_result, indent=2))
        
        # Logout
        print("\nLogging out...")
        logout_result = client.logout()
        print(json.dumps(logout_result, indent=2))

if __name__ == "__main__":
    main()
```

## 🟨 JavaScript/Node.js Examples

### Using fetch API

```javascript
class KaopizAuthClient {
    constructor(baseUrl = 'https://localhost:7001') {
        this.baseUrl = baseUrl;
        this.accessToken = null;
        this.refreshToken = null;
    }

    async makeRequest(method, endpoint, data = null, authRequired = false) {
        const url = `${this.baseUrl}${endpoint}`;
        const headers = {
            'Content-Type': 'application/json'
        };

        if (authRequired && this.accessToken) {
            headers['Authorization'] = `Bearer ${this.accessToken}`;
        }

        const config = {
            method,
            headers
        };

        if (data) {
            config.body = JSON.stringify(data);
        }

        try {
            const response = await fetch(url, config);
            return await response.json();
        } catch (error) {
            console.error('Request failed:', error);
            throw error;
        }
    }

    async login(email, password) {
        const result = await this.makeRequest('POST', '/api/auth/login', {
            email,
            password
        });

        if (result.success && result.data) {
            this.accessToken = result.data.accessToken;
            this.refreshToken = result.data.refreshToken;
        }

        return result;
    }

    async getCurrentUser() {
        return this.makeRequest('GET', '/api/users/me', null, true);
    }

    async updateProfile(firstName, lastName) {
        return this.makeRequest('PUT', '/api/users/me', {
            firstName,
            lastName
        }, true);
    }

    async refreshAccessToken() {
        if (!this.refreshToken) {
            return { success: false, message: 'No refresh token available' };
        }

        const result = await this.makeRequest('POST', '/api/auth/refresh', {
            refreshToken: this.refreshToken
        });

        if (result.success && result.data) {
            this.accessToken = result.data.accessToken;
            this.refreshToken = result.data.refreshToken;
        }

        return result;
    }

    async logout() {
        if (!this.refreshToken) {
            return { success: false, message: 'No refresh token available' };
        }

        const result = await this.makeRequest('POST', '/api/auth/logout', {
            refreshToken: this.refreshToken
        }, true);

        if (result.success) {
            this.accessToken = null;
            this.refreshToken = null;
        }

        return result;
    }
}

// Usage example
async function example() {
    const client = new KaopizAuthClient();

    try {
        // Login
        console.log('Logging in...');
        const loginResult = await client.login('john.doe@example.com', 'SecurePass123!');
        console.log(JSON.stringify(loginResult, null, 2));

        if (loginResult.success) {
            // Get user profile
            console.log('\nGetting user profile...');
            const userResult = await client.getCurrentUser();
            console.log(JSON.stringify(userResult, null, 2));

            // Update profile
            console.log('\nUpdating profile...');
            const updateResult = await client.updateProfile('John', 'Updated');
            console.log(JSON.stringify(updateResult, null, 2));

            // Logout
            console.log('\nLogging out...');
            const logoutResult = await client.logout();
            console.log(JSON.stringify(logoutResult, null, 2));
        }
    } catch (error) {
        console.error('Error:', error);
    }
}

// Run the example
example();
```

## 📱 React Hook Example

```typescript
import { useState, useCallback, useEffect } from 'react';

interface User {
    id: string;
    email: string;
    firstName: string;
    lastName: string;
    fullName: string;
    roles: string[];
}

interface AuthState {
    user: User | null;
    accessToken: string | null;
    isAuthenticated: boolean;
    isLoading: boolean;
}

export const useKaopizAuth = (baseUrl = 'https://localhost:7001') => {
    const [authState, setAuthState] = useState<AuthState>({
        user: null,
        accessToken: null,
        isAuthenticated: false,
        isLoading: false
    });

    const makeRequest = useCallback(async (
        method: string,
        endpoint: string,
        data?: any,
        authRequired = false
    ) => {
        const url = `${baseUrl}${endpoint}`;
        const headers: HeadersInit = {
            'Content-Type': 'application/json'
        };

        if (authRequired && authState.accessToken) {
            headers['Authorization'] = `Bearer ${authState.accessToken}`;
        }

        const config: RequestInit = {
            method,
            headers
        };

        if (data) {
            config.body = JSON.stringify(data);
        }

        const response = await fetch(url, config);
        return response.json();
    }, [baseUrl, authState.accessToken]);

    const login = useCallback(async (email: string, password: string) => {
        setAuthState(prev => ({ ...prev, isLoading: true }));

        try {
            const result = await makeRequest('POST', '/api/auth/login', {
                email,
                password
            });

            if (result.success && result.data) {
                setAuthState({
                    user: result.data.user,
                    accessToken: result.data.accessToken,
                    isAuthenticated: true,
                    isLoading: false
                });

                // Store refresh token securely
                localStorage.setItem('refreshToken', result.data.refreshToken);
            } else {
                setAuthState(prev => ({ ...prev, isLoading: false }));
            }

            return result;
        } catch (error) {
            setAuthState(prev => ({ ...prev, isLoading: false }));
            throw error;
        }
    }, [makeRequest]);

    const logout = useCallback(async () => {
        const refreshToken = localStorage.getItem('refreshToken');
        
        if (refreshToken) {
            try {
                await makeRequest('POST', '/api/auth/logout', {
                    refreshToken
                }, true);
            } catch (error) {
                console.error('Logout request failed:', error);
            }
        }

        setAuthState({
            user: null,
            accessToken: null,
            isAuthenticated: false,
            isLoading: false
        });

        localStorage.removeItem('refreshToken');
    }, [makeRequest]);

    const getCurrentUser = useCallback(async () => {
        return makeRequest('GET', '/api/users/me', null, true);
    }, [makeRequest]);

    return {
        ...authState,
        login,
        logout,
        getCurrentUser
    };
};
```

## 🧪 Testing Examples

### Jest Tests

```javascript
const KaopizAuthClient = require('./kaopiz-auth-client');

describe('KaopizAuthClient', () => {
    let client;
    
    beforeEach(() => {
        client = new KaopizAuthClient('https://localhost:7001');
        // Mock fetch for testing
        global.fetch = jest.fn();
    });

    afterEach(() => {
        jest.resetAllMocks();
    });

    test('should login successfully', async () => {
        const mockResponse = {
            success: true,
            data: {
                accessToken: 'mock-access-token',
                refreshToken: 'mock-refresh-token',
                user: {
                    id: '123',
                    email: 'test@example.com',
                    firstName: 'Test',
                    lastName: 'User'
                }
            }
        };

        global.fetch.mockResolvedValueOnce({
            json: () => Promise.resolve(mockResponse)
        });

        const result = await client.login('test@example.com', 'password');

        expect(result.success).toBe(true);
        expect(client.accessToken).toBe('mock-access-token');
        expect(client.refreshToken).toBe('mock-refresh-token');
    });

    test('should handle login failure', async () => {
        const mockResponse = {
            success: false,
            message: 'Invalid credentials'
        };

        global.fetch.mockResolvedValueOnce({
            json: () => Promise.resolve(mockResponse)
        });

        const result = await client.login('invalid@example.com', 'wrongpassword');

        expect(result.success).toBe(false);
        expect(client.accessToken).toBeNull();
    });
});
```

---

These examples demonstrate various ways to interact with the Kaopiz Auth API. Choose the approach that best fits your technology stack and requirements.