import { useContext } from 'react';
import { AuthContext, AuthContextValue } from '../contexts/AuthContext';

/**
 * Custom hook to access authentication context
 * @returns Authentication context value
 * @throws Error if used outside AuthProvider
 */
export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  
  return context;
}