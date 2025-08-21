// Authentication related types matching the backend API models

export interface LoginRequest {
  email: string;
  password: string;
  rememberMe: boolean;
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
export interface AuthState {
  isAuthenticated: boolean;
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  expiresAt: Date | null;
  isLoading: boolean;
}

export interface AuthContextType {
  authState: AuthState;
  login: (credentials: LoginRequest) => Promise<boolean>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<boolean>;
  isTokenExpired: () => boolean;
  hasRole: (role: string) => boolean;
}