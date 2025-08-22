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
      isLoading: false
    }
  }
import { TokenPair, User } from '../types/auth';

/**
 * Storage keys for authentication data
 */
const STORAGE_KEYS = {
  ACCESS_TOKEN: 'kaopiz_access_token',
  REFRESH_TOKEN: 'kaopiz_refresh_token',
  ACCESS_TOKEN_EXPIRES_AT: 'kaopiz_access_token_expires_at',
  REFRESH_TOKEN_EXPIRES_AT: 'kaopiz_refresh_token_expires_at',
  USER_DATA: 'kaopiz_user_data',
  REMEMBER_ME: 'kaopiz_remember_me'
} as const;

/**
 * Storage change event name for cross-tab synchronization
 */
export const STORAGE_CHANGE_EVENT = 'kaopiz_auth_storage_change';

/**
 * Stored authentication session interface
 */
interface StoredSession {
  user: User;
  tokens: TokenPair;
  rememberMe: boolean;
}

/**
 * Token storage utility class for secure token management
 */
export class TokenStorage {
  /**
   * Store authentication session data
   * @param user User data
   * @param tokens Token pair
   * @param rememberMe Whether to persist session
   */
  static storeSession(user: User, tokens: TokenPair, rememberMe: boolean): void {
    try {
      const storage = rememberMe ? localStorage : sessionStorage;
      
      // Store tokens
      storage.setItem(STORAGE_KEYS.ACCESS_TOKEN, tokens.accessToken);
      storage.setItem(STORAGE_KEYS.REFRESH_TOKEN, tokens.refreshToken);
      storage.setItem(STORAGE_KEYS.ACCESS_TOKEN_EXPIRES_AT, tokens.accessTokenExpiresAt);
      storage.setItem(STORAGE_KEYS.REFRESH_TOKEN_EXPIRES_AT, tokens.refreshTokenExpiresAt);
      
      // Store user data
      storage.setItem(STORAGE_KEYS.USER_DATA, JSON.stringify(user));
      storage.setItem(STORAGE_KEYS.REMEMBER_ME, JSON.stringify(rememberMe));
      
      // Dispatch custom event for cross-tab synchronization
      this.dispatchStorageChangeEvent('session_stored', { user, tokens, rememberMe });
    } catch (error) {
      console.error('Failed to store authentication session:', error);
    }
  }

  /**
   * Retrieve stored authentication session
   * @returns Stored session data or null if not found
   */
  static getStoredSession(): StoredSession | null {
    try {
      // Try localStorage first (remember me), then sessionStorage
      const storages = [localStorage, sessionStorage];
      
      for (const storage of storages) {
        const accessToken = storage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
        const refreshToken = storage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
        const accessTokenExpiresAt = storage.getItem(STORAGE_KEYS.ACCESS_TOKEN_EXPIRES_AT);
        const refreshTokenExpiresAt = storage.getItem(STORAGE_KEYS.REFRESH_TOKEN_EXPIRES_AT);
        const userDataStr = storage.getItem(STORAGE_KEYS.USER_DATA);
        const rememberMeStr = storage.getItem(STORAGE_KEYS.REMEMBER_ME);
        
        if (accessToken && refreshToken && accessTokenExpiresAt && refreshTokenExpiresAt && userDataStr) {
          const user = JSON.parse(userDataStr) as User;
          const rememberMe = rememberMeStr ? JSON.parse(rememberMeStr) : false;
          
          const tokens: TokenPair = {
            accessToken,
            refreshToken,
            accessTokenExpiresAt,
            refreshTokenExpiresAt
          };
          
          // Check if refresh token is still valid
          if (this.isTokenValid(tokens.refreshTokenExpiresAt)) {
            return { user, tokens, rememberMe };
          } else {
            // Clean up expired session
            this.clearSession();
          }
        }
      }
      
      return null;
    } catch (error) {
      console.error('Failed to retrieve stored session:', error);
      return null;
    }
  }

  /**
   * Update stored tokens
   * @param tokens New token pair
   */
  static updateTokens(tokens: TokenPair): void {
    try {
      const rememberMeStr = localStorage.getItem(STORAGE_KEYS.REMEMBER_ME) || 
                           sessionStorage.getItem(STORAGE_KEYS.REMEMBER_ME);
      const rememberMe = rememberMeStr ? JSON.parse(rememberMeStr) : false;
      const storage = rememberMe ? localStorage : sessionStorage;
      
      storage.setItem(STORAGE_KEYS.ACCESS_TOKEN, tokens.accessToken);
      storage.setItem(STORAGE_KEYS.REFRESH_TOKEN, tokens.refreshToken);
      storage.setItem(STORAGE_KEYS.ACCESS_TOKEN_EXPIRES_AT, tokens.accessTokenExpiresAt);
      storage.setItem(STORAGE_KEYS.REFRESH_TOKEN_EXPIRES_AT, tokens.refreshTokenExpiresAt);
      
      // Dispatch event for cross-tab sync
      this.dispatchStorageChangeEvent('tokens_updated', { tokens });
    } catch (error) {
      console.error('Failed to update tokens:', error);
    }
  }

