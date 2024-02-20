import React, { createContext, useContext, useState } from 'react';

const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [authToken, setAuthToken] = useState(() => localStorage.getItem('token'));

  const login = async (username, password) => {
    try {
        //TODO:Change env variable this for production
        const response = await fetch('https://localhost:5001/Auth/Login', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ Email: username, Password: password }),
        });
        if (response.ok) {
          const responseWithTokenData = await response.json();
          // Handle successful login (e.g., update state, store token)
          setIsAuthenticated(true);
          setAuthToken(responseWithTokenData.token);
        } else {
          // Handle errors or unsuccessful login attempts
          throw new Error('Login failed');
        }
      } catch (error) {
        console.error(error);
        throw error; // Rethrow to be handled in Login.js
      }
  };

  const logout = () => {
    setIsAuthenticated(false);
  };

  return (
    <AuthContext.Provider value={{ isAuthenticated, login, logout }}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);