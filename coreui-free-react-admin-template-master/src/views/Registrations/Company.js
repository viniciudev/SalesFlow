import React, { Component, Suspense } from "react";
import {
  Badge,
  Button,
  Card,
  CardBody,
  CardFooter,
  CardHeader,
  Col,
  Collapse,
  DropdownItem,
  DropdownMenu,
  DropdownToggle,
  Fade,
  Form,
  FormGroup,
  FormText,
  FormFeedback,
  Input,
  InputGroup,
  InputGroupAddon,
  InputGroupButtonDropdown,
  InputGroupText,
  Label,
  Row,
  Table,
  Nav,
  NavItem,
  NavLink,
  TabContent,
  TabPane,
} from "reactstrap";
import InputMask from "react-input-mask";
import json_city from "./json_cities.json";
import Axios from "axios";
import { URL_Company } from "../../services/companyService";

class FormCompany extends Component {
  state = {
    listCities: [],
    formCompany: {
      id: 0,
      corporateName: "",
      name: "",
      cnpj: "",
      zipCode: "",
      address: "",
      neighborhood: "",
      state: "",
      city: "",
      commercialPhone: "",
      cellphone: "",
      email: "",
    },
    errors: {},
    loading: false,
  };

  buscaCidades = (e) => {
    const data = json_city.states;
    const val = e.target.value;
    if (val != "") {
      this.setValues(e, "state");
      var filterObj = data.find(function (item, i) {
        if (item.sigla === val) {
          const city = item.cidades;
          return city;
        }
      });
      this.state.listCities = filterObj.cidades;
      this.setState({
        ...this.state,
      });
    }
  };

  setValues = (e, field) => {
    const { formCompany } = this.state;
    formCompany[field] = e.target.value;
    this.setState({ formCompany });
  };

  async componentDidMount() {
    await Axios.get(URL_Company)
      .then((resp) => {
        console.log(resp.data);
        this.setState({
          formCompany: resp.data,
        });
      })
      .catch((err) => {
        console.error("Error fetching company data:", err);
      });
  }

  handleSubmit = (e) => {
    e.preventDefault();
    this.update();
  };

  update = () => {
    this.setState({ loading: true });
    console.log(this.state.formCompany);

    Axios.put(
      `${URL_Company}/${this.state.formCompany.id}`,
      this.state.formCompany
    )
      .then((resp) => {
        this.setState({ loading: false });
        alert("Empresa atualizada com sucesso!");
      })
      .catch((err) => {
        this.setState({ loading: false });
        console.error("Error updating company data:", err);
        alert("Erro ao atualizar empresa.");
      });
  };

