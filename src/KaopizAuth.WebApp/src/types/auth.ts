export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
}

// Authentication related TypeScript interfaces and types

/**
 * User types supported by the system
 */
export enum UserType {
  EndUser = 'EndUser',
  Admin = 'Admin',
  Partner = 'Partner'
}

/**
 * User interface representing authenticated user data
 */
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
  userType: UserType;
  isEmailConfirmed: boolean;
  createdAt: string;
  lastLoginAt?: string;
}

/**
 * Login credentials interface
 */
export interface LoginCredentials {
  email: string;
  password: string;
  rememberMe: boolean;
  userType: UserType;
}

/**
 * Registration data interface
 */
export interface RegisterData {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  userType: UserType;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface RefreshTokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface LogoutRequest {
  refreshToken: string;
  revokeAllTokens?: boolean;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  roles: string[];
}

export interface ApiResponse<T = any> {
  success: boolean;
  message: string;
  data?: T;
  errors?: Record<string, string[]>;
}

// Frontend specific types
export interface AuthContextType {
  authState: AuthState;
  login: (credentials: LoginRequest) => Promise<boolean>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<boolean>;
  isTokenExpired: () => boolean;
  hasRole: (role: string) => boolean;
  user: User;
}

export interface FormFieldError {
  [key: string]: string;
}

export interface FormState<T> {
  values: T;
  errors: FormFieldError;
  isSubmitting: boolean;
  isValid: boolean;
}

/**
 * JWT token pair interface
 */
export interface TokenPair {
  accessToken: string;
  refreshToken: string;
  accessTokenExpiresAt: string;
  refreshTokenExpiresAt: string;
}

/**
 * Authentication response from API
 */
export interface AuthResponse {
  user: User;
  tokens: TokenPair;
}

/**
 * Authentication state interface
 */
export interface AuthState {
  user: User | null;
  tokens: TokenPair | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  rememberMe: boolean;
}

/**
 * Authentication action types
 */
export enum AuthActionType {
  LOGIN_START = 'LOGIN_START',
  LOGIN_SUCCESS = 'LOGIN_SUCCESS',
  LOGIN_FAILURE = 'LOGIN_FAILURE',
  REGISTER_START = 'REGISTER_START',
  REGISTER_SUCCESS = 'REGISTER_SUCCESS',
  REGISTER_FAILURE = 'REGISTER_FAILURE',
  LOGOUT = 'LOGOUT',
  RESTORE_SESSION = 'RESTORE_SESSION',
  CLEAR_ERROR = 'CLEAR_ERROR',
  REFRESH_TOKEN_SUCCESS = 'REFRESH_TOKEN_SUCCESS',
  REFRESH_TOKEN_FAILURE = 'REFRESH_TOKEN_FAILURE'
}

/**
 * Authentication action interfaces
 */
export interface LoginStartAction {
  type: AuthActionType.LOGIN_START;
}

export interface LoginSuccessAction {
  type: AuthActionType.LOGIN_SUCCESS;
  payload: {
    user: User;
    tokens: TokenPair;
    rememberMe: boolean;
  };
}

export interface LoginFailureAction {
  type: AuthActionType.LOGIN_FAILURE;
  payload: {
    error: string;
  };
}

export interface RegisterStartAction {
  type: AuthActionType.REGISTER_START;
}

export interface RegisterSuccessAction {
  type: AuthActionType.REGISTER_SUCCESS;
  payload: {
    user: User;
    tokens: TokenPair;
  };
}

export interface RegisterFailureAction {
  type: AuthActionType.REGISTER_FAILURE;
  payload: {
    error: string;
  };
}

export interface LogoutAction {
  type: AuthActionType.LOGOUT;
}

export interface RestoreSessionAction {
  type: AuthActionType.RESTORE_SESSION;
  payload: {
    user: User;
    tokens: TokenPair;
    rememberMe: boolean;
  };
}

export interface ClearErrorAction {
  type: AuthActionType.CLEAR_ERROR;
}

export interface RefreshTokenSuccessAction {
  type: AuthActionType.REFRESH_TOKEN_SUCCESS;
  payload: {
    tokens: TokenPair;
  };
}

export interface RefreshTokenFailureAction {
  type: AuthActionType.REFRESH_TOKEN_FAILURE;
}

/**
 * Union type for all authentication actions
 */
export type AuthAction =
  | LoginStartAction
  | LoginSuccessAction
  | LoginFailureAction
  | RegisterStartAction
  | RegisterSuccessAction
  | RegisterFailureAction
  | LogoutAction
  | RestoreSessionAction
  | ClearErrorAction
  | RefreshTokenSuccessAction
  | RefreshTokenFailureAction;

/**
 * API error response interface
 */
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>;
  statusCode: number;
}