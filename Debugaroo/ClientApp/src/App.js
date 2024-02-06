import React, { Component } from 'react';
import { Route, Routes } from 'react-router-dom';
import AppRoutes from './AppRoutes';
import Admin from './layouts/Admin/Admin';
import RTL from './layouts/RTL/RTL';

export default class App extends Component {
  static displayName = App.name;

  render() {
    return (
      <RTL>
        <Routes>
          {AppRoutes.map((route, index) => {
            const { element, ...rest } = route;
            return <Route key={index} {...rest} element={element} />;
          })}
        </Routes>
      </RTL>
    );
  }
}
