import React, { Component, Suspense } from 'react';
import {
    Button,
    Card,
    CardBody,
    CardHeader,
    Col,
    Form,
    FormGroup,
    FormFeedback,
    Input,
    Label,
    Row,
    Table
} from 'reactstrap';
import InputMask from 'react-input-mask';
import { FaSpinner } from 'react-icons/fa'
import { removeAll } from 'character-remover';
import Axios from 'axios';
import { URL_Prospects, URL_PhasesProspects } from '../../services/prospectsService';
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js';
import Pagination, { bootstrap5PaginationPreset } from 'react-responsive-pagination'

class FormProspect extends Component {

    state = {
        model: { id: 0, name: '', cellPhone: '' },
        errors: {},
        loading: false,
        listPhasesProspect: []
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
        const cellPhone = removeAll(dados.cellPhone)

        if (!cellPhone || cellPhone.length < 11) {
            isError++;
            errors.cellPhoneError = true;
        }
        else
            errors.cellPhoneError = false;

        this.setState({
            errors: errors
        });

        return isError;
    }

    save = async () => {
        const { model } = this.state
        if (this.validate() == 0) {
            if (model.id > 0) {
                this.setState({ loading: true })
                await Axios.put(URL_Prospects, model).then(resp => {
                    const { data } = resp
                    this.setState({ model: data })
                }).catch(() => { this.setState({ loading: false }) })
                this.setState({ loading: false })
                this.props.consultProspects()
            }
            else {
                this.setState({ loading: true })
                await Axios.post(URL_Prospects, model).then(resp => {
                    const { data } = resp
                    this.setState({ model: data })
                }).catch(() => { this.setState({ loading: false }) })
                this.setState({ loading: false })
                this.props.consultProspects()
            }

        }
    }

    setValues = (e, field) => {
        const { model } = this.state
        model[field] = e.target.value
        this.setState({ model })
    }
    componentWillMount() {
        PubSub.subscribe('edit-client', (topic, client) => {

            this.setState({
                model: client,
            })
            this.consultPhases()
        })
    }

    clear = () => {
        this.setState({ model: { id: 0, name: '', cellPhone: '' }, listPhasesProspect: [] })
    }

    consultPhases = async () => {
        const { model } = this.state
        if (model.id > 0) {
            await Axios.get(URL_PhasesProspects, { params: { idProspect: model.id } }).then(resp => {
                const { data } = resp
                this.setState({ listPhasesProspect: data })
            })
        }
    }

    render() {
        const { errors, loading, model, listPhasesProspect } = this.state
        return (<div>
            <Card>
                <CardHeader>
                    <strong>Cadastro</strong>
                    <small> Prospect</small>
                </CardHeader>
                <CardBody>
                    <Form>
                        <FormGroup>
                            <div className='form-row'>
                                <div className="col-md-2">
                                    <Label htmlFor="name">Id:</Label>
                                    <Input
                                        type="text"
                                        disabled
                                        value={model.id}
                                        className="form-control-warning"
                                    />
                                </div>
                                <div className="col-md-6">
                                    <Label htmlFor="name">Nome:*</Label>
                                    <Input
                                        type="text"
                                        value={model.name}
                                        className="form-control-warning"
                                        invalid={errors.nameError}
                                        onChange={e => this.setValues(e, 'name')}
                                    />
                                    <FormFeedback></FormFeedback>

                                </div>
                                <div className="col-md-4">
                                    <Label htmlFor="cellphone">Telefone:*</Label>
                                    <Input id="cellphone"
                                        name="cellphone"
                                        type="text"
                                        value={model.cellPhone}
                                        invalid={errors.cellPhoneError}
                                        mask='(99) 99999-9999'
                                        tag={InputMask}

                                        onChange={e => this.setValues(e, 'cellPhone')}
                                    >
                                    </Input>
                                    <FormFeedback />
                                </div>
                            </div>
                        </FormGroup>

                        <Button
                            onClick={e => this.save()}
                            color="success"
                            disabled={loading}
                        >
                            {loading && <FaSpinner className='fa fa-spinner fa-spin' />}
                            {loading && " Salvando..."}
                            {!loading && <i className="fa fa-dot-circle-o"></i>}
                            {!loading && " Salvar"}
                        </Button>
                        {" "}
                        <Button
                            onClick={e => this.clear()}
                            color="secondary"
                        >

                            <i className="fa fa-times-circle-o"></i>
                            {" Limpar"}
                        </Button>
                        <p className="float-right text-sm">
                            <i>Os campos marcados com (*) são obrigatórios</i>
                        </p>
                    </Form>
                </CardBody>
            </Card>
            {
                model.id > 0 ?
                    <Phases
                        consultPhases={this.consultPhases}
                        listPhasesProspect={listPhasesProspect}
                        idProspects={model.id}
                    /> : null
            }
        </div>
        )
    }
}


