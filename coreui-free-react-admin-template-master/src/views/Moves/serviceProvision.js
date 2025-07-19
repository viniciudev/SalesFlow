import React, { Component } from 'react';
import {
    Badge,
    Button,
    Card,
    CardBody,
    Col,
    FormFeedback,
    Input,
    Label,
    Row,
    Table,
    Nav,
    NavItem,
    NavLink,
    TabPane,
    TabContent,
    CardHeader,
    Form,
    FormGroup,
} from 'reactstrap';

import swal from 'sweetalert';
import moment from 'moment';
import Axios from 'axios';
import { URL_ServiceProvision } from '../../services/serviceProvisionService'
import { URL_ServiceProvisionItems } from '../../services/serviceProvisionItemsService'

import CurrencyInput from 'react-currency-input';
import ConvertToUSD from './../../ConvertCurrency';
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js'
import Pagination from 'react-js-pagination';
import { URL_Service } from '../../services/serviceProvidedService'
import { URL_Product } from './../../services/productService';
import { URL_SearchClient } from './../../services/searchClientService';
import { URL_Budget } from '../../services/budgetService';
import { typeItem } from '../../utils/enums/typeItem_enum';
import Widget02 from './../../widgets/Widget02';
import Select from 'react-select';
import { URL_BudgetPerformad } from './../../services/budgetPerformadService';

class FormBudgetPerformed extends Component {
    constructor(props) {
        super(props);
        this.toggleAccordion = this.toggleAccordion.bind(this);
        this.state = {
            collapse: false,
            accordion: [true, false, false],
            fadeIn: true,
            modelService: { id: 0, description: '', idClient: 0, nameClient: '', idBudget: 0 },
            formCompare: { differentAmounts: [], nonExistentProducts: [], differentValues: [] }
        };
    }

    toggleAccordion(tab) {

        const prevState = this.state.accordion;
        const state = prevState.map((x, index) => tab === index ? !x : false);

        this.setState({
            accordion: state,
        });
    }
    componentWillMount() {
        PubSub.subscribe('edit-service', (topic, service) => {

            this.setState({
                modelService: service,
            })

        })

    }

    compare = async () => {
        const { modelService } = this.state
        await Axios.get(URL_BudgetPerformad, {
            params: {
                idServiceProvision: modelService.id,
                idBudget: modelService.idBudget
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({ formCompare: data });
            }
        })
    }

    render() {
        const { formCompare } = this.state
        return (
            <div className="animated fadeIn">
                <Row>
                    <Col xl="12">
                        <Card>
                            <CardHeader>
                                <i className="fa fa-align-justify"></i> Comparar <small>orçamento x serviço</small>
                            </CardHeader>
                            <CardBody>
                                <Row>
                                    <Col xl="3">
                                        <Button onClick={e => this.compare()} color='primary'>Comparar</Button>
                                    </Col>
                                </Row>
                                <br />
                                <Table responsive size="sm" borderless>
                                    {/* <thead>
                                        <tr>
                                            <th>Username</th>
                                            <th>Date registered</th>
                                            <th>Role</th>
                                            <th>Status</th>
                                        </tr>
                                    </thead> */}

                                    <tbody>
                                        {
                                            formCompare.differentAmounts.map(d => (
                                                <tr>
                                                    <td className='text-danger'>{d.description}</td>
                                                    <td>{d.nameProduct}</td>
                                                    <td>Qtd. Orçamento: {d.amountBudget}</td>
                                                    <td>Qtd. Serviço: {d.amountService}</td>

                                                    {/* <td>
                                                <Badge color="success">Active</Badge>
                                            </td> */}
                                                </tr>
                                            ))
                                        }
                                        {
                                            formCompare.differentValues.map(d => (
                                                <tr>
                                                    <td className='text-danger'>{d.description}</td>
                                                    <td>{d.nameProduct}</td>
                                                    <td>Valor Orçamento:
                                                        {<NumberFormat
                                                            displayType={'text'}
                                                            value={d.valueBudget}
                                                            thousandSeparator={'.'}
                                                            decimalSeparator={','}
                                                            prefix={'R$'} />}</td>
                                                    <td>Valor Serviço: {<NumberFormat
                                                        displayType={'text'}
                                                        value={d.valueService}
                                                        thousandSeparator={'.'}
                                                        decimalSeparator={','}
                                                        prefix={'R$'} />}</td>

                                                    {/* <td>
                                                <Badge color="success">Active</Badge>
                                            </td> */}
                                                </tr>
                                            ))
                                        }
                                        {
                                            formCompare.nonExistentProducts.map(n => (
                                                <tr>
                                                    <td className='text-danger'>{n.description}</td>
                                                    <td>{n.nameProduct}</td>
                                                </tr>
                                            ))
                                        }

                                    </tbody>
                                </Table>
                            </CardBody>
                        </Card>

                    </Col>
                </Row>
            </div>
        );
    }
}
class ListServiceProvision extends Component {
    state = {
        listClient: [], modelFilter: { idClient: 0 },
        formService: { results: [], currentPage: '', pageCount: '', pageSize: '' },
    }

