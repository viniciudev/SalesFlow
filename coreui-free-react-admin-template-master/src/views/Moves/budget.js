import React, { Component, Suspense } from 'react';
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
    Col,
    Row
} from 'reactstrap';
import CurrencyInput from 'react-currency-input';
import ConvertToUSD from './../../ConvertCurrency';
import Axios from 'axios';
import { URL_Product } from './../../services/productService';
import swal from 'sweetalert';
import { URL_Budget } from '../../services/budgetService';
import { URL_BudgetItems } from '../../services/budgetService';
import { URL_Service } from '../../services/serviceProvidedService'
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js';
import Select from 'react-select';
import Widget02 from './../../widgets/Widget02';
import { typeItem } from '../../utils/enums/typeItem_enum';
import moment from 'moment';
import Pagination from 'react-js-pagination';
import { URL_SearchClient } from './../../services/searchClientService';

class FormBudget extends Component {

    state = {
        listItems: [],
        listClient: [],
        errors: {},
        errors_items: {},
        modelBudget: { id: 0, description: '', idClient: 0, nameClient: '' },
        modelBudgetItems: { id: 0, idBudget: 0, typeItem: 0, idItem: 0, value: '', amount: 0, date: '', nameItem: '' },
        formBudgetItems: { results: [], currentPage: '', pageCount: '', pageSize: '' }
    }

    setValues = (e, field) => {
        const { modelBudget } = this.state;
        if (field == "idClient")
            modelBudget[field] = e.value;
        else
            modelBudget[field] = e.target.value;
        this.setState({ modelBudget });
    }

    setValues_Items = (e, field) => {
        const { modelBudgetItems } = this.state;

        if (field == "idItem") {
            modelBudgetItems[field] = e.value;
            modelBudgetItems['nameItem'] = e.label
        }
        else
            modelBudgetItems[field] = e.target.value;

        this.setState({ modelBudgetItems });
        if (field == 'typeItem') {
            this.consultItem();
        }
    }

    validate_items = () => {
        let isError = 0;
        const data = this.state.modelBudgetItems
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
        const dados = this.state.modelBudget
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
        this.setState({ modelBudget: { id: 0, name: '', value: '' } })
    }

    insertItem = async () => {
        if (this.validate_items() == 0) {
            const { modelBudget, modelBudgetItems } = this.state
            const value = ConvertToUSD(modelBudgetItems.value)
            let map = {
                id: modelBudgetItems.id,
                idBudget: modelBudget.id,
                typeItem: modelBudgetItems.typeItem,
                idItem: modelBudgetItems.idItem,
                value: parseFloat(value),
                amount: modelBudgetItems.amount,
                date: new Date()
            }
            if (modelBudgetItems.id > 0) {
                await Axios.put(URL_BudgetItems, map).then(resp => {
                    const { data } = resp
                    if (data) {
                        this.consultItemsByIdBudget();
                        this.clearInsertItems();
                    } else {
                        swal("Atualização não concluída!", {
                            buttons: true,
                            icon: 'warning',
                            dangerMode: true,

                        })
                    }
                })
            }
            else {
                await Axios.post(URL_BudgetItems, map).then(resp => {
                    const { data } = resp
                    if (data == "") {
                        this.consultItemsByIdBudget();
                        this.clearInsertItems();
                    } else {
                        swal(data, {
                            buttons: true,
                            icon: 'warning',
                            dangerMode: true,

                        })
                    }
                })
            }

        }
    }

    clearInsertItems() {
        this.setState({
            modelBudgetItems: { id: 0, idBudget: 0, typeItem: 0, idItem: 0, value: '', amount: 0, date: '', nameItem: '' },
        })
    }