  /**
   * Clear all stored authentication data
   */
  static clearSession(): void {
    try {
      // Clear from both storages
      const storages = [localStorage, sessionStorage];
      
      storages.forEach(storage => {
        Object.values(STORAGE_KEYS).forEach(key => {
          storage.removeItem(key);
        });
      });
      
      // Dispatch event for cross-tab sync
      this.dispatchStorageChangeEvent('session_cleared', {});
    } catch (error) {
      console.error('Failed to clear session:', error);
    }
  }

  /**
   * Get access token from storage
   * @returns Access token or null
   */
  static getAccessToken(): string | null {
    try {
      return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN) || 
             sessionStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
    } catch (error) {
      console.error('Failed to get access token:', error);
      return null;
    }
  }

  /**
   * Get refresh token from storage
   * @returns Refresh token or null
   */
  static getRefreshToken(): string | null {
    try {
      return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN) || 
             sessionStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
    } catch (error) {
      console.error('Failed to get refresh token:', error);
      return null;
    }
  }

  /**
   * Check if access token is still valid (not expired)
   * @returns True if token is valid
   */
  static isAccessTokenValid(): boolean {
    try {
      const expiresAt = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN_EXPIRES_AT) || 
                       sessionStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN_EXPIRES_AT);
      
      if (!expiresAt) return false;
      
      return this.isTokenValid(expiresAt);
    } catch (error) {
      console.error('Failed to check access token validity:', error);
      return false;
    }
  }

  /**
   * Check if refresh token is still valid (not expired)
   * @returns True if token is valid
   */
  static isRefreshTokenValid(): boolean {
    try {
      const expiresAt = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN_EXPIRES_AT) || 
                       sessionStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN_EXPIRES_AT);
      
      if (!expiresAt) return false;
      
      return this.isTokenValid(expiresAt);
    } catch (error) {
      console.error('Failed to check refresh token validity:', error);
      return false;
    }
  }

  /**
   * Check if a token expiry time is still valid
   * @param expiresAt ISO string of expiry time
   * @returns True if token is still valid
   */
  private static isTokenValid(expiresAt: string): boolean {
    try {
      const expiryTime = new Date(expiresAt).getTime();
      const currentTime = Date.now();
      const bufferTime = 60000; // 1 minute buffer
      
      return expiryTime > (currentTime + bufferTime);
    } catch (error) {
      console.error('Failed to parse token expiry time:', error);
      return false;
    }
  }

  /**
   * Dispatch custom storage change event for cross-tab synchronization
   * @param action Action that occurred
   * @param data Associated data
   */
  private static dispatchStorageChangeEvent(action: string, data: any): void {
    try {
      const event = new CustomEvent(STORAGE_CHANGE_EVENT, {
        detail: { action, data, timestamp: Date.now() }
      });
      
      window.dispatchEvent(event);
    } catch (error) {
      console.error('Failed to dispatch storage change event:', error);
    }
  }

  /**
   * Add listener for storage change events (cross-tab sync)
   * @param callback Function to call when storage changes
   * @returns Cleanup function to remove listener
   */
  static addStorageChangeListener(callback: (event: CustomEvent) => void): () => void {
    const handleStorageChange = (event: CustomEvent) => {
      callback(event);
    };

    const handleBrowserStorageChange = (event: StorageEvent) => {
      // Handle native storage events from other tabs
      if (event.key && Object.values(STORAGE_KEYS).includes(event.key as any)) {
        const customEvent = new CustomEvent(STORAGE_CHANGE_EVENT, {
          detail: { 
            action: event.newValue ? 'session_stored' : 'session_cleared',
            data: {},
            timestamp: Date.now()
          }
        });
        
        handleStorageChange(customEvent);
      }
    };

    window.addEventListener(STORAGE_CHANGE_EVENT, handleStorageChange as EventListener);
    window.addEventListener('storage', handleBrowserStorageChange);

    // Return cleanup function
    return () => {
      window.removeEventListener(STORAGE_CHANGE_EVENT, handleStorageChange as EventListener);
      window.removeEventListener('storage', handleBrowserStorageChange);
    };
  }
}