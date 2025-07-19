import React, { Component } from 'react';
import { Button, Card, CardBody, Col, Container, InputGroup, InputGroupAddon, InputGroupText, Row } from 'reactstrap';
import { ErrorMessage, Formik, Form, Field } from 'formik'
import axios from 'axios'
import { history } from '../../../history'
import * as yup from 'yup'
import { URL_User } from '../../../services/userService'
import swal from 'sweetalert';
import InputMask from 'react-input-mask';
import CharacterRemover from 'character-remover';
import { FaSpinner } from 'react-icons/fa'
class Register extends Component {
  state = {
    loading: false
  }
  render() {
    const { loading } = this.state
    const handleSubmit = async values => {
      this.setState({ loading: true })
      const map = {
        name: values.firtName,
        email: values.email,
        password: values.password,
        birthDate: new Date(),
        role: 'Admin',
        cellPhone: CharacterRemover.removeAll(values.cellPhone)
      }
      await axios.post(URL_User, map)
        .then(resp => {
          const { data } = resp
          if (data == 'Salvo com Sucesso!') {
            swal(data, {
              icon: "success"
            }).then(ok => {
              if (ok)
                history.push('/login')
            })
          }
          else {
            swal(data, {
              icon: "warning"
            })
          }
        }).catch(() => this.setState({ loading: false }))
      this.setState({ loading: false })
    }

    const validations = yup.object().shape({
      firtName: yup.string().required('Informe seu Primeiro nome, min 4 caracteres!'),
      email: yup.string().email("Email inválido!").required("Informe o Email."),
      password: yup.string().required("Informe a Senha, mínimo de 3 caracteres!"),
      Confirmedpassword: yup.string().oneOf([yup.ref('password'), null], 'Senha não confere!'),
      cellPhone: yup.string().required("Informe o telefone."),

    })
    return (
      <div className="app flex-row align-items-center">
        <Container>
          <Row className="justify-content-center">
            <Col md="9" lg="7" xl="6">
              <Card className="mx-4">
                <CardBody className="p-4">
                  <Formik
                    initialValues={{
                      firtName: '',
                      email: '',
                      password: '',
                      Confirmedpassword: '',
                      cellPhone: ''
                    }}
                    onSubmit={handleSubmit}
                    validationSchema={validations}
                  >
                    <Form>
                      <h1>Registrar</h1>
                      <p className="text-muted">Criar Conta</p>
                      <InputGroup className="mb-3">
                        <InputGroupAddon addonType="prepend">
                          <InputGroupText>
                            <i className="icon-user"></i>
                          </InputGroupText>
                        </InputGroupAddon>
                        <Field className="form-control" name="firtName" placeholder="Primeiro nome" autoComplete="username" />
                        <ErrorMessage
                          style={{ color: 'red' }}
                          name="firtName"
                        />
                      </InputGroup>
                      <InputGroup className="mb-3">
                        <InputGroupAddon addonType="prepend">
                          <InputGroupText>@</InputGroupText>
                        </InputGroupAddon>
                        <Field className="form-control" name="email" placeholder="Email" autoComplete="email" />
                        <ErrorMessage
                          component="span"
                          name="email"
                          className="Login-Error"
                        />
                      </InputGroup>
                      <InputGroup className="mb-3">
                        <InputGroupAddon addonType="prepend">
                          <InputGroupText>
                            <i className="icon-lock"></i>
                          </InputGroupText>
                        </InputGroupAddon>
                        <Field className="form-control" name="password" placeholder="Password" type='password' />
                        <ErrorMessage
                          component="span"
                          name="password"
                          className="Login-Error"
                        />
                      </InputGroup>
                      <InputGroup className="mb-4">
                        <InputGroupAddon addonType="prepend">
                          <InputGroupText>
                            <i className="icon-lock"></i>
                          </InputGroupText>
                        </InputGroupAddon>
                        <Field className="form-control" name="Confirmedpassword" type="password" placeholder="Confirme o password" autoComplete="new-password" />
                        <ErrorMessage
                          component="span"
                          name="Confirmedpassword"
                          className="Login-Error"

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
                            return <InputMask mask='(99) 9 9999-9999'
                              {...field}
                              id={"cellPhone"}
                              className="form-control"
                              placeholder="Telefone"
                            />

                          }}
                          name="cellPhone"
                        />
                        <ErrorMessage
                          component="span"
                          name="cellPhone"
                          className="Login-Error"
                        />
                      </InputGroup>
                      <Button
                        block
                        type="submit"
                        color="success"
                        disabled={loading}
                      >
                        {loading && <FaSpinner className='fa fa-spinner fa-spin' />}
                        {loading && " Registrando..."}
                        {/* {!loading && <i className="fa fa-plus-circle"></i>} */}
                        {!loading && " Registrar"}
                      </Button>
                    </Form>
                  </Formik>
                </CardBody>
                {/* <CardFooter className="p-4">
                  <Row>
                    <Col xs="12" sm="6">
                      <Button className="btn-facebook mb-1" block><span>facebook</span></Button>
                    </Col>
                    <Col xs="12" sm="6">
                      <Button className="btn-twitter mb-1" block><span>twitter</span></Button>
                    </Col>
                  </Row>
                </CardFooter> */}
              </Card>
            </Col>
          </Row>
        </Container>
      </div>
    );
  }
}

export default Register;