    onEdit = (service) => {
        PubSub.publish('edit-service', service)
    }

    consultServicePagination = async (pageSizeValue, pageNumber) => {
        const { modelFilter } = this.state
        let pageSize = ''
        if (pageSizeValue.target != undefined)
            pageSize = pageSizeValue.target.value;
        else
            pageSize = pageSizeValue;

        await Axios.get(`${URL_ServiceProvision}`, {
            params: {
                idClient: modelFilter.idClient,
                pageSize: pageSize, pageNumber: pageNumber
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formService: data
                })
            }
        })
    }

    consultAll = async () => {
        const { modelFilter } = this.state
        await Axios.get(URL_ServiceProvision, {
            params: { idClient: modelFilter.idClient }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formService: data
                })
            }
        })
    }

    componentDidMount() {
        this.consultAll();
    }

    setValues = (e, field) => {
        const { modelFilter } = this.state;
        if (field == "idClient")
            modelFilter[field] = e != null ? e.value : 0;
        else
            modelFilter[field] = e.target.value;
        this.setState({ modelFilter });
    }

    consultClient = async (e) => {
        if (e && e.length > 2) {
            await Axios.get(`${URL_SearchClient}/contains`, {
                params: { 'textOption': e }
            }).then(resp => {
                const { data } = resp
                if (data) {
                    let list = [];
                    data.forEach(element => {
                        const item = {
                            label: element.name,
                            value: element.id
                        }
                        list.push(item);
                    });
                    this.setState({ listClient: list })
                }
            })
        }
    }

    render() {
        const { listClient, formService } = this.state
        const { currentPage, pageSize, rowCount, results } = formService;

        return (
            <Card>
                <CardBody>
                    <div className="row">
                        <div className="col-md-3">

                            <Select
                                placeholder="Cliente..."
                                onInputChange={e => { this.consultClient(e) }}
                                name="idClient"
                                onChange={e => this.setValues(e, "idClient")}
                                options={listClient}
                                isClearable={true}
                                noOptionsMessage={() => "Digite o nome do Cliente!"}
                            />

                        </div>
                        <div className="col-md-2">
                            <Button
                                color="primary"
                                onClick={e => this.consultAll()}
                            >
                                <i class="fa fa-search"></i>
                            </Button>
                        </div>
                    </div>
                    <br />
                    <Table responsive size="sm">
                        <thead>
                            <tr>
                                <th>Data</th>
                                <th>Cliente</th>
                                <th>Descrição</th>
                                <th>Opções</th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                results.map(s => (

                                    <tr>
                                        <td>{moment(s.date).format("DD-MM-YYYY")}</td>
                                        <td>{s.nameClient}</td>
                                        <td>{s.description}</td>
                                        {/* <td> <NumberFormat
                                            displayType={'text'}
                                            value={s.value}
                                            thousandSeparator={'.'}
                                            decimalSeparator={','}
                                            prefix={'R$'}
                                        /></td> */}
                                        <td>
                                            <Button onClick={e => this.onEdit(s)} color="secondary" outline>
                                                <i className="cui-pencil"></i>
                                            </Button>
                                        </td>
                                    </tr>
                                ))
                            }

                        </tbody>
                    </Table>
                    <div className="d-flex align-items-left">
                        <div>
                            <select className="custom-select"
                                name="selectOptionAmount"
                                onChange={(pageSize) => this.consultServicePagination(pageSize, currentPage)}
                                value={pageSize}
                                multiple="">
                                <option >10</option>
                                <option defaultValue="1">25</option>
                                <option defaultValue="2">50</option>
                                <option defaultValue="3">100</option>
                            </select>
                        </div>
                        <div className="ml-auto">
                            <Pagination
                                activePage={currentPage}
                                totalItemsCount={rowCount}
                                itemsCountPerPage={pageSize}
                                onChange={(pageNumber) => this.consultServicePagination(pageSize, pageNumber)}
                                itemClass="page-item"
                                linkClass="page-link"
                                firstPageText="Primeira"
                                lastPageText="Última"
                            />
                        </div>
                    </div>


                </CardBody>
            </Card>
        )
    }

}

