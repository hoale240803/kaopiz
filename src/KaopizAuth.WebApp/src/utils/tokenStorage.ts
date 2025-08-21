import { AuthState } from '../types/auth';

const ACCESS_TOKEN_KEY = 'access_token';
const REFRESH_TOKEN_KEY = 'refresh_token';
const USER_KEY = 'user';
const EXPIRES_AT_KEY = 'expires_at';
const REMEMBER_ME_KEY = 'remember_me';

export class TokenStorage {
  static saveTokens(
    accessToken: string, 
    refreshToken: string, 
    expiresAt: Date, 
    user: any, 
    rememberMe: boolean = false
  ): void {
    const storage = rememberMe ? localStorage : sessionStorage;
    
    storage.setItem(ACCESS_TOKEN_KEY, accessToken);
    storage.setItem(REFRESH_TOKEN_KEY, refreshToken);
    storage.setItem(EXPIRES_AT_KEY, expiresAt.toISOString());
    storage.setItem(USER_KEY, JSON.stringify(user));
    
    // Save remember me preference in localStorage
    localStorage.setItem(REMEMBER_ME_KEY, rememberMe.toString());
  }

  static getTokens(): { 
    accessToken: string | null;
    refreshToken: string | null;
    expiresAt: Date | null;
    user: any | null;
  } {
    // Check if user had remember me enabled
    const rememberMe = localStorage.getItem(REMEMBER_ME_KEY) === 'true';
    const storage = rememberMe ? localStorage : sessionStorage;

    const accessToken = storage.getItem(ACCESS_TOKEN_KEY);
    const refreshToken = storage.getItem(REFRESH_TOKEN_KEY);
    const expiresAtStr = storage.getItem(EXPIRES_AT_KEY);
    const userStr = storage.getItem(USER_KEY);

    return {
      accessToken,
      refreshToken,
      expiresAt: expiresAtStr ? new Date(expiresAtStr) : null,
      user: userStr ? JSON.parse(userStr) : null,
    };
  }

  static clearTokens(): void {
    // Clear from both storages
    [localStorage, sessionStorage].forEach(storage => {
      storage.removeItem(ACCESS_TOKEN_KEY);
      storage.removeItem(REFRESH_TOKEN_KEY);
      storage.removeItem(EXPIRES_AT_KEY);
      storage.removeItem(USER_KEY);
    });
    localStorage.removeItem(REMEMBER_ME_KEY);
  }

  static isTokenExpired(expiresAt: Date): boolean {
    // Consider token expired 1 minute before actual expiry to allow for refresh
    const buffer = 60 * 1000; // 1 minute in milliseconds
    return new Date().getTime() >= (expiresAt.getTime() - buffer);
  }

  static canRefreshToken(expiresAt: Date): boolean {
    // Can refresh if token expires within 5 minutes (matching backend logic)
    const buffer = 5 * 60 * 1000; // 5 minutes in milliseconds
    return new Date().getTime() >= (expiresAt.getTime() - buffer);
  }

  static getInitialAuthState(): AuthState {
    const { accessToken, refreshToken, expiresAt, user } = this.getTokens();
    
    const isAuthenticated = !!(
      accessToken && 
      refreshToken && 
      expiresAt && 
      user && 
      !this.isTokenExpired(expiresAt)
    );

    return {
      isAuthenticated,
      user: isAuthenticated ? user : null,
      accessToken: isAuthenticated ? accessToken : null,
      refreshToken: isAuthenticated ? refreshToken : null,
      expiresAt: isAuthenticated ? expiresAt : null,
      isLoading: false,
    };
  }
}