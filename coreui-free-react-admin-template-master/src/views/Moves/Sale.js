
import React, { Component } from 'react';
import NumberFormat from 'react-number-format';
import moment from 'moment';
import Select from 'react-select';
import {
    Button,
    Card,
    CardBody,
    CardFooter,
    CardHeader,
    CardTitle,
    Col,
    Collapse,
    FormGroup,
    FormFeedback,
    Input,
    Label,
    Row,
    Table,
    Nav,
    NavItem,
    NavLink,
    TabContent,
    TabPane,
    Alert,
    UncontrolledCollapse,
    Form
} from 'reactstrap';
import CurrencyInput from 'react-currency-input'
import { URL_SearchClient } from '../../services/searchClientService';
import Axios from 'axios';
import { URL_Salesman } from '../../services/salesmanService';
import { URL_Service } from '../../services/serviceProvidedService';
import { URL_Product } from '../../services/productService';
import { typeItem } from '../../utils/enums/typeItem_enum';
import ConvertToUSD from './../../ConvertCurrency';
import { URL_Sale } from '../../services/saleService';
import { URL_SaleItems } from '../../services/saleItemsService';
import { FaSpinner } from 'react-icons/fa'
import Pagination, { bootstrap5PaginationPreset } from 'react-responsive-pagination'
import PubSub from 'pubsub-js';
import swal from 'sweetalert';
import { URL_CostCenter } from '../../services/costCenterService';

class ListSales extends Component {
    state = {
        accordionState: [true, false, false, false, false, false],
        ListSales: { results: [], currentPage: 0, pageCount: 0, pageSize: 0 },
        formFilter: { saleDateFinal: new Date, saleDate: new Date, idClient: 0, idSeller: 0 },
    }


    toggleAccordion = id => {
        let accordionState = this.state.accordionState.map((val, i) => {
            return id === i ? !val : (this.state.oneAtATime ? false : val)
        })
        this.setState({
            accordionState
        })
    }

    toggleLarge = () => {
        this.setState({
            large: !this.state.large
        });
    }

    consultSales = async () => {
        const { formFilter } = this.state
        await Axios.get(URL_Sale, {
            params: {
                saleDate: formFilter.saleDate,
                saleDateFinal: formFilter.saleDateFinal,
                idClient: formFilter.idClient,
                idSeller: formFilter.idSeller,
            }
        }).then(resp => {
            const { data } = resp
            this.setState({ ListSales: data })
        }).catch(() => { })
    }

    queryPagination = async (pageSizeValue, pageNumber) => {
        const { formFilter } = this.state
        let pageSize = ''
        if (pageSizeValue != undefined)
            pageSize = !pageSizeValue.target ? pageSizeValue : pageSizeValue.target.value;
        else
            pageSize = pageSizeValue;
        await Axios.get(URL_Sale, {
            params: {
                saleDate: formFilter.saleDate,
                saleDateFinal: formFilter.saleDateFinal,
                idClient: formFilter.idClient,
                idSeller: formFilter.idSeller,
                pageNumber: pageNumber,
                pageSize: pageSize
            }
        }).then(resp => {
            const { data } = resp
            this.setState({ ListSales: data })
        }).catch(() => { })

    }

    setValues = (e, field) => {
        const { formFilter } = this.state
        if (field == "idClient") {
            formFilter[field] = e != null ? e.value : 0;
            formFilter['nameClient'] = e != null ? e.label : ''
        } else if (field == "idSeller") {
            formFilter[field] = e != null ? e.value : 0;
            formFilter['nameSeller'] = e != null ? e.label : ''
        }
        else
            formFilter[field] = e.target.value
        this.setState({ formFilter })
    }



    onEdit = (sale) => {
        PubSub.publish('edit-sale', sale)
        this.props.toggleTab('sal')
    }