class FormServiceProvision extends Component {
    state = {
        listItems: [],
        listClient: [],
        listBudget: [],
        errors: {},
        errors_items: {},
        modelService: { id: 0, description: '', idClient: 0, nameClient: '', idBudget: 0 },
        modelServiceItems: { id: 0, idServiceProvision: 0, typeItem: 0, idItem: 0, nameItem: '', value: '', amount: 0, date: '' },
        formServiceItems: { results: [], currentPage: '', pageCount: '', pageSize: '' }
    }

    setValues = (e, field) => {
        const { modelService } = this.state;
        if (field == "idClient" || field == "idBudget")
            modelService[field] = e.value;
        else
            modelService[field] = e.target.value;
        this.setState({ modelService });
    }

    setValues_Items = (e, field) => {
        const { modelServiceItems } = this.state;
        if (field == "idItem") {
            modelServiceItems[field] = e.value;
            modelServiceItems['nameItem'] = e.label
        }
        else
            modelServiceItems[field] = e.target.value;

        this.setState({ modelServiceItems });
        if (field == 'typeItem') {
            this.consultItem();
        }
    }


    validate_items = () => {
        let isError = 0;
        const data = this.state.modelServiceItems
        const errors_items = {}
        const value = ConvertToUSD(data.value != '' ? data.value : 0);


        if (data.idItem == 0) {
            isError++;
            errors_items.idItemError = true;
        }
        else
            errors_items.idItemError = false;

        if (parseFloat(value) == 0) {
            isError++;
            errors_items.valueError = true;
        }
        else
            errors_items.valueError = false;
        if (data.amount == 0) {
            isError++;
            errors_items.amountError = true;
        }
        else
            errors_items.amountError = false;

        this.setState({
            errors_items
        });

        return isError;
    }

    validate = () => {
        let isError = 0;
        const dados = this.state.modelService
        const errors = {}

        if (!dados.description) {
            isError++;
            errors.nameError = true;
        }
        else
            errors.nameError = false;
        if (dados.idClient == 0) {
            isError++;
            errors.idClientError = true;
        }
        else
            errors.idClientError = false;

        this.setState({
            errors
        });

        return isError;
    }

    clear() {
        this.setState({ modelService: { id: 0, name: '', value: '', idBudget: 0 } })
    }

    clearInsertItems() {
        this.setState({
            modelServiceItems: { id: 0, idServiceProvision: 0, typeItem: 0, idItem: 0, nameItem: '', value: '', amount: 0, date: '' },
        })
    }

    insertItem = async () => {
        if (this.validate_items() == 0) {
            const { modelService, modelServiceItems } = this.state
            const value = ConvertToUSD(modelServiceItems.value)
            let map = {
                id: modelServiceItems.id,
                IdServiceProvision: modelService.id,
                typeItem: modelServiceItems.typeItem,
                idItem: modelServiceItems.idItem,
                value: parseFloat(value),
                amount: modelServiceItems.amount,
                date: new Date()
            }
            if (modelServiceItems.id == 0) {
                await Axios.post(URL_ServiceProvisionItems, map).then(resp => {
                    const { data } = resp
                    if (data == "") {
                        this.consultItemsByIdService();
                        this.clearInsertItems()
                    } else {
                        swal(data, {
                            dangerMode: true,
                            icon: 'warning',
                        })
                    }
                })
            } else {
                await Axios.put(URL_ServiceProvisionItems, map).then(resp => {
                    const { data } = resp
                    if (data) {
                        this.consultItemsByIdService();
                        this.clearInsertItems()
                    } else {
                        swal('Erro ao atualizar!', {
                            dangerMode: true,
                            icon: 'warning',
                        })
                    }
                })
            }

        }
    }



