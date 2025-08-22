import { test, expect } from '@playwright/test';

const BASE_URL = 'https://localhost:5001';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the application
    await page.goto(BASE_URL);
  });

  test('should register a new user successfully', async ({ page }) => {
    // Navigate to register page
    await page.click('text=Register');
    
    // Fill registration form
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'Test@123456');
    await page.fill('[data-testid="confirm-password-input"]', 'Test@123456');
    await page.fill('[data-testid="first-name-input"]', 'John');
    await page.fill('[data-testid="last-name-input"]', 'Doe');
    
    // Submit registration
    await page.click('[data-testid="register-button"]');
    
    // Verify success message or redirect
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="success-message"]')).toContainText('Registration successful');
  });

  test('should login with valid credentials', async ({ page }) => {
    // Navigate to login page
    await page.click('text=Login');
    
    // Fill login form
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'Test@123456');
    
    // Submit login
    await page.click('[data-testid="login-button"]');
    
    // Verify successful login (redirect to dashboard or profile)
    await expect(page.locator('[data-testid="user-dashboard"]')).toBeVisible();
    await expect(page.locator('[data-testid="welcome-message"]')).toContainText('Welcome');
  });

  test('should show error for invalid login credentials', async ({ page }) => {
    // Navigate to login page
    await page.click('text=Login');
    
    // Fill login form with invalid credentials
    await page.fill('[data-testid="email-input"]', 'invalid@example.com');
    await page.fill('[data-testid="password-input"]', 'WrongPassword');
    
    // Submit login
    await page.click('[data-testid="login-button"]');
    
    // Verify error message
    await expect(page.locator('[data-testid="error-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="error-message"]')).toContainText('Invalid credentials');
  });

  test('should logout successfully', async ({ page }) => {
    // First login
    await page.click('text=Login');
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'Test@123456');
    await page.click('[data-testid="login-button"]');
    
    // Wait for dashboard
    await expect(page.locator('[data-testid="user-dashboard"]')).toBeVisible();
    
    // Logout
    await page.click('[data-testid="logout-button"]');
    
    // Verify redirect to login page
    await expect(page.locator('[data-testid="login-form"]')).toBeVisible();
  });

  test('should validate email format on registration', async ({ page }) => {
    // Navigate to register page
    await page.click('text=Register');
    
    // Fill form with invalid email
    await page.fill('[data-testid="email-input"]', 'invalid-email');
    await page.fill('[data-testid="password-input"]', 'Test@123456');
    await page.fill('[data-testid="confirm-password-input"]', 'Test@123456');
    await page.fill('[data-testid="first-name-input"]', 'John');
    await page.fill('[data-testid="last-name-input"]', 'Doe');
    
    // Try to submit
    await page.click('[data-testid="register-button"]');
    
    // Verify validation error
    await expect(page.locator('[data-testid="email-error"]')).toBeVisible();
    await expect(page.locator('[data-testid="email-error"]')).toContainText('Invalid email format');
  });

  test('should validate password confirmation match', async ({ page }) => {
    // Navigate to register page
    await page.click('text=Register');
    
    // Fill form with mismatched passwords
    await page.fill('[data-testid="email-input"]', 'test2@example.com');
    await page.fill('[data-testid="password-input"]', 'Test@123456');
    await page.fill('[data-testid="confirm-password-input"]', 'DifferentPassword');
    await page.fill('[data-testid="first-name-input"]', 'Jane');
    await page.fill('[data-testid="last-name-input"]', 'Smith');
    
    // Try to submit
    await page.click('[data-testid="register-button"]');
    
    // Verify validation error
    await expect(page.locator('[data-testid="password-confirmation-error"]')).toBeVisible();
    await expect(page.locator('[data-testid="password-confirmation-error"]')).toContainText('Passwords do not match');
  });
});