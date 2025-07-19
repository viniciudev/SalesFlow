import React, { Component } from 'react';
import { Link } from 'react-router-dom';
import { Button, Card, CardBody, CardGroup, Col, Container, Input, InputGroup, InputGroupAddon, InputGroupText, Row } from 'reactstrap';
import axios from 'axios'
import { history } from '../../../history'
import { ErrorMessage, Formik, Form, Field } from 'formik'
import * as yup from 'yup'
import { URL_User } from '../../../services/userService'

class Login extends Component {
  state = { loading: false }
  render() {
    const { loading } = this.state

    const handleSubmit = async values => {
      this.setState({ loading: true })
      const map = {
        password: values.password,
        email: values.email
      }
      await axios.post(`${URL_User}/authenticate`, map)
        .then(resp => {
          const { data } = resp
          if (data) {
            if (data.message == undefined) {
              localStorage.setItem('app-token', data.token)
              localStorage.setItem('idCompany', data.idCompany)
              localStorage.setItem('guid', data.guid)
              history.push('/dashboard')
            }
            else {
              alert(data.message, { icon: "warning" })
            }
          }

        }).catch(() => { this.setState({ loading: false }) })
      this.setState({ loading: false })
    }

    const validations = yup.object().shape({
      email: yup.string().email("Email inválido.").required("Email."),
      password: yup.string().required("Senha.")
    });

    return (
      <div className="app flex-row align-items-center">
        <Container>
          <Row className="justify-content-center">
            <Card className="p-4 "
            >
              <CardBody className="text-center ">
                <Formik
                  initialValues={{
                    email: '',
                    password: ''
                  }}
                  onSubmit={handleSubmit}
                  validationSchema={validations}
                >
                  <Form >
                    <h1>Login</h1>
                    <p className="text-muted">Faça login em sua conta</p>
                    <InputGroup className="mb-3">
                      <InputGroupAddon addonType="prepend">
                        <InputGroupText>
                          <i className="icon-user"></i>
                        </InputGroupText>
                      </InputGroupAddon>
                      <Field className="form-control"
                        name="email" placeholder="Email" autoComplete="username" />
                      <ErrorMessage
                        style={{ color: "red" }}
                        component="span"
                        name="email"
                        className="Login-Error"
                      />
                    </InputGroup>
                    <InputGroup className="mb-4">
                      <InputGroupAddon addonType="prepend">
                        <InputGroupText>
                          <i className="icon-lock"></i>
                        </InputGroupText>
                      </InputGroupAddon>
                      <Field name="password" className="form-control" placeholder="Password" type="password" autoComplete="current-password" />
                      <ErrorMessage
                        style={{ color: "red" }}
                        component="span"
                        name="password"
                        className="Login-Error"
                      />
                    </InputGroup>
                    <Row>
                      <Col xs="12">
                        <Button
                          className="px-4"
                          block
                          type="submit"
                          color="primary"
                          disabled={loading}
                        >
                          {loading && "Acessando..."}
                          {!loading && "Login"}
                        </Button>
                      </Col>
                      {/* <Col xs="6" className="text-right">
                        <Button color="link" className="px-0">Esqueceu a senha?</Button>
                      </Col> */}
                    </Row>
                    <Row>
                      <Col xs="12">
                        <div>
                          <p className="mt-3 text-muted">Cadastre-se</p>
                          <Button color="secondary"

                            onClick={e => history.push("/register")}
                            block active tabIndex={-1}>Novo Registro</Button>
                        </div>
                      </Col>
                    </Row>
                  </Form>
                </Formik>
              </CardBody>
            </Card>
          </Row>
        </Container>
      </div>
    );
  }
}

export default Login;
