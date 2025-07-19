import React, { Component, Suspense } from 'react';
import {
    ModalHeader,
    Button,
    Card,
    CardBody,
    CardFooter,
    Col,
    FormFeedback,
    Input,
    Label,
    Row,
    Table,
    Modal,
    ModalBody,
    Container,
    Nav,
    NavItem,
    NavLink,
    TabPane,
    TabContent
} from 'reactstrap';

import swal from 'sweetalert';
import moment from 'moment';
import axios from 'axios';
import ContentWrapper from '../Theme/ContentWrapper';
import { URL_DescriptionFiles } from '../../services/fileService';

import CurrencyInput from 'react-currency-input';
import ConvertToUSD from './../../ConvertCurrency';
import NumberFormat from 'react-number-format';
import PubSub from 'pubsub-js'
import Pagination from 'react-js-pagination';
import Resizer from "react-image-file-resizer";
import { URL_File } from '../../services/fileService';

class FormUpload extends Component {
    listFiles = [];
    state = {
        modelFile: {
            id: 0, nameProduct: '', descriptionProduct: '',
            valueProduct: '', groupItems: 0
        },
        files: [],
        filesBlob: [],
        errors: {}
    }

    onUpload = e => {

        let files = [];
        let filesBlob = []
        for (let index = 0; index < e.target.files.length; index++) {
            const elementBlob = URL.createObjectURL(e.target.files[index])
            filesBlob.push(elementBlob)
            this.resizeFile(e.target.files[index]).then(resp => {
                files.push(resp)
            })
        }
        this.setState({
            files: files,
            filesBlob: filesBlob
        })
    };

    resizeFile = (file) =>
        new Promise((resolve) => {
            Resizer.imageFileResizer(
                file,
                600,
                360,
                "JPEG",
                70,
                0,
                (uri) => {
                    resolve(uri);
                },
                "file"
            );
        });

    validate = () => {
        let isError = 0;
        const dados = this.state.modelFile
        const errors = {}

        if (dados.nameProduct.length <= 2) {
            isError++;
            errors.nameProductError = true;
        }
        else
            errors.nameProductError = false;

        if (dados.descriptionProduct.length <= 2) {
            isError++;
            errors.descriptionProductError = true;
        }
        else
            errors.descriptionProductError = false;

        if (dados.groupItems == '0' || dados.groupItems == '') {
            isError++;
            errors.groupError = true;
        }
        else
            errors.groupError = false;
        this.setState({
            errors: errors
        });

        return isError;
    }

    clear = () => {
        this.setState({
            modelFile: {
                nameProduct: '', descriptionProduct: '', valueProduct: '', groupItems: 0
            },
            filesBlob: [],
            files: []
        })
    }

    clearFiles = () => {
        this.setState({ filesBlob: [], files: [] })
    }

    setValues = (e, field) => {
        const { modelFile } = this.state;
        modelFile[field] = e.target.value;
        this.setState({ modelFile });
        this.validate()
    }

    NewsFiles = () => {
        swal("Irá limpar todos os campos!", {
            icon: "warning",
            buttons: true,
            dangerMode: true,
        }).then(resp => {
            if (resp) {
                this.clearFiles()
                this.setState({
                    modelFile: {
                        nameProduct: '', descriptionProduct: '', valueProduct: '',
                        groupItems: 0
                    }, filesBlob: [], files: []
                })
            }
        })
    }

    save = async () => {
        if (this.validate() == 0) {
            const { files, modelFile } = this.state;
            const valueProduct = ConvertToUSD(modelFile.valueProduct)

            if (files.length == 0) {

                swal('Nenhuma Imagem Inserida!', {
                    icon: 'warning'
                })
            }
            else {
                const descriptionFiles = {
                    nameProduct: modelFile.nameProduct,
                    descriptionProduct: modelFile.descriptionProduct,
                    valueProduct: parseFloat(valueProduct),
                    groupItems: parseInt(modelFile.groupItems),
                }
                const formData = new FormData();
                formData.append('data', JSON.stringify(descriptionFiles));
                files.forEach(element => {
                    formData.append('dataFiles', element)
                });
                await axios.post(URL_DescriptionFiles, formData).
                    then(resp => {
                        const { data } = resp
                        if (data) {
                            swal("Salvo com sucesso!", {
                                icon: 'success'
                            })
                            this.clear()
                        }
                    })
            }
        }
    }

