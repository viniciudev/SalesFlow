// import React, { Component, Suspense } from 'react';
// import {

//     Button,
//     Card,
//     CardBody,
//     CardHeader,
//     Form,
//     FormGroup,
//     FormFeedback,
//     Input,
//     Label,
//     Table
// } from 'reactstrap';
// import CurrencyInput from 'react-currency-input';
// import ConvertToUSD from './../../ConvertCurrency';
// import Axios from 'axios';
// import { URL_Service } from './../../services/serviceProvidedService';
// import swal from 'sweetalert';
// import NumberFormat from 'react-number-format';
// import PubSub from 'pubsub-js';
// import { FaSpinner } from 'react-icons/fa'
// class FormService extends Component {

//     state = {
//         errors: {},
//         modelService: { id: 0, name: '', value: '' },
//         loading: false
//     }
//     setValues = (e, field) => {
//         const { modelService } = this.state;
//         modelService[field] = e.target.value;
//         this.setState({ modelService });
//     }

//     validate = () => {
//         let isError = 0;
//         const dados = this.state.modelService
//         const errors = {}

//         if (!dados.name) {
//             isError++;
//             errors.nameError = true;
//         }
//         else
//             errors.nameError = false;

//         this.setState({
//             errors
//         });

//         return isError;
//     }
//     clear() {
//         this.setState({ modelService: { id: 0, name: '', value: '' } })
//     }

//     save = async () => {
//         if (this.validate() == 0) {
//             this.setState({ loading: true })
//             const { modelService } = this.state
//             const valueService = ConvertToUSD(modelService.value)
//             let data = {
//                 idCompany: modelService.idCompany,
//                 id: modelService.id,
//                 name: modelService.name,
//                 value: parseFloat(valueService)
//             }
//             if (data.id > 0) {
//                 await Axios.put(URL_Service, data).then(resp => {
//                     const { data } = resp
//                     if (data) {
//                         swal('Atualizado com Sucesso!', {
//                             icon: 'success'
//                         }).then(r => {
//                             if (r)
//                                 this.clear();
//                             this.props.consultAll();
//                         })
//                     }
//                 }).catch(() => { this.setState({ loading: false }) })
//             } else {
//                 await Axios.post(URL_Service, data).then(resp => {
//                     const { data } = resp
//                     if (data) {
//                         swal('Salvo com Sucesso!', {
//                             icon: 'success'
//                         }).then(r => {
//                             if (r)
//                                 this.clear();
//                             this.props.consultAll();
//                         })
//                     }
//                 }).catch(() => { this.setState({ loading: false }) })
//             }
//             this.setState({ loading: false })
//         }
//     }
//     componentWillMount() {
//         PubSub.subscribe('edit-service', (topic, service) => {

//             this.setState({
//                 modelService: service,
//             })
//         })
//     }

//     render() {
//         const { modelService, errors, loading } = this.state
//         return (
//             <Card>
//                 <CardHeader>
//                     <strong>Cadastro</strong>
//                     <small> Serviços</small>
//                 </CardHeader>
//                 <CardBody>
//                     <Form>
//                         <FormGroup>
//                             <div className='form-row'>
//                                 <div className="col-md-2">
//                                     <Label htmlFor="name">Id:</Label>
//                                     <Input
//                                         type="text"
//                                         value={modelService.id}
//                                         disabled
//                                     />
//                                 </div>
//                                 <div className="col-md-8">
//                                     <Label htmlFor="name">Nome Serviço:*</Label>
//                                     <Input
//                                         type="text"
//                                         onChange={e => this.setValues(e, 'name')}
//                                         value={modelService.name}
//                                         invalid={errors.nameError}
//                                     />
//                                     <FormFeedback></FormFeedback>
//                                 </div>
//                                 <div className="col-md-2">
//                                     <Label htmlFor="serviceValue">Valor Serviço:</Label>
//                                     <CurrencyInput
//                                         className="form-control"
//                                         type="text"
//                                         decimalSeparator=","
//                                         thousandSeparator="."
//                                         prefix="R$"
//                                         onChangeEvent={e => this.setValues(e, 'value')}
//                                         value={modelService.value}
//                                     >
//                                     </CurrencyInput>
//                                 </div>
//                             </div>
//                         </FormGroup>
//                         <Button
//                             size="sm"
//                             onClick={e => this.save()}
//                             color="success"
//                             disabled={loading}
//                         >
//                             {loading && <FaSpinner className='fa fa-spinner fa-spin' />}
//                             {loading && " Salvando..."}
//                             {!loading && <i className="fa fa-dot-circle-o"></i>}
//                             {!loading && " Salvar"}
//                         </Button>
//                         <p className="float-right text-sm">
//                             <i>Os campos marcados com (*) são obrigatórios</i>
//                         </p>
//                     </Form>
//                 </CardBody>
//             </Card>
//         )
//     }
// }

