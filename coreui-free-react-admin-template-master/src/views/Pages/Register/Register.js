import React, { Component } from "react";
import {
  Button,
  Card,
  CardBody,
  Col,
  Container,
  InputGroup,
  InputGroupAddon,
  InputGroupText,
  Row,
} from "reactstrap";
import { ErrorMessage, Formik, Form, Field } from "formik";
import axios from "axios";
import { history } from "../../../history";
import * as yup from "yup";
import { URL_User } from "../../../services/userService";
import swal from "sweetalert";
import InputMask from "react-input-mask";
import CharacterRemover from "character-remover";
import { FaSpinner } from "react-icons/fa";

class Register extends Component {
  state = {
    loading: false,
    showCnpjField: false,
  };

  // Função para validar CNPJ
  validateCNPJ = (cnpj) => {
    if (!cnpj) return false;

    const cleanCnpj = CharacterRemover.removeAll(cnpj);

    // Verifica se tem 14 dígitos
    if (cleanCnpj.length !== 14) return false;

    // Elimina CNPJs inválidos conhecidos
    if (/^(\d)\1+$/.test(cleanCnpj)) return false;

    // Validação dos dígitos verificadores
    let tamanho = cleanCnpj.length - 2;
    let numeros = cleanCnpj.substring(0, tamanho);
    let digitos = cleanCnpj.substring(tamanho);
    let soma = 0;
    let pos = tamanho - 7;

    // Primeiro dígito verificador
    for (let i = tamanho; i >= 1; i--) {
      soma += numeros.charAt(tamanho - i) * pos--;
      if (pos < 2) pos = 9;
    }

    let resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);
    if (resultado !== parseInt(digitos.charAt(0))) return false;

    // Segundo dígito verificador
    tamanho = tamanho + 1;
    numeros = cleanCnpj.substring(0, tamanho);
    soma = 0;
    pos = tamanho - 7;

    for (let i = tamanho; i >= 1; i--) {
      soma += numeros.charAt(tamanho - i) * pos--;
      if (pos < 2) pos = 9;
    }

    resultado = soma % 11 < 2 ? 0 : 11 - (soma % 11);
    if (resultado !== parseInt(digitos.charAt(1))) return false;

