import React, { createContext, useContext, useReducer, useEffect, useCallback } from 'react';
import { AuthState, AuthContextType, LoginRequest, UserDto } from '../types/auth';
import { httpService } from '../services/httpService';
import { TokenStorage } from '../utils/tokenStorage';

// Auth reducer actions
type AuthAction = 
  | { type: 'LOGIN_START' }
  | { type: 'LOGIN_SUCCESS'; payload: { user: UserDto; accessToken: string; refreshToken: string; expiresAt: Date } }
  | { type: 'LOGIN_FAILURE' }
  | { type: 'LOGOUT' }
  | { type: 'REFRESH_TOKEN_SUCCESS'; payload: { accessToken: string; refreshToken: string; expiresAt: Date } }
  | { type: 'SET_LOADING'; payload: boolean };

// Auth reducer
const authReducer = (state: AuthState, action: AuthAction): AuthState => {
  switch (action.type) {
    case 'LOGIN_START':
      return { ...state, isLoading: true };
    
    case 'LOGIN_SUCCESS':
      return {
        isAuthenticated: true,
        user: action.payload.user,
        accessToken: action.payload.accessToken,
        refreshToken: action.payload.refreshToken,
        expiresAt: action.payload.expiresAt,
        isLoading: false,
      };
    
    case 'LOGIN_FAILURE':
      return {
        isAuthenticated: false,
        user: null,
        accessToken: null,
        refreshToken: null,
        expiresAt: null,
        isLoading: false,
      };
    
    case 'LOGOUT':
      return {
        isAuthenticated: false,
        user: null,
        accessToken: null,
        refreshToken: null,
        expiresAt: null,
        isLoading: false,
      };
    
    case 'REFRESH_TOKEN_SUCCESS':
      return {
        ...state,
        accessToken: action.payload.accessToken,
        refreshToken: action.payload.refreshToken,
        expiresAt: action.payload.expiresAt,
      };
    
    case 'SET_LOADING':
      return { ...state, isLoading: action.payload };
    
    default:
      return state;
  }
};

// Create context
const AuthContext = createContext<AuthContextType | null>(null);

// Auth provider component
export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [authState, dispatch] = useReducer(authReducer, TokenStorage.getInitialAuthState());

  // Login function
  const login = useCallback(async (credentials: LoginRequest): Promise<boolean> => {
    dispatch({ type: 'LOGIN_START' });
    
    try {
      const response = await httpService.login(credentials);
      
      if (response.success && response.data) {
        const { accessToken, refreshToken, expiresAt, user } = response.data;
        const expiryDate = new Date(expiresAt);
        
        // Save tokens to storage
        TokenStorage.saveTokens(accessToken, refreshToken, expiryDate, user, credentials.rememberMe);
        
        // Update state
        dispatch({
          type: 'LOGIN_SUCCESS',
          payload: {
            user,
            accessToken,
            refreshToken,
            expiresAt: expiryDate,
          },
        });
        
        return true;
      } else {
        dispatch({ type: 'LOGIN_FAILURE' });
        return false;
      }
    } catch (error) {
      console.error('Login failed:', error);
      dispatch({ type: 'LOGIN_FAILURE' });
      return false;
    }
  }, []);

  // Logout function
  const logout = useCallback(async (): Promise<void> => {
    try {
      if (authState.refreshToken) {
        await httpService.logout({ 
          refreshToken: authState.refreshToken,
          revokeAllTokens: false 
        });
      }
    } catch (error) {
      console.error('Logout API call failed:', error);
    } finally {
      // Always clear local storage and state
      TokenStorage.clearTokens();
      dispatch({ type: 'LOGOUT' });
    }
  }, [authState.refreshToken]);

  // Refresh token function
  const refreshToken = useCallback(async (): Promise<boolean> => {
    if (!authState.refreshToken) {
      return false;
    }

    try {
      const response = await httpService.refreshToken({ 
        refreshToken: authState.refreshToken 
      });
      
      if (response.success && response.data) {
        const { accessToken, refreshToken: newRefreshToken, expiresAt } = response.data;
        const expiryDate = new Date(expiresAt);
        
        // Update storage (maintain current remember me setting)
        const rememberMe = localStorage.getItem('remember_me') === 'true';
        TokenStorage.saveTokens(accessToken, newRefreshToken, expiryDate, authState.user, rememberMe);
        
        // Update state
        dispatch({
          type: 'REFRESH_TOKEN_SUCCESS',
          payload: {
            accessToken,
            refreshToken: newRefreshToken,
            expiresAt: expiryDate,
          },
        });
        
        return true;
      } else {
        // Refresh failed, logout user
        await logout();
        return false;
      }
    } catch (error) {
      console.error('Token refresh failed:', error);
      await logout();
      return false;
    }
  }, [authState.refreshToken, authState.user, logout]);

  // Check if token is expired
  const isTokenExpired = useCallback((): boolean => {
    if (!authState.expiresAt) return true;
    return TokenStorage.isTokenExpired(authState.expiresAt);
  }, [authState.expiresAt]);

  // Check if user has specific role
  const hasRole = useCallback((role: string): boolean => {
    return authState.user?.roles?.includes(role) ?? false;
  }, [authState.user?.roles]);

  // Auto refresh token effect
  useEffect(() => {
    if (!authState.isAuthenticated || !authState.expiresAt) {
      return;
    }

    const checkAndRefreshToken = () => {
      if (TokenStorage.canRefreshToken(authState.expiresAt!)) {
        refreshToken();
      }
    };

    // Check immediately
    checkAndRefreshToken();

    // Set up interval to check every minute
    const interval = setInterval(checkAndRefreshToken, 60 * 1000);

    return () => clearInterval(interval);
  }, [authState.isAuthenticated, authState.expiresAt, refreshToken]);

  const contextValue: AuthContextType = {
    authState,
    login,
    logout,
    refreshToken,
    isTokenExpired,
    hasRole,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
};

// Custom hook to use auth context
export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};