// class ListFormService extends Component {
//     onEdit = (service) => {
//         PubSub.publish('edit-service', service)
//     }

//     render() {
//         const { formService } = this.props
//         return (
//             <Card>
//                 <CardHeader>
//                     <strong>Consultar</strong>
//                     <small> Serviços Cadastrados</small>
//                 </CardHeader>
//                 <CardBody>
//                     <Table responsive size="sm">
//                         <thead>
//                             <tr>
//                                 <th>Nome</th>
//                                 <th>Valor</th>
//                                 <th>Opções</th>
//                             </tr>
//                         </thead>
//                         <tbody>
//                             {
//                                 formService.results.map(s => (
//                                     <tr key={s.id}>
//                                         <td>{s.name}</td>
//                                         <td> <NumberFormat
//                                             displayType={'text'}
//                                             value={s.value}
//                                             thousandSeparator={'.'}
//                                             decimalSeparator={','}
//                                             prefix={'R$'}
//                                         /></td>
//                                         <td>
//                                             <Button onClick={e => this.onEdit(s)} color="secondary" outline>
//                                                 <i className="cui-pencil"></i>
//                                             </Button>
//                                         </td>
//                                     </tr>
//                                 ))
//                             }

//                         </tbody>
//                     </Table>
//                 </CardBody>
//             </Card>
//         )
//     }
// }

// export default class Service extends Component {

//     state = {
//         formService: { results: [], currentPage: '', pageCount: '', pageSize: '' }
//     }

//     componentDidMount() {
//         this.consultAll()
//     }

//     consultAll = async () => {
//         await Axios.get(URL_Service).then(resp => {
//             const { data } = resp
//             if (data) {
//                 this.setState({
//                     formService: data
//                 })
//             }
//         })
//     }

//     render() {
//         return (
//             <div>
//                 <div className="row">

//                     <div className="col-md-4 my-3">
//                         <ListFormService
//                             formService={this.state.formService}
//                         />

//                     </div>

//                     <div className="col-md-8 my-3" >
//                         <FormService
//                             consultAll={this.consultAll} />
//                     </div>
//                 </div>
//             </div>
//         )

//     }
// }
import React, { Component, Suspense } from "react";
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  Form,
  FormGroup,
  FormFeedback,
  Input,
  Label,
  Table,
  Row,
  Col,
  Badge,
} from "reactstrap";
import CurrencyInput from "react-currency-input";
import ConvertToUSD from "./../../ConvertCurrency";
import Axios from "axios";
import { URL_Service } from "./../../services/serviceProvidedService";
import swal from "sweetalert";
import NumberFormat from "react-number-format";
import PubSub from "pubsub-js";
import { FaSpinner, FaPlus, FaTimes } from "react-icons/fa";

class FormService extends Component {
  state = {
    errors: {},
    modelService: {
      id: 0,
      name: "",
      value: "",
      deadline: "",
      capacity: "",
      experience: "",
      details: [],
    },
    newDetail: "",
    loading: false,
  };

  setValues = (e, field) => {
    const { modelService } = this.state;
    modelService[field] = e.target.value;
    this.setState({ modelService });
  };

  handleDetailChange = (e) => {
    this.setState({ newDetail: e.target.value });
  };

  addDetail = () => {
    const { newDetail, modelService } = this.state;
    if (newDetail.trim() !== "") {
      const updatedDetails = [...modelService.details, newDetail.trim()];
      this.setState({
        modelService: { ...modelService, details: updatedDetails },
        newDetail: "",
      });
    }
  };