    render() {
        const { filesBlob, errors } = this.state
        return (
            <div>
                <ContentWrapper>
                    <Card>
                        <CardBody>
                            <Container className="container-md">
                                <div className="row">
                                    <Col xs="6" md="9">
                                        <label htmlFor="inputImage" title="Upload image file" className="btn btn-info btn-sucess">
                                            <input ref="inputImage"
                                                id="inputImage"
                                                name="file"
                                                onChange={e => this.onUpload(e)}
                                                type="file"
                                                multiple
                                                className="sr-only" />

                                            <span title="Import image with Blob URLs" className="docs-tooltip">
                                                <em className="fa fa-upload"></em>{' '}
                                                upload
                                            </span>
                                        </label>
                                    </Col>
                                </div>
                                <div className="row">
                                    <div className="mt-3">
                                        {filesBlob.length > 0 ?
                                            <div>
                                                <CreateImageItem filesBlob={filesBlob} /></div>
                                            :
                                            <div><small>Nenhuma Imagem Importada!.</small></div>
                                        }
                                    </div>
                                </div>
                                {/* <div className="md-col-3">
                                    <Button size="sm" color="danger"
                                        onClick={e => this.clearFiles()}>
                                        <i className="fa fa-eraser"></i> Limpar Imagens</Button>
                                </div> */}

                            </Container>
                        </CardBody>
                        <CardBody>
                            <Row>
                                <Col md={3}>
                                    <Label>Nome do Produto:*</Label>
                                    <Input
                                        name='nameProduct'
                                        type='text'
                                        invalid={errors.nameProductError}
                                        onChange={e => this.setValues(e, 'nameProduct')}
                                        value={this.state.modelFile.nameProduct}
                                    />
                                    <FormFeedback invalid>Campo Obrigatório!</FormFeedback>
                                </Col>
                                <Col md={6}>
                                    <Label>Descrição Produto:*</Label>
                                    <Input
                                        name='descriptionProduct'
                                        type='text'
                                        invalid={errors.descriptionProductError}
                                        onChange={e => this.setValues(e, 'descriptionProduct')}
                                        value={this.state.modelFile.descriptionProduct}
                                    />
                                    <FormFeedback invalid>Campo Obrigatório!</FormFeedback>
                                </Col>
                                <Col md={2}>
                                    <Label>Valor Produto:*</Label>
                                    <CurrencyInput
                                        className="form-control"
                                        type="text"
                                        decimalSeparator=","
                                        thousandSeparator="."
                                        prefix="R$"
                                        onChangeEvent={e => this.setValues(e, 'valueProduct')}
                                        value={this.state.modelFile.valueProduct}
                                    >
                                    </CurrencyInput>
                                </Col>
                            </Row>
                            <Row>
                                <Col md={3}>
                                    <Label>Grupo:*</Label>
                                    <Input
                                        type="select"
                                        onChange={e => this.setValues(e, 'groupItems')}
                                        value={this.state.modelFile.groupItems}
                                        invalid={errors.groupError}
                                    >
                                        <option Value='0'>Selecione...</option>
                                        <option value='1'>Prod. Mary Key</option>
                                        <option value='2'>Semi Joiás</option>
                                        <option value='3'>Bolsas</option>
                                    </Input>
                                    <FormFeedback >Campo Obrigatório!</FormFeedback>
                                </Col>
                            </Row>
                            <p className="float-right text-sm">
                                <i>Os campos marcados com (*) são obrigatórios</i>
                            </p>
                            <br />
                            <div className="float-left">
                                <Button size="sm" color="primary" onClick={e => this.NewsFiles()}>
                                    <i className="fa fa-plus-square-o"></i> Novo Cadastro</Button>{' '}

                                <Button size="sm" color="success" onClick={e => this.save()}>
                                    <i className="fa fa-dot-circle-o"></i> Salvar</Button>
                            </div>
                        </CardBody>
                    </Card>
                </ContentWrapper>
            </div>
        );
    }
}

