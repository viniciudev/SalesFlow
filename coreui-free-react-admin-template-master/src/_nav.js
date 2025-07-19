
export default {
  items: [
    {
      name: 'Dashboard',
      url: '/dashboard',
      icon: 'icon-speedometer',
      badge: {
        variant: 'info',
        // text: 'NEW',
      },
    },
    {
      title: true,
      name: 'Cadastros',
      wrapper: {            // optional wrapper object
        element: '',        // required valid HTML5 element tag
        attributes: {}        // optional valid JS object with JS API naming ex: { className: "my-class", style: { fontFamily: "Verdana" }, id: "my-id"}
      },
      class: ''             // optional class names space delimited list for title item ex: "text-center"
    },
    // {
    //   name: 'Arquivos',
    //   url: '/register/file',
    //   icon: 'fa fa-image',
    // },
    {
      name: 'Prospect',
      url: '/register/prospect',
      icon: 'cui-dollar',
    },
    {
      name: 'Clientes',
      url: '/register/client',
      icon: 'icon-people',
    },

    {
      name: 'Vendedor',
      url: '/register/salesman',
      icon: 'fa fa-handshake-o',
    },
    {
      name: 'Serviços',
      url: '/register/service',
      icon: 'icon-wrench',
    },

    {
      name: 'Produtos',
      url: '/register/product',
      icon: 'icon-basket-loaded',
    },

    // {
    //   name: 'Empresas',
    //   url: '/register/company',
    //   icon: 'fa fa-cubes',
    // },
    {
      title: true,
      name: 'Movimentações',
      wrapper: {            // optional wrapper object
        element: '',        // required valid HTML5 element tag
        attributes: {}        // optional valid JS object with JS API naming ex: { className: "my-class", style: { fontFamily: "Verdana" }, id: "my-id"}
      },
      class: ''             // optional class names space delimited list for title item ex: "text-center"
    },

    {
      name: 'Vendas',
      url: '/moves/sales',
      icon: 'fa fa-shopping-cart',
    },
    {
      name: 'Comissões',
      url: '/register/commission',
      icon: 'fa fa-percent',
    },
    {
      name: 'Fechamentos',
      url: '/moves/consultClosures',
      icon: "fa fa-list-ul",
    },
    // {
    //   name: 'Orçamentos',
    //   url: '/moves/budget',
    //   icon: "fa fa-list-ul",
    // },
    // {
    //   name: 'Serviços Prestados',
    //   url: '/moves/serviceProvision',
    //   icon: "fa fa-cogs",
    // }
  ],
};
