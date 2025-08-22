import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { useForm } from '../hooks/useForm';
import { LoginCredentials, UserType } from '../types/auth';
import './Login.css';

/**
 * Login form data interface
 */
interface LoginFormData {
  email: string;
  password: string;
  userType: UserType;
  rememberMe: boolean;
}

/**
 * Login page component with form validation
 */
export function LoginPage() {
  const { login, isAuthenticated, error, clearError } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);

  // Get the intended destination from location state
  const from = location.state?.from?.pathname || '/';

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, navigate, from]);

  // Clear errors when component mounts
  useEffect(() => {
    clearError();
  }, [clearError]);

  /**
   * Form validation function
   */
  const validateForm = (values: LoginFormData) => {
    const errors: Record<string, string> = {};

    if (!values.email) {
      errors.email = 'Email is required';
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(values.email)) {
      errors.email = 'Please enter a valid email address';
    }

    if (!values.password) {
      errors.password = 'Password is required';
    } else if (values.password.length < 6) {
      errors.password = 'Password must be at least 6 characters';
    }

    if (!values.userType) {
      errors.userType = 'Please select a user type';
    }

    return errors;
  };

  /**
   * Form submission handler
   */
  const handleSubmit = async (values: LoginFormData) => {
    try {
      await login(values);
      // Navigation will happen automatically via useEffect when isAuthenticated changes
    } catch (error) {
      // Error handling is managed by the AuthContext
      console.error('Login failed:', error);
    }
  };

  const {
    values,
    errors,
    isSubmitting,
    handleChange,
    handleSubmit: onSubmit,
  } = useForm<LoginFormData>({
    initialValues: {
      email: '',
      password: '',
      userType: UserType.EndUser,
      rememberMe: false,
    },
    validate: validateForm,
    onSubmit: handleSubmit,
  });

  return (
    <div className="login-page">
      <div className="login-container">
        <div className="login-card">
          <div className="login-header">
            <h1 className="login-title">Welcome back</h1>
            <p className="login-subtitle">Sign in to your account</p>
          </div>

          <form onSubmit={onSubmit} className="login-form">
            {/* Global error message */}
            {error && (
              <div className="error-banner">
                <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                  <path
                    fillRule="evenodd"
                    clipRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
                    fill="currentColor"
                  />
                </svg>
                {error}
              </div>
            )}

            {/* User Type Selection */}
            <div className="form-group">
              <label htmlFor="userType" className="form-label">
                User Type
              </label>
              <select
                id="userType"
                name="userType"
                value={values.userType}
                onChange={(e) => handleChange('userType', e.target.value as UserType)}
                className={`form-select ${errors.userType ? 'error' : ''}`}
                disabled={isSubmitting}
              >
                <option value="">Select user type</option>
                <option value={UserType.EndUser}>End User</option>
                <option value={UserType.Admin}>Admin</option>
                <option value={UserType.Partner}>Partner</option>
              </select>
              {errors.userType && (
                <div className="field-error">{errors.userType}</div>
              )}
            </div>

            {/* Email field */}
            <div className="form-group">
              <label htmlFor="email" className="form-label">
                Email address
              </label>
              <input
                id="email"
                name="email"
                type="email"
                autoComplete="email"
                value={values.email}
                onChange={(e) => handleChange('email', e.target.value)}
                className={`form-input ${errors.email ? 'error' : ''}`}
                placeholder="Enter your email"
                disabled={isSubmitting}
              />
              {errors.email && (
                <div className="field-error">{errors.email}</div>
              )}
            </div>

            {/* Password field */}
            <div className="form-group">
              <label htmlFor="password" className="form-label">
                Password
              </label>
              <div className="password-input-container">
                <input
                  id="password"
                  name="password"
                  type={showPassword ? 'text' : 'password'}
                  autoComplete="current-password"
                  value={values.password}
                  onChange={(e) => handleChange('password', e.target.value)}
                  className={`form-input ${errors.password ? 'error' : ''}`}
                  placeholder="Enter your password"
                  disabled={isSubmitting}
                />
                <button
                  type="button"
                  className="password-toggle"
                  onClick={() => setShowPassword(!showPassword)}
                  disabled={isSubmitting}
                >
                  {showPassword ? (
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                      <path
                        d="M3.28 2.22a.75.75 0 00-1.06 1.06l14.5 14.5a.75.75 0 101.06-1.06L3.28 2.22zM10 6a4 4 0 014 4c0 .3-.04.6-.1.87l-1.43-1.43A2.5 2.5 0 0010.56 7.1L9.13 5.67c.29-.06.58-.1.87-.1z"
                        fill="currentColor"
                      />
                      <path
                        d="M10 14a4 4 0 01-4-4c0-.3.04-.6.1-.87l1.43 1.43A2.5 2.5 0 009.44 12.9l1.43 1.43c-.29.06-.58.1-.87.1z"
                        fill="currentColor"
                      />
                    </svg>
                  ) : (
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                      <path
                        d="M10 12.5a2.5 2.5 0 100-5 2.5 2.5 0 000 5z"
                        fill="currentColor"
                      />
                      <path
                        fillRule="evenodd"
                        clipRule="evenodd"
                        d="M.664 10C1.88 6.24 5.538 3.5 10 3.5s8.12 2.74 9.336 6.5C18.12 13.76 14.462 16.5 10 16.5S1.88 13.76.664 10zM10 14a4 4 0 100-8 4 4 0 000 8z"
                        fill="currentColor"
                      />
                    </svg>
                  )}
                </button>
              </div>
              {errors.password && (
                <div className="field-error">{errors.password}</div>
              )}
            </div>

            {/* Remember me checkbox */}
            <div className="form-group">
              <label className="checkbox-label">
                <input
                  type="checkbox"
                  checked={values.rememberMe}
                  onChange={(e) => handleChange('rememberMe', e.target.checked)}
                  className="checkbox-input"
                  disabled={isSubmitting}
                />
                <span className="checkbox-text">Remember me for 30 days</span>
              </label>
            </div>

            {/* Submit button */}
            <button
              type="submit"
              disabled={isSubmitting}
              className="submit-button"
            >
              {isSubmitting ? (
                <>
                  <div className="button-spinner"></div>
                  Signing in...
                </>
              ) : (
                'Sign in'
              )}
            </button>
          </form>

          <div className="login-footer">
            <p className="signup-link">
              Don't have an account?{' '}
              <Link to="/register" className="link">
                Sign up
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}