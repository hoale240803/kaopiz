import { test, expect } from '@playwright/test';

const API_BASE_URL = 'https://localhost:5001/api';

test.describe('API Authentication Tests', () => {
  test('should register user via API', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/auth/register`, {
      data: {
        email: 'api-test@example.com',
        password: 'Test@123456',
        firstName: 'API',
        lastName: 'User'
      }
    });

    expect(response.status()).toBe(200);
    
    const body = await response.json();
    expect(body.success).toBe(true);
    expect(body.message).toBeTruthy();
  });

  test('should login user via API and return tokens', async ({ request }) => {
    // First register
    await request.post(`${API_BASE_URL}/auth/register`, {
      data: {
        email: 'api-login@example.com',
        password: 'Test@123456',
        firstName: 'API',
        lastName: 'Login'
      }
    });

    // Then login
    const response = await request.post(`${API_BASE_URL}/auth/login`, {
      data: {
        email: 'api-login@example.com',
        password: 'Test@123456'
      }
    });

    expect(response.status()).toBe(200);
    
    const body = await response.json();
    expect(body.accessToken).toBeTruthy();
    expect(body.refreshToken).toBeTruthy();
    expect(body.expiresIn).toBeGreaterThan(0);
  });

  test('should return 401 for invalid credentials', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/auth/login`, {
      data: {
        email: 'nonexistent@example.com',
        password: 'WrongPassword'
      }
    });

    expect(response.status()).toBe(401);
  });

  test('should return 400 for invalid email format', async ({ request }) => {
    const response = await request.post(`${API_BASE_URL}/auth/register`, {
      data: {
        email: 'invalid-email',
        password: 'Test@123456',
        firstName: 'Invalid',
        lastName: 'Email'
      }
    });

    expect(response.status()).toBe(400);
  });

  test('should refresh token successfully', async ({ request }) => {
    // Register and login to get refresh token
    await request.post(`${API_BASE_URL}/auth/register`, {
      data: {
        email: 'refresh-test@example.com',
        password: 'Test@123456',
        firstName: 'Refresh',
        lastName: 'Test'
      }
    });

    const loginResponse = await request.post(`${API_BASE_URL}/auth/login`, {
      data: {
        email: 'refresh-test@example.com',
        password: 'Test@123456'
      }
    });

    const loginBody = await loginResponse.json();
    
    // Use refresh token to get new access token
    const refreshResponse = await request.post(`${API_BASE_URL}/auth/refresh`, {
      data: {
        refreshToken: loginBody.refreshToken
      }
    });

    expect(refreshResponse.status()).toBe(200);
    
    const refreshBody = await refreshResponse.json();
    expect(refreshBody.accessToken).toBeTruthy();
    expect(refreshBody.refreshToken).toBeTruthy();
    expect(refreshBody.expiresIn).toBeGreaterThan(0);
  });
});