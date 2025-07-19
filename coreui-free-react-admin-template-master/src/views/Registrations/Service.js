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
    Table
} from 'reactstrap';
import CurrencyInput from 'react-currency-input';
import ConvertToUSD from './../../ConvertCurrency';
import Axios from 'axios';
import { URL_Service } from './../../services/serviceProvidedService';
import swal from 'sweetalert';
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js';
import { FaSpinner } from 'react-icons/fa'
class FormService extends Component {

    state = {
        errors: {},
        modelService: { id: 0, name: '', value: '' },
        loading: false
    }
    setValues = (e, field) => {
        const { modelService } = this.state;
        modelService[field] = e.target.value;
        this.setState({ modelService });
    }

    validate = () => {
        let isError = 0;
        const dados = this.state.modelService
        const errors = {}

        if (!dados.name) {
            isError++;
            errors.nameError = true;
        }
        else
            errors.nameError = false;

        this.setState({
            errors
        });

        return isError;
    }
    clear() {
        this.setState({ modelService: { id: 0, name: '', value: '' } })
    }

    save = async () => {
        if (this.validate() == 0) {
            this.setState({ loading: true })
            const { modelService } = this.state
            const valueService = ConvertToUSD(modelService.value)
            let data = {
                idCompany: modelService.idCompany,
                id: modelService.id,
                name: modelService.name,
                value: parseFloat(valueService)
            }
            if (data.id > 0) {
                await Axios.put(URL_Service, data).then(resp => {
                    const { data } = resp
                    if (data) {
                        swal('Atualizado com Sucesso!', {
                            icon: 'success'
                        }).then(r => {
                            if (r)
                                this.clear();
                            this.props.consultAll();
                        })
                    }
                }).catch(() => { this.setState({ loading: false }) })
            } else {
                await Axios.post(URL_Service, data).then(resp => {
                    const { data } = resp
                    if (data) {
                        swal('Salvo com Sucesso!', {
                            icon: 'success'
                        }).then(r => {
                            if (r)
                                this.clear();
                            this.props.consultAll();
                        })
                    }
                }).catch(() => { this.setState({ loading: false }) })
            }
            this.setState({ loading: false })
        }
    }
    componentWillMount() {
        PubSub.subscribe('edit-service', (topic, service) => {

            this.setState({
                modelService: service,
            })
        })
    }

    render() {
        const { modelService, errors, loading } = this.state
        return (
            <Card>
                <CardHeader>
                    <strong>Cadastro</strong>
                    <small> Serviços</small>
                </CardHeader>
                <CardBody>
                    <Form>
                        <FormGroup>
                            <div className='form-row'>
                                <div className="col-md-2">
                                    <Label htmlFor="name">Id:</Label>
                                    <Input
                                        type="text"
                                        value={modelService.id}
                                        disabled
                                    />
                                </div>
                                <div className="col-md-8">
                                    <Label htmlFor="name">Nome Serviço:*</Label>
                                    <Input
                                        type="text"
                                        onChange={e => this.setValues(e, 'name')}
                                        value={modelService.name}
                                        invalid={errors.nameError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="col-md-2">
                                    <Label htmlFor="serviceValue">Valor Serviço:</Label>
                                    <CurrencyInput
                                        className="form-control"
                                        type="text"
                                        decimalSeparator=","
                                        thousandSeparator="."
                                        prefix="R$"
                                        onChangeEvent={e => this.setValues(e, 'value')}
                                        value={modelService.value}
                                    >
                                    </CurrencyInput>
                                </div>
                            </div>
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
        )
    }
}


class ListFormService extends Component {
    onEdit = (service) => {
        PubSub.publish('edit-service', service)
    }

    render() {
        const { formService } = this.props
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
                                <th>Opções</th>
                            </tr>
                        </thead>
                        <tbody>
                            {
                                formService.results.map(s => (
                                    <tr key={s.id}>
                                        <td>{s.name}</td>
                                        <td> <NumberFormat
                                            displayType={'text'}
                                            value={s.value}
                                            thousandSeparator={'.'}
                                            decimalSeparator={','}
                                            prefix={'R$'}
                                        /></td>
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
                </CardBody>
            </Card>
        )
    }
}


export default class Service extends Component {

    state = {
        formService: { results: [], currentPage: '', pageCount: '', pageSize: '' }
    }

    componentDidMount() {
        this.consultAll()
    }

    consultAll = async () => {
        await Axios.get(URL_Service).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formService: data
                })
            }
        })
    }

    render() {
        return (
            <div>
                <div className="row">

                    <div className="col-md-4 my-3">
                        <ListFormService
                            formService={this.state.formService}
                        />

                    </div>

                    <div className="col-md-8 my-3" >
                        <FormService
                            consultAll={this.consultAll} />
                    </div>
                </div>
            </div>
        )

    }
}