    consultItemsByIdBudget = async () => {
        const { modelBudget } = this.state
        await Axios.get(`${URL_BudgetItems}/GedByIdBudget`, {
            params: {
                idBudget: modelBudget.id
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formBudgetItems: data
                })
            }
        })
    }

    consultItemsByIdBudgetPagination = async (pageSizeValue, pageNumber) => {
        let pageSize = ''
        if (pageSizeValue.target != undefined)
            pageSize = pageSizeValue.target.value;
        else
            pageSize = pageSizeValue;
        const { modelBudget } = this.state

        await Axios.get(`${URL_BudgetItems}/GedByIdBudget`, {
            params: {
                idBudget: modelBudget.id,
                pageSize: pageSize, pageNumber: pageNumber
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formBudgetItems: data
                })
            }
        })
    }





    componentWillMount() {
        PubSub.subscribe('edit-budget', (topic, budget) => {

            this.setState({
                modelBudget: budget,
            })
            this.consultItemsByIdBudget()

        })
    }

    saveBudget = async () => {
        if (this.validate() == 0) {
            const { modelBudget } = this.state

            let data = {
                date: new Date(),
                description: modelBudget.description,
                idClient: modelBudget.idClient
            }
            Axios.post(URL_Budget, data).then(resp => {
                const { data } = resp
                if (data) {
                    this.setState({ modelBudget: data })
                }
            })
        }
    }

    consultItem = async () => {


        const { modelBudgetItems } = this.state;

        if (modelBudgetItems.typeItem == typeItem.product) {
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

    newBudget() {

        swal("Novo Orçamento?", {
            buttons: true,
            dangerMode: true,
            icon: 'warning'
        }).then(resp => {
            if (resp) {
                this.setState({
                    listClient: [],
                    modelBudget: { id: 0, description: '', idClient: 0, nameClient: '' },
                    modelBudgetItems: { id: 0, idBudget: 0, typeItem: 0, idItem: 0, value: '', amount: 0, date: '', nameItem: '' },
                    formBudgetItems: { results: [], currentPage: '', pageCount: '', pageSize: '' }
                })
            }
        })

    }
    sumItensBudget(results) {
        let value = 0;

        results.forEach(element => {
            const sum = element.value * element.amount
            value += sum;
        });
        return value;

    }

    deleteItemBudget = async (id) => {
        await Axios.delete(`${URL_BudgetItems}/${id}`).then(resp => {
            const { data } = resp
            if (data) {
                this.consultItemsByIdBudget();
                this.clearInsertItems();
            }
        })
    }

    editItemBudget(model) {
        this.setState({
            modelBudgetItems: model
        })
    }

    render() {
        const { modelBudget, modelBudgetItems, errors_items, errors, listItems, formBudgetItems, listClient } = this.state
        const { currentPage, pageSize, rowCount, results } = formBudgetItems;

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
            value: modelBudget.idClient,
            label: modelBudget.nameClient
        }]

        return (
            <Card>
                <CardHeader>
                    <strong>Cadastro</strong>
                    <small> Orçamento</small>
                </CardHeader>
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
                                    <Label>Descrição Orçamento:*</Label>
                                    <Input
                                        type="text"
                                        onChange={e => this.setValues(e, 'description')}
                                        value={modelBudget.description}
                                        invalid={errors.nameError}
                                        disabled={modelBudget.id > 0 ? true : false}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>


                                <div className="mt-2">
                                    <br />
                                    {modelBudget.id > 0 ? null :
                                        <Button
                                            onClick={e => this.saveBudget()}
                                            size="sm"
                                            color="success"><i class="fa fa-check"></i>{' '}Salvar</Button>
                                    }
                                </div>
                            </div>
                            <br />
                            {modelBudget.id > 0 ?
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
                                                                isDisabled={modelBudgetItems.id > 0 ? true : false}
                                                                onChange={e => this.setValues_Items(e, "idItem")}
                                                                options={listItems}
                                                                styles={customStyles}
                                                                value={{
                                                                    value: modelBudgetItems.idItem,
                                                                    label: modelBudgetItems.nameItem,
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
                                                        value={modelBudgetItems.value}
                                                    >
                                                    </CurrencyInput>
                                                </div>
                                                <div className="col-md-2">
                                                    <Label>Quantidade*:</Label>
                                                    <Input
                                                        value={modelBudgetItems.amount}
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
                                                    <Button
                                                        onClick={e => this.insertItem()}
                                                        size="sm"
                                                        color="primary">
                                                        <i class="fa fa-plus-circle"></i>{' '}Inserir</Button>
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
                                                                            title="Editar"
                                                                            onClick={e => this.editItemBudget(item)}
                                                                            color="primary">
                                                                            <i className="cui-pencil"></i>
                                                                        </Button>{' '}
                                                                        <Button
                                                                            title="Excluir"
                                                                            onClick={e => this.deleteItemBudget(item.id)}
                                                                            color="danger">
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
                                                                onChange={(pageSize) => this.consultItemsByIdBudgetPagination(pageSize, currentPage)}
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
                                                                onChange={(pageNumber) => this.consultItemsByIdBudgetPagination(pageSize, pageNumber)}
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
                                                            value={this.sumItensBudget(results)}
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
                        <div>
                            {modelBudget.id > 0 ?
                                <Button
                                    onClick={e => this.newBudget()}
                                    size="sm"
                                    color="primary"><i class="fa fa-plus"></i>{' '}Novo Orçamento</Button>
                                : null}</div>

                        <p className="float-right text-sm">
                            <i>Os campos marcados com (*) são obrigatórios</i>
                        </p>

                    </Form>
                </CardBody>
            </Card>
        )
    }
}


class ListBudgets extends Component {

    state = {
        listClient: [], modelFilter: { idClient: 0 },
        formBudget: { results: [], currentPage: '', pageCount: '', pageSize: '' },
    }

    onEdit = (budget) => {
        PubSub.publish('edit-budget', budget)
    }

    consultBudgetPagination = async (pageSizeValue, pageNumber) => {
        const { modelFilter } = this.state
        let pageSize = ''
        if (pageSizeValue.target != undefined)
            pageSize = pageSizeValue.target.value;
        else
            pageSize = pageSizeValue;

        await Axios.get(`${URL_Budget}`, {
            params: {
                idClient: modelFilter.idClient,
                pageSize: pageSize, pageNumber: pageNumber
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formBudget: data
                })
            }
        })

        this.props.consultBudgetPagination(pageSize, pageNumber)
    }

    consultAll = async () => {
        const { modelFilter } = this.state
        await Axios.get(URL_Budget, {
            params: { idClient: modelFilter.idClient }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formBudget: data
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
        const { currentPage, pageSize, rowCount, results } = this.state.formBudget;
        const { listClient } = this.state
        return (
            <Card>
                <CardHeader>
                    <strong>Consultar</strong>
                    <small> Orçamentos Cadastrados</small>
                </CardHeader>
                <CardBody>
                    <div className="row">
                        <div className="col-md-10">

                            <Select
                                placeholder="Cliente..."
                                onInputChange={e => { this.consultClient(e) }}
                                name="idClient"
                                onChange={e => this.setValues(e, "idClient")}
                                options={listClient}
                                isClearable={true}

                                //value={listClient.length == 0 ? valueName : undefined}
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
                                <th>Nome</th>
                                <th>Data</th>
                                <th>Cliente</th>
                                <th>Opções</th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                results.map(s => (
                                    <tr>
                                        <td>{s.description}</td>
                                        <td>{moment(s.date).format("DD-MM-YYYY")}</td>
                                        <td>{s.nameClient}</td>
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
                                onChange={(pageSize) => this.consultBudgetPagination(pageSize, currentPage)}
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
                                onChange={(pageNumber) => this.consultBudgetPagination(pageSize, pageNumber)}
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


export default class Budget extends Component {
    state = {
        modelFilter: {},
    }

    render() {
        return (<div>
            <div className="row">

                <div className="col-md-4 my-3">
                    <ListBudgets
                        formBudget={this.state.formBudget}
                        consultBudgetPagination={this.consultBudgetPagination}
                    />
                </div>
                <div className="col-md-8 my-3" >
                    <FormBudget
                        consultAll={this.consultAll}
                    />
                </div>
            </div>
        </div>)
    }
}