class ListFormProspect extends Component {

    state = { name: '' }
    onEdit = (client) => {
        PubSub.publish('edit-client', client)
    }

    query = (pageSizeValue, pageNumber, name) => {
        let pageSize = ''
        if (pageSizeValue != undefined)
            pageSize = pageSizeValue;

        this.props.consultByPagination(pageSize, pageNumber, name);
    }

    render() {
        const { name } = this.state
        const { listProspect } = this.props
        const { results, currentPage, pageSize, rowCount } = listProspect
        return (
            <Card>
                <CardHeader>
                    <strong>Consultar</strong>
                    <small> Prospects Cadastrados</small>
                </CardHeader>
                <CardBody>
                    <Form>
                        <Row className=" align-items-center">
                            <Col>
                                <Input
                                    name="name"
                                    placeholder="busca por nome."
                                    type="text"
                                    onChange={e => this.setState({ name: e.target.value })}
                                    value={name}
                                />
                            </Col>
                            <Col>
                                <Button
                                    onClick={e => this.query(pageSize, currentPage, name)}
                                    color="secondary"
                                >
                                    <i className="fa fa-search"></i>
                                    {" Buscar"}
                                </Button>
                            </Col>
                        </Row>
                    </Form>
                    <br />
                    <Table responsive size="sm">
                        <thead>
                            <tr>
                                <th>Nome</th>
                                <th>Telefone</th>
                                <th>Opções</th>
                            </tr>
                        </thead>
                        <tbody>
                            {results.map(e => (
                                <tr key={e.id}>
                                    <td>{e.name}</td>
                                    <td><NumberFormat
                                        displayType={'text'}
                                        value={e.cellPhone}
                                        format="(##) # #### ####"
                                    /></td>
                                    <td>
                                        <Button
                                            title="Virou Cliente"
                                            onClick={x => this.onEdit(e)}
                                            color="warning" outline>
                                            <i className="fa fa-star"></i>
                                        </Button>
                                        <Button
                                            title="Editar"
                                            onClick={x => this.onEdit(e)}
                                            color="secondary" outline>

                                            <i className="cui-pencil"></i>
                                        </Button>
                                    </td>
                                </tr>))}
                        </tbody>
                    </Table>
                    <div className="d-flex align-items-left">
                        <div>
                            <select className="custom-select"
                                name="selectOptionAmount"
                                onChange={(pageSize) => this.query(pageSize.target.value, currentPage, name)}
                                value={pageSize}
                                multiple="">
                                <option value={10}>10</option>
                                <option value={25}>25</option>
                                <option value={50}>50</option>
                                <option value={100}>100</option>
                            </select>
                        </div>
                        <div className="ml-auto">
                            <Pagination
                                maxWidth={2}
                                {...bootstrap5PaginationPreset}
                                current={currentPage}
                                total={Math.round(rowCount / 10, 1) + 1}
                                onPageChange={(pageNumber) => this.query(pageSize, pageNumber, name)}
                                narrowStrategy={['dropEllipsis', 'dropNav']}
                                renderNav={false}
                            />
                        </div>
                    </div>
                </CardBody>
            </Card>
        )
    }
}

