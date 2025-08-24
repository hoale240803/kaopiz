import {
  ApiError,
  AuthResponse,
  LoginCredentials,
  RegisterData,
  TokenPair
} from '../types/auth';
import { TokenStorage } from '../utils/tokenStorage';

/**
 * Base API URL - in a real app this would come from environment variables
 */
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5001/api';

/**
 * Authentication service using native Fetch API
 */
export class AuthService {
  /**
   * Login user with credentials
   * @param credentials Login credentials
   * @returns Promise with authentication response
   */
  static async login(credentials: LoginCredentials): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(credentials),
      });

      if (!response.ok) {
        const errorData = await this.handleErrorResponse(response);
        throw errorData;
      }

      const data: AuthResponse = await response.json();
      return data;
    } catch (error) {
      if (error instanceof Error) {
        throw {
          message: error.message,
          statusCode: 0
        } as ApiError;
      }
      throw error;
    }
  }

  /**
   * Register new user
   * @param userData Registration data
   * @returns Promise with authentication response
   */
  static async register(userData: RegisterData): Promise<AuthResponse> {
    try {
      const response = await fetch(`${API_BASE_URL}/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
      });

      if (!response.ok) {
        const errorData = await this.handleErrorResponse(response);
        throw errorData;
      }

      const data: AuthResponse = await response.json();
      return data;
    } catch (error) {
      if (error instanceof Error) {
        throw {
          message: error.message,
          statusCode: 0
        } as ApiError;
      }
      throw error;
    }
  }

  /**
   * Refresh access token using refresh token
   * @returns Promise with new token pair
   */
  static async refreshToken(): Promise<TokenPair> {
    try {
      const refreshToken = TokenStorage.getRefreshToken();
      
      if (!refreshToken) {
        throw {
          message: 'No refresh token available',
          statusCode: 401
        } as ApiError;
      }

      const response = await fetch(`${API_BASE_URL}/auth/refresh`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ refreshToken }),
      });

      if (!response.ok) {
        const errorData = await this.handleErrorResponse(response);
        throw errorData;
      }

      const data: TokenPair = await response.json();
      return data;
    } catch (error) {
      if (error instanceof Error) {
        throw {
          message: error.message,
          statusCode: 0
        } as ApiError;
      }
      throw error;
    }
  }

  /**
   * Logout user and invalidate tokens
   * @returns Promise that resolves when logout is complete
   */
  static async logout(): Promise<void> {
    try {
      const refreshToken = TokenStorage.getRefreshToken();
      
      if (refreshToken) {
        // Attempt to invalidate token on server
        await fetch(`${API_BASE_URL}/auth/logout`, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${TokenStorage.getAccessToken()}`,
          },
          body: JSON.stringify({ refreshToken }),
        });
      }
    } catch (error) {
      // Don't throw on logout errors - still clear local storage
      console.warn('Failed to logout on server:', error);
    } finally {
      // Always clear local storage
      TokenStorage.clearSession();
    }
  }

  /**
   * Make authenticated API request with automatic token refresh
   * @param url Request URL
   * @param options Fetch options
   * @returns Promise with response
   */
  static async authenticatedFetch(url: string, options: RequestInit = {}): Promise<Response> {
    const makeRequest = async (token: string): Promise<Response> => {
      return fetch(url, {
        ...options,
        headers: {
          ...options.headers,
          'Authorization': `Bearer ${token}`,
        },
      });
    };

    let accessToken = TokenStorage.getAccessToken();
    
    if (!accessToken) {
      throw {
        message: 'No access token available',
        statusCode: 401
      } as ApiError;
    }

    // If access token is expired, try to refresh
    if (!TokenStorage.isAccessTokenValid()) {
      if (TokenStorage.isRefreshTokenValid()) {
        try {
          const newTokens = await this.refreshToken();
          TokenStorage.updateTokens(newTokens);
          accessToken = newTokens.accessToken;
        } catch (error) {
          // Refresh failed, user needs to login again
          TokenStorage.clearSession();
          throw {
            message: 'Session expired, please login again',
            statusCode: 401
          } as ApiError;
        }
      } else {
        // Both tokens expired
        TokenStorage.clearSession();
        throw {
          message: 'Session expired, please login again',
          statusCode: 401
        } as ApiError;
      }
    }

    const response = await makeRequest(accessToken);

    // If we get 401, try refreshing once more
    if (response.status === 401 && TokenStorage.isRefreshTokenValid()) {
      try {
        const newTokens = await this.refreshToken();
        TokenStorage.updateTokens(newTokens);
        return makeRequest(newTokens.accessToken);
      } catch (error) {
        TokenStorage.clearSession();
        throw {
          message: 'Session expired, please login again',
          statusCode: 401
        } as ApiError;
      }
    }

    return response;
  }

  /**
   * Handle error response from API
   * @param response Failed response
   * @returns Promise with parsed error data
   */
  private static async handleErrorResponse(response: Response): Promise<ApiError> {
    try {
      const errorData = await response.json();
      return {
        message: errorData.message || 'An error occurred',
        errors: errorData.errors,
        statusCode: response.status
      };
    } catch (error) {
      return {
        message: `HTTP ${response.status}: ${response.statusText}`,
        statusCode: response.status
      };
    }
  }

  /**
   * Check if user is currently authenticated
   * @returns True if user has valid tokens
   */
  static isAuthenticated(): boolean {
    return TokenStorage.isRefreshTokenValid();
  }

  /**
   * Get current access token if valid
   * @returns Access token or null
   */
  static getCurrentAccessToken(): string | null {
    if (TokenStorage.isAccessTokenValid()) {
      return TokenStorage.getAccessToken();
    }
    return null;
  }
}