  removeDetail = (index) => {
    const { modelService } = this.state;
    const updatedDetails = [...modelService.details];
    updatedDetails.splice(index, 1);
    this.setState({
      modelService: { ...modelService, details: updatedDetails },
    });
  };

  validate = () => {
    let isError = 0;
    const dados = this.state.modelService;
    const errors = {};

    if (!dados.name) {
      isError++;
      errors.nameError = true;
    } else {
      errors.nameError = false;
    }

    if (!dados.deadline) {
      isError++;
      errors.deadlineError = true;
    } else {
      errors.deadlineError = false;
    }

    this.setState({ errors });
    return isError;
  };

  clear() {
    this.setState({
      modelService: {
        id: 0,
        name: "",
        value: "",
        deadline: "",
        capacity: "",
        experience: "",
        details: [],
      },
      newDetail: "",
    });
  }

  save = async () => {
    if (this.validate() === 0) {
      this.setState({ loading: true });
      const { modelService } = this.state;
      const valueService = ConvertToUSD(modelService.value);

      let data = {
        idCompany: modelService.idCompany,
        id: modelService.id,
        name: modelService.name,
        value: parseFloat(valueService),
        deadline: modelService.deadline,
        capacity: modelService.capacity,
        experience: modelService.experience,
        details: modelService.details,
      };

      try {
        if (data.id > 0) {
          await Axios.put(URL_Service, data);
          swal("Atualizado com Sucesso!", {
            icon: "success",
          }).then((r) => {
            if (r) this.clear();
            this.props.consultAll();
          });
        } else {
          await Axios.post(URL_Service, data);
          swal("Salvo com Sucesso!", {
            icon: "success",
          }).then((r) => {
            if (r) this.clear();
            this.props.consultAll();
          });
        }
      } catch (error) {
        console.error("Erro ao salvar serviço:", error);
        swal("Erro ao salvar serviço", {
          icon: "error",
        });
      } finally {
        this.setState({ loading: false });
      }
    }
  };

  componentWillMount() {
    PubSub.subscribe("edit-service", (topic, service) => {
      this.setState({
        modelService: {
          ...this.state.modelService,
          ...service,
          details: service.details || [],
        },
      });
    });
  }