class CreateImageItem extends Component {
    render() {
        const { filesBlob } = this.props
        return (
            <div>
                <Row>
                    {
                        filesBlob.map(f => (
                            <Col md={3}>
                                <img className="img-fluid mb-2"
                                    src={f}
                                    alt="Item" />
                            </Col>
                        ))
                    }
                </Row>
            </div>);
    }
}

class ListFile extends Component {

    state = {
        formFiles: {
            results: [], currentPage: '', pageCount: '',
            pageSize: '', rowCount: '', firstRowOnPage: '', lastRowOnPage: ''
        },
        modelFile: {},
        formFilter: {
            selectOption: 0,
            textOption: '',
            pageNumber: 1,
            pageSize: 10
        }
        , files: []
    }

    toggleModal = () => {
        this.setState({
            modal: !this.state.modal,
        });
    }

    setValuesFilter = (e, field) => {
        const { formFilter } = this.state;
        formFilter[field] = e.target.value;
        this.setState({ formFilter });
    }

    consult = async () => {
        const { formFilter } = this.state
        await axios.get(URL_DescriptionFiles, {
            params: {
                selectOption: formFilter.selectOption,
                textOption: formFilter.textOption
            }
        }).then(resp => {
            const { data } = resp
            if (data)
                this.setState({ formFiles: data })
        })
    }

    componentDidMount() {
        this.consult()
    }

    delete = (e, idDescriptionFiles) => {

        swal('Deseja Realmente Excluir o Arquivo?', {
            icon: 'warning',
            dangerMode: true,
            buttons: true
        }).then(resp => {
            if (resp) {
                axios.delete(`${URL_DescriptionFiles}/${idDescriptionFiles}`).then(resp => {
                    const { data } = resp
                    if (data) {
                        this.consult();
                    }
                })
            }
        })
    }

    query = async (pageSizeValue, pageNumber) => {
        let pageSize = ''
        if (pageSizeValue.target != undefined)
            pageSize = pageSizeValue.target.value
        else
            pageSize = pageSizeValue;

        const { formFilter } = this.state
        await axios.get(URL_DescriptionFiles, {
            params: {
                selectOption: formFilter.selectOption,
                textOption: formFilter.textOption,
                pageNumber: pageNumber,
                pageSize: pageSize
            }
        }).then(response => {
            const { data } = response

            this.setState({
                formFiles: data
            })
        })
    }

    viewImages = async (f) => {
        await axios.get(`${URL_File}/${f.id}`).then(resp => {
            const { data } = resp

            if (data) {
                this.setState({ files: data })
                this.toggleModal()
            }
        })
    }

