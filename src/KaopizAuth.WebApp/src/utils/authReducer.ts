import type { AuthState, AuthAction } from '../types/auth';
import { AuthActionType } from '../types/auth';

/**
 * Initial authentication state
 */
export const initialAuthState: AuthState = {
  user: null,
  tokens: null,
  isAuthenticated: false,
  isLoading: false,
  error: null,
  rememberMe: false,
};

/**
 * Authentication reducer for managing auth state
 * @param state Current authentication state
 * @param action Action to process
 * @returns New authentication state
 */
export function authReducer(state: AuthState, action: AuthAction): AuthState {
  switch (action.type) {
    case AuthActionType.LOGIN_START:
      return {
        ...state,
        isLoading: true,
        error: null,
      };

    case AuthActionType.LOGIN_SUCCESS:
      return {
        ...state,
        user: action.payload.user,
        tokens: action.payload.tokens,
        isAuthenticated: true,
        isLoading: false,
        error: null,
        rememberMe: action.payload.rememberMe,
      };

    case AuthActionType.LOGIN_FAILURE:
      return {
        ...state,
        user: null,
        tokens: null,
        isAuthenticated: false,
        isLoading: false,
        error: action.payload.error,
        rememberMe: false,
      };

    case AuthActionType.REGISTER_START:
      return {
        ...state,
        isLoading: true,
        error: null,
      };

    case AuthActionType.REGISTER_SUCCESS:
      return {
        ...state,
        user: action.payload.user,
        tokens: action.payload.tokens,
        isAuthenticated: true,
        isLoading: false,
        error: null,
        rememberMe: false, // Registration doesn't set remember me by default
      };

    case AuthActionType.REGISTER_FAILURE:
      return {
        ...state,
        user: null,
        tokens: null,
        isAuthenticated: false,
        isLoading: false,
        error: action.payload.error,
        rememberMe: false,
      };

    case AuthActionType.LOGOUT:
      return {
        ...initialAuthState,
      };

    case AuthActionType.RESTORE_SESSION:
      return {
        ...state,
        user: action.payload.user,
        tokens: action.payload.tokens,
        isAuthenticated: true,
        isLoading: false,
        error: null,
        rememberMe: action.payload.rememberMe,
      };

    case AuthActionType.CLEAR_ERROR:
      return {
        ...state,
        error: null,
      };

    case AuthActionType.REFRESH_TOKEN_SUCCESS:
      return {
        ...state,
        tokens: action.payload.tokens,
        error: null,
      };

    case AuthActionType.REFRESH_TOKEN_FAILURE:
      return {
        ...initialAuthState,
      };

    default:
      return state;
  }
}