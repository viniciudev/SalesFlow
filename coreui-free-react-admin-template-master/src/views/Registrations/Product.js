import React, { Component } from "react";
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
} from "reactstrap";
import CurrencyInput from "react-currency-input";
import ConvertToUSD from "./../../ConvertCurrency";
import Axios from "axios";
import { URL_Product } from "./../../services/productService";
import swal from "sweetalert";
import NumberFormat from "react-number-format";
import PubSub from "pubsub-js";
import { FaSpinner } from "react-icons/fa";

class FormProduct extends Component {
  state = {
    errors: {},
    modelProduct: {
      id: 0,
      name: "",
      value: "",
      description: "",
      quantity: 0,
      image: null,
    },
    loading: false,
    previewImage: null,
  };

  setValues = (e, field) => {
    const { modelProduct } = this.state;
    modelProduct[field] = e.target.value;
    this.setState({ modelProduct });
  };

  handleImageChange = (e) => {
    const file = e.target.files[0];
    if (file) {
      const reader = new FileReader();
      reader.onloadend = () => {
        this.setState({
          modelProduct: {
            ...this.state.modelProduct,
            image: file,
          },
          previewImage: reader.result,
        });
      };
      reader.readAsDataURL(file);
    }
  };

  validate = () => {
    let isError = 0;
    const dados = this.state.modelProduct;
    const errors = {};

    if (!dados.name) {
      isError++;
      errors.nameError = true;
    } else errors.nameError = false;

    if (!dados.description) {
      isError++;
      errors.descriptionError = true;
    } else errors.descriptionError = false;

    if (dados.quantity < 0) {
      isError++;
      errors.quantityError = true;
    } else errors.quantityError = false;

    this.setState({
      errors,
    });

    return isError;
  };

  clear() {
    this.setState({
      modelProduct: {
        id: 0,
        name: "",
        value: "",
        description: "",
        quantity: 0,
        image: null,
      },
      previewImage: null,
    });
  }

  save = async () => {
    if (this.validate() == 0) {
      this.setState({ loading: true });
      const { modelProduct } = this.state;
      const valueService = ConvertToUSD(modelProduct.value);

      const formData = new FormData();
      formData.append("idCompany", modelProduct.idCompany);
      formData.append("id", modelProduct.id);
      formData.append("name", modelProduct.name);
      formData.append("value", parseFloat(valueService));
      formData.append("description", modelProduct.description);
      formData.append("quantity", modelProduct.quantity);

      // Se é uma nova imagem ou edição de imagem
      if (modelProduct.image && typeof modelProduct.image !== "string") {
        formData.append("image", modelProduct.image);
      }

      const config = {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      };

      // const url =
      //   modelProduct.id > 0 ? `${URL_Product}/${modelProduct.id}` : URL_Product;

      // const method = modelProduct.id > 0 ? "put" : "post";

      await Axios.post(URL_Product, formData, config)
        .then((resp) => {
          swal("Salvo com Sucesso!", {
            icon: "success",
          }).then((r) => {
            if (r) this.clear();
            this.props.consultAll();
          });
        })
        .catch(() => {
          this.setState({ loading: false });
        });

      this.setState({ loading: false });
    }
  };

  componentWillMount() {
    PubSub.subscribe("edit-product", (topic, service) => {
      this.setState({
        modelProduct: {
          ...service,
          description: service.description || "",
          quantity: service.quantity || 0,
        },
        previewImage:
          service.imageUrl || service.image
            ? `data:image/jpeg;base64,${service.image}`
            : null,
      });
    });
  }

