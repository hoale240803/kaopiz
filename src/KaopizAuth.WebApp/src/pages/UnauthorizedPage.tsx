import { Link } from 'react-router-dom';
import './UnauthorizedPage.css';

/**
 * Unauthorized page component for access denied scenarios
 */
export function UnauthorizedPage() {
  return (
    <div className="unauthorized-page">
      <div className="unauthorized-container">
        <div className="unauthorized-content">
          <div className="error-icon">
            <svg width="64" height="64" viewBox="0 0 24 24" fill="none">
              <path
                d="M12 2a10 10 0 1010 10A10.011 10.011 0 0012 2zm0 18a8 8 0 118-8 8.009 8.009 0 01-8 8z"
                fill="currentColor"
              />
              <path
                d="M12 6v6m0 4h.01"
                stroke="white"
                strokeWidth="2"
                strokeLinecap="round"
                strokeLinejoin="round"
              />
            </svg>
          </div>
          
          <h1 className="error-title">Access Denied</h1>
          
          <p className="error-message">
            You don't have permission to access this page. Please contact your administrator 
            if you believe this is an error.
          </p>
          
          <div className="error-actions">
            <Link to="/" className="primary-button">
              Go to Home
            </Link>
            <button 
              onClick={() => window.history.back()} 
              className="secondary-button"
            >
              Go Back
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}