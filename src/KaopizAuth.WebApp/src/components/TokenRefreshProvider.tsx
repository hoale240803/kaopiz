import { useEffect, useCallback } from 'react';
import { useAuth } from '../hooks/useAuth';
import { AuthService } from '../services/authService';
import { TokenStorage } from '../utils/tokenStorage';

/**
 * Token refresh configuration
 */
const TOKEN_REFRESH_CONFIG = {
  // Check token validity every 5 minutes
  CHECK_INTERVAL: 5 * 60 * 1000,
  // Refresh token if it expires within 15 minutes
  REFRESH_THRESHOLD: 15 * 60 * 1000,
  // Maximum number of refresh attempts
  MAX_RETRY_ATTEMPTS: 3,
} as const;

/**
 * TokenRefreshProvider component that handles automatic token refresh
 */
export function TokenRefreshProvider({ children }: { children: React.ReactNode }) {
  const { isAuthenticated, logout } = useAuth();

  /**
   * Check if token needs refresh and attempt to refresh it
   */
  const checkAndRefreshToken = useCallback(async () => {
    if (!isAuthenticated) {
      return;
    }

    try {
      const storedSession = TokenStorage.getStoredSession();
      
      if (!storedSession?.tokens) {
        return;
      }

      const { accessTokenExpiresAt } = storedSession.tokens;
      const expiryTime = new Date(accessTokenExpiresAt).getTime();
      const currentTime = Date.now();
      const timeUntilExpiry = expiryTime - currentTime;

      // If token expires within the threshold, refresh it
      if (timeUntilExpiry <= TOKEN_REFRESH_CONFIG.REFRESH_THRESHOLD) {
        console.log('Access token expires soon, attempting refresh...');
        
        let retryCount = 0;
        let refreshSuccessful = false;

        while (retryCount < TOKEN_REFRESH_CONFIG.MAX_RETRY_ATTEMPTS && !refreshSuccessful) {
          try {
            const newTokens = await AuthService.refreshToken();
            TokenStorage.updateTokens(newTokens);
            refreshSuccessful = true;
            
            console.log('Token refresh successful');
          } catch (error) {
            retryCount++;
            console.warn(`Token refresh attempt ${retryCount} failed:`, error);
            
            if (retryCount >= TOKEN_REFRESH_CONFIG.MAX_RETRY_ATTEMPTS) {
              console.error('Max refresh attempts reached, logging out user');
              await logout();
              break;
            }
            
            // Wait before retry (exponential backoff)
            await new Promise(resolve => setTimeout(resolve, Math.pow(2, retryCount) * 1000));
          }
        }
      }
    } catch (error) {
      console.error('Error during token refresh check:', error);
    }
  }, [isAuthenticated, logout]);

  /**
   * Handle page visibility change to check tokens when user returns to tab
   */
  const handleVisibilityChange = useCallback(() => {
    if (document.visibilityState === 'visible' && isAuthenticated) {
      checkAndRefreshToken();
    }
  }, [checkAndRefreshToken, isAuthenticated]);

  /**
   * Handle window focus to check tokens when user focuses the window
   */
  const handleWindowFocus = useCallback(() => {
    if (isAuthenticated) {
      checkAndRefreshToken();
    }
  }, [checkAndRefreshToken, isAuthenticated]);

  /**
   * Set up automatic token refresh
   */
  useEffect(() => {
    if (!isAuthenticated) {
      return;
    }

    // Initial check
    checkAndRefreshToken();

    // Set up periodic checks
    const intervalId = setInterval(checkAndRefreshToken, TOKEN_REFRESH_CONFIG.CHECK_INTERVAL);

    // Set up event listeners for when user returns to the page
    document.addEventListener('visibilitychange', handleVisibilityChange);
    window.addEventListener('focus', handleWindowFocus);

    // Cleanup
    return () => {
      clearInterval(intervalId);
      document.removeEventListener('visibilitychange', handleVisibilityChange);
      window.removeEventListener('focus', handleWindowFocus);
    };
  }, [isAuthenticated, checkAndRefreshToken, handleVisibilityChange, handleWindowFocus]);

  /**
   * Handle network online/offline events
   */
  useEffect(() => {
    const handleOnline = () => {
      if (isAuthenticated) {
        console.log('Network restored, checking token status...');
        checkAndRefreshToken();
      }
    };

    const handleOffline = () => {
      console.log('Network lost, token refresh will be paused');
    };

    window.addEventListener('online', handleOnline);
    window.addEventListener('offline', handleOffline);

    return () => {
      window.removeEventListener('online', handleOnline);
      window.removeEventListener('offline', handleOffline);
    };
  }, [isAuthenticated, checkAndRefreshToken]);

  return <>{children}</>;
}