  render() {
    const { modelProduct, errors, loading, previewImage } = this.state;
    return (
      <Card>
        <CardHeader>
          <strong>Cadastro</strong>
          <small> Produtos</small>
        </CardHeader>
        <CardBody>
          <Form>
            <FormGroup>
              <div className="form-row">
                <div className="col-md-2">
                  <Label htmlFor="name">Id:</Label>
                  <Input type="text" value={modelProduct.id} disabled />
                </div>
                <div className="col-md-8">
                  <Label htmlFor="name">Nome Produto:*</Label>
                  <Input
                    type="text"
                    onChange={(e) => this.setValues(e, "name")}
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
                    onChangeEvent={(e) => this.setValues(e, "value")}
                    value={modelProduct.value}
                  ></CurrencyInput>
                </div>
              </div>
            </FormGroup>

            {/* Novo campo para descrição */}
            <FormGroup>
              <Label htmlFor="description">Descrição do Produto:*</Label>
              <Input
                type="textarea"
                onChange={(e) => this.setValues(e, "description")}
                value={modelProduct.description}
                invalid={errors.descriptionError}
                rows="3"
              />
              <FormFeedback>
                Por favor, insira uma descrição para o produto
              </FormFeedback>
            </FormGroup>

            {/* Novo campo para quantidade */}
            <FormGroup>
              <div className="form-row">
                <div className="col-md-3">
                  <Label htmlFor="quantity">Quantidade:*</Label>
                  <Input
                    type="number"
                    onChange={(e) => this.setValues(e, "quantity")}
                    value={modelProduct.quantity}
                    invalid={errors.quantityError}
                    min="0"
                  />
                  <FormFeedback>Quantidade não pode ser negativa</FormFeedback>
                </div>
              </div>
            </FormGroup>

            {/* Novo campo para imagem */}
            <FormGroup>
              <Label htmlFor="image">Imagem do Produto:</Label>
              <Input
                type="file"
                onChange={this.handleImageChange}
                accept="image/*"
              />
              {previewImage && (
                <div className="mt-2">
                  <img
                    src={previewImage}
                    alt="Preview"
                    style={{ maxWidth: "200px", maxHeight: "200px" }}
                  />
                </div>
              )}
            </FormGroup>

            <Button
              size="sm"
              onClick={(e) => this.save()}
              color="success"
              disabled={loading}
            >
              {loading && <FaSpinner className="fa fa-spinner fa-spin" />}
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
    );
  }
}

class ListFormProduct extends Component {
  onEdit = (product) => {
    PubSub.publish("edit-product", product);
  };

  render() {
    const { formProduct } = this.props;
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
                <th>Quantidade</th>
                <th>Opções</th>
              </tr>
            </thead>
            <tbody>
              {formProduct.results.map((s) => (
                <tr key={s.id}>
                  <td>{s.name}</td>
                  <td>
                    <NumberFormat
                      displayType={"text"}
                      value={s.value}
                      thousandSeparator={"."}
                      decimalSeparator={","}
                      prefix={"R$"}
                    />
                  </td>
                  <td>{s.quantity}</td>
                  <td>
                    <Button
                      onClick={(e) => this.onEdit(s)}
                      color="secondary"
                      outline
                    >
                      <i className="cui-pencil"></i>
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>
    );
  }
}

export default class Product extends Component {
  state = {
    formProduct: { results: [], currentPage: "", pageCount: "", pageSize: "" },
  };

  componentDidMount() {
    this.consultAll();
  }

  consultAll = async () => {
    await Axios.get(URL_Product).then((resp) => {
      const { data } = resp;
      if (data) {
        // Processar os dados para incluir a URL da imagem quando existir
        const processedData = data.results.map((product) => ({
          ...product,
          imageUrl: product.imageBytes
            ? `data:image/jpeg;base64,${product.imageBytes}`
            : null,
        }));

        this.setState({
          formProduct: {
            ...data,
            results: processedData,
          },
        });
      }
    });
  };

  // E no componentWillMount do FormProduct:
  componentWillMount() {
    PubSub.subscribe("edit-product", (topic, service) => {
      this.setState({
        modelProduct: {
          ...service,
          description: service.description || "",
          quantity: service.quantity || 0,
        },
        previewImage:
          service.imageUrl ||
          (service.imageBytes
            ? `data:image/jpeg;base64,${service.imageBytes}`
            : null),
      });
    });
  }

  render() {
    return (
      <div>
        <div className="row">
          <div className="col-md-4 my-3">
            <ListFormProduct formProduct={this.state.formProduct} />
          </div>
          <div className="col-md-8 my-3">
            <FormProduct consultAll={this.consultAll} />
          </div>
        </div>
      </div>
    );
  }
}
