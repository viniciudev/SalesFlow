import React, { Component } from 'react';
import {
    Button,
    Card,
    CardBody,
    CardFooter,
    CardHeader,
    CardTitle,
    Collapse,
    FormGroup,
    FormFeedback,
    Input,
    Label,
    Table,
    Nav,
    NavItem,
    NavLink,
    TabContent,
    TabPane,
    Form,
    Row,
    Col,
} from 'reactstrap';
import InputMask from 'react-input-mask';
import json_city from './json_cities.json'
import CharacterRemover from 'character-remover';
import { FaSpinner } from 'react-icons/fa'
import Axios from 'axios';
import { URL_Salesman } from '../../services/salesmanService';
import swal from 'sweetalert';
import { URL_SearchZipCode } from '../../services/searchZipCodeService';
import Pagination, { bootstrap5PaginationPreset } from 'react-responsive-pagination'
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js';
import Switch from 'react-input-switch';
import { URL_Commission } from '../../services/commissionService';
import { URL_CostCenter } from '../../services/costCenterService';
import Select from 'react-select';
import { URL_Product } from '../../services/productService';
import { URL_Service } from '../../services/serviceProvidedService';
class FormSalesman extends Component {

    state = {
        listCities: [],
        model: {
            id: 0, name: '', document: '',
            zipCode: '', address: '', bairro: '',
            nameState: '', nameCity: '', telephone: ''
        },
        errors: {},
        loading: false
    }

    buscaCidades = (e) => {
        const data = json_city.states;
        const val = e.target.value
        this.setValues(e, 'nameState')
        if (val != '') {
            var filterObj = data.find(function (item, i) {
                if (item.sigla === val) {
                    const city = item.cidades
                    return city
                }
            })
            this.setState({
                listCities: filterObj.cidades
            })
        }
    }

    setValues = (e, field) => {
        const { model } = this.state
        model[field] = e.target.value
        this.setState({ model })
        this.validate();
    }

    validate = () => {
        let isError = 0;
        const dados = this.state.model
        const errors = {}

        if (!dados.name) {
            isError++;
            errors.nameError = true;
        }
        else
            errors.nameError = false;
        // if (!dados.bairro) {
        //     isError++;
        //     errors.bairroError = true;
        // }
        // else
        //     errors.bairroError = false;
        if (!CharacterRemover.removeAll(dados.telephone)) {
            isError++;
            errors.telephoneError = true;
        }
        else
            errors.telephoneError = false;
        // if (!CharacterRemover.removeAll(dados.zipCode)) {
        //     isError++;
        //     errors.zipCodeError = true;
        // } else
        //     errors.zipCodeError = false;
        // if (!dados.address) {
        //     isError++;
        //     errors.addressError = true;
        // } else
        //     errors.addressError = false;
        // if (!dados.nameCity) {
        //     isError++;
        //     errors.nameCityError = true;
        // } else
        //     errors.nameCityError = false;
        // if (!dados.nameState) {
        //     isError++;
        //     errors.nameStateError = true;
        // } else
        //     errors.nameStateError = false;

        this.setState({ errors: errors })
        return isError;
    }

    componentWillMount() {
        PubSub.subscribe('edit-salesman', (topic, salesman) => {
            this.setState({
                model: salesman,
                listCities: [`${salesman.nameCity}`]
            })
        })
    }

    searchCep = async (e) => {
        const { model } = this.state
        let zipcode = CharacterRemover.removeAll(e.target.value)
        if (zipcode.length >= 8) {
            await Axios.get(`${URL_SearchZipCode}/${zipcode}`).then(resp => {
                const { data } = resp
                model.address = data.logradouro;
                model.nameCity = data.localidade;
                model.bairro = data.bairro;
                model.nameState = data.uf;
                this.setState({ model, listCities: [`${data.localidade}`] })
            })
        }
    }

    save = async () => {
        if (this.validate() == 0) {
            this.setState({ loading: true })
            const { model } = this.state
            let map = {
                id: model.id,
                name: model.name,
                document: model.document,
                zipCode: CharacterRemover.removeAll(model.zipCode),
                address: model.address,
                bairro: model.bairro,
                nameState: model.nameState,
                nameCity: model.nameCity,
                telephone: CharacterRemover.removeAll(model.telephone)
            }
            await Axios.post(URL_Salesman, map).then(resp => {
                const { data } = resp
                if (data) {
                    swal({ title: 'Salvo com sucesso!', icon: 'success' })
                    this.setState({ loading: false, })
                    this.consulListSaleman(data)
                }
            }).catch(() => { this.setState({ loading: false }) })
        }
    }

    consulListSaleman(model) {
        PubSub.publish('consult-salesman')
        PubSub.publish('edit-salesman', model)
    }