    return true;
  };

  render() {
    const { loading, showCnpjField } = this.state;

    const handleSubmit = async (values) => {
      this.setState({ loading: true });

      const map = {
        name: values.firtName,
        email: values.email,
        password: values.password,
        birthDate: new Date(),
        role: "Admin",
        cellPhone: CharacterRemover.removeAll(values.cellPhone),
        typeUser: values.typeUser,
        cnpj: values.cnpj ? CharacterRemover.removeAll(values.cnpj) : null,
      };

      try {
        const resp = await axios.post(URL_User, map);
        const { data } = resp;

        if (data === "Salvo com Sucesso!") {
          swal("Conta criada com sucesso! Email de verificação enviado.", {
            icon: "success",
          }).then((ok) => {
            if (ok) history.push("/login");
          });
          // Se não for cliente, enviar email de verificação
          // if (values.typeUser !== "0") {
          //   try {
          //     await axios.post("/api/send-verification-email", {
          //       email: values.email,
          //       name: values.firtName,
          //       userType: values.typeUser,
          //     });

          //     swal("Conta criada com sucesso! Email de verificação enviado.", {
          //       icon: "success",
          //     }).then((ok) => {
          //       if (ok) history.push("/login");
          //     });
          //   } catch (emailError) {
          //     console.error("Erro ao enviar email:", emailError);
          //     swal(
          //       "Conta criada, mas houve um erro ao enviar o email de verificação.",
          //       {
          //         icon: "warning",
          //       }
          //     ).then((ok) => {
          //       if (ok) history.push("/login");
          //     });
          //   }
          // } else {
          //   swal(data, {
          //     icon: "success",
          //   }).then((ok) => {
          //     if (ok) history.push("/login");
          //   });
          // }
        } else {
          swal(data, {
            icon: "warning",
          });
        }
      } catch (error) {
        console.error("Erro ao criar conta:", error);
        swal("Erro ao criar conta. Tente novamente.", {
          icon: "error",
        });
      } finally {
        this.setState({ loading: false });
      }
    };

    const handleTypeUserChange = (value, setFieldValue) => {
      const isNotClient = value !== "0";
      this.setState({ showCnpjField: isNotClient });

      // Limpar campo CNPJ quando não for necessário
      if (!isNotClient) {
        setFieldValue("cnpj", "");
      }
    };

    const validations = yup.object().shape({
      firtName: yup
        .string()
        .min(4, "Mínimo de 4 caracteres")
        .required("Informe seu Primeiro nome!"),
      email: yup.string().email("Email inválido!").required("Informe o Email."),
      password: yup
        .string()
        .min(3, "Mínimo de 3 caracteres")
        .required("Informe a Senha!"),
      Confirmedpassword: yup
        .string()
        .oneOf([yup.ref("password"), null], "Senha não confere!")
        .required("Confirme a senha"),
      cellPhone: yup.string().required("Informe o telefone."),
      typeUser: yup.string().required("Selecione o tipo de usuário"),
      cnpj: yup.string().when("typeUser", {
        is: (typeUser) => typeUser !== "0",
        then: yup
          .string()
          .required("CNPJ é obrigatório")
          .test("cnpj-length", "CNPJ deve ter 14 dígitos", (value) => {
            if (!value) return false;
            const cleanCnpj = CharacterRemover.removeAll(value);
            return cleanCnpj.length === 14;
          })
          .test("cnpj-valid", "CNPJ inválido", (value) => {
            if (!value) return false;
            return this.validateCNPJ(value);
          }),
      }),
    });

    return (
      <div className="app flex-row align-items-center">
        <Container>
          <Row className="justify-content-center">
            <Col md="9" lg="7" xl="6">
              <Card className="mx-4">
                <CardBody className="p-4">
                  <Formik
                    initialValues={{
                      firtName: "",
                      email: "",
                      password: "",
                      Confirmedpassword: "",
                      cellPhone: "",
                      typeUser: "",
                      cnpj: "",
                    }}
                    onSubmit={handleSubmit}
                    validationSchema={validations}
                  >
                    {({ setFieldValue, values }) => (
                      <Form>
                        <h1>Registrar</h1>
                        <p className="text-muted">Criar Conta</p>

                        <InputGroup className="mb-3">
                          <InputGroupAddon addonType="prepend">
                            <InputGroupText>
                              <i className="icon-user"></i>
                            </InputGroupText>
                          </InputGroupAddon>
                          <Field
                            className="form-control"
                            name="firtName"
                            placeholder="Primeiro nome"
                            autoComplete="username"
                          />
                          <ErrorMessage
                            component="span"
                            name="firtName"
                            className="text-danger small d-block mt-1"
                          />
                        </InputGroup>

                        <InputGroup className="mb-3">
                          <InputGroupAddon addonType="prepend">
                            <InputGroupText>@</InputGroupText>
                          </InputGroupAddon>
                          <Field
                            className="form-control"
                            name="email"
                            placeholder="Email"
                            autoComplete="email"
                          />
                          <ErrorMessage
                            component="span"
                            name="email"
                            className="text-danger small d-block mt-1"
                          />
                        </InputGroup>

                        <InputGroup className="mb-3">
                          <InputGroupAddon addonType="prepend">
                            <InputGroupText>
                              <i className="icon-lock"></i>
                            </InputGroupText>
                          </InputGroupAddon>
                          <Field
                            className="form-control"
                            name="password"
                            placeholder="Password"
                            type="password"
                          />
                          <ErrorMessage
                            component="span"
                            name="password"
                            className="text-danger small d-block mt-1"
                          />
                        </InputGroup>

                        <InputGroup className="mb-4">
                          <InputGroupAddon addonType="prepend">
                            <InputGroupText>
                              <i className="icon-lock"></i>
                            </InputGroupText>
                          </InputGroupAddon>
                          <Field
                            className="form-control"
                            name="Confirmedpassword"
                            type="password"
                            placeholder="Confirme o password"
                            autoComplete="new-password"
                          />
                          <ErrorMessage
                            component="span"
                            name="Confirmedpassword"
                            className="text-danger small d-block mt-1"
                          />
                        </InputGroup>

                        <InputGroup className="mb-4">
                          <InputGroupAddon addonType="prepend">
                            <InputGroupText>
                              <i className="icon-phone"></i>
                            </InputGroupText>
                          </InputGroupAddon>
                          <Field
                            render={({ field }) => {
                              return (
                                <InputMask
                                  mask="(99) 9 9999-9999"
                                  {...field}
                                  id={"cellPhone"}
                                  className="form-control"
                                  placeholder="Telefone"
                                />
                              );
                            }}
                            name="cellPhone"
                          />
                          <ErrorMessage
                            component="span"
                            name="cellPhone"
                            className="text-danger small d-block mt-1"
                          />
                        </InputGroup>

                        <InputGroup className="mb-4">
                          <InputGroupAddon addonType="prepend">
                            <InputGroupText>
                              <i className="icon-briefcase"></i>
                            </InputGroupText>
                          </InputGroupAddon>
                          <Field
                            as="select"
                            name="typeUser"
                            className="form-control"
                            onChange={(e) => {
                              setFieldValue("typeUser", e.target.value);
                              handleTypeUserChange(
                                e.target.value,
                                setFieldValue
                              );
                            }}
                          >
                            <option value="">
                              Selecione o tipo de usuário
                            </option>
                            <option value={0}>Cliente</option>
                            <option value={1}>Fornecedor</option>
                            <option value={2}>Gestor</option>
                          </Field>
                          <ErrorMessage
                            component="span"
                            name="typeUser"
                            className="text-danger small d-block mt-1"
                          />
                        </InputGroup>

                        {showCnpjField && (
                          <InputGroup className="mb-4">
                            <InputGroupAddon addonType="prepend">
                              <InputGroupText>
                                <i className="icon-doc"></i>
                              </InputGroupText>
                            </InputGroupAddon>
                            <Field
                              render={({ field }) => {
                                return (
                                  <InputMask
                                    mask="99.999.999/9999-99"
                                    {...field}
                                    id={"cnpj"}
                                    className="form-control"
                                    placeholder="CNPJ"
                                  />
                                );
                              }}
                              name="cnpj"
                            />
                            <ErrorMessage
                              component="span"
                              name="cnpj"
                              className="text-danger small d-block mt-1"
                            />
                          </InputGroup>
                        )}

                        <Button
                          block
                          type="submit"
                          color="success"
                          disabled={loading}
                          className="mt-3"
                        >
                          {loading && (
                            <FaSpinner className="fa fa-spinner fa-spin me-2" />
                          )}
                          {loading ? "Registrando..." : "Registrar"}
                        </Button>
                      </Form>
                    )}
                  </Formik>
                </CardBody>
              </Card>
            </Col>
          </Row>
        </Container>
      </div>
    );
  }
}

export default Register;