    render() {

        const { formFiles, files } = this.state
        const { results, currentPage, pageSize, rowCount } = formFiles;
        return (
            <div>
                <Card>
                    <CardBody>
                        <form name="formFilter">
                            <div className="form-row">
                                <div className="col-md-2">
                                    <select name="selectOption"
                                        onChange={e => this.setValuesFilter(e, 'selectOption')}
                                        defaultValue="0"
                                        className="custom-select"
                                        multiple="">
                                        <option Value="0">Nome</option>
                                    </select>
                                </div>
                                <div className="col-md-5">
                                    <div className="form-group mb-4">
                                        <Input className="form-control mb-2"
                                            type="text"
                                            placeholder="Pesquisar por Nome Produto... "
                                            onChange={e => this.setValuesFilter(e, 'textOption')}
                                        />
                                    </div>
                                </div>
                                <div className="col-md-1">
                                    <Button color="primary"
                                        onClick={e => this.consult()}
                                    >Pesquisar</Button>
                                </div>
                            </div>
                        </form>
                        <div className="mt-3">

                            <hr />
                            <Row>
                                <Col xs={8} md={12}>
                                    <Table responsive="sm">
                                        <thead class="thead-light">
                                            <tr>
                                                <th>Nome Produto</th>
                                                <th>Descrição Produto</th>
                                                <th>Valor Produto</th>
                                                <th >Opções</th>
                                            </tr>
                                        </thead>
                                        <tbody>
                                            {
                                                results.map(f => (
                                                    <tr key={f.id}>
                                                        <td>{f.nameProduct}</td>
                                                        <td>
                                                            {f.descriptionProduct}
                                                        </td>
                                                        <td>
                                                            {<NumberFormat
                                                                displayType={'text'}
                                                                value={f.valueProduct}
                                                                decimalSeparator=','
                                                                thousandSeparator='.'
                                                                prefix='R$'
                                                            />}
                                                        </td>
                                                        <td>
                                                            <Button title="Visualizar imagens" size="sm"
                                                                onClick={e => this.viewImages(f)}
                                                            >
                                                                <i className="fa fa-eye"></i>
                                                            </Button>{' '}
                                                            <Button color="danger" title="Excluir" size="sm"
                                                                onClick={e => this.delete(e, f.id)}
                                                            >
                                                                <i className="fa fa-trash-o"></i>
                                                            </Button>
                                                        </td>
                                                    </tr>
                                                ))}
                                        </tbody>
                                    </Table>
                                    <CardFooter>
                                        <div className="d-flex align-items-left">
                                            <div>
                                                <select
                                                    className="custom-select"
                                                    name="selectOptionAmount"
                                                    onChange={(pageSize) => this.query(pageSize, currentPage)}
                                                    value={pageSize}
                                                >
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
                                                    onChange={(pageNumber) => this.query(pageSize, pageNumber)}
                                                    itemClass="page-item"
                                                    linkClass="page-link"
                                                    firstPageText="Primeira"
                                                    lastPageText="Última"
                                                />
                                            </div>
                                        </div>
                                    </CardFooter>
                                </Col>
                            </Row>
                            <Modal
                                centered
                                size="lg"
                                isOpen={this.state.modal}
                                toggle={this.toggleModal}>
                                <ModalHeader
                                    toggle={this.toggleModal}>
                                    Imagens Salvas
                                </ModalHeader>
                                <ModalBody>
                                    <div>
                                        <Row>
                                            {files.map(f => (
                                                <Col  >
                                                    <div>
                                                        <img src={"data:image/png;base64," + f.files}
                                                            style={{ width: "60%" }}
                                                        />
                                                    </div>
                                                </Col>
                                            ))}
                                        </Row>
                                    </div>
                                </ModalBody>
                            </Modal>
                        </div>
                    </CardBody>
                </Card>
            </div>
        );
    }
}

export default class File extends Component {
    state = {
        activeTab: 'con',
    }
    toggleTab = tab => {
        if (this.state.activeTab !== tab) {
            this.setState({
                activeTab: tab
            });
        }
    }

    render() {
        return (
            <div>
                <Col xs="12" md="12" className="mb-4">
                    <Nav tabs>
                        <NavItem>
                            <NavLink
                                className={this.state.activeTab === 'con' ? 'active' : ''}
                                onClick={() => { this.toggleTab('con'); }}
                            >
                                Lista Imagens
                            </NavLink>
                        </NavItem>
                        <NavItem>
                            <NavLink
                                className={this.state.activeTab === 'edit' ? 'active' : ''}
                                onClick={() => { this.toggleTab('edit'); }}
                            >
                                Cadastro de Imagens
                            </NavLink>
                        </NavItem>

                    </Nav>
                    <TabContent activeTab={this.state.activeTab} onSelect>
                        <TabPane tabId="con" role="tabpanel">
                            <ListFile />
                        </TabPane>
                    </TabContent>
                    <TabContent activeTab={this.state.activeTab} onSelect>
                        <TabPane tabId="edit" role="tabpanel">
                            <FormUpload toggleTab={this.toggleTab} />
                        </TabPane>
                    </TabContent>
                </Col>
            </div>
        )
    }
}