    consultItemsByIdService = async () => {
        const { modelService } = this.state
        await Axios.get(`${URL_ServiceProvisionItems}/GedByIdServiceProvision`, {
            params: {
                idServiceProvision: modelService.id
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formServiceItems: data
                })
            }
        })
    }

    consultItemsByIdServicePagination = async (pageSizeValue, pageNumber) => {
        let pageSize = ''
        if (pageSizeValue.target != undefined)
            pageSize = pageSizeValue.target.value;
        else
            pageSize = pageSizeValue;
        const { modelService } = this.state

        await Axios.get(`${URL_ServiceProvisionItems}/GedByIdServiceProvision`, {
            params: {
                idServiceProvision: modelService.id,
                pageSize: pageSize, pageNumber: pageNumber
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formServiceItems: data
                })
            }
        })
    }





    componentWillMount() {
        PubSub.subscribe('edit-service', (topic, service) => {

            this.setState({
                listBudget: [],
                listClient: [],
                modelService: service,
            })
            this.consultItemsByIdService()
            this.props.toggleTab("ser")
        })

    }

    saveService = async () => {
        if (this.validate() == 0) {
            const { modelService } = this.state

            let data = {
                id: modelService.id,
                date: new Date(),
                description: modelService.description,
                idClient: modelService.idClient,
                idBudget: modelService.idBudget
            }

            if (data.id > 0) {
                Axios.put(URL_ServiceProvision, data).then(resp => {
                    const { data } = resp
                    if (data) {
                        swal("Serviço Atualizado!", { icon: "success" })
                    }
                })
            } else {
                Axios.post(URL_ServiceProvision, data).then(resp => {
                    const { data } = resp
                    if (data) {
                        this.setState({ modelService: data })
                        this.consultItemsByIdService()
                    }
                })
            }

        }
    }

    consultItem = async () => {


        const { modelServiceItems } = this.state;

        if (modelServiceItems.typeItem == typeItem.product) {
            await Axios.get(URL_Product).then(resp => {
                const { data } = resp
                if (data) {
                    let list = [];
                    data.results.forEach(element => {
                        const item = {
                            label: element.name,
                            value: element.id
                        }
                        list.push(item);
                    });
                    this.setState({ listItems: list })
                }
            })
        } else {
            await Axios.get(URL_Service).then(resp => {
                const { data } = resp
                if (data) {
                    let list = [];
                    data.results.forEach(element => {
                        const item = {
                            label: element.name,
                            value: element.id
                        }
                        list.push(item);
                    });
                    this.setState({ listItems: list })
                }
            })
        }

    }
    consultClient = async (e) => {
        if (e && e.length > 2) {
            await Axios.get(`${URL_SearchClient}/contains`, {
                params: { 'textOption': e }
            }).then(resp => {
                const { data } = resp
                if (data) {
                    let list = [];
                    data.forEach(element => {
                        const item = {
                            label: element.name,
                            value: element.id
                        }
                        list.push(item);
                    });
                    this.setState({ listClient: list })
                }
            })
        }
    }
    consultBudget = async (e) => {
        if (e && e.length > 2) {
            await Axios.get(`${URL_Budget}/GetByDescription`, {
                params: { 'textOption': e }
            }).then(resp => {
                const { data } = resp
                if (data) {
                    let list = [];
                    data.forEach(element => {
                        const item = {
                            label: element.description,
                            value: element.id
                        }
                        list.push(item);
                    });
                    this.setState({ listBudget: list })
                }
            })
        }
    }

    newService() {

        swal("Novo Serviço?", {
            buttons: true,
            dangerMode: true,
            icon: 'warning'
        }).then(resp => {
            if (resp) {
                this.setState({
                    listClient: [],
                    listBudget: [],
                    modelService: { id: 0, description: '', idClient: 0, nameClient: '' },
                    modelServiceItems: { id: 0, idServiceProvision: 0, typeItem: 0, idItem: 0, value: '', amount: 0, date: '' },
                    formServiceItems: { results: [], currentPage: '', pageCount: '', pageSize: '' }
                })
            }
        })

    }
    sumItensService(results) {
        let value = 0;

        results.forEach(element => {
            const sum = element.value * element.amount
            value += sum;
        });
        return value;

    }

    deleteItemService = async (id) => {
        await Axios.delete(`${URL_ServiceProvisionItems}/${id}`).then(resp => {
            const { data } = resp
            if (data) {
                this.consultItemsByIdService();
                this.clearInsertItems();
            }
        })
    }

    editItemService(model) {
        this.setState({
            modelServiceItems: model
        })
    }

    render() {
        const { modelService, modelServiceItems, errors_items, errors,
            listItems, formServiceItems, listClient, listBudget } = this.state
        const { currentPage, pageSize, rowCount, results } = formServiceItems;

        const customStyles = {
            control: (base, state) => ({
                ...base,
                borderColor: errors_items.idItemError ? 'red' : 'lightgray',

            })
        }
        const customStylesClient = {
            control: (base, state) => ({
                ...base,
                borderColor: errors.idClientError ? 'red' : 'lightgray',

            })
        }

        const valueName = [{
            value: modelService.idClient,
            label: modelService.nameClient
        }]
        const valueNameBudget = [{
            value: modelService.idBudget,
            label: modelService.descriptionBudget
        }]

        return (
            <Card>
                {/* <CardHeader>
                    <strong>Cadastro</strong>
                    <small> Serviço Prestado</small>
                </CardHeader> */}
                <CardBody>
                    <Form>
                        <FormGroup>
                            <div className='form-row'>
                                <div className="col-md-4">
                                    <Label for="services">Cliente:</Label>
                                    <Select
                                        placeholder="Cliente..."
                                        onInputChange={e => { this.consultClient(e) }}
                                        name="idClient"
                                        onChange={e => this.setValues(e, "idClient")}
                                        options={listClient}
                                        styles={customStylesClient}
                                        value={listClient.length == 0 ? valueName : undefined}
                                        noOptionsMessage={() => "Digite o nome do Cliente!"}
                                    />

                                </div>
                                <div className="col-md-4">
                                    <Label for="services">Orçamento:</Label>
                                    <Select
                                        placeholder="Orçamento..."
                                        onInputChange={e => { this.consultBudget(e) }}
                                        name="idBudget"
                                        onChange={e => this.setValues(e, "idBudget")}
                                        options={listBudget}
                                        //styles={customStylesClient}
                                        value={listBudget.length == 0 ? valueNameBudget : undefined}
                                        noOptionsMessage={() => "Digite a descrição do orçamento"}
                                    />
                                </div>
                                <div className="col-md-4">
                                    <Label>Descrição Serviço:*</Label>
                                    <Input
                                        type="text"
                                        onChange={e => this.setValues(e, 'description')}
                                        value={modelService.description}
                                        invalid={errors.nameError}
                                        disabled={modelService.id > 0 ? true : false}

                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="mt-2">
                                    <br />
                                    <Button
                                        onClick={e => this.saveService()}
                                        size="sm"
                                        color="success"><i class="fa fa-check"></i>{' '}Salvar</Button>
                                </div>
                            </div>
                            <br />
                            {modelService.id > 0 ?
                                <div disabled={true} className='card' >
                                    <div className='card-b'>
                                        <div className='card-body'>
                                            <div className='form-row'>
                                                <div className="col-md-8">
                                                    <Label>Produto/Serviço*:</Label>
                                                    <div className="form-row">
                                                        <div className="col-md-3">
                                                            <FormGroup check className="radio">
                                                                <Input className="form-check-input"
                                                                    type="radio"
                                                                    id="radio1"
                                                                    name="radios"
                                                                    onChange={e => this.setValues_Items(e, 'typeItem')}
                                                                    value="0" />
                                                                <Label check className="form-check-label" htmlFor="radio1">Produtos</Label>
                                                            </FormGroup>
                                                            <FormGroup check className="radio">
                                                                <Input className="form-check-input"
                                                                    type="radio"
                                                                    id="radio2"
                                                                    name="radios"
                                                                    onChange={e => this.setValues_Items(e, 'typeItem')}
                                                                    value="1" />
                                                                <Label check className="form-check-label" htmlFor="radio2">Serviços</Label>
                                                            </FormGroup>
                                                        </div>
                                                        <div className="col-md-9">
                                                            <Select
                                                                name="services"
                                                                placeholder="Serviço/Produto..."
                                                                isDisabled={modelServiceItems.id > 0 ? true : false}
                                                                onChange={e => this.setValues_Items(e, "idItem")}
                                                                options={listItems}
                                                                styles={customStyles}
                                                                value={{
                                                                    value: modelServiceItems.idItem,
                                                                    label: modelServiceItems.nameItem,

                                                                }}

                                                            />

                                                        </div>
                                                    </div>
                                                </div>
                                                <div className="col-md-2">
                                                    <Label htmlFor="serviceValue">Valor Produto:</Label>
                                                    <CurrencyInput
                                                        className={errors_items.valueError ?
                                                            "form-control border border-danger" :
                                                            "form-control"}
                                                        type="text"
                                                        decimalSeparator=","
                                                        thousandSeparator="."
                                                        prefix="R$"
                                                        onChangeEvent={e => this.setValues_Items(e, 'value')}
                                                        value={modelServiceItems.value}
                                                    >
                                                    </CurrencyInput>
                                                </div>
                                                <div className="col-md-2">
                                                    <Label>Quantidade*:</Label>
                                                    <Input
                                                        value={modelServiceItems.amount}
                                                        invalid={errors_items.amountError}
                                                        onChange={e => this.setValues_Items(e, 'amount')}
                                                        type="number"
                                                    >
                                                    </Input>
                                                    <FormFeedback></FormFeedback>
                                                </div>
                                            </div>
                                            <div className='form-row'>
                                                <div className="mt-2">
                                                    <Button onClick={e => this.insertItem()} size="sm" color="primary"><i class="fa fa-plus-circle"></i>{' '}Inserir</Button>
                                                </div>

                                            </div>

                                            <br />
                                            <div className="form-row">
                                                <div className="col-md-12">
                                                    <Table responsive size="sm">
                                                        <thead>
                                                            <tr>
                                                                <th>Nome</th>
                                                                <th>Valor Un</th>
                                                                <th>Quantidade</th>
                                                                <th>Valor Total</th>
                                                                <th>Data</th>
                                                                <th>Item</th>
                                                                <th>Opções</th>
                                                            </tr>
                                                        </thead>
                                                        <tbody>
                                                            {results.map(item => (
                                                                <tr>
                                                                    <td>{item.nameItem}</td>
                                                                    <td>{<NumberFormat
                                                                        displayType={'text'}
                                                                        value={item.value}
                                                                        thousandSeparator={'.'}
                                                                        decimalSeparator={','}
                                                                        prefix={'R$'} />}</td>
                                                                    <td>{item.amount}</td>
                                                                    <td>{<NumberFormat
                                                                        displayType={'text'}
                                                                        value={item.value * item.amount}
                                                                        thousandSeparator={'.'}
                                                                        decimalSeparator={','}
                                                                        prefix={'R$'} />}</td>
                                                                    <td>{moment(item.date).format("DD-MM-YYYY")}</td>
                                                                    <td>
                                                                        {
                                                                            item.typeItem == typeItem.product ?
                                                                                <span className="badge badge-primary">Produto</span> :
                                                                                <span className="badge badge-warning">Serviço</span>
                                                                        }

                                                                    </td>
                                                                    <td>
                                                                        <Button
                                                                            onClick={e => this.editItemService(item)}
                                                                            color="primary"
                                                                            title="Editar"
                                                                        >
                                                                            <i className="cui-pencil"></i>

                                                                        </Button>{' '}
                                                                        <Button
                                                                            onClick={e => this.deleteItemService(item.id)}
                                                                            color="danger"
                                                                            title="Excluir">
                                                                            <i className="fa fa-trash-o"></i>
                                                                        </Button>

                                                                    </td>
                                                                </tr>
                                                            ))}
                                                        </tbody>
                                                    </Table>
                                                    <div className="d-flex align-items-left">
                                                        <div>
                                                            <select className="custom-select"
                                                                name="selectOptionAmount"
                                                                onChange={(pageSize) => this.consultItemsByIdServicePagination(pageSize, currentPage)}
                                                                value={pageSize}
                                                                multiple="">
                                                                <option >10</option>
                                                                <option defaultValue="1">25</option>
                                                                <option defaultValue="2">50</option>
                                                                <option defaultValue="3">100</option>
                                                            </select>
                                                        </div>
                                                        <div className="ml-auto">
                                                            <Pagination
                                                                activePage={currentPage}
                                                                totalItemsCount={rowCount}
                                                                itemsCountPerPage={pageSize}
                                                                onChange={(pageNumber) => this.consultItemsByIdServicePagination(pageSize, pageNumber)}
                                                                itemClass="page-item"
                                                                linkClass="page-link"
                                                                firstPageText="Primeira"
                                                                lastPageText="Última"
                                                            />
                                                        </div>
                                                    </div>
                                                </div>
                                            </div>
                                            <hr />

                                            <div className="animated fadeIn">
                                                <Row>
                                                    <Col md={4}>
                                                        <Widget02 header={<NumberFormat
                                                            displayType={'text'}
                                                            value={this.sumItensService(results)}
                                                            thousandSeparator={'.'}
                                                            decimalSeparator={','}

                                                            decimalScale={2}
                                                            prefix={'R$'} />
                                                        }
                                                            mainText="Valor Total"
                                                            icon="fa fa-money"
                                                            color="primary"
                                                            variant="1" />
                                                    </Col>
                                                </Row>
                                            </div>
                                        </div>
                                    </div>
                                </div>
                                : null}
                        </FormGroup>
                        <FormGroup>
                            <Row>

                                <div>
                                    {modelService.id > 0 ?
                                        <Button
                                            onClick={e => this.newService()}
                                            size="sm"
                                            color="primary"><i class="fa fa-plus"></i>{' '}Novo Serviço</Button>
                                        : null}</div>

                                <div>
                                    {modelService.idBudget > 0 ?
                                        <Button
                                            onClick={e => this.newService()}
                                            size="sm"
                                            color="warning">
                                            <span class="fas fa-align-right"></span>
                                            {' '}
                                            Comparar Orçamento e Serviço</Button>
                                        : null}</div>

                            </Row>
                        </FormGroup>


                        <p className="float-right text-sm">
                            <i>Os campos marcados com (*) são obrigatórios</i>
                        </p>

                    </Form>
                </CardBody>
            </Card>
        )
    }

}
export default class ServiceProvision extends Component {
    state = {
        activeTab: 'con',
        modelService: { id: 0, description: '', idClient: 0, nameClient: '', idBudget: 0 },
    }