  render() {
    const { modelService, errors, loading, newDetail } = this.state;

    return (
      <Card>
        <CardHeader>
          <strong>Cadastro</strong>
          <small> Serviços</small>
        </CardHeader>
        <CardBody>
          <Form>
            <FormGroup>
              <Row>
                <Col md={2}>
                  <Label htmlFor="id">Id:</Label>
                  <Input type="text" value={modelService.id} disabled />
                </Col>
                <Col md={5}>
                  <Label htmlFor="name">Nome Serviço:*</Label>
                  <Input
                    type="text"
                    onChange={(e) => this.setValues(e, "name")}
                    value={modelService.name}
                    invalid={errors.nameError}
                  />
                  <FormFeedback>Campo obrigatório</FormFeedback>
                </Col>
                <Col md={5}>
                  <Label htmlFor="serviceValue">Valor Serviço:</Label>
                  <CurrencyInput
                    className="form-control"
                    type="text"
                    decimalSeparator=","
                    thousandSeparator="."
                    prefix="R$"
                    onChangeEvent={(e) => this.setValues(e, "value")}
                    value={modelService.value}
                  />
                </Col>
              </Row>
            </FormGroup>

            <FormGroup>
              <Row>
                <Col md={4}>
                  <Label htmlFor="deadline">Prazo:*</Label>
                  <Input
                    type="date"
                    onChange={(e) => this.setValues(e, "deadline")}
                    value={modelService.deadline}
                    invalid={errors.deadlineError}
                  />
                  <FormFeedback>Campo obrigatório</FormFeedback>
                </Col>
                <Col md={4}>
                  <Label htmlFor="capacity">Capacidade:</Label>
                  <Input
                    type="text"
                    onChange={(e) => this.setValues(e, "capacity")}
                    value={modelService.capacity}
                    placeholder="Ex: 10 unidades/dia"
                  />
                </Col>
                <Col md={4}>
                  <Label htmlFor="experience">Experiência:</Label>
                  <Input
                    type="text"
                    onChange={(e) => this.setValues(e, "experience")}
                    value={modelService.experience}
                    placeholder="Ex: 5 anos no mercado"
                  />
                </Col>
              </Row>
            </FormGroup>

            <FormGroup>
              <Label htmlFor="details">Detalhes:</Label>
              <Row className="mb-2">
                <Col md={10}>
                  <Input
                    type="text"
                    value={newDetail}
                    onChange={this.handleDetailChange}
                    placeholder="Adicionar novo detalhe"
                  />
                </Col>
                <Col md={2}>
                  <Button
                    color="primary"
                    onClick={this.addDetail}
                    disabled={!newDetail.trim()}
                  >
                    <FaPlus /> Adicionar
                  </Button>
                </Col>
              </Row>

              <div className="details-container">
                {modelService.details.map((detail, index) => (
                  <Badge
                    key={index}
                    color="secondary"
                    className="mr-2 mb-2 p-2 detail-badge"
                  >
                    {detail}
                    <FaTimes
                      className="ml-2 cursor-pointer"
                      onClick={() => this.removeDetail(index)}
                    />
                  </Badge>
                ))}

                {modelService.details.length === 0 && (
                  <p className="text-muted">Nenhum detalhe adicionado</p>
                )}
              </div>
            </FormGroup>

            <Button
              size="sm"
              onClick={this.save}
              color="success"
              disabled={loading}
            >
              {loading && <FaSpinner className="fa fa-spinner fa-spin" />}
              {loading && " Salvando..."}
              {!loading && <i className="fa fa-dot-circle-o"></i>}
              {!loading && " Salvar"}
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

class ListFormService extends Component {
  onEdit = (service) => {
    PubSub.publish("edit-service", service);
  };

  render() {
    const { formService } = this.props;

    return (
      <Card>
        <CardHeader>
          <strong>Consultar</strong>
          <small> Serviços Cadastrados</small>
        </CardHeader>
        <CardBody>
          <Table responsive size="sm">
            <thead>
              <tr>
                <th>Nome</th>
                <th>Valor</th>
                <th>Prazo</th>
                <th>Capacidade</th>
                <th>Experiência</th>
                <th>Detalhes</th>
                <th>Opções</th>
              </tr>
            </thead>
            <tbody>
              {formService.results.map((s) => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>
                    <NumberFormat
                      displayType={"text"}
                      value={s.value}
                      thousandSeparator={"."}
                      decimalSeparator={","}
                      prefix={"R$"}
                    />
                  </td>
                  <td>
                    {s.deadline
                      ? new Date(s.deadline).toLocaleDateString("pt-BR")
                      : "-"}
                  </td>
                  <td>{s.capacity || "-"}</td>
                  <td>{s.experience || "-"}</td>
                  <td>
                    {s.details && s.details.length > 0 ? (
                      <span>
                        {s.details.slice(0, 2).map((d, i) => (
                          <Badge key={i} color="info" className="mr-1">
                            {d}
                          </Badge>
                        ))}
                        {s.details.length > 2 && (
                          <Badge color="light">+{s.details.length - 2}</Badge>
                        )}
                      </span>
                    ) : (
                      "-"
                    )}
                  </td>
                  <td>
                    <Button
                      onClick={() => this.onEdit(s)}
                      color="secondary"
                      outline
                    >
                      <i className="cui-pencil"></i>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>
    );
  }
}

export default class Service extends Component {
  state = {
    formService: { results: [], currentPage: "", pageCount: "", pageSize: "" },
  };

  componentDidMount() {
    this.consultAll();
  }

  consultAll = async () => {
    try {
      const resp = await Axios.get(URL_Service);
      const { data } = resp;
      if (data) {
        this.setState({ formService: data });
      }
    } catch (error) {
      console.error("Erro ao carregar serviços:", error);
      swal("Erro ao carregar serviços", {
        icon: "error",
      });
    }
  };

  render() {
    return (
      <div className="container-fluid">
        <div className="row">
          <div className="col-md-5 my-3">
            <ListFormService formService={this.state.formService} />
          </div>
          <div className="col-md-7 my-3">
            <FormService consultAll={this.consultAll} />
          </div>
        </div>
      </div>
    );
  }
}