    newRegister = () => {
        window.location.reload()
    }
    render() {
        const { model, errors, loading } = this.state
        return (
            <Card>
                <CardHeader>
                    <strong>Cadastro</strong>
                    <small> Vendedor</small>
                    {' '}
                    <Button size="sm"
                        onClick={e => this.newRegister()}
                        color="secondary"
                    >
                        {<i className="fa fa-plus-square-o"></i>}
                        {" Novo registro"}
                    </Button>
                </CardHeader>
                <CardBody>
                    <Form>
                        <FormGroup>
                            <div className='form-row'>
                                <div className="col-md-1">
                                    <Label htmlFor="name">Id:</Label>
                                    <Input
                                        type="text"
                                        value={model.id}
                                        disabled
                                    />
                                </div>
                                <div className="col-md-7">
                                    <Label htmlFor="name">Nome:*</Label>
                                    <Input
                                        type="text"
                                        id="name"
                                        className="form-control-danger"
                                        onChange={e => this.setValues(e, 'name')}
                                        value={model.name}
                                        invalid={errors.nameError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="col-md-4">
                                    <Label htmlFor="cpf">Documento:</Label>
                                    <Input
                                        type="number"
                                        id="cpf"
                                        onChange={e => this.setValues(e, 'document')}
                                        value={model.document}
                                        className="form-control-warning"
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
                                        mask='99.999-999'
                                        tag={InputMask}
                                        onChange={e => this.setValues(e, 'zipCode')}
                                        value={model.zipCode}
                                        invalid={errors.zipCodeError}
                                        onChangeCapture={e => this.searchCep(e)}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="col-md-6">
                                    <Label htmlFor="address">Endereço:</Label>
                                    <Input
                                        className="form-control-warning"
                                        type="text"
                                        id="number"
                                        onChange={e => this.setValues(e, 'address')}
                                        value={model.address}
                                        invalid={errors.addressError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="col-md-3">
                                    <Label htmlFor="bairro">Bairro:</Label>
                                    <Input
                                        className="form-control-warning"
                                        type="text"
                                        id="bairro"
                                        onChange={e => this.setValues(e, 'bairro')}
                                        value={model.bairro}
                                        invalid={errors.bairroError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                            </div>
                        </FormGroup>
                        <FormGroup>
                            <FormGroup row className="my-0">
                                <div className="col-md-3">
                                    <Label htmlFor="state">Estado:</Label>
                                    <Input id="state"
                                        name="state"
                                        type="select"
                                        value={model.nameState}
                                        onChange={this.buscaCidades}
                                        invalid={errors.nameStateError}
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
                                <FormFeedback></FormFeedback>
                                <div className="col-md-4">
                                    <FormGroup>
                                        <Label htmlFor="city">Cidade:</Label>
                                        <Input id="nameCity"
                                            name="city"
                                            type="select"
                                            invalid={errors.nameCityError}
                                            onChange={e => this.setValues(e, 'nameCity')}
                                            value={model.nameCity}
                                        >
                                            {this.state.listCities.map(city => (<option key={city} >{city}</option>))}

                                        </Input>
                                    </FormGroup>
                                </div>
                                <div className="col-md-4">
                                    <Label htmlFor="cellphone">Telefone:*</Label>
                                    <Input id="cellphone"
                                        name="cellphone"
                                        type="text"
                                        invalid={errors.telephoneError}
                                        mask='(99) 99999-9999'
                                        tag={InputMask}
                                        value={model.telephone}
                                        onChange={e => this.setValues(e, 'telephone')}
                                    >
                                    </Input>
                                </div>
                            </FormGroup>
                        </FormGroup>
                        <Button size="sm"
                            onClick={e => this.save()}
                            color="success"
                            disabled={loading}
                        >
                            {loading && <FaSpinner />}
                            {loading && " Salvando"}
                            {!loading && <i className="fa fa-dot-circle-o"></i>}
                            {!loading && " Salvar"}
                        </Button>
                        <p className="float-right text-sm">
                            <i>Os campos marcados com (*) são obrigatórios</i>
                        </p>
                    </Form>
                </CardBody>
            </Card>
        )
    }
}

class FormCommission extends Component {
    state = {
        activeTabCommission: 'pro'
    }

    toggleTabCommission = tab => {
        if (this.state.activeTabCommission !== tab) {
            this.setState({
                activeTabCommission: tab
            });
        }

    }

    selectedSeller = name => {
        this.setState({ name: name })
    }

    render() {
        const { name } = this.state
        return (
            <div>
                <div className='float-right'>Vendedor: {name}</div>
                <Nav
                    tabs>
                    <NavItem >
                        <NavLink
                            className={this.state.activeTabCommission === 'pro' ? 'active' : ''}
                            onClick={() => { this.toggleTabCommission('pro'); }}
                        >
                            Produto
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink
                            className={this.state.activeTabCommission === 'ser' ? 'active' : ''}
                            onClick={() => { this.toggleTabCommission('ser'); }}
                        >
                            Serviço
                        </NavLink>
                    </NavItem>

                </Nav>
                <TabContent activeTab={this.state.activeTabCommission}>
                    <TabPane tabId="pro" role="tabpanel">
                        <CommissionProduct selectedSeller={this.selectedSeller} />
                    </TabPane>
                </TabContent>
                <TabContent activeTab={this.state.activeTabCommission}>
                    <TabPane tabId="ser" role="tabpanel">
                        <CommissionService />
                    </TabPane>
                </TabContent>
            </div>
        );
    }
}
class ListFormSalesman extends Component {
    state = { listSalesman: { results: [], currentPage: 0, pageCount: 0, pageSize: 0, rowCount: 0 }, }

    consulListSaleman = async () => {
        await Axios.get(URL_Salesman).then(resp => {
            const { data } = resp;
            if (data)
                this.setState({ listSalesman: data })
        })
    }

    componentWillMount() {
        PubSub.subscribe('consult-salesman', (topic) => {
            this.consulListSaleman();
        })
    }

    componentDidMount() {
        this.consulListSaleman();
    }

    consulListSalemanPagination = async (pageSizeValue, pageNumber) => {
        let pageSize = ''
        pageSize = pageSizeValue;
        await Axios.get(URL_Salesman, {
            params: {
                pageNumber: pageNumber,
                pageSize: pageSize
            }
        }).then(resp => {
            const { data } = resp;
            if (data)
                this.setState({ listSalesman: data })
        })
    }


    onEdit = (salesman) => {
        PubSub.publish('edit-salesman', salesman)
    }

    render() {
        const { listSalesman } = this.state
        const { results, currentPage, pageSize, rowCount } = listSalesman
        return (
            <Card>
                <CardHeader>
                    <strong>Consultar</strong>
                    <small> Vendedores Cadastrados</small>
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
                            {results.map(v => (<tr key={v.id}>
                                <td>{v.name}</td>
                                <td> <NumberFormat
                                    displayType={'text'}
                                    value={v.telephone}
                                    format="(##) # #### ####"
                                /></td>
                                <td>
                                    <Button color="secondary" outline
                                        onClick={e => this.onEdit(v)}
                                    >
                                        <i className="cui-pencil"></i>
                                    </Button>
                                </td>
                            </tr>))}
                        </tbody>
                    </Table>
                    <div className="d-flex align-items-left">
                        {/* <div>
                            <select className="custom-select"
                                name="selectOptionAmount"
                                onChange={(pageSize) => this.consulListSalemanPagination(pageSize, currentPage)}
                                value={pageSize}
                                multiple="">
                                <option >10</option>
                                <option defaultValue="1">25</option>
                                <option defaultValue="2">50</option>
                                <option defaultValue="3">100</option>
                            </select>
                        </div> */}
                        <div className="ml-auto">
                            <Pagination
                                maxWidth={2}
                                {...bootstrap5PaginationPreset}
                                current={currentPage}
                                total={Math.round(rowCount / 10, 1) + 1}
                                onPageChange={(pageNumber) => this.consulListSalemanPagination(pageSize, pageNumber)}
                                narrowStrategy={['dropEllipsis', 'dropNav']}
                                renderNav={false} />
                        </div>
                    </div>
                </CardBody>
            </Card>
        )
    }
}

export default class Prospect extends Component {
    render() {
        return (
            <div>
                <div className="row">
                    <div className="col-md-4 my-3">
                        <ListFormSalesman />
                    </div>
                    <div className="col-md-8 my-3" >
                        <Tabs />
                    </div>
                </div>
            </div>
        )
    }
}

class Tabs extends Component {
    state = {
        activeTab: 'sal',
        model: {}
    }

    componentWillMount() {
        PubSub.subscribe('edit-salesman', (topic, salesman) => {
            this.setState({
                model: salesman,
            })

        })

    }

    toggleTab = tab => {
        if (this.state.activeTab !== tab) {
            this.setState({
                activeTab: tab
            });
        }
    }

    render() {
        const { model } = this.state
        return (
            <div>
                <Nav tabs>
                    <NavItem>
                        <NavLink
                            className={this.state.activeTab === 'sal' ? 'active' : ''}
                            onClick={() => { this.toggleTab('sal'); }}
                        >
                            Cadastro
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink
                            className={this.state.activeTab === 'con' ? 'active' : ''}
                            onClick={() => { this.toggleTab('con'); }}
                            disabled={model.id > 0 ? false : true}
                        >
                            Comissão
                        </NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.state.activeTab}>
                    <TabPane tabId="sal" role="tabpanel">
                        <FormSalesman toggleTab={this.toggleTab}
                        />
                    </TabPane>
                </TabContent>
                <TabContent activeTab={this.state.activeTab}>
                    <TabPane tabId="con" role="tabpanel">
                        <FormCommission />
                    </TabPane>
                </TabContent>

            </div>
        )
    }
}
class CommissionService extends Component {

    state = {
        formServices: {
            results: [
            ], currentPage: 0, pageCount: 0, pageSize: 0, rowCount: 0,
        },
        loading: false,
        model: {},
        modelAll: { percentage: 0, status: 0 },
        formCostCenter: [],
        accordionState: [
            false, false, false, false, false, false,
            false, false, false, false, false, false,
            false, false, false, false, false, false
        ],
        modelCommission: {
            id: 0, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
            idSalesman: 0, nameSeller: '', idService: 0, descriptionService: ''
        },
        errors_commission: {}
    }

    clearFields() {
        this.setState({
            modelCommission: {
                id: 0, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
                idSalesman: 0, nameSeller: '', idService: 0, descriptionService: ''
            },
        })
    }
    toggleAccordion = id => {
        let accordionState = this.state.accordionState.map((val, i) => {
            return id === i ? !val : (this.state.oneAtATime ? false : val)
        })
        this.setState({
            accordionState
        })
    }

    componentWillMount() {
        PubSub.subscribe('edit-salesman', (topic, salesman) => {
            this.setState({
                model: salesman,
            })
            this.consultItem()
            this.consultServices()
            this.consultCostCenter()
        })

    }

    consultServices = async () => {
        const { model } = this.state
        await Axios.get(`${URL_Commission}/GetAllPagedServicesCommission`,
            { params: { idSeller: model.id } }).then(response => {
                const { data } = response
                this.setState({
                    formServices: data,
                })
            }).catch(() => { this.setState({ loadingTable: false }) })
    }

    query = async (pageSizeValue, pageNumber) => {
        let pageSize = ''
        pageSize = pageSizeValue;
        const { model } = this.state
        await Axios.get(`${URL_Commission}/GetAllPagedServicesCommission`, {
            params: {
                pageNumber: pageNumber,
                pageSize: pageSize,
                idSeller: model.id
            }
        }).then(response => {
            const { data } = response
            this.setState({
                formServices: data,
            })
        }).catch(() => { this.setState({ loadingTable: false }) })
    }

    setValues = (e, field, idService) => {
        const { formServices } = this.state
        formServices.results.forEach(element => {

            if (element.idService == idService && field == 'status') {
                element.status = e
            } else {
                if (element.idService == idService) {
                    element[field] = e.target.value
                }
            }
        });
        this.setState({ formServices })
    }

    setValuesAll = (e, field) => {
        const { modelAll } = this.state
        if (field == 'status')
            modelAll[field] = e
        else
            modelAll[field] = e.target.value
        this.setState({ modelAll })
    }

    applyToAll = () => {
        const { formServices, modelAll } = this.state
        formServices.results.forEach(element => {
            element.percentage = modelAll.percentage
            element.status = modelAll.status
            element.idCostCenter = modelAll.idCostCenter
            element.commissionDay = modelAll.commissionDay
            element.typeDay = modelAll.typeDay
        });
        this.setState({ formServices })
    }

    consultCostCenter = async (id) => {
        await Axios.get(`${URL_CostCenter}/GetByIdCompany`).then(resp => {
            const { data } = resp
            this.setState({ formCostCenter: data })
        })
    }
    validate = (map) => {
        let isError = 0;
        map.forEach(element => {
            if (!element.idCostCenter || element.idCostCenter == 0) {
                swal({
                    text: 'Serviço ' + element.descriptionService + ' não possui centro de custo informado!',
                    buttons: true, icon: 'warning',
                })
                isError++
            }
        });
        return isError
    }

    save = async () => {
        const { modelCommission, model } = this.state
        if (this.validate_commission() == 0) {
            // let map = []
            // formServices.results.forEach(element => {
            //     if (element.percentage > 0 || element.status > 0) {
            let data = {
                id: modelCommission.id,
                idService: modelCommission.idService,
                IdSalesman: model.id,
                percentage: modelCommission.percentage,
                status: modelCommission.status,
                idCostCenter: modelCommission.idCostCenter,
                commissionDay: modelCommission.commissionDay,
                typeDay: modelCommission.typeDay,
                descriptionService: modelCommission.descriptionService
            }
            // map.push(data);
            //     }
            // });
            console.log(data, 'dataaaaaaaaaa')
            this.setState({ loading: true })

            await Axios.post(URL_Commission, data).then(resp => {
                const { data } = resp
                if (data == 'success') {
                    swal({ text: 'Comissão Salva com sucesso!', icon: 'success' })
                    this.consultServices()
                    this.clearFields()
                } else {
                    swal({ text: data, icon: 'warning' })
                }
            }).catch(() => { this.setState({ loading: false }) })

            this.setState({ loading: false })
        }
    }

    consultItem = async (e) => {
        await Axios.get(`${URL_Service}/GetListByName`, {
            params: { textOption: e }
        }).then(resp => {
            const { data } = resp
            if (data) {
                let list = [];
                data.forEach(element => {
                    const item = {
                        label: element.name,
                        value: element.id,
                    }
                    list.push(item);
                });
                this.setState({ listItems: list })
            }
        })
    }

    setValuesCommissions = (e, field) => {
        const { modelCommission } = this.state
        if (field == "idService") {
            modelCommission[field] = e != null ? e.value : 0;
            modelCommission['descriptionService'] = e != null ? e.label : ''
        }
        else
            modelCommission[field] = e.target.type == "checkbox" ? e.target.checked : e.target.value
        this.setState({ modelCommission })
        this.validate_commission()
    }

    validate_commission = () => {
        let isError = 0;
        const data = this.state.modelCommission
        const errors_commission = {}
        if (data.idService == 0) {
            isError++;
            errors_commission.idServiceError = true;
        }
        else
            errors_commission.idServiceError = false;

        if (data.percentage == 0) {
            isError++;
            errors_commission.percentageError = true;
        }
        else
            errors_commission.percentageError = false;
        if (data.commissionDay == 0) {
            isError++;
            errors_commission.commissionDayError = true;
        }
        else
            errors_commission.commissionDayError = false;
        if (data.idCostCenter == 0) {
            isError++;
            errors_commission.idCostCenterError = true;
        }
        else
            errors_commission.idCostCenterError = false;

        this.setState({
            errors_commission: errors_commission
        });
        return isError;
    }

    onEdit(c) {
        this.setState({
            modelCommission: c
        })
    }

    render() {
        const { formServices, loading, modelAll, formCostCenter, modelCommission, errors_commission, listItems } = this.state
        const { pageSize, currentPage, rowCount } = formServices
        const customStylesService = {
            control: (base, state) => ({
                ...base,
                borderColor: errors_commission.idServiceError ? 'red' : 'lightgray'
            })
        }
        return (
            <div>
                {/* <CardHeader onClick={() => this.toggleAccordion(0)}>
                    <CardTitle tag="h6">
                        <a className="text-inherit">
                            <small>
                                <em className="fa fa-plus text-primary mr-2"></em>
                            </small>
                            <span>Aplicar a todos</span>
                        </a>
                    </CardTitle>
                </CardHeader>
                 <Collapse isOpen={this.state.accordionState[0]}>
                    <div className='row'>
                        <div className='col-md-2' >
                            <Label size='sm'>Comissão:</Label>
                            <Input
                                size='sm'
                                type='number'
                                onChange={e => this.setValuesAll(e, 'percentage')}
                                value={modelAll.percentage}
                            />
                        </div>
                        <div className='col-md-1'>
                            <Label size='sm'>Status:</Label>
                            <div className='row'>
                                <div className='col-md-12'>
                                    {' '} <Switch
                                        size='sm'
                                        value={modelAll.status}
                                        onChange={e => this.setValuesAll(e, 'status')}
                                        styles={{
                                            track: {
                                                backgroundColor: 'grey'
                                            },
                                            trackChecked: {
                                                backgroundColor: 'green'
                                            },
                                            button: {
                                                backgroundColor: 'white'
                                            },
                                            buttonChecked: {
                                                backgroundColor: 'white'
                                            }
                                        }} />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-3'>
                            <Label size='sm'>Centro de Custo</Label>
                            <Input
                                size='sm'
                                type='select'
                                onChange={e => this.setValuesAll(e, 'idCostCenter')}
                                value={modelAll.idCostCenter}
                            >
                                <option value={0}>Selecione...</option>
                                {formCostCenter.map(e => (
                                    <option key={e.id} value={e.id}>{e.name}</option>
                                ))}
                            </Input>
                        </div>
                        <div className='col-md-2'>
                            <Label size='sm'>Vencimento</Label>
                            <Input
                                size='sm'
                                type='number'
                                onChange={e => this.setValuesAll(e, 'commissionDay')}
                                value={modelAll.commissionDay}
                            />
                        </div>
                        <div className='col-md-2'>
                            <Label size='sm'>Fixo/Corrido</Label>
                            <Input
                                size='sm'
                                type='select'
                                onChange={e => this.setValuesAll(e, 'typeDay')}
                                value={modelAll.typeDay}
                            >
                                <option value={0}>Corrido</option>
                                <option value={1}>Fixo</option>
                            </Input>
                        </div>
                        <div className='col-md-2 mt-4' >
                            <Button color='success' title="Aplicar a todos"
                                size="sm"
                                onClick={e => this.applyToAll()}
                            >
                                <em className="fa fa-check-square-o"></em>
                                Aplicar
                            </Button>
                        </div>
                    </div>
                </Collapse> */}
                <Form>
                    <Row className="row-cols-lg-auto g-3 align-items-center">
                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >
                                Serviço:
                            </Label>
                            <Select
                                name="idService"
                                placeholder="Serviço..."
                                // onInputChange={e => { this.consultItem(e) }}
                                onChange={e => this.setValuesCommissions(e, "idService")}
                                options={listItems}
                                isClearable={true}
                                styles={customStylesService}
                                value={{
                                    value: modelCommission.idService,
                                    label: modelCommission.descriptionService
                                }}
                            />
                        </Col>
                        <Col md={2}>
                            <Label
                                className="visually-hidden"
                            >
                                Percentual:
                            </Label>
                            <Input
                                type='number'
                                value={modelCommission.percentage}
                                onChange={e => this.setValuesCommissions(e, 'percentage')}
                                invalid={errors_commission.percentageError}
                            />
                            <FormFeedback />
                        </Col>
                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >
                                Centro de custo:
                            </Label>
                            <Input
                                type="select"
                                value={modelCommission.idCostCenter}
                                onChange={e => this.setValuesCommissions(e, 'idCostCenter')}
                                invalid={errors_commission.idCostCenterError}
                            >
                                <option value={0}>Selecione...</option>
                                {formCostCenter.map(e => (
                                    <option key={e.id} value={e.id}>{e.name}</option>
                                ))}
                            </Input>
                            <FormFeedback />
                        </Col>

                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >Pagamento:</Label>
                            <Input
                                type="select"
                                value={modelCommission.typeDay}
                                onChange={e => this.setValuesCommissions(e, 'typeDay')}
                            >
                                <option value={0}>Dias Corridos</option>
                                <option value={1}>Dia Fixo</option>
                            </Input>
                        </Col>
                        <Col md={2}>
                            <Label
                                className="visually-hidden"
                            >
                                {modelCommission.typeDay == 0 ? "Dias:" : "Dia:"}
                            </Label>
                            <Input
                                type='number'
                                value={modelCommission.commissionDay}
                                onChange={e => this.setValuesCommissions(e, 'commissionDay')}
                                invalid={errors_commission.commissionDayError}
                            />
                            <FormFeedback />
                        </Col>
                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >Status:</Label>
                            <Input
                                type="select"
                                value={modelCommission.status}
                                onChange={e => this.setValuesCommissions(e, 'status')}
                            >
                                <option value={0}>Ativo</option>
                                <option value={1}>Inativo</option>
                            </Input>
                        </Col>
                        <Col>

                            {/* <Row>
                                <Button color='success' title="Aplicar a todos"
                                    size="sm"
                                    onClick={e => this.applyToAll()}
                                >
                                    <em className="fa fa-check-square-o"></em>
                                    Adicionar todos os itens
                                </Button>
                            </Row> */}
                            <Row>
                                <div className='mt-4'>
                                    <Button color='success'
                                        title="Adicionar"
                                        size="sm"
                                        onClick={e => this.save()}
                                        disabled={loading}
                                    >{loading && (
                                        <i
                                            className="fa fa-spinner fa-spin"
                                        />
                                    )}
                                        {loading && <span> Adicionando...</span>}
                                        {!loading && (<i className="fa fa-save" />)}
                                        {!loading && <span> Salvar</span>}
                                    </Button>
                                </div>
                            </Row>
                        </Col>

                    </Row>
                </Form>
                <div className='row mt-2' >
                    <div className="col">
                        <Table size="sm" striped responsive>
                            <thead className="thead-light">
                                <tr className='text-center'>
                                    <th>Serviço</th>
                                    <th>% Comissão</th>
                                    <th>Centro de custo</th>
                                    <th>Forma pagamento</th>
                                    <th>Vencimento</th>
                                    <th>Status</th>
                                    <th>Opções</th>
                                </tr>
                            </thead>
                            <tbody>
                                {
                                    formServices.results.map(s => (
                                        <tr className='text-center'
                                            key={s.idService}>
                                            <td className='col-md-2'>{s.descriptionService}</td>
                                            <td className='col-md-2'>
                                                {s.percentage}
                                            </td>
                                            <td className='col-md-3'>
                                                {formCostCenter.length > 0 && formCostCenter.find(x => x.id == s.idCostCenter)?.name}
                                            </td>
                                            <td className='col-md-2'>
                                                {s.typeDay == 0 ? "Corrido" : "Fixo"}
                                            </td>
                                            <td className='col-md-1'>
                                                {s.commissionDay}
                                            </td>
                                            <td className='col-md-1'>{
                                                s.status === 0 ?
                                                    <span className='text-success' >Ativo</span>
                                                    : <span className='text-secondary' >Inativo</span>
                                            }   </td>
                                            <td>
                                                <Button
                                                    title='editar'
                                                    onClick={e => { this.onEdit(s) }}
                                                >
                                                    <em className='fa fa-edit'></em>
                                                </Button>
                                            </td>
                                        </tr>
                                    ))
                                }
                            </tbody>
                        </Table>
                        <CardFooter>
                            <div className="d-flex align-items-left">
                                {/* <div>
                                    <select className="custom-select"
                                        name="selectOptionAmount"
                                        onChange={(pageSize) => this.query(pageSize, currentPage)}
                                        value={pageSize}
                                        multiple="">
                                        <option >10</option>
                                        <option defaultValue="1">25</option>
                                        <option defaultValue="2">50</option>
                                        <option defaultValue="3">100</option>
                                    </select>
                                </div> */}
                                {/* <div>
                                    <Button color='success'
                                        title="Salvar"
                                        size="sm"
                                        onClick={e => this.save()}
                                        disabled={loading}
                                    >{loading && (
                                        <i
                                            className="fa fa-spinner fa-spin"
                                        />
                                    )}
                                        {loading && <span> Salvando...</span>}
                                        {!loading && (<i className="fa fa-save" />)}
                                        {!loading && <span> Salvar Comissão</span>}
                                    </Button>
                                </div> */}
                                <div className="ml-auto">
                                    <Pagination
                                        maxWidth={2}
                                        {...bootstrap5PaginationPreset}
                                        current={currentPage}
                                        total={Math.round(rowCount / 10, 1) + 1}
                                        onPageChange={(pageNumber) => this.query(pageSize, pageNumber)}
                                        narrowStrategy={['dropEllipsis', 'dropNav']}
                                        renderNav={false}
                                    />
                                </div>
                            </div>
                        </CardFooter>
                    </div>
                </div>
            </div >
        )
    }
}
class CommissionProduct extends Component {

    state = {
        model: {},
        formProducts: {
            results: [
            ], currentPage: 0, pageCount: 0, pageSize: 0, rowCount: 0,
        },
        loading: false,
        modelAll: { percentage: 0, status: 0 },
        formCostCenter: [],
        accordionState: [
            false, false, false, false, false, false,
            false, false, false, false, false, false,
            false, false, false, false, false, false
        ],
        modelCommission: {
            id: 0, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
            idSalesman: 0, nameSeller: '', idProduct: 0, descriptionProduct: ''
        },
        errors_commission: {}
    }

    clearFields() {
        this.setState({
            model: {},
            formProducts: {
                results: [
                ], currentPage: 0, pageCount: 0, pageSize: 0, rowCount: 0,
            },
            loading: false,
            modelAll: { percentage: 0, status: 0 },
            formCostCenter: [],
        })
    }
    toggleAccordion = id => {
        let accordionState = this.state.accordionState.map((val, i) => {
            return id === i ? !val : (this.state.oneAtATime ? false : val)
        })
        this.setState({
            accordionState
        })
    }

    componentWillMount() {
        PubSub.subscribe('edit-salesman', (topic, salesman) => {
            this.setState({
                model: salesman,
            })
            this.consultItem()
            this.consultProducts()
            this.consultCostCenter()
            this.props.selectedSeller(salesman.name)
        })

    }

    consultProducts = async () => {
        const { model } = this.state
        await Axios.get(`${URL_Commission}/GetAllPagedProductsCommission`
            , { params: { idSeller: model.id } }).then(response => {
                const { data } = response
                this.setState({
                    formProducts: data,
                })
            }).catch(() => { this.setState({ loadingTable: false }) })
    }

    query = async (pageSizeValue, pageNumber) => {
        let pageSize = ''
        pageSize = pageSizeValue;
        const { model } = this.state
        await Axios.get(`${URL_Commission}/GetAllPagedProductsCommission`, {
            params: {
                pageNumber: pageNumber,
                pageSize: pageSize,
                idSeller: model.id
            }
        }).then(response => {
            const { data } = response
            this.setState({
                formProducts: data,
            })
        }).catch(() => { this.setState({ loadingTable: false }) })
    }

    setValues = (e, field, idProduct) => {
        const { formProducts } = this.state
        formProducts.results.forEach(element => {

            if (element.idProduct == idProduct && field == 'status') {
                element.status = e
            } else {
                if (element.idProduct == idProduct) {
                    element[field] = e.target.value
                }
            }
        });
        this.setState({ formProducts })
    }

    setValuesAll = (e, field) => {
        const { modelAll } = this.state
        if (field == 'status')
            modelAll[field] = e
        else
            modelAll[field] = e.target.value
        this.setState({ modelAll })
    }

    applyToAll = () => {
        const { formProducts, modelAll } = this.state
        formProducts.results.forEach(element => {
            element.percentage = modelAll.percentage
            element.status = modelAll.status
            element.idCostCenter = modelAll.idCostCenter
            element.commissionDay = modelAll.commissionDay
            element.typeDay = modelAll.typeDay
        });
        this.setState({ formProducts })
    }

    consultCostCenter = async () => {
        await Axios.get(`${URL_CostCenter}/GetByIdCompany`).then(resp => {
            const { data } = resp
            this.setState({ formCostCenter: data })
        })
    }

    validate = (map) => {
        let isError = 0;
        map.forEach(element => {
            if (!element.idCostCenter || element.idCostCenter == 0) {
                swal({
                    text: 'Produto ' + element.descriptionProduct + ' não possui centro de custo informado!',
                    buttons: true, icon: 'warning',
                })
                isError++
            }
        });
        return isError
    }

    save = async (s) => {

        const { formProducts, model, modelCommission } = this.state
        if (this.validate_sharedCommission() == 0) {
            this.setState({ loading: true })
            // let map = []
            // formProducts.results.forEach(element => {
            //     if (element.percentage > 0 || element.status > 0) {
            let data = {
                id: modelCommission.id,
                idProduct: modelCommission.idProduct,
                IdSalesman: model.id,
                percentage: modelCommission.percentage,
                status: modelCommission.status,
                idCostCenter: modelCommission.idCostCenter,
                commissionDay: modelCommission.commissionDay,
                typeDay: modelCommission.typeDay,
                descriptionProduct: modelCommission.nameProduct
            }
            //     map.push(data);
            // }
            // });


            // if (this.validate(map) == 0) {
            await Axios.post(URL_Commission, data).then(resp => {
                const { data } = resp
                if (data == 'success') {
                    swal({ text: 'Comissão Salva com sucesso!', icon: 'success' })
                    this.consultProducts()
                    this.clearFields()
                } else {
                    swal({ text: data, icon: 'warning' })
                }

            }).catch(() => { this.setState({ loading: false }) })
        }
        this.setState({ loading: false })
        // }
        this.consultCostCenter()
    }

    clearFields() {
        this.setState({
            modelCommission: {
                id: 0, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
                idSalesman: 0, nameSeller: '', idProduct: 0, descriptionProduct: ''
            },
        })
    }
    onEdit(c) {
        this.setState({ modelCommission: c })
    }
    consultItem = async (e) => {
        const { modelCommission } = this.state;

        await Axios.get(`${URL_Product}/GetListByName`, {
            params: { textOption: e }
        }).then(resp => {
            const { data } = resp
            if (data) {
                let list = [];
                data.forEach(element => {
                    const item = {
                        label: element.name,
                        value: element.id,
                    }
                    list.push(item);
                });
                this.setState({ listItems: list })
            }
        })
    }

    setValuesCommissions = (e, field) => {
        const { modelCommission } = this.state
        if (field == "idProduct") {
            modelCommission[field] = e != null ? e.value : 0;
            modelCommission['descriptionProduct'] = e != null ? e.label : ''
        }
        else
            modelCommission[field] = e.target.type == "checkbox" ? e.target.checked : e.target.value
        this.setState({ modelCommission })
        this.validate_sharedCommission()
    }

    validate_sharedCommission = () => {
        let isError = 0;
        const data = this.state.modelCommission
        const errors_commission = {}
        if (data.idProduct == 0) {
            isError++;
            errors_commission.idProductError = true;
        }
        else
            errors_commission.idProductError = false;

        if (data.percentage == 0) {
            isError++;
            errors_commission.percentageError = true;
        }
        else
            errors_commission.percentageError = false;
        if (data.commissionDay == 0) {
            isError++;
            errors_commission.commissionDayError = true;
        }
        else
            errors_commission.commissionDayError = false;
        if (data.idCostCenter == 0) {
            isError++;
            errors_commission.idCostCenterError = true;
        }
        else
            errors_commission.idCostCenterError = false;

        this.setState({
            errors_commission: errors_commission
        });
        return isError;
    }

    render() {
        const { formProducts, loading, modelAll, formCostCenter, modelCommission, errors_commission, listItems } = this.state
        const { pageSize, currentPage, rowCount } = formProducts
        const customStylesProduct = {
            control: (base, state) => ({
                ...base,
                borderColor: errors_commission.idProductError ? 'red' : 'lightgray'
            })
        }
        return (
            <div>
                {/* <CardHeader onClick={() => this.toggleAccordion(0)}>
                    <CardTitle tag="h6">
                        <a className="text-inherit">
                            <small>
                                <em className="fa fa-plus text-primary mr-2"></em>
                            </small>
                            <span>Aplicar a todos</span>
                        </a>
                    </CardTitle>
                </CardHeader>
                 <Collapse isOpen={this.state.accordionState[0]}>
                    <div className='row'>
                        <div className='col-md-2' >
                            <Label size='sm'>Comissão:</Label>
                            <Input
                                size='sm'
                                type='number'
                                onChange={e => this.setValuesAll(e, 'percentage')}
                                value={modelAll.percentage}
                            />
                        </div>
                        <div className='col-md-1'>
                            <Label size='sm'>Status:</Label>
                            <div className='row'>
                                <div className='col-md-12'>
                                    {' '} <Switch
                                        size='sm'
                                        value={modelAll.status}
                                        onChange={e => this.setValuesAll(e, 'status')}
                                        styles={{
                                            track: {
                                                backgroundColor: 'grey'
                                            },
                                            trackChecked: {
                                                backgroundColor: 'green'
                                            },
                                            button: {
                                                backgroundColor: 'white'
                                            },
                                            buttonChecked: {
                                                backgroundColor: 'white'
                                            }
                                        }} />
                                </div>
                            </div>
                        </div>
                        <div className='col-md-3'>
                            <Label size='sm'>Centro de Custo</Label>
                            <Input
                                size='sm'
                                type='select'
                                onChange={e => this.setValuesAll(e, 'idCostCenter')}
                                value={modelAll.idCostCenter}
                            >
                                <option value={0}>Selecione...</option>
                                {formCostCenter.map(e => (
                                    <option key={e.id} value={e.id}>{e.name}</option>
                                ))}
                            </Input>
                        </div>
                        <div className='col-md-2'>
                            <Label size='sm'>Vencimento</Label>
                            <Input
                                size='sm'
                                type='number'
                                onChange={e => this.setValuesAll(e, 'commissionDay')}
                                value={modelAll.commissionDay}
                            />
                        </div>
                        <div className='col-md-2'>
                            <Label size='sm'>Fixo/Corrido</Label>
                            <Input
                                size='sm'
                                type='select'
                                onChange={e => this.setValuesAll(e, 'typeDay')}
                                value={modelAll.typeDay}
                            >
                                <option value={0}>Corrido</option>
                                <option value={1}>Fixo</option>
                            </Input>
                        </div>
                        <div className='col-md-2 mt-4' >
                            <Button color='success' title="Aplicar a todos"
                                size="sm"
                                onClick={e => this.applyToAll()}
                            >
                                <em className="fa fa-check-square-o"></em>
                                Aplicar
                            </Button>
                        </div>
                    </div>
                </Collapse> */}
                <Form>
                    <Row className="row-cols-lg-auto g-3 align-items-center">
                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >
                                Produto:
                            </Label>
                            <Select
                                name="idproduct"
                                placeholder="Produto..."
                                // onInputChange={e => { this.consultItem(e) }}
                                onChange={e => this.setValuesCommissions(e, "idProduct")}
                                options={listItems}
                                isClearable={true}
                                styles={customStylesProduct}
                                value={{
                                    value: modelCommission.idProduct,
                                    label: modelCommission.descriptionProduct
                                }}
                            />
                        </Col>
                        <Col md={2}>
                            <Label
                                className="visually-hidden"
                            >
                                Percentual:
                            </Label>
                            <Input
                                type='number'
                                value={modelCommission.percentage}
                                onChange={e => this.setValuesCommissions(e, 'percentage')}
                                invalid={errors_commission.percentageError}
                            />
                            <FormFeedback />
                        </Col>
                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >
                                Centro de custo:
                            </Label>
                            <Input
                                type="select"
                                value={modelCommission.idCostCenter}
                                onChange={e => this.setValuesCommissions(e, 'idCostCenter')}
                                invalid={errors_commission.idCostCenterError}
                            >
                                <option value={0}>Selecione...</option>
                                {formCostCenter.map(e => (
                                    <option key={e.id} value={e.id}>{e.name}</option>
                                ))}
                            </Input>
                            <FormFeedback />
                        </Col>

                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >Pagamento:</Label>
                            <Input
                                type="select"
                                value={modelCommission.typeDay}
                                onChange={e => this.setValuesCommissions(e, 'typeDay')}
                            >
                                <option value={0}>Dias Corridos</option>
                                <option value={1}>Dia Fixo</option>
                            </Input>
                        </Col>
                        <Col md={2}>
                            <Label
                                className="visually-hidden"
                            >
                                {modelCommission.typeDay == 0 ? "Dias:" : "Dia:"}
                            </Label>
                            <Input
                                type='number'
                                value={modelCommission.commissionDay}
                                onChange={e => this.setValuesCommissions(e, 'commissionDay')}
                                invalid={errors_commission.commissionDayError}
                            />
                            <FormFeedback />
                        </Col>
                        <Col md={3}>
                            <Label
                                className="visually-hidden"
                            >Status:</Label>
                            <Input
                                type="select"
                                value={modelCommission.status}
                                onChange={e => this.setValuesCommissions(e, 'status')}
                            >
                                <option value={0}>Ativo</option>
                                <option value={1}>Inativo</option>
                            </Input>
                        </Col>
                        <Col>

                            {/* <Row>
                                <Button color='success' title="Aplicar a todos"
                                    size="sm"
                                    onClick={e => this.applyToAll()}
                                >
                                    <em className="fa fa-check-square-o"></em>
                                    Adicionar todos os itens
                                </Button>
                            </Row> */}
                            <Row>
                                <div className='mt-4'>
                                    <Button color='success'
                                        title="Adicionar"
                                        size="sm"
                                        onClick={e => this.save()}
                                        disabled={loading}
                                    >{loading && (
                                        <i
                                            className="fa fa-spinner fa-spin"
                                        />
                                    )}
                                        {loading && <span> Adicionando...</span>}
                                        {!loading && (<i className="fa fa-save" />)}
                                        {!loading && <span> Salvar</span>}
                                    </Button>
                                </div>
                            </Row>
                        </Col>

                    </Row>
                </Form>
                <div className='row mt-2' >
                    <div className="col">
                        <Table size="sm" striped responsive>
                            <thead className="thead-light">
                                <tr className='text-center'>
                                    <th>Produto</th>
                                    <th>% Comissão</th>
                                    <th>Centro de custo</th>
                                    <th>Forma pagamento</th>

                                    <th>Vencimento</th>

                                    <th>Status</th>
                                    <th>Opções</th>
                                </tr>
                            </thead>
                            <tbody>
                                {
                                    formProducts.results.map(s => (
                                        <tr className='text-center'
                                            key={s.idProduct}>
                                            <td className='col-md-2'>{s.descriptionProduct}</td>
                                            <td className='col-md-2'>
                                                {s.percentage}
                                            </td>
                                            <td className='col-md-3'>
                                                {formCostCenter.length > 0 && formCostCenter.find(x => x.id == s.idCostCenter)?.name}
                                            </td>


                                            <td className='col-md-2'>
                                                {s.typeDay == 0 ? "Corrido" : "Fixo"}
                                            </td>
                                            <td className='col-md-1'>
                                                {s.commissionDay}
                                            </td>

                                            <td className='col-md-1'>{
                                                s.status === 0 ?
                                                    <span className='text-success' >Ativo</span>
                                                    : <span className='text-secondary' >Inativo</span>
                                            }   </td>
                                            <td>
                                                <Button
                                                    title='editar'
                                                    onClick={e => { this.onEdit(s) }}
                                                >
                                                    <em className='fa fa-edit'></em>
                                                </Button>
                                            </td>
                                        </tr>
                                    ))
                                }
                            </tbody>
                        </Table>
                        <CardFooter>
                            <div className="d-flex align-items-left">
                                {/* <div>
                                    <select className="custom-select"
                                        name="selectOptionAmount"
                                        onChange={(pageSize) => this.query(pageSize, currentPage)}
                                        value={pageSize}
                                        multiple="">
                                        <option >10</option>
                                        <option defaultValue="1">25</option>
                                        <option defaultValue="2">50</option>
                                        <option defaultValue="3">100</option>
                                    </select>
                                </div> */}
                                {/* <div>
                                    <Button color='success'
                                        title="Salvar"
                                        size="sm"
                                        onClick={e => this.save()}
                                        disabled={loading}
                                    >{loading && (
                                        <i
                                            className="fa fa-spinner fa-spin"
                                        />
                                    )}
                                        {loading && <span> Salvando...</span>}
                                        {!loading && (<i className="fa fa-save" />)}
                                        {!loading && <span> Salvar Comissão</span>}
                                    </Button>
                                </div> */}
                                <div className="ml-auto">
                                    <Pagination
                                        maxWidth={2}
                                        {...bootstrap5PaginationPreset}
                                        current={currentPage}
                                        total={Math.round(rowCount / 10, 1) + 1}
                                        onPageChange={(pageNumber) => this.query(pageSize, pageNumber)}
                                        narrowStrategy={['dropEllipsis', 'dropNav']}
                                        renderNav={false}
                                    />
                                </div>
                            </div>
                        </CardFooter>
                    </div>
                </div>
            </div >
        )
    }
}