  render() {
    return (
      <Card>
        <CardHeader>
          <strong>Cadastro</strong>
          <small> Empresa</small>
        </CardHeader>
        <CardBody>
          <Form onSubmit={this.handleSubmit}>
            <FormGroup>
              <div className="form-row">
                <div className="col-md-5">
                  <Label htmlFor="corporateName">Responsável Legal:</Label>
                  <Input
                    type="text"
                    id="corporateName"
                    className="form-control-warning"
                    value={this.state.formCompany.corporateName}
                    onChange={(e) => this.setValues(e, "corporateName")}
                  />
                </div>
                <div className="col-md-5">
                  <Label htmlFor="name">Nome Empresa:</Label>
                  <Input
                    type="text"
                    id="name"
                    className="form-control-warning"
                    value={this.state.formCompany.name}
                    onChange={(e) => this.setValues(e, "name")}
                  />
                </div>
                <div className="col-md-2">
                  <Label htmlFor="cnpj">Cnpj:</Label>
                  <Input
                    type="text"
                    id="cnpj"
                    mask="999.999.999/9999-99"
                    tag={InputMask}
                    className="form-control-warning"
                    value={this.state.formCompany.cnpj}
                    onChange={(e) => this.setValues(e, "cnpj")}
                  />
                </div>
              </div>
            </FormGroup>
            <FormGroup>
              <div className="row">
                <div className="col-md-3">
                  <Label htmlFor="zipCode">Cep:</Label>
                  <Input
                    className="form-control"
                    type="text"
                    id="zipCode"
                    mask="99.999-999"
                    tag={InputMask}
                    value={this.state.formCompany.zipCode}
                    onChange={(e) => this.setValues(e, "zipCode")}
                  />
                </div>
                <div className="col-md-6">
                  <Label htmlFor="address">Endereço:</Label>
                  <Input
                    className="form-control-warning"
                    type="text"
                    id="address"
                    value={this.state.formCompany.address}
                    onChange={(e) => this.setValues(e, "address")}
                  />
                </div>
                <div className="col-md-3">
                  <Label htmlFor="neighborhood">Bairro:</Label>
                  <Input
                    className="form-control-warning"
                    type="text"
                    id="neighborhood"
                    value={this.state.formCompany.neighborhood}
                    onChange={(e) => this.setValues(e, "neighborhood")}
                  />
                </div>
              </div>
            </FormGroup>
            <FormGroup>
              <FormGroup row className="my-0">
                <div className="col-md-2">
                  <Label htmlFor="state">Estado:</Label>
                  <Input
                    id="state"
                    name="state"
                    type="select"
                    value={this.state.formCompany.state}
                    onChange={this.buscaCidades}
                  >
                    <option value="">UF</option>
                    <option value="AC"> Acre</option>
                    <option value="AL"> Alagoas</option>
                    <option value="AP"> Amapá</option>
                    <option value="AM"> Amazonas</option>
                    <option value="BA"> Bahia</option>
                    <option value="CE"> Ceará</option>
                    <option value="DF"> Distrito Federal</option>
                    <option value="ES"> Espírito Santo</option>
                    <option value="GO"> Goiás</option>
                    <option value="MA"> Maranhão</option>
                    <option value="MT"> Mato Grosso</option>
                    <option value="MS"> Mato Grosso do Sul</option>
                    <option value="MG"> Minas Gerais</option>
                    <option value="PA"> Pará</option>
                    <option value="PB"> Paraíba</option>
                    <option value="PR"> Paraná</option>
                    <option value="PE"> Pernambuco</option>
                    <option value="PI"> Piauí</option>
                    <option value="RJ"> Rio de Janeiro</option>
                    <option value="RN"> Rio Grande do Norte</option>
                    <option value="RS"> Rio Grande do Sul</option>
                    <option value="RO"> Rondônia</option>
                    <option value="RR"> Roraima</option>
                    <option value="SC"> Santa Catarina</option>
                    <option value="SP"> São Paulo</option>
                    <option value="SE"> Sergipe</option>
                    <option value="TO"> Tocantins</option>
                  </Input>
                </div>
                <div className="col-md-4">
                  <FormGroup>
                    <Label htmlFor="city">Cidade:</Label>
                    <Input
                      id="city"
                      name="city"
                      type="select"
                      value={this.state.formCompany.city}
                      onChange={(e) => this.setValues(e, "city")}
                    >
                      <option value="">Selecione uma cidade</option>
                      {this.state.listCities.map((city, index) => (
                        <option key={index} value={city}>
                          {city}
                        </option>
                      ))}
                    </Input>
                  </FormGroup>
                </div>
                <div className="col-md-3">
                  <Label htmlFor="commercialPhone">Telefone Comercial:</Label>
                  <Input
                    id="commercialPhone"
                    name="commercialPhone"
                    type="text"
                    mask="(99) 9999-9999"
                    tag={InputMask}
                    value={this.state.formCompany.commercialPhone}
                    onChange={(e) => this.setValues(e, "commercialPhone")}
                  />
                </div>
                <div className="col-md-3">
                  <Label htmlFor="cellphone">Telefone Celular:</Label>
                  <Input
                    id="cellphone"
                    name="cellphone"
                    type="text"
                    mask="(99)9 9999-9999"
                    tag={InputMask}
                    value={this.state.formCompany.cellphone}
                    onChange={(e) => this.setValues(e, "cellphone")}
                  />
                </div>
              </FormGroup>
              <FormGroup>
                <div className="form-row">
                  <div className="col-md-6">
                    <Label htmlFor="email">Email:</Label>
                    <Input
                      className="form-control-warning"
                      type="text"
                      id="email"
                      value={this.state.formCompany.email}
                      onChange={(e) => this.setValues(e, "email")}
                    />
                  </div>
                </div>
              </FormGroup>
            </FormGroup>
            <Button
              type="submit"
              size="sm"
              color="success"
              disabled={this.state.loading}
            >
              <i className="fa fa-dot-circle-o"></i>{" "}
              {this.state.loading ? "Salvando..." : "Salvar"}
            </Button>
            <p className="float-right text-sm">
              <i>Os campos marcados com (*) são obrigatórios</i>
            </p>
          </Form>
        </CardBody>
      </Card>
    );
  }
}

class ListFormCompany extends Component {
  render() {
    return (
      <Card>
        <CardHeader>
          <strong>Consultar</strong>
          <small> Empresas Cadastrados</small>
        </CardHeader>
        <CardBody>
          <Table responsive size="sm">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Telefone</th>
                <th>Opções</th>
              </tr>
            </thead>
            <tbody>
              <tr>
                <td>Carwyn Fachtna</td>
                <td>(34) 99229-5856</td>
                <td>
                  <Button color="secondary" outline>
                    <i className="cui-pencil"></i>&nbsp;Editar
                  </Button>
                </td>
              </tr>
            </tbody>
          </Table>
        </CardBody>
      </Card>
    );
  }
}

class FormUser extends Component {
  render() {
    return (
      <Card>
        <CardBody>
          <Form></Form>
        </CardBody>
      </Card>
    );
  }
}

export default class Company extends Component {
  state = {
    activeTab: "con",
  };

  toggleTab = (tab) => {
    if (this.state.activeTab !== tab) {
      this.setState({
        activeTab: tab,
      });
    }
  };

  render() {
    return (
      <div>
        <FormCompany />
      </div>
    );
  }
}
