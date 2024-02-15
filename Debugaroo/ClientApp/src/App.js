import React, { Component } from 'react';
import { BrowserRouter, Route, Routes, Navigate } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import Admin from './layouts/Admin/Admin';
import RTL from './layouts/RTL/RTL';
import Login from './components/Login';
import { AuthProvider } from './contexts/AuthContext';
import ProtectedRoute from './components/ProtectedRoute';

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/admin/*" element={<ProtectedRoute><Admin /></ProtectedRoute>} />
          <Route path="/" element={<Navigate to="/login" />} />
          <Route path="*" element={<Navigate to="/admin/dashboard" replace />} />
        </Routes>
    </AuthProvider>
    );
  }
}

