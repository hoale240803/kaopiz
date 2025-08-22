import { useState } from 'react';
import { useAuth } from '../hooks/useAuth';
import { UserType } from '../types/auth';
import './TestPage.css';

/**
 * Test page for verifying authentication flows
 */
export function TestPage() {
  const { user, isAuthenticated, login, logout, isLoading, error, clearError } = useAuth();
  const [testResults, setTestResults] = useState<string[]>([]);

  const addTestResult = (result: string) => {
    setTestResults(prev => [...prev, `${new Date().toLocaleTimeString()}: ${result}`]);
  };

  const runAuthTests = async () => {
    setTestResults([]);
    addTestResult('Starting authentication flow tests...');

    // Test 1: Check initial state
    addTestResult(`Initial authentication state: ${isAuthenticated ? 'Authenticated' : 'Not authenticated'}`);
    
    if (isAuthenticated && user) {
      addTestResult(`Current user: ${user.fullName} (${user.userType})`);
      addTestResult(`User roles: ${user.roles.join(', ') || 'None'}`);
      addTestResult(`Email verified: ${user.isEmailConfirmed ? 'Yes' : 'No'}`);
    }

    // Test 2: Test login with invalid credentials
    addTestResult('Testing login with invalid credentials...');
    try {
      await login({
        email: 'invalid@test.com',
        password: 'invalidpassword',
        userType: UserType.EndUser,
        rememberMe: false,
      });
      addTestResult('❌ Login with invalid credentials should have failed');
    } catch (error) {
      addTestResult('✅ Login with invalid credentials correctly failed');
    }

    addTestResult('Authentication flow tests completed');
  };

  const testLogout = async () => {
    addTestResult('Testing logout...');
    try {
      await logout();
      addTestResult('✅ Logout successful');
    } catch (error) {
      addTestResult('❌ Logout failed');
    }
  };

  return (
    <div className="test-page">
      <div className="test-container">
        <h1>Authentication Flow Testing</h1>
        
        <div className="test-section">
          <h2>Current Authentication State</h2>
          <div className="status-grid">
            <div className="status-item">
              <span className="status-label">Authenticated:</span>
              <span className={`status-value ${isAuthenticated ? 'authenticated' : 'not-authenticated'}`}>
                {isAuthenticated ? '✅ Yes' : '❌ No'}
              </span>
            </div>
            
            <div className="status-item">
              <span className="status-label">Loading:</span>
              <span className="status-value">{isLoading ? '⏳ Yes' : '✅ No'}</span>
            </div>
            
            <div className="status-item">
              <span className="status-label">Error:</span>
              <span className="status-value">{error ? `❌ ${error}` : '✅ None'}</span>
            </div>
          </div>
          
          {error && (
            <button onClick={clearError} className="clear-error-btn">
              Clear Error
            </button>
          )}
        </div>

        {user && (
          <div className="test-section">
            <h2>User Information</h2>
            <div className="user-info">
              <div className="info-row">
                <span className="info-label">Name:</span>
                <span className="info-value">{user.fullName}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Email:</span>
                <span className="info-value">{user.email}</span>
              </div>
              <div className="info-row">
                <span className="info-label">User Type:</span>
                <span className="info-value">{user.userType}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Roles:</span>
                <span className="info-value">{user.roles.join(', ') || 'None'}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Email Verified:</span>
                <span className="info-value">{user.isEmailConfirmed ? 'Yes' : 'No'}</span>
              </div>
              <div className="info-row">
                <span className="info-label">Created:</span>
                <span className="info-value">{new Date(user.createdAt).toLocaleString()}</span>
              </div>
              {user.lastLoginAt && (
                <div className="info-row">
                  <span className="info-label">Last Login:</span>
                  <span className="info-value">{new Date(user.lastLoginAt).toLocaleString()}</span>
                </div>
              )}
            </div>
          </div>
        )}

        <div className="test-section">
          <h2>Test Actions</h2>
          <div className="test-actions">
            <button onClick={runAuthTests} className="test-btn" disabled={isLoading}>
              Run Authentication Tests
            </button>
            
            {isAuthenticated && (
              <button onClick={testLogout} className="test-btn logout" disabled={isLoading}>
                Test Logout
              </button>
            )}
          </div>
        </div>

        {testResults.length > 0 && (
          <div className="test-section">
            <h2>Test Results</h2>
            <div className="test-results">
              {testResults.map((result, index) => (
                <div key={index} className="test-result">
                  {result}
                </div>
              ))}
            </div>
            <button 
              onClick={() => setTestResults([])} 
              className="clear-results-btn"
            >
              Clear Results
            </button>
          </div>
        )}

        <div className="test-section">
          <h2>Protected Route Testing</h2>
          <div className="route-tests">
            <p>Test different route protections:</p>
            <ul>
              <li>
                <a href="/" target="_blank" rel="noopener noreferrer">
                  Home (Protected - All authenticated users)
                </a>
              </li>
              <li>
                <a href="/admin" target="_blank" rel="noopener noreferrer">
                  Admin (Protected - Admin users only)
                </a>
              </li>
              <li>
                <a href="/unauthorized" target="_blank" rel="noopener noreferrer">
                  Unauthorized Page (Public)
                </a>
              </li>
              <li>
                <a href="/login" target="_blank" rel="noopener noreferrer">
                  Login Page (Public)
                </a>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  );
}