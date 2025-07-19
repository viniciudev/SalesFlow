import React, { Component } from "react";
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
    CardSubtitle,
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
    Form,
    Tooltip,
    InputGroupText
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
import { format, parse, parseISO } from 'date-fns'
import { URL_Closures } from "../../services/closuresService";


export default class ConsultClosures extends Component {

    state = {
        ListClosures: { results: [], currentPage: 0, pageCount: 0, pageSize: 0 },
        accordionState: [true, false, false, false, false, false],
        listSeller: [],
        formFilter: {
            idSalesman: 0,
            checkinDate: format(new Date, 'yyyy-MM-dd'),
            checkinDateFinal: format(new Date, 'yyyy-MM-dd')
        },
        guid: '',
        tooltipOpen: false
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
        if (field == 'idSalesman') {
            formFilter[field] = e != null ? e.value : 0
            formFilter['nameSalesman'] = e != null ? e.label : ''
        } else
            formFilter[field] = e.target.value
        this.setState({ formFilter })
    }

    componentDidMount() {
        let guid = localStorage.getItem('guid')
        this.setState({ guid: guid })
        this.consultSalesman()
    }

    consult = async () => {
        const { formFilter } = this.state
        if (formFilter.idSalesman > 0) {
            await Axios.get(`${URL_Closures}/GetByPaged`, {
                params: {
                    pageNumber: 1,
                    pageSize: 10,
                    idSalesman: formFilter.idSalesman,
                    checkinDate: formFilter.checkinDate,
                    checkinDateFinal: formFilter.checkinDateFinal
                }
            }).then(resp => {
                const { data } = resp
                this.setState({ ListClosures: data })
            }).catch((error) => { console.log(error) })
        }
    }

    queryPagination = async (pageSizeValue, pageNumber) => {

        let pageSize = ''
        if (pageSizeValue != undefined)
            pageSize = !pageSizeValue.target ? pageSizeValue : pageSizeValue.target.value;
        else
            pageSize = pageSizeValue;

        const { formFilter } = this.state
        if (formFilter.idSalesman > 0) {
            await Axios.get(`${URL_Closures}/GetByPaged`, {
                params: {
                    pageNumber: pageNumber,
                    pageSize: pageSize,
                    idSalesman: formFilter.idSalesman,
                    checkinDate: formFilter.checkinDate,
                    checkinDateFinal: formFilter.checkinDateFinal
                }
            }).then(resp => {
                const { data } = resp
                this.setState({ ListClosures: data })
            }).catch((error) => { console.log(error) })
        }
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
    copiedLink = (path) => {
        const host = window.location.host;
        this.setState({
            tooltipOpenClick: !this.state.tooltipOpenClick,
            tooltipOpen: false
        })
        const { modelClinic } = this.state
        if (navigator && navigator.clipboard && navigator.clipboard.writeText) {
            return navigator.clipboard.writeText(`${host}${path}`);
        }
    }

    render() {
        const { ListClosures, formFilter, listSeller, guid, tooltipOpen, tooltipOpenClick } = this.state
        const { results, currentPage, pageSize, rowCount } = ListClosures;
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
                                                                <Label htmlFor="salesman">Vendedor:</Label>
                                                                <Select
                                                                    name="idSalesman"
                                                                    placeholder="Vendedor..."
                                                                    onChange={e => this.setValues(e, "idSalesman")}
                                                                    options={listSeller}
                                                                    value={{
                                                                        value: formFilter.idSalesman,
                                                                        label: formFilter.nameSalesman
                                                                    }}
                                                                    isClearable={true}
                                                                    noOptionsMessage={() => "Digite o nome do Vendedor!"}
                                                                /></div>

                                                            <div className="col-md-3">
                                                                <Label htmlFor="initialDate">Data inicial check-in:</Label>
                                                                <Input
                                                                    type="date"
                                                                    value={formFilter.checkinDate}
                                                                    onChange={e => this.setValues(e, 'checkinDate')}
                                                                />
                                                            </div>
                                                            <div className="col-md-3">
                                                                <Label htmlFor="finalDate">Data final check-in:</Label>
                                                                <Input
                                                                    type="date"
                                                                    value={formFilter.checkinDateFinal}
                                                                    onChange={e => this.setValues(e, 'checkinDateFinal')}
                                                                />
                                                            </div>
                                                            <div className="col-md-2 mt-4">
                                                                <Button className="btn btn-sm btn-primary"
                                                                    color="primary" onClick={e => this.consult()}>
                                                                    <em className="fa fa-search fa-fw"></em>
                                                                    Buscar
                                                                </Button>
                                                            </div>
                                                        </div>
                                                    </Col>
                                                </Row>
                                            </FormGroup>
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
                            <a className="text-inherit">Fechamentos</a>
                        </CardTitle>
                        <CardSubtitle>
                            <div><a href={`/register/closures/${guid}`} target="_blank">Link do vendedor</a>
                                {' '}
                                <div id='Tooltip-copy' onClick={e => this.copiedLink(`/register/closures/${guid}`)}
                                    className='fa fa-copy'></div>
                                <Tooltip
                                    isOpen={tooltipOpen}
                                    target={'Tooltip-copy'}
                                    toggle={this.toggle}
                                >
                                    Click para copiar o link!
                                </Tooltip>
                                <Tooltip
                                    isOpen={tooltipOpenClick}
                                    target={'Tooltip-copy'}
                                >
                                    Link Copiado!
                                </Tooltip>

                            </div>
                        </CardSubtitle>
                    </CardHeader>
                    <Table size="sm" striped responsive>
                        <thead className="thead-light">
                            <tr>
                                <th>Km rodado</th>
                                <th>Despesas</th>
                                <th>Valor Total Despesas</th>
                                <th>Check-in</th>
                                <th>Check-out</th>
                                {/* <th className=""><strong>Opções</strong></th> */}
                            </tr>
                        </thead>
                        <tbody>
                            {results.map(e => (
                                <tr key={e.id}>
                                    <td>
                                        {e.kilometerTraveled}
                                    </td>
                                    <td>
                                        {e.closuresDetails.map((c, index) => (
                                            <li key={index} >{c.description}</li>
                                        ))}
                                    </td>
                                    <td>
                                        {<NumberFormat
                                            prefix={'R$'}
                                            thousandSeparator=','
                                            decimalSeparator='.'
                                            displayType={'text'}
                                            value={e.valueSumDetails}
                                        />
                                        }
                                    </td>
                                    <td>
                                        {moment(e.dateInit).format('DD-MM-YYYY')}
                                    </td>
                                    <td>
                                        {moment(e.dateFinal).format('DD-MM-YYYY')}
                                    </td>
                                    {/* <td>
                                        <button className="btn btn-sm btn-secondary" title="Editar" size="sm"
                                            onClick={x => this.onEdit(e)}
                                        >
                                            <em className="cui-pencil"></em>
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