    render() {
        const { listClient, listSeller } = this.props
        const { ListSales, formFilter, } = this.state
        const { results, currentPage, pageSize, rowCount } = ListSales;
        return (
            <div>
                <Col lg="13">
                    <div>
                        {/* Checkout Process */}
                        <form action="" method="post" noValidate>
                            <div id="accordion">
                                {/* Checkout Method */}
                                <div className="card b mb-2">
                                    <CardHeader onClick={() => this.toggleAccordion(0)}>
                                        <CardTitle tag="h4">
                                            <a className="text-inherit">
                                                <small>
                                                    <em className="fa fa-plus text-primary mr-2"></em>
                                                </small>
                                                <span>Filtros</span>
                                            </a>
                                        </CardTitle>
                                    </CardHeader>
                                    <Collapse isOpen={this.state.accordionState[0]}>
                                        <CardBody id="collapse01">
                                            <FormGroup>
                                                <Row>
                                                    <Col lg="12">

                                                        <div className="form-row">
                                                            <div className="col-md-3">
                                                                <Label htmlFor="initialDate">Data Inicial Venda:</Label>
                                                                <Input
                                                                    type="date"
                                                                    value={formFilter.saleDate}
                                                                    onChange={e => this.setValues(e, 'saleDate')}
                                                                />
                                                            </div>
                                                            <div className="col-md-3">
                                                                <Label htmlFor="finalDate">Data Final Venda:</Label>
                                                                <Input
                                                                    type="date"
                                                                    value={formFilter.saleDateFinal}
                                                                    onChange={e => this.setValues(e, 'saleDateFinal')}
                                                                />
                                                            </div>
                                                            <div className="col-md-3">
                                                                <Label htmlFor="client">Cliente:</Label>
                                                                <Select
                                                                    placeholder="Cliente..."
                                                                    name="idClient"
                                                                    onChange={e => this.setValues(e, "idClient")}
                                                                    options={listClient}
                                                                    value={{
                                                                        value: formFilter.idClient,
                                                                        label: formFilter.nameClient
                                                                    }}
                                                                    isClearable={true}
                                                                    noOptionsMessage={() => "Digite o nome do Cliente!"}

                                                                />
                                                            </div>
                                                            <div className="col-md-3">
                                                                <Label htmlFor="salesman">Vendedor:</Label>
                                                                <Select
                                                                    name="idSeller"
                                                                    placeholder="Vendedor..."
                                                                    onChange={e => this.setValues(e, "idSeller")}
                                                                    options={listSeller}
                                                                    value={{
                                                                        value: formFilter.idSeller,
                                                                        label: formFilter.nameSeller
                                                                    }}
                                                                    isClearable={true}
                                                                    noOptionsMessage={() => "Digite o nome do Vendedor!"}
                                                                /></div>
                                                            <div className="col-md-2">
                                                                <Button className="btn btn-sm btn-primary"
                                                                    color="primary" onClick={e => this.consultSales()}>
                                                                    <em className="fa fa-search fa-fw"></em>
                                                                    Buscar
                                                                </Button>
                                                            </div>
                                                        </div>
                                                    </Col>
                                                </Row>
                                            </FormGroup>
                                            {/* <Modal isOpen={this.state.large} toggle={this.toggleLarge}
                                                position={'fixed'}
                                                className={'modal-lg ' + 'modal-info '}>
                                                <ModalHeader toggle={this.toggleLarge}>
                                                    <em className='fa fa-shopping-cart'></em>{' '}
                                                    Vendas</ModalHeader>
                                                <FormModalSales />
                                            </Modal> */}
                                        </CardBody>
                                    </Collapse>
                                </div>
                            </div>
                        </form>
                    </div>
                </Col>

                <hr className="my-2" />
                <div className="card b">
                    <CardHeader>
                        <CardTitle tag="h4">
                            <a className="text-inherit">Vendas</a>
                        </CardTitle>
                    </CardHeader>
                    <Table size="sm" striped responsive>
                        <thead className="thead-light">
                            <tr>
                                <th>Cliente</th>
                                <th>Data Lançamento</th>
                                <th>Data Venda</th>
                                <th>Vendedor</th>
                                {/* <th>Status</th> */}
                                <th>Valor</th>
                                <th className=""><strong>Opções</strong></th>
                            </tr>
                        </thead>
                        <tbody>
                            {results.map(e => (

                                <tr key={e.id}>
                                    <td>
                                        {e.nameClient}
                                    </td>
                                    <td>
                                        {moment(e.releaseDate).format('DD-MM-YYYY')}
                                    </td>
                                    <td>
                                        {moment(e.saleDate).format('DD-MM-YYYY')}
                                    </td>
                                    <td>
                                        {e.nameSeller}
                                    </td>
                                    {/* <td>
                                        <span className="badge badge-warning">Aberto</span>
                                    </td> */}
                                    <td>
                                        {<NumberFormat
                                            prefix={'R$'}
                                            thousandSeparator=','
                                            decimalSeparator='.'
                                            displayType={'text'}
                                            value={e.valueSale}
                                        />
                                        }
                                    </td>
                                    <td>
                                        <button className="btn btn-sm btn-secondary" title="Editar" size="sm"
                                            onClick={x => this.onEdit(e)}
                                        >
                                            <em className="cui-pencil"></em>
                                        </button>
                                        {/* <button className="btn btn-sm btn-danger" title="Excluir" size="sm"
                                        //onClick={e => this.delete(fin)}
                                        ><em className="cui-circle-x "></em>
                                        </button>
                                        <button className="btn btn-sm btn-success" title="Imprimir" size="sm"
                                        //onClick={() => window.print("eiiiiiiiii")}
                                        ><em className="fa fa-print"></em>
                                        </button> */}
                                    </td>
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                    <CardFooter>
                        <div className="d-flex align-items-left">
                            <div>
                                <select className="custom-select"
                                    name="selectOptionAmount"
                                    onChange={(pageSize) => this.queryPagination(pageSize, 1)}
                                    defaultValue="" multiple="">
                                    <option >10</option>
                                    <option defaultValue="1">25</option>
                                    <option defaultValue="2">50</option>
                                    <option defaultValue="3">100</option>
                                </select>
                            </div>
                            <div className="ml-auto">
                                <Pagination
                                    maxWidth={2}
                                    {...bootstrap5PaginationPreset}
                                    current={currentPage}
                                    total={Math.round(rowCount / 10, 1) + 1}
                                    onPageChange={(pageNumber) => this.queryPagination(pageSize, pageNumber)}
                                    narrowStrategy={['dropEllipsis', 'dropNav']}
                                    renderNav={false}
                                />
                            </div>
                        </div>
                    </CardFooter>

                </div>
            </div>
        )
    }
}
class FormSales extends Component {

    state = {
        model: {
            id: 0, releaseDate: new Date, saleDate: new Date, idClient: 0, idSeller: 0
        },
        loading: false,
        // listClient: [],
        // listSeller: [],
        errors: {},
        errors_commission: {},
        ListSateItems: { results: [], currentPage: '', pageCount: '', pageSize: '' },
        modelItems: {
            id: 0, idProduct: 0, idService: 0, value: '',
            amount: 0, date: '', nameItem: '', typeItem: 0, enableRecurrence: false, recurringAmount: 0
        },
        modelCommission: {
            id: 0, enableSharedCommission: false, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
            idSalesman: 0, nameSeller: '', recurringAmount: 0
        },
        listItems: [],
        errors_items: {},
        checked: false,
        formCostCenter: []
    }

    componentDidMount() {
        this.consultCostCenter()
        // this.consultClient()
    }


    setValues = (e, field) => {
        console.log(e, field)
        const { model } = this.state
        if (field == "idClient") {
            model[field] = e.value;
            model['nameClient'] = e.label
        } else if (field == "idSeller") {
            model[field] = e != null ? e.value : 0;
            model['nameSeller'] = e != null ? e.label : ''
        }
        else
            model[field] = e.target.value
        this.setState({ model })
    }

    setValuesCommissions = (e, field) => {
        const { modelCommission } = this.state
        if (field == "idSalesman") {
            modelCommission[field] = e != null ? e.value : 0;
            modelCommission['nameSeller'] = e != null ? e.label : ''
        }
        else
            modelCommission[field] = e.target.type == "checkbox" ? e.target.checked : e.target.value
        this.setState({ modelCommission })
        this.validate_sharedCommission()
    }

    // consultClient = async () => {
    //     await Axios.get(URL_SearchClient
    //     ).then(resp => {
    //         const { data } = resp
    //         if (data) {
    //             let list = [];
    //             data.forEach(element => {
    //                 const item = {
    //                     label: element.name,
    //                     value: element.id
    //                 }
    //                 list.push(item);
    //             });
    //             this.setState({ listClient: list })
    //         }
    //     })
    // }

    // consultSalesman = async (e) => {
    //     if (e && e.length > 2) {
    //         await Axios.get(`${URL_Salesman}/GetListByName`, {
    //             params: { textOption: e }
    //         }).then(resp => {
    //             const { data } = resp
    //             if (data) {
    //                 let list = [];
    //                 data.forEach(element => {
    //                     const item = {
    //                         label: element.name,
    //                         value: element.id
    //                     }
    //                     list.push(item);
    //                 });
    //                 this.setState({ listSeller: list })
    //             }
    //         })
    //     }
    // }

    setValues_Items = (e, field) => {
        const { modelItems } = this.state;
        if (field == "idItem") {
            modelItems['idService'] = 0
            modelItems['idProduct'] = 0

            modelItems[modelItems.typeItem == typeItem.product ? "idProduct" : "idService"] = e.value;
            modelItems['nameItem'] = e.label
            modelItems['value'] = e.currencyValue
        }
        else
            modelItems[field] = e.target.type == "checkbox" ? e.target.checked : e.target.value;

        this.setState({ modelItems });
        if (field == 'typeItem') {
            this.consultItem();
        }
    }

    consultItem = async () => {
        const { modelItems } = this.state;
        if (modelItems.typeItem == typeItem.product) {
            await Axios.get(URL_Product).then(resp => {
                const { data } = resp
                if (data) {
                    let list = [];
                    data.results.forEach(element => {
                        const item = {
                            label: element.name,
                            value: element.id,
                            currencyValue: element.value
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
                            value: element.id,
                            currencyValue: element.value
                        }
                        list.push(item);
                    });
                    this.setState({ listItems: list })
                }
            })
        }
    }

    validate_items = () => {
        let isError = 0;
        const data = this.state.modelItems
        const errors_items = {}
        const value = ConvertToUSD(data.value != '' ? data.value : 0);
        if (data.idProduct == 0 && data.idService == 0) {
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
            errors_items: errors_items
        });
        return isError;
    }

    validate_sharedCommission = () => {
        let isError = 0;
        const data = this.state.modelCommission
        const errors_commission = {}
        if (data.enableSharedCommission) {
            if (data.idSalesman == 0) {
                isError++;
                errors_commission.idSalesmanError = true;
            }
            else
                errors_commission.idSalesmanError = false;

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

        }
        else {
            this.setState({
                errors_commission: {}
            });
        }
        return isError;
    }

    validate = () => {
        let isError = 0;
        const data = this.state.model
        const errors = {}
        if (!data.saleDate) {
            isError++;
            errors.saleDateError = true;
        }
        else
            errors.saleDateError = false;
        if (!data.releaseDate) {
            isError++;
            errors.releaseDateError = true;
        }
        else
            errors.releaseDateError = false;
        if (data.idClient == 0) {
            isError++;
            errors.idClientError = true;
        }
        else
            errors.idClientError = false;
        this.setState({
            errors: errors
        });
        return isError;
    }

    insertItem = async () => {
        if (this.validate_items() == 0 &&
            this.validate() == 0 &&
            this.validate_sharedCommission() == 0) {
            this.setState({ loading: true })
            const { model, modelItems, modelCommission } = this.state
            const value = ConvertToUSD(modelItems.value)
            let mapSaleItems = {
                id: modelItems.id,
                idSale: model.id,
                idProduct: modelItems.idProduct,
                idService: modelItems.idService,
                value: parseFloat(value),
                amount: modelItems.amount,
                inclusionDate: new Date,
                idSeller: model.idSeller,
                typeItem: modelItems.typeItem,
                enableRecurrence: modelItems.enableRecurrence,
                recurringAmount: modelItems.enableRecurrence ? modelItems.recurringAmount : 1,
                SharedCommissions: modelCommission.percentage > 0 ? [modelCommission] : []
            }

            if (model.id == 0) {
                model.saleItems = [mapSaleItems];
                await Axios.post(`${URL_Sale}/PostWithItems`, model).then(resp => {
                    const { data } = resp
                    model.id = data;
                    this.setState({ model, loading: false })
                    this.clearItensFields();
                    this.consultArrayItems();
                }).catch(() => { this.setState({ loading: false }) })
            } else {
                if (mapSaleItems.id > 0) {
                    await Axios.put(URL_SaleItems, mapSaleItems).then(resp => {
                        this.setState({ loading: false })
                        this.clearItensFields();
                        this.consultArrayItems();
                    }).catch(() => { this.setState({ loading: false }) })
                } else {
                    await Axios.post(URL_SaleItems, mapSaleItems).then(resp => {
                        const { data } = resp
                        this.setState({ loading: false })
                        this.clearItensFields();
                        this.consultArrayItems();
                    }).catch(() => { this.setState({ loading: false }) })
                }
            }

        }
    }

    clearItensFields() {
        this.setState({
            modelItems: {
                id: 0, idProduct: 0, idService: 0, value: '',
                amount: 0, date: '', nameItem: '', typeItem: 0, enableRecurrence: false, recurringAmount: 0
            },
            modelCommission: {
                id: 0, enableSharedCommission: false, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
                idSalesman: 0, nameSeller: '', recurringAmount: 0, typeItem: 0
            },
            // listItems: [],
        })
    }

    consultArrayItems = async () => {
        const { model } = this.state
        await Axios.get(URL_SaleItems, {
            params: {
                idSale: model.id
            }
        }).then(resp => {
            const { data } = resp
            this.setState({ ListSateItems: data })
        })
    }

    componentWillMount = () => {
        PubSub.subscribe('edit-sale', (topic, sale) => {
            this.clearItensFields()
            this.consultItemsByIdSale(sale.id)
        })
    }

    consultItemsByIdSale = async (id) => {
        await Axios.get(`${URL_Sale}/GetByIdSale`, {
            params: {
                id: id
            }
        }).then(resp => {
            const { data } = resp
            this.setState({
                model: data,
                ListSateItems: { results: data.saleItems }
            })
        })
    }
    editItemSale = (item) => {
        this.setState({
            modelItems: item,
            modelCommission: item.sharedCommissions.length > 0 ? item.sharedCommissions[0] :
                {
                    id: 0, enableSharedCommission: false, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
                    idSalesman: 0, nameSeller: '', recurringAmount: 0
                },
        })
    }

    deleteItemSale = async (id) => {
        await Axios.delete(`${URL_SaleItems}/${id}`).then(resp => {
            const { data } = resp
            if (data) {
                this.clearItensFields();
                this.consultArrayItems()
            }
        })
    }
    newSale = () => {
        swal({ title: "Deseja realmente iniciar nova venda?", buttons: true, icon: 'warning' }).then(resp => {
            if (resp) {
                this.setState({
                    model: {
                        id: 0, releaseDate: new Date, saleDate: new Date, idClient: 0, idSeller: 0
                    },
                    ListSateItems: { results: [], currentPage: '', pageCount: '', pageSize: '' },
                    modelItems: {
                        id: 0, idProduct: 0, idService: 0, value: '', amount: 0,
                        date: '', nameItem: '', typeItem: 0, enableRecurrence: false, recurringAmount: 0
                    },
                    modelCommission: {
                        id: 0, enableSharedCommission: false, percentage: 0, commissionDay: 0, typeDay: 0, idCostCenter: 0,
                        idSalesman: 0, nameSeller: '', recurringAmount: 0
                    },

                })
            }
        })
    }
    saveSale = async () => {
        const { model } = this.state
        if (model.id > 0) {
            await Axios.put(URL_Sale, model).then(resp => {
                const { data } = resp
                if (data) {
                    swal({ title: 'Salvo com sucesso!', icon: 'success' })
                }
            })
        }
    }

    consultCostCenter = async (id) => {
        await Axios.get(`${URL_CostCenter}/GetByIdCompany`).then(resp => {
            const { data } = resp
            this.setState({ formCostCenter: data })
        })
    }

    // loadOptions = async (e) => {
    //     const{listClient}=this.state
    //     new Promise<listClient>((resolve) => {
    //         setTimeout(() => {
    //           resolve(filterColors(e));
    //         }, 1000);})

    // };
    //  filterColors = (inputValue: string) => {
    //     return colourOptions.filter((i) =>
    //       i.label.toLowerCase().includes(inputValue.toLowerCase())
    //     );
    //   };

    render() {
        const { listClient, listSeller } = this.props
        const { model, errors, ListSateItems, listItems
            , modelItems, errors_items, loading, modelCommission, formCostCenter,
            errors_commission } = this.state
        const { currentPage, pageSize, rowCount, results } = ListSateItems;

        const customStylesClient = {
            control: (base, state) => ({
                ...base,
                borderColor: errors.idClientError ? 'red' : 'lightgray'
            })
        }

        const customStyles = {
            control: (base, state) => ({
                ...base,
                borderColor: errors_items.idItemError ? 'red' : 'lightgray'
            })
        }

        const customStylesSalesman = {
            control: (base, state) => ({
                ...base,
                borderColor: errors_commission.idSalesmanError ? 'red' : 'lightgray'
            })
        }

        const sumTotalSale = results.reduce((partialSum, a) => partialSum + a.value * a.amount, 0);

        // const filterColors = async (e) => {
        //     let list = [];
        //     await Axios.get(`${URL_SearchClient}/contains`, { params: { textOption: e } }).then(resp => {
        //         const { data } = resp

        //         data.forEach(element => {
        //             const item = {
        //                 label: element.name,
        //                 value: element.id
        //             }
        //             list.push(item);
        //         });
        //     })
        //     return list.filter((i) =>
        //         i.label.toLowerCase().includes(e.toLowerCase())
        //     );
        // };

        // const promiseOptions = (e) =>
        //     new Promise((resolve) => {
        //         setTimeout(() => {
        //             resolve(filterColors(e));
        //         }, 500);
        //     });

        return (
            <div>
                <Card>
                    <CardBody>
                        <FormGroup>
                            <div className="row">
                                <div className="col-md-3">
                                    <Label for="dateThrow">Data Lançamento:*</Label>
                                    <Input
                                        type="date"
                                        value={moment(model.releaseDate).format('YYYY-MM-DD')}
                                        onChange={e => this.setValues(e, 'releaseDate')}
                                        invalid={errors.releaseDateError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="col-md-3">
                                    <Label for="dateSale">Data Venda:*</Label>
                                    <Input
                                        type="date"
                                        name="saleDate"
                                        value={moment(model.saleDate).format('YYYY-MM-DD')}
                                        onChange={e => this.setValues(e, 'saleDate')}
                                        invalid={errors.saleDateError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                            </div>
                            <div className="row">
                                <div className="col-md-6">
                                    <Label for="nameClient">Nome Cliente:*</Label>
                                    <Select
                                        isDisabled={model.id > 0 ? true : false}
                                        options={listClient}
                                        placeholder="Selecione o cliente..."
                                        onChange={e => this.setValues(e, "idClient")}
                                        styles={customStylesClient}
                                        value={{
                                            value: model.idClient,
                                            label: model.nameClient
                                        }}
                                        noOptionsMessage={() => "Digite o nome do Cliente!"}
                                    />
                                </div>
                                <div className="col-md-6">
                                    <Label >Nome Vendedor:</Label>
                                    <Select
                                        isDisabled={model.id > 0 ? true : false}
                                        name="idSeller"
                                        placeholder="Selecione o vendedor..."
                                        onChange={e => this.setValues(e, "idSeller")}
                                        options={listSeller}
                                        isClearable={true}
                                        value={{
                                            value: model.idSeller,
                                            label: model.nameSeller
                                        }}
                                    />
                                </div>
                            </div>
                            <hr />
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
                                                    value="0"
                                                />
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
                                                isDisabled={modelItems.id > 0 ? true : false}
                                                onChange={e => this.setValues_Items(e, "idItem")}
                                                options={listItems}
                                                styles={customStyles}
                                                value={
                                                    modelItems.typeItem == typeItem.product ?
                                                        {
                                                            value: modelItems.idProduct,
                                                            label: modelItems.nameItem,
                                                        } : {
                                                            value: modelItems.idService,
                                                            label: modelItems.nameItem,
                                                        }} />
                                        </div>
                                    </div>
                                </div>
                                <div className="col-md-2">
                                    <Label >Valor Produto:</Label>
                                    <CurrencyInput
                                        className=
                                        {errors_items.valueError ?
                                            "form-control border border-danger" :
                                            "form-control"}
                                        type="text"
                                        decimalSeparator=","
                                        thousandSeparator="."
                                        prefix="R$"
                                        onChangeEvent={e => this.setValues_Items(e, 'value')}
                                        value={modelItems.value}
                                    >
                                    </CurrencyInput>
                                </div>
                                <div className="col-md-2">
                                    <Label>Quantidade*:</Label>
                                    <Input
                                        value={modelItems.amount}
                                        invalid={errors_items.amountError}
                                        onChange={e => this.setValues_Items(e, 'amount')}
                                        type="number"
                                    >
                                    </Input>
                                    <FormFeedback></FormFeedback>
                                </div>
                            </div>
                            <div className='row'>
                                <div className='col-md'>
                                    <FormGroup check>
                                        <Input
                                            disabled={modelItems.amount > 0 && model.idSeller > 0 ? false : true}
                                            id="toggler"
                                            type="checkbox"
                                            checked={modelItems.enableRecurrence}
                                            onChange={e => this.setValues_Items(e, "enableRecurrence")}
                                        />
                                        <Label
                                            check
                                            for="toggler"
                                        >
                                            {"Comissão recorrente "}
                                        </Label>
                                    </FormGroup>
                                    <Collapse
                                        isOpen={modelItems.enableRecurrence}
                                    >
                                        <Card>
                                            <CardBody>
                                                <Form>
                                                    <Row className="row-cols-lg-auto g-3 align-items-center">
                                                        <Col md={4}>
                                                            <Label
                                                                className="visually-hidden"
                                                            >
                                                                Quantidade de recorrência:
                                                            </Label>
                                                            <Input
                                                                value={modelItems.recurringAmount}
                                                                onChange={e => this.setValues_Items(e, "recurringAmount")}
                                                                placeholder="quantidade..."
                                                                type="number"
                                                            />
                                                        </Col>
                                                    </Row>
                                                </Form>
                                            </CardBody>
                                        </Card>
                                    </Collapse>
                                </div>
                            </div>
                            <div className='row'>
                                <div className='col-md'>
                                    <FormGroup check>
                                        <Input
                                            disabled={modelItems.amount > 0 && model.idSeller > 0 ? false : true}
                                            type="checkbox"
                                            checked={modelCommission.enableSharedCommission}
                                            onChange={e => this.setValuesCommissions(e, "enableSharedCommission")}
                                        />
                                        <Label
                                            check
                                        >
                                            {"Comissão compartilhada "}
                                        </Label>
                                    </FormGroup>
                                    <Collapse
                                        //se habilitado todos os campos vai ser obigatório
                                        isOpen={modelCommission.enableSharedCommission}
                                    >
                                        <Card>
                                            <CardBody>
                                                <Form>
                                                    <Row className="row-cols-lg-auto g-3 align-items-center">
                                                        <Col>
                                                            <Label
                                                                className="visually-hidden"
                                                            >
                                                                Vendedor:
                                                            </Label>
                                                            <Select
                                                                name="idSeller"
                                                                placeholder="Vendedor..."
                                                                onChange={e => this.setValuesCommissions(e, "idSalesman")}
                                                                options={listSeller}
                                                                isClearable={true}
                                                                styles={customStylesSalesman}
                                                                value={{
                                                                    value: modelCommission.idSalesman,
                                                                    label: modelCommission.nameSeller
                                                                }}
                                                            />
                                                        </Col>
                                                        <Col>
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
                                                        <Col>
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

                                                        <Col>
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
                                                        <Col>
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
                                                        <Col>
                                                            <Label
                                                                className="visually-hidden"
                                                            >
                                                                Recorrência:
                                                            </Label>
                                                            <Input
                                                                type='number'
                                                                value={modelCommission.recurringAmount}
                                                                onChange={e => this.setValuesCommissions(e, 'recurringAmount',)}

                                                            />
                                                        </Col>
                                                    </Row>
                                                </Form>
                                            </CardBody>
                                        </Card>
                                    </Collapse>
                                </div>
                            </div>
                            <div className='form-row'>
                                <div className="mt-2">
                                    <Button size="sm"
                                        onClick={e => this.insertItem()}
                                        color="primary"
                                        disabled={loading}
                                    >
                                        {loading && <FaSpinner className='fa fa-spinner fa-spin' />}
                                        {loading && " Inserindo"}
                                        {!loading && <i className="fa fa-plus-circle"></i>}
                                        {!loading && " Salvar item"}
                                    </Button>
                                </div>

                            </div>

                            <br />
                            <div className="form-row">
                                <div className="col-md-12">
                                    <Table responsive size="sm">
                                        <thead>
                                            <tr>
                                                <th>Item</th>
                                                <th>Valor Un</th>
                                                <th>Quantidade</th>
                                                <th>Valor Total</th>

                                                <th>Opções</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {results.map(item => (
                                                <tr key={item.id}>
                                                    <td>
                                                        {item.nameItem}
                                                    </td>
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
                                                    <td>
                                                        <Button
                                                            title="Editar"
                                                            onClick={e => this.editItemSale(item)}
                                                            color="primary">
                                                            <i className="cui-pencil"></i>
                                                        </Button>{' '}
                                                        <Button
                                                            title="Excluir"
                                                            onClick={e => this.deleteItemSale(item.id)}
                                                            color="danger">
                                                            <i className="fa fa-trash-o"></i>
                                                        </Button>

                                                    </td>
                                                </tr>
                                            ))}
                                        </tbody>

                                    </Table>
                                    <div className="d-flex align-items-left">
                                        {/* <div>
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
                                        </div> */}
                                        {results.length > 0 ?
                                            <div>
                                                <label className='text-success font-weight-bold'>Valor Total da Venda:</label>
                                                {' '}{sumTotalSale.toLocaleString('pt-BR', { style: 'currency', currency: 'BRL' })}</div>
                                            : ''}
                                        <div className="ml-auto">
                                            {/* <Pagination
                                                activePage={currentPage}
                                                totalItemsCount={rowCount}
                                                itemsCountPerPage={pageSize}
                                                onChange={(pageNumber) => this.consultItemsByIdBudgetPagination(pageSize, pageNumber)}
                                                itemClass="page-item"
                                                linkClass="page-link"
                                                firstPageText="Primeira"
                                                lastPageText="Última"
                                            /> */}
                                        </div>
                                    </div>
                                </div>
                            </div>
                            <hr />
                            <Button
                                onClick={e => { this.newSale() }}
                            >Nova Venda</Button>{' '}
                            {/* {model.id > 0 ?
                                <Button
                                    color='success'
                                    onClick={e => { this.saveSale() }}
                                > Salvar Venda</Button> : null} */}
                            {/* <div className="animated fadeIn">
                                        <Row>
                                            <Col md={4}>
                                                <Widget02 header={<NumberFormat
                                                    displayType={'text'}
                                                    // value={this.sumItensBudget(results)}
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
                                        </Row> */}
                            {/* </div>
                                                </div>
                                            </div>
                                        </div>
                                         </div> */}
                            {/* </div> */}
                        </FormGroup>
                    </CardBody>
                </Card>
            </div>
        )
    }
}
export default class SaleOperation extends Component {
    state = { activeTab: 'sal', listClient: [], listSeller: [] }

    toggleTab = tab => {
        if (this.state.activeTab !== tab) {
            this.setState({
                activeTab: tab
            });
        }
    }
    componentDidMount() {
        this.consultClient()
        this.consultSalesman()
    }

    consultClient = async () => {
        await Axios.get(URL_SearchClient).then(resp => {
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

    consultSalesman = async () => {
        await Axios.get(`${URL_Salesman}/GetAllList`).then(resp => {
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
                this.setState({ listSeller: list })
            }
        })
    }

    render() {
        const { listClient, listSeller } = this.state
        return (
            <div>
                <Nav tabs>
                    <NavItem>
                        <NavLink
                            className={this.state.activeTab === 'sal' ? 'active' : ''}
                            onClick={() => { this.toggleTab('sal'); }}
                        >
                            Venda
                        </NavLink>
                    </NavItem>
                    <NavItem>
                        <NavLink
                            className={this.state.activeTab === 'con' ? 'active' : ''}
                            onClick={() => { this.toggleTab('con'); }}
                        >
                            Consulta Vendas
                        </NavLink>
                    </NavItem>
                </Nav>
                <TabContent activeTab={this.state.activeTab}>
                    <TabPane tabId="sal" role="tabpanel">
                        <FormSales listClient={listClient}
                            listSeller={listSeller}
                            toggleTab={this.toggleTab} />
                    </TabPane>
                </TabContent>
                <TabContent activeTab={this.state.activeTab}>
                    <TabPane tabId="con" role="tabpanel">
                        <ListSales listClient={listClient}
                            listSeller={listSeller}
                            toggleTab={this.toggleTab} />
                    </TabPane>
                </TabContent>
            </div>
        )
    }
}