class Phases extends Component {
    state = {
        loading: false, model: {
            id: 0, info: ''
        }
    }

    setValues(e, field) {
        const { model } = this.state
        model[field] = e.target.value
        this.setState({ model })
    }

    save = async () => {
        const { model } = this.state
        if (model.info) {
            this.setState({ loading: true })
            const map = {
                idProspects: this.props.idProspects,
                info: model.info
            }
            await Axios.post(URL_PhasesProspects, map).then(resp => {
                const { data } = resp
                if (data) {
                    this.props.consultPhases()
                }
            })
            this.setState({
                loading: false,
                model: {
                    id: 0, info: ''
                }
            })
        }
    }

    delete = async (id) => {
        await Axios.delete(`${URL_PhasesProspects}/${id}`).then(resp => {
            const { data } = resp
            if (data) {
                this.props.consultPhases()
            }
        })
    }

    render() {
        const { loading, model } = this.state
        const { listPhasesProspect } = this.props

        return (<Card>
            <CardHeader>
                <strong>Etapas</strong>
                {/* <small> Prospect</small> */}
            </CardHeader>
            <CardBody>
                <div className='row'>
                    <div className='col-md-8'>
                        <Input
                            type="text"
                            onChange={e => this.setValues(e, 'info')}
                            value={model.info}
                        />
                    </div>
                    <div className='col-md-3'>
                        <Button
                            onClick={e => this.save()}
                            color="success"
                            disabled={loading}
                        >
                            {loading && <FaSpinner className='fa fa-spinner fa-spin' />}
                            {loading && " Adicionando..."}
                            {!loading && <i className="fa fa-plus"></i>}
                            {!loading && " Adicionar"}
                        </Button>
                    </div>
                </div>
                <div className='row mt-2'>
                    <div className='col-md-12'>
                        <Table borderless size="sm">
                            <thead>
                                <tr>
                                    <th>Informação</th>
                                    <th>Opções</th>
                                </tr>
                            </thead>
                            <tbody>
                                {listPhasesProspect.map(e => (
                                    <tr>
                                        <td>{e.info}</td>
                                        <td>
                                            <Button
                                                size="sm"
                                                title="Excluir"
                                                onClick={x => this.delete(e.id)}
                                                color="danger" outline>
                                                <i className="cui-trash"></i>
                                            </Button>
                                        </td>
                                    </tr>))}
                            </tbody>
                        </Table>
                    </div>
                </div>
            </CardBody>
        </Card>)
    }
}
export default class Prospect extends Component {

    state = {
        listProspect: { results: [], currentPage: 0, pageCount: 0, pageSize: 0, rowCount: 0 },
    }
    componentDidMount() {
        this.consultProspects()
    }

    consultByPagination = async (pageSize, pageNumber, name) => {
        await Axios.get(URL_Prospects, {
            params: {
                pageNumber: pageNumber,
                pageSize: pageSize,
                textOption: name
            }
        }).then(resp => {
            const { data } = resp
            this.setState({ listProspect: data })
        })
    }

    consultProspects = async () => {
        await Axios.get(URL_Prospects, { params: {} }).then(resp => {
            const { data } = resp
            this.setState({ listProspect: data })
        })
    }

    render() {
        const { listProspect } = this.state
        return (
            <div className="row">
                <div className="col-md-4">
                    <ListFormProspect
                        consultByPagination={this.consultByPagination}
                        listProspect={listProspect} />
                </div>
                <div className="col-md-8" >
                    <div className='row-md-10'>
                        <FormProspect consultProspects={this.consultProspects} />
                    </div>

                </div>
            </div>
        )
    }
}