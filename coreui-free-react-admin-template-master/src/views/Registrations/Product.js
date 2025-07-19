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
    Table
} from 'reactstrap';
import CurrencyInput from 'react-currency-input';
import ConvertToUSD from './../../ConvertCurrency';
import Axios from 'axios';
import { URL_Product } from './../../services/productService';
import swal from 'sweetalert';
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js';
import { FaSpinner } from 'react-icons/fa'
class FormProduct extends Component {

    state = {
        errors: {},
        modelProduct: { id: 0, name: '', value: '' },
        loading: false
    }
    setValues = (e, field) => {
        const { modelProduct } = this.state;
        modelProduct[field] = e.target.value;
        this.setState({ modelProduct });
    }

    validate = () => {
        let isError = 0;
        const dados = this.state.modelProduct
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
        this.setState({ modelProduct: { id: 0, name: '', value: '' } })
    }

    save = async () => {
        if (this.validate() == 0) {
            this.setState({ loading: true })
            const { modelProduct } = this.state
            const valueService = ConvertToUSD(modelProduct.value)
            let data = {
                idCompany: modelProduct.idCompany,
                id: modelProduct.id,
                name: modelProduct.name,
                value: parseFloat(valueService)
            }

            if (data.id > 0) {
                await Axios.put(URL_Product, data).then(resp => {
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
                await Axios.post(URL_Product, data).then(resp => {
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
        PubSub.subscribe('edit-product', (topic, service) => {

            this.setState({
                modelProduct: service,
            })
        })
    }

    render() {
        const { modelProduct, errors, loading } = this.state
        return (
            <Card>
                <CardHeader>
                    <strong>Cadastro</strong>
                    <small> Produtos</small>
                </CardHeader>
                <CardBody>
                    <Form>
                        <FormGroup>
                            <div className='form-row'>
                                <div className="col-md-2">
                                    <Label htmlFor="name">Id:</Label>
                                    <Input
                                        type="text"
                                        value={modelProduct.id}
                                        disabled
                                    />
                                </div>
                                <div className="col-md-8">
                                    <Label htmlFor="name">Nome Produto:*</Label>
                                    <Input
                                        type="text"
                                        onChange={e => this.setValues(e, 'name')}
                                        value={modelProduct.name}
                                        invalid={errors.nameError}
                                    />
                                    <FormFeedback></FormFeedback>
                                </div>
                                <div className="col-md-2">
                                    <Label htmlFor="serviceValue">Valor Produto:</Label>
                                    <CurrencyInput
                                        className="form-control"
                                        type="text"
                                        decimalSeparator=","
                                        thousandSeparator="."
                                        prefix="R$"
                                        onChangeEvent={e => this.setValues(e, 'value')}
                                        value={modelProduct.value}
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


class ListFormProduct extends Component {
    onEdit = (product) => {
        PubSub.publish('edit-product', product)
    }

    render() {
        const { formProduct } = this.props
        return (
            <Card>
                <CardHeader>
                    <strong>Consultar</strong>
                    <small> Produtos Cadastrados</small>
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
                                formProduct.results.map(s => (
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


export default class Product extends Component {

    state = {
        formProduct: { results: [], currentPage: '', pageCount: '', pageSize: '' }
    }

    componentDidMount() {
        this.consultAll()
    }

    consultAll = async () => {
        await Axios.get(URL_Product).then(resp => {
            const { data } = resp
            if (data) {
                this.setState({
                    formProduct: data
                })
            }
        })
    }

    render() {
        return (
            <div>
                <div className="row">

                    <div className="col-md-4 my-3">
                        <ListFormProduct
                            formProduct={this.state.formProduct}
                        />

                    </div>

                    <div className="col-md-8 my-3" >
                        <FormProduct
                            consultAll={this.consultAll} />
                    </div>
                </div>
            </div>
        )

    }
}