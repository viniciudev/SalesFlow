import React, { useState } from 'react';
import { Router, Route, Switch } from 'react-router-dom';
import './App.scss';
import { history } from './history'
import PrivateRoute from './PrivateRoute'
import axios from 'axios';
import * as serviceWorkerRegistration from './serviceWorker'
import Loader from './utils/Loader/Loader';

const loading = () => <div className="animated fadeIn pt-3 text-center">Loading...</div>;
const DefaultLayout = React.lazy(() => import('./containers/DefaultLayout'));
const Login = React.lazy(() => import('./views/Pages/Login'));
const Register = React.lazy(() => import('./views/Pages/Register'));
const Page404 = React.lazy(() => import('./views/Pages/Page404'));
const Page500 = React.lazy(() => import('./views/Pages/Page500'));
const Closures = React.lazy(() => import('./views/Closures/closures'))

export default function App() {
  const [isLoading, setIsLoading] = useState(false);
  axios.interceptors.request.use(async (config) => {
    setIsLoading(true);
    let idCompany = localStorage.getItem('idCompany')
    if (!config.url.endsWith('/login')) {

      config.headers.tenantid = parseInt(idCompany)
    }
    return config;
  }, (error) => {
    // I cand handle a request with errors here
    return Promise.reject(error);
  });

  axios.interceptors.response.use(function (response) {
    setIsLoading(false);
    return response;
  }, function (error) {
    return Promise.reject(error);
  });

  return (
    <div >
      <Loader value={isLoading} />
      <Router history={history}>
        <React.Suspense fallback={loading()}>
          <Switch>
            <Route exact path="/login" name="Login Page" render={props => <Login {...props} />} />
            <Route exact path="/register" name="Register Page" render={props => <Register {...props} />} />
            <Route exact path="/404" name="Page 404" render={props => <Page404 {...props} />} />
            <Route exact path="/500" name="Page 500" render={props => <Page500 {...props} />} />
            <Route exact path="/register/closures/:guid" name="Fechamentos" render={props => <Closures {...props} />} />
            <PrivateRoute path="/" name="login" render={props => <DefaultLayout {...props} />} />
          </Switch>
        </React.Suspense>
      </Router>
    </div>
  );
}

