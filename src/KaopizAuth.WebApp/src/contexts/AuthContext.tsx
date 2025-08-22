import { createContext, useReducer, useEffect, ReactNode } from 'react';
import { 
  AuthState, 
  LoginCredentials, 
  RegisterData, 
  User, 
  AuthActionType,
  ApiError 
} from '../types/auth';
import { authReducer, initialAuthState } from '../utils/authReducer';
import { TokenStorage, STORAGE_CHANGE_EVENT } from '../utils/tokenStorage';
import { AuthService } from '../services/authService';

/**
 * Authentication context value interface
 */
export interface AuthContextValue {
  // State
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  rememberMe: boolean;

  // Actions
  login: (credentials: LoginCredentials) => Promise<void>;
  register: (userData: RegisterData) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
}

/**
 * Authentication context
 */
export const AuthContext = createContext<AuthContextValue | null>(null);

/**
 * Authentication provider props
 */
interface AuthProviderProps {
  children: ReactNode;
}

/**
 * Authentication provider component
 */
export function AuthProvider({ children }: AuthProviderProps) {
  const [state, dispatch] = useReducer(authReducer, initialAuthState);

  /**
   * Initialize authentication state from stored session
   */
  useEffect(() => {
    const initializeAuth = () => {
      const storedSession = TokenStorage.getStoredSession();
      
      if (storedSession) {
        dispatch({
          type: AuthActionType.RESTORE_SESSION,
          payload: {
            user: storedSession.user,
            tokens: storedSession.tokens,
            rememberMe: storedSession.rememberMe,
          },
        });
      }
    };

    initializeAuth();
  }, []);

  /**
   * Set up cross-tab synchronization
   */
  useEffect(() => {
    const cleanup = TokenStorage.addStorageChangeListener((event) => {
      const { action, data } = event.detail;

      switch (action) {
        case 'session_stored':
          if (data.user && data.tokens) {
            dispatch({
              type: AuthActionType.RESTORE_SESSION,
              payload: {
                user: data.user,
                tokens: data.tokens,
                rememberMe: data.rememberMe || false,
              },
            });
          }
          break;

        case 'session_cleared':
          dispatch({
            type: AuthActionType.LOGOUT,
          });
          break;

        case 'tokens_updated':
          if (data.tokens) {
            dispatch({
              type: AuthActionType.REFRESH_TOKEN_SUCCESS,
              payload: {
                tokens: data.tokens,
              },
            });
          }
          break;
      }
    });

    return cleanup;
  }, []);

  /**
   * Login function
   */
  const login = async (credentials: LoginCredentials): Promise<void> => {
    dispatch({ type: AuthActionType.LOGIN_START });

    try {
      const response = await AuthService.login(credentials);
      
      // Store session
      TokenStorage.storeSession(response.user, response.tokens, credentials.rememberMe);

      dispatch({
        type: AuthActionType.LOGIN_SUCCESS,
        payload: {
          user: response.user,
          tokens: response.tokens,
          rememberMe: credentials.rememberMe,
        },
      });
    } catch (error) {
      const apiError = error as ApiError;
      dispatch({
        type: AuthActionType.LOGIN_FAILURE,
        payload: {
          error: apiError.message || 'Login failed',
        },
      });
      throw error;
    }
  };

  /**
   * Register function
   */
  const register = async (userData: RegisterData): Promise<void> => {
    dispatch({ type: AuthActionType.REGISTER_START });

    try {
      const response = await AuthService.register(userData);
      
      // Store session (registration doesn't set remember me)
      TokenStorage.storeSession(response.user, response.tokens, false);

      dispatch({
        type: AuthActionType.REGISTER_SUCCESS,
        payload: {
          user: response.user,
          tokens: response.tokens,
        },
      });
    } catch (error) {
      const apiError = error as ApiError;
      dispatch({
        type: AuthActionType.REGISTER_FAILURE,
        payload: {
          error: apiError.message || 'Registration failed',
        },
      });
      throw error;
    }
  };

  /**
   * Logout function
   */
  const logout = async (): Promise<void> => {
    try {
      await AuthService.logout();
    } catch (error) {
      console.warn('Logout error:', error);
      // Continue with local logout even if server logout fails
    } finally {
      dispatch({ type: AuthActionType.LOGOUT });
    }
  };

  /**
   * Clear error function
   */
  const clearError = (): void => {
    dispatch({ type: AuthActionType.CLEAR_ERROR });
  };

  /**
   * Context value
   */
  const contextValue: AuthContextValue = {
    // State
    user: state.user,
    isAuthenticated: state.isAuthenticated,
    isLoading: state.isLoading,
    error: state.error,
    rememberMe: state.rememberMe,

    // Actions
    login,
    register,
    logout,
    clearError,
  };

  return (
    <AuthContext.Provider value={contextValue}>
      {children}
    </AuthContext.Provider>
  );
}