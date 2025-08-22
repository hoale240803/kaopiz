import { ReactNode } from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import { UserType } from '../types/auth';

/**
 * Props for ProtectedRoute component
 */
interface ProtectedRouteProps {
  children: ReactNode;
  roles?: string[];
  userTypes?: UserType[];
  redirectTo?: string;
}

/**
 * ProtectedRoute component that guards routes based on authentication and authorization
 * @param children Child components to render if authorized
 * @param roles Required roles to access the route
 * @param userTypes Required user types to access the route
 * @param redirectTo Route to redirect to if unauthorized (defaults to /login)
 */
export function ProtectedRoute({ 
  children, 
  roles = [], 
  userTypes = [], 
  redirectTo = '/login' 
}: ProtectedRouteProps) {
  const { isAuthenticated, user, isLoading } = useAuth();
  const location = useLocation();

  // Show loading spinner while checking authentication
  if (isLoading) {
    return (
      <div className="loading-container">
        <div className="loading-spinner">Loading...</div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated || !user) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />;
  }

  // Check role-based access control
  if (roles.length > 0) {
    const hasRequiredRole = roles.some(role => user.roles.includes(role));
    if (!hasRequiredRole) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  // Check user type-based access control
  if (userTypes.length > 0) {
    const hasRequiredUserType = userTypes.includes(user.userType);
    if (!hasRequiredUserType) {
      return <Navigate to="/unauthorized" replace />;
    }
  }

  // User is authenticated and authorized, render children
  return <>{children}</>;
}