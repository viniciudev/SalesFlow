import React, { Component } from "react"
import NumberFormat from 'react-number-format';
import moment from 'moment';
// import Pagination from 'react-js-pagination';
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
import { URL_Financial } from "../../services/financialService";
export default class Commissions extends Component {

    render() {
        return (<div><ConsultCommissions /></div>)
    }
}
class ConsultCommissions extends Component {

    state = {
        accordionState: [true, false, false, false, false, false],
        formFilter: { saleDateFinal: new Date, saleDate: new Date, idSeller: 0 },
        ListCommissions: { results: [], currentPage: 0, pageCount: 0, pageSize: 0 },
        listSeller: []
    }

    toggleAccordion = id => {
        let accordionState = this.state.accordionState.map((val, i) => {
            return id === i ? !val : (this.state.oneAtATime ? false : val)
        })
        this.setState({
            accordionState
        })
    }

    setValues = (e, field) => {
        const { formFilter } = this.state
        if (field == "idSeller") {
            formFilter[field] = e != null ? e.value : 0;
            formFilter['nameSeller'] = e != null ? e.label : ''
        }
        else
            formFilter[field] = e.target.value
        this.setState({ formFilter })
    }

    componentDidMount() {
        this.consultSalesman()
    }

    consultSalesman = async (e) => {
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

    consultCommissions = async () => {
        const { formFilter } = this.state
        await Axios.get(`${URL_Financial}/GetCommissions`, {
            params: {
                idSeller: formFilter.idSeller,
                saleDate: formFilter.saleDate,
                saleDateFinal: formFilter.saleDateFinal
            }
        }).then(resp => {
            const { data } = resp
            this.setState({ ListCommissions: data })
        }).catch(() => { })
    }

    queryPagination = async (pageSizeValue, pageNumber) => {
        const { formFilter } = this.state
        let pageSize = ''
        if (pageSizeValue != undefined)
            pageSize = pageSizeValue;
        else
            pageSize = pageSizeValue;
        await Axios.get(`${URL_Financial}/GetCommissions`, {
            params: {
                idSeller: formFilter.idSeller,
                saleDate: formFilter.saleDate,
                saleDateFinal: formFilter.saleDateFinal,
                pageNumber: pageNumber,
                pageSize: pageSize
            }
        }).then(resp => {
            const { data } = resp
            this.setState({ ListCommissions: data })
        }).catch(() => { })
    }
    render() {
        const { formFilter, ListCommissions, listSeller } = this.state
        const { results, currentPage, rowCount, pageSize } = ListCommissions
        return (
            <div>
                <div>
                    {/* Checkout Process */}
                    <form action="" method="post" noValidate>
                        <div id="accordion">
                            {/* Checkout Method */}
                            <div className="card b mb-2">
                                <CardHeader onClick={() => this.toggleAccordion(0)}>
                                    <CardTitle tag="h5">
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
                                            <div className="form-row">
                                                <div className="col-md-3">
                                                    <Label htmlFor="salesman">Vendedor:</Label>
                                                    <Select
                                                        size="lg"
                                                        name="idSeller"
                                                        placeholder="Selecione o vendedor..."
                                                        onChange={e => this.setValues(e, "idSeller")}
                                                        options={listSeller}
                                                        value={{
                                                            value: formFilter.idSeller,
                                                            label: formFilter.nameSeller
                                                        }}
                                                        isClearable={true}
                                                        noOptionsMessage={() => "Digite o nome do Vendedor!"}
                                                    ></Select>

                                                </div>
                                                <div className="col-md-3">
                                                    <Label htmlFor="initialDate">Data Inicial:</Label>
                                                    <Input
                                                        type="date"
                                                        value={formFilter.saleDate}
                                                        onChange={e => this.setValues(e, 'saleDate')}
                                                    />
                                                </div>
                                                <div className="col-md-3">
                                                    <Label htmlFor="finalDate">Data Final:</Label>
                                                    <Input
                                                        type="date"
                                                        value={formFilter.saleDateFinal}
                                                        onChange={e => this.setValues(e, 'saleDateFinal')}
                                                    />
                                                </div>
                                                <div className="col-md-2 mt-4">
                                                    <Button className="btn btn-sm btn-primary"
                                                        color="primary" onClick={e => this.consultCommissions()}>
                                                        <em className="fa fa-search fa-fw"></em>
                                                        Buscar
                                                    </Button>
                                                </div>
                                            </div>
                                        </FormGroup>
                                    </CardBody>
                                </Collapse>
                            </div>
                        </div>
                    </form>
                </div>
                <div className="card b">
                    <CardHeader>
                        <CardTitle tag="h6">
                            <a className="text-inherit">Commissões</a>
                        </CardTitle>
                    </CardHeader>
                    <Table size="sm" striped responsive>
                        <thead className="thead-light">
                            <tr>
                                <th>Código</th>
                                <th>Cliente/Origem</th>
                                <th>Data Comissão</th>
                                <th>Data Venda</th>
                                <th>Vencimento</th>
                                <th>Item</th>
                                <th>%</th>
                                <th>Valor</th>
                                {/* <th className=""><strong>Opções</strong></th> */}
                            </tr>
                        </thead>
                        <tbody>
                            {results.map(e => (

                                <tr key={e.id}>
                                    <td>
                                        {e.idFinancial}
                                    </td>
                                    <td>
                                        {e.origin}
                                    </td>
                                    <td>
                                        {moment(e.releaseDate).format('DD-MM-YYYY')}
                                    </td>
                                    <td>
                                        {moment(e.dateSale).format('DD-MM-YYYY')}
                                    </td>
                                    <td>
                                        {moment(e.dueDate).format('DD-MM-YYYY')}
                                    </td>
                                    <td>
                                        {e.item}
                                    </td>
                                    <td>
                                        <span className="badge badge-warning">{e.percentage}</span>
                                    </td>
                                    <td>
                                        {<NumberFormat
                                            prefix={'R$'}
                                            thousandSeparator=','
                                            decimalSeparator='.'
                                            displayType={'text'}
                                            value={e.value}
                                        />
                                        }
                                    </td>
                                    {/* <td>
                                        <button className="btn btn-sm btn-secondary" title="Editar" size="sm"
                                            onClick={x => this.onEdit(e)}
                                        >
                                            <em className="cui-pencil"></em>
                                        </button>
                                        <button className="btn btn-sm btn-danger" title="Excluir" size="sm"
                                        //onClick={e => this.delete(fin)}
                                        >
                                            <em className="cui-circle-x "></em>
                                        </button>

                                        <button className="btn btn-sm btn-success" title="Imprimir" size="sm"
                                        //onClick={() => window.print("eiiiiiiiii")}
                                        >

                                            <em className="fa fa-print"></em>
                                        </button>

                                    </td> */}
                                </tr>
                            ))}
                        </tbody>
                    </Table>
                    <CardFooter>
                        <div className="d-flex align-items-left">
                            <div>
                                <select className="custom-select"
                                    name="selectOptionAmount"
                                    onChange={(pageSize) => this.queryPagination(pageSize, '')}
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
                    <div className="text-center">
                        <h6 className="text-success">Total comissões período selecionado:
                            {results.reduce((partialSum, a) => partialSum + a.value, 0)
                                .toLocaleString('pt-br', { style: 'currency', currency: 'BRL' })}
                        </h6></div>
                </div>
            </div>

        );
    }
}