    componentWillMount() {
        PubSub.subscribe('edit-service', (topic, service) => {

            this.setState({
                modelService: service,
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
        const { modelService } = this.state
        return (
            <div>
                <Col xs="12" md="12" className="mb-4">
                    <Nav tabs>
                        <NavItem>
                            <NavLink
                                className={this.state.activeTab === 'con' ? 'active' : ''}
                                onClick={() => { this.toggleTab('con'); }}
                            >
                                Consulta Serviços Cadastrados
                            </NavLink>
                        </NavItem>
                        <NavItem>
                            <NavLink
                                className={this.state.activeTab === 'ser' ? 'active' : ''}
                                onClick={() => { this.toggleTab('ser'); }}
                            >
                                Cadastro Serviços
                            </NavLink>
                        </NavItem>
                        {modelService.id > 0 ?
                            <NavItem
                            >
                                <NavLink
                                    className={this.state.activeTab === 'bud' ? 'active' : ''}
                                    onClick={() => { this.toggleTab('bud'); }}
                                >
                                    Orçado x Prestado
                                </NavLink>
                            </NavItem> : null}

                    </Nav>
                    <TabContent activeTab={this.state.activeTab} onSelect>
                        <TabPane tabId="con" role="tabpanel">
                            <ListServiceProvision />
                        </TabPane>
                    </TabContent>
                    <TabContent activeTab={this.state.activeTab} onSelect>
                        <TabPane tabId="ser" role="tabpanel">
                            <FormServiceProvision toggleTab={this.toggleTab} />
                        </TabPane>
                    </TabContent>
                    <TabContent activeTab={this.state.activeTab} onSelect>
                        <TabPane tabId="bud" role="tabpanel">
                            <FormBudgetPerformed />
                        </TabPane>
                    </TabContent>
                </Col>
            </div>
        )
    }


}