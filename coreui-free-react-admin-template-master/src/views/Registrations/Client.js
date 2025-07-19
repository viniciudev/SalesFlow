import React, { Component } from 'react';
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
} from 'reactstrap';
import InputMask from 'react-input-mask';
import json_city from './json_cities.json';
import swal from 'sweetalert';
import axios from 'axios';
import CharacterRemover from 'character-remover';
import PubSub from 'pubsub-js';
import { URL_Client } from './../../services/clientService'
import NumberFormat from 'react-number-format';
import { URL_SearchZipCode } from '../../services/searchZipCodeService';
import Pagination, { bootstrap5PaginationPreset } from 'react-responsive-pagination'
import { removeAll } from 'character-remover';
import { FaSpinner } from 'react-icons/fa'
class FormProvider extends Component {
    state = {
        errors: {},
        listCities: [],
        modelClient: {
            id: 0,
            name: '',
            document: '',
            zipCode: '',
            address: '',
            bairro: '',
            cellPhone: '',
            nameCity: '',
            nameState: '',
        },
        visible: true,
        loading: false
    }

    validate = () => {
        let isError = 0;
        const dados = this.state.modelClient
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
            errors
        });

        return isError;
    }

    componentWillMount() {
        PubSub.subscribe('edit-client', (topic, client) => {

            this.setState({
                modelClient: client,
                listCities: [`${client.nameCity}`]

            })
        })
    }
    setValues = (e, field) => {
        const { modelClient } = this.state;
        modelClient[field] = e.target.value;
        if (field == 'nameState')
            this.buscaCidades(e)
        this.setState({ modelClient });
    }

    save = async () => {
        const { modelClient } = this.state
        if (this.validate() == 0) {
            this.setState({ loading: true })
            let data = {
                idCompany: modelClient.idCompany,
                id: parseInt(modelClient.id),
                name: modelClient.name,
                document: modelClient.document,
                zipCode: CharacterRemover.removeAll(modelClient.zipCode),
                address: modelClient.address,
                bairro: modelClient.bairro,
                nameState: modelClient.nameState,
                nameCity: modelClient.nameCity,
                cellPhone: CharacterRemover.removeAll(modelClient.cellPhone)
            }

            if (data.id > 0) {
                await axios.put(URL_Client, data).then(resp => {
                    const { data } = resp
                    if (data) {
                        swal("Atualizado com sucesso!", { icon: "success" }).then(r => {
                            if (r) {
                                this.clearModelClient();
                                this.props.consultAll();
                            }
                        })

                    }
                }).catch(() => { this.setState({ loading: false }) })
            } else {
                await axios.post(URL_Client, data).then(resp => {
                    const { data } = resp
                    if (data) {
                        swal("Salvo com sucesso!", { icon: "success" }).then(r => {
                            if (r) {
                                this.clearModelClient();
                                this.props.consultAll();
                            }
                        })
                    }
                }).catch(() => { this.setState({ loading: false }) })
            }
            this.setState({ loading: false })
        }
    }
    clearModelClient() {
        this.setState({
            modelClient: {
                id: 0,
                name: '',
                document: '',
                zipCode: '',
                address: '',
                bairro: '',
                cellPhone: '',
                nameCity: '',
                nameState: '',
            },
            listCities: []
        })
    }

    buscaCidades = (e) => {
        const data = json_city.states;
        const val = e.target.value
        if (val != '') {
            var filterObj = data.find(function (item, i) {

                if (item.sigla === val) {
                    const city = item.cidades
                    return city
                }
            })
            this.state.listCities = filterObj.cidades
            this.setState({

            })
        }

    }
    searchCep = async (e) => {
        const { modelClient } = this.state
        let zipcode = CharacterRemover.removeAll(e.target.value)
        if (zipcode.length >= 8) {
            await axios.get(`${URL_SearchZipCode}/${zipcode}`).then(resp => {
                const { data } = resp
                modelClient.address = data.logradouro;
                modelClient.nameCity = data.localidade;
                modelClient.bairro = data.bairro;
                modelClient.nameState = data.uf;
                this.setState({ modelClient, listCities: [`${data.localidade}`] })
            })
        }
    }

    render() {
        const { modelClient, errors, loading } = this.state;
        return (
            <div>
                <Card>
                    <CardHeader>
                        <strong>Cadastro</strong>
                        <small> Cliente</small>
                    </CardHeader>
                    <CardBody>
                        <Form>
                            <FormGroup>
                                <div className='form-row'>
                                    <div className="col-md-2">
                                        <Label htmlFor="name">Id:</Label>
                                        <Input
                                            type="text"
                                            id="id"

                                            disabled={true}
                                            value={modelClient.id}
                                        />
                                    </div>
                                    <div className="col-md-6">
                                        <Label htmlFor="name">Nome:*</Label>
                                        <Input
                                            id="name"
                                            className="form-control-warning"
                                            type="text"
                                            invalid={errors.nameError}
                                            value={modelClient.name}
                                            onChange={e => this.setValues(e, 'name')}
                                        />
                                        <FormFeedback></FormFeedback>
                                    </div>
                                    <div className="col-md-4">
                                        <Label htmlFor="document">Documento:</Label>
                                        <Input
                                            type="text"
                                            className="form-control-warning"
                                            value={modelClient.document}
                                            onChange={e => this.setValues(e, 'document')}
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
                                            mask='99.999-999'
                                            tag={InputMask}
                                            value={modelClient.zipCode}

                                            onChange={e => this.setValues(e, 'zipCode')}
                                            onChangeCapture={e => this.searchCep(e)}
                                        />
                                    </div>
                                    <div className="col-md-6">
                                        <Label htmlFor="address">Endereço:</Label>
                                        <Input
                                            className="form-control-warning"
                                            type="text"
                                            value={modelClient.address}
                                            onChange={e => this.setValues(e, 'address')}
                                        />
                                    </div>
                                    <div className="col-md-3">
                                        <Label htmlFor="bairro">Bairro:</Label>
                                        <Input
                                            className="form-control-warning"
                                            type="text"
                                            value={modelClient.bairro}
                                            onChange={e => this.setValues(e, 'bairro')}
                                        />
                                    </div>
                                </div>
                            </FormGroup>
                            <FormGroup>
                                <FormGroup row className="my-0">
                                    <div className="col-md-4">
                                        <Label htmlFor="state">Estado:</Label>
                                        <Input
                                            type="select"
                                            value={modelClient.nameState}
                                            onChange={e => this.setValues(e, 'nameState')}
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
                                    <div className="col-md-4">
                                        <FormGroup>
                                            <Label htmlFor="city">Cidade:</Label>
                                            <Input id="nameCity"
                                                type="select"
                                                invalid={errors.nameCityError}
                                                value={modelClient.nameCity}
                                                onChange={e => this.setValues(e, 'nameCity')}
                                            >

                                                {this.state.listCities.map(city => (
                                                    <option key={city}>{city}</option>))}
                                            </Input>
                                            <FormFeedback></FormFeedback>
                                        </FormGroup>
                                    </div>
                                    <div className="col-md-4">
                                        <Label htmlFor="cellphone">Telefone Celular:*</Label>
                                        <Input id="cellphone"
                                            name="cellphone"
                                            type="text"
                                            invalid={errors.cellPhoneError}
                                            mask='(99) 9 9999-9999'
                                            tag={InputMask}
                                            value={modelClient.cellPhone}
                                            onChange={e => this.setValues(e, 'cellPhone')}
                                        >
                                        </Input>
                                        <FormFeedback></FormFeedback>
                                    </div>
                                </FormGroup>
                            </FormGroup>
                            <Button
                                size="sm"
                                onClick={e => this.save()}
                                color="success"
                                disabled={loading}
                            >
                                {loading && <FaSpinner className='fa fa-spinner fa-spin' />}
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
            </div>
        )
    }
}


class ListFormProvider extends Component {
    state = {
        modelClient: {},
        formFilter: {}
    }
    query = (pageSizeValue, pageNumber) => {
        let pageSize = ''
        if (pageSizeValue != undefined)
            pageSize = pageSizeValue;

        this.props.consultByPagination(pageSize, pageNumber);
    }

    onEdit = (client) => {
        PubSub.publish('edit-client', client)
    }

    render() {
        const { formClient } = this.props

        const { results, currentPage, pageSize, rowCount } = formClient;
        return (
            <Card>
                <CardHeader>
                    <strong>Consultar</strong>
                    <small> Clientes Cadastrados</small>
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
                            {
                                results.map(c => (
                                    <tr key={c.id}>
                                        <td>{c.name}</td>
                                        <td>{
                                            <NumberFormat
                                                displayType={'text'}
                                                value={c.cellPhone}
                                                format="(##) # #### ####"
                                            />
                                        }</td>
                                        <td>
                                            <Button
                                                onClick={e => this.onEdit(c)}
                                                color="secondary"
                                                outline>
                                                <i className="cui-pencil text-dark"></i>
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
                                onChange={(pageSize) => this.query(pageSize.target.value, currentPage)}
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
                </CardBody>
            </Card>
        )
    }
}


export default class Provider extends Component {

    state = {
        visible: false,
        formClient: { results: [], currentPage: 0, pageCount: 0, pageSize: 0, rowCount: 0 },
    }

    componentDidMount() {
        this.consultAll();
    }

    consultByPagination = async (pageSize, pageNumber) => {
        await axios.get(URL_Client, {
            params: {
                pageSize: pageSize, pageNumber: pageNumber
            }
        }).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formClient: data
                })
            }
        })
    }
    consultAll = async () => {
        await axios.get(URL_Client).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formClient: data
                })
            }
        })
    }
    render() {
        const { formClient } = this.state
        return (
            <div>

                <div className="row">

                    <div className="col-md-5 my-3">
                        <ListFormProvider
                            formClient={formClient}
                            consultByPagination={this.consultByPagination}
                        />

                    </div>

                    <div className="col-md-7 my-3" >
                        <FormProvider
                            consultAll={this.consultAll}
                        />
                    </div>
                </div>
            </div>
        )

    }
}