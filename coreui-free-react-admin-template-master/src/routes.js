import React from 'react';


const Dashboard = React.lazy(() => import('./views/Dashboard'));
const Users = React.lazy(() => import('./views/Users/Users'));
const User = React.lazy(() => import('./views/Users/User'));
const Files = React.lazy(() => import('./views/Registrations/File'));
const Client = React.lazy(() => import('./views/Registrations/Client'));
const Prospect = React.lazy(() => import('./views/Registrations/Prospect'));
const Salesman = React.lazy(() => import('./views/Registrations/Salesman'));
const Service = React.lazy(() => import('./views/Registrations/Service'));
const Product = React.lazy(() => import('./views/Registrations/Product'));
const Company = React.lazy(() => import('./views/Registrations/Company'));
const Sale = React.lazy(() => import('./views/Moves/Sale'));
const Budget = React.lazy(() => import('./views/Moves/budget'));
const ServiceProvision = React.lazy(() => import('./views/Moves/serviceProvision'));
const Commission = React.lazy(() => import('./views/Moves/Commission'));
const ConsultClosures = React.lazy(() => import('./views/Moves/consultClosures'));
//icons-----
const CoreUIIcons = React.lazy(() => import('./views/Icons/CoreUIIcons'));
const Flags = React.lazy(() => import('./views/Icons/Flags'));
const FontAwesome = React.lazy(() => import('./views/Icons/FontAwesome'));
const SimpleLineIcons = React.lazy(() => import('./views/Icons/SimpleLineIcons'));
// const Locks = React.lazy(() => import('./views/Locks/locks'))


// https://github.com/ReactTraining/react-router/tree/master/packages/react-router-config
const routes = [
  { path: '/', exact: true, name: 'login' },
  { path: '/dashboard', name: 'Dashboard', component: Dashboard },
  { path: '/register/file', name: 'Arquivos', component: Files },
  { path: '/register/client', name: 'Clientes', component: Client },
  { path: '/register/prospect', name: 'Prospect', component: Prospect },
  { path: '/register/salesman', name: 'Vendedor', component: Salesman },
  { path: '/register/service', name: 'Serviço', component: Service },
  { path: '/register/product', name: 'Produto', component: Product },
  { path: '/register/company', name: 'Empresa', component: Company },
  { path: '/moves/sales', name: 'Vendas', component: Sale },
  { path: '/moves/budget', name: 'budget', component: Budget },
  { path: '/moves/serviceProvision', name: 'ServiceProvision', component: ServiceProvision },
  { path: '/moves/consultClosures', name: 'closures', component: ConsultClosures },
  { path: '/users', exact: true, name: 'Users', component: Users },
  { path: '/users/:id', exact: true, name: 'User Details', component: User },
  { path: '/register/commission', name: 'Comissões', component: Commission },
  //icons---
  { path: '/icons', exact: true, name: 'Icons', component: CoreUIIcons },
  { path: '/icons/coreui-icons', name: 'CoreUI Icons', component: CoreUIIcons },
  { path: '/icons/flags', name: 'Flags', component: Flags },
  { path: '/icons/font-awesome', name: 'Font Awesome', component: FontAwesome },
  { path: '/icons/simple-line-icons', name: 'Simple Line Icons', component: SimpleLineIcons },
  // { path: '/register/locks', name: 'Fechamentos', component: Locks },
];

export default routes;
