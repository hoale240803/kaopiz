import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useAuth } from '../hooks/useAuth';
import './Navigation.css';

/**
 * Navigation component with user menu and logout functionality
 */
export function Navigation() {
  const { user, logout, isAuthenticated } = useAuth();
  const [isUserMenuOpen, setIsUserMenuOpen] = useState(false);

  const handleLogout = async () => {
    try {
      await logout();
    } catch (error) {
      console.error('Logout failed:', error);
    }
  };

  if (!isAuthenticated || !user) {
    return null;
  }

  const toggleUserMenu = () => {
    setIsUserMenuOpen(!isUserMenuOpen);
  };

  const closeUserMenu = () => {
    setIsUserMenuOpen(false);
  };

  return (
    <nav className="navigation">
      <div className="nav-container">
        <div className="nav-brand">
          <Link to="/" className="brand-link">
            Kaopiz Auth
          </Link>
        </div>

        <div className="nav-content">
          <div className="nav-links">
            <Link to="/" className="nav-link">
              Home
            </Link>
            <Link to="/test" className="nav-link">
              Test
            </Link>
            {user.userType === 'Admin' && (
              <Link to="/admin" className="nav-link">
                Admin Panel
              </Link>
            )}
          </div>

          <div className="user-menu-container">
            <button
              className="user-menu-button"
              onClick={toggleUserMenu}
              onBlur={closeUserMenu}
            >
              <div className="user-avatar">
                {user.firstName.charAt(0)}{user.lastName.charAt(0)}
              </div>
              <div className="user-info">
                <span className="user-name">{user.fullName}</span>
                <span className="user-type">{user.userType}</span>
              </div>
              <svg 
                className={`chevron-icon ${isUserMenuOpen ? 'rotated' : ''}`}
                width="16" 
                height="16" 
                viewBox="0 0 16 16" 
                fill="none"
              >
                <path 
                  d="M4 6l4 4 4-4" 
                  stroke="currentColor" 
                  strokeWidth="2" 
                  strokeLinecap="round" 
                  strokeLinejoin="round"
                />
              </svg>
            </button>

            {isUserMenuOpen && (
              <div className="user-menu-dropdown">
                <div className="menu-header">
                  <div className="menu-user-info">
                    <div className="menu-user-name">{user.fullName}</div>
                    <div className="menu-user-email">{user.email}</div>
                  </div>
                </div>
                
                <div className="menu-divider"></div>
                
                <div className="menu-items">
                  <Link to="/profile" className="menu-item">
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                      <path 
                        d="M8 8a3 3 0 100-6 3 3 0 000 6zM8 9c-2.5 0-6 1.5-6 3v1h12v-1c0-1.5-3.5-3-6-3z" 
                        fill="currentColor"
                      />
                    </svg>
                    Profile
                  </Link>
                  
                  <Link to="/settings" className="menu-item">
                    <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                      <path 
                        d="M8 10a2 2 0 100-4 2 2 0 000 4z" 
                        fill="currentColor"
                      />
                      <path 
                        d="M14 6l-1.5-1.5-1 1-2-2 1-1L9 1H7l-1.5 1.5 1 1-2 2-1-1L2 6v2l1.5 1.5 1-1 2 2-1 1L7 13h2l1.5-1.5-1-1 2-2 1 1L14 8V6z" 
                        fill="currentColor"
                      />
                    </svg>
                    Settings
                  </Link>
                </div>
                
                <div className="menu-divider"></div>
                
                <button
                  className="menu-item logout-button"
                  onClick={handleLogout}
                >
                  <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                    <path 
                      d="M6 13H3a1 1 0 01-1-1V4a1 1 0 011-1h3M11 9l3-3-3-3M14 6H6" 
                      stroke="currentColor" 
                      strokeWidth="2" 
                      strokeLinecap="round" 
                      strokeLinejoin="round"
                    />
                  </svg>
                  Sign out
                </button>
              </div>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}