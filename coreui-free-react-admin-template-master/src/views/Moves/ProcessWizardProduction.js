import React, { useState } from "react";
import {
  Card,
  CardBody,
  CardHeader,
  Form,
  FormGroup,
  Label,
  Input,
  Button,
  Row,
  Col,
  Progress,
  Alert,
  Nav,
  NavItem,
  NavLink,
} from "reactstrap";
import {
  CheckCircle,
  ArrowLeft,
  ArrowRight,
  Package,
  Palette,
  Grid3X3,
} from "lucide-react";

const ProcessWizard = () => {
  const [currentStep, setCurrentStep] = useState(1);
  const [formData, setFormData] = useState({
    producaoFio: {
      fiacao: "",
      tex: "",
      materiaPrima: "",
      lotes: "",
      certificacoes: "",
    },
    tinturaria: {
      cores: "",
      lotes: "",
      tipoCorante: "reativo",
    },
    malharia: {
      galga: "",
      tipoMalha: "",
      largura: "",
      acabamentos: "",
    },
  });
  const [completedSteps, setCompletedSteps] = useState([]);

  const steps = [
    {
      id: 1,
      title: " Produção de fio",
      icon: Package,
      description: "Configurações de fiação e matéria-prima",
    },
    {
      id: 2,
      title: " Tinturaria",
      icon: Palette,
      description: "Processo de coloração e tratamento",
    },
    {
      id: 3,
      title: " Malharia",
      icon: Grid3X3,
      description: "Especificações de malha e acabamento",
    },
  ];

  const handleInputChange = (step, field, value) => {
    setFormData((prev) => ({
      ...prev,
      [step]: {
        ...prev[step],
        [field]: value,
      },
    }));
  };

  const validateStep = (step) => {
    switch (step) {
      case 1:
        const { fiacao, tex, materiaPrima, lotes, certificacoes } =
          formData.producaoFio;
        return !!(fiacao && tex && materiaPrima && lotes && certificacoes);
      case 2:
        const {
          cores,
          lotes: lotesTinturaria,
          tipoCorante,
        } = formData.tinturaria;
        return !!(cores && lotesTinturaria && tipoCorante);
      case 3:
        const { galga, tipoMalha, largura, acabamentos } = formData.malharia;
        return !!(galga && tipoMalha && largura && acabamentos);
      default:
        return false;
    }
  };

  const handleNext = () => {
    if (validateStep(currentStep)) {
      if (!completedSteps.includes(currentStep)) {
        setCompletedSteps((prev) => [...prev, currentStep]);
      }
      if (currentStep < 3) {
        setCurrentStep(currentStep + 1);
      }
    }
  };

  const handlePrevious = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const handleStepClick = (step) => {
    if (step <= currentStep || completedSteps.includes(step - 1)) {
      setCurrentStep(step);
    }
  };

  const handleSubmit = () => {
    if (validateStep(3)) {
      if (!completedSteps.includes(3)) {
        setCompletedSteps((prev) => [...prev, 3]);
      }
      alert("Processo concluído com sucesso!");
      console.log("Dados do processo:", formData);
    }
  };

  const getStepIcon = (step, stepNumber) => {
    const IconComponent = step.icon;
    const isCompleted = completedSteps.includes(stepNumber);
    const isCurrent = currentStep === stepNumber;

    if (isCompleted) {
      return <CheckCircle className="w-6 h-6 text-green-600" />;
    }

    return (
      <IconComponent
        className={`w-6 h-6 ${isCurrent ? "text-blue-600" : "text-gray-400"}`}
      />
    );
  };

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <Form>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="fiacao">Fiação</Label>
                  <Input
                    type="text"
                    name="fiacao"
                    id="fiacao"
                    placeholder="Ex: Ring Spun"
                    value={formData.producaoFio.fiacao}
                    onChange={(e) =>
                      handleInputChange("producaoFio", "fiacao", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="tex">TEX</Label>
                  <Input
                    type="text"
                    name="tex"
                    id="tex"
                    placeholder="Ex: 30/1"
                    value={formData.producaoFio.tex}
                    onChange={(e) =>
                      handleInputChange("producaoFio", "tex", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="materiaPrima">Matéria Prima</Label>
                  <Input
                    type="text"
                    name="materiaPrima"
                    id="materiaPrima"
                    placeholder="Ex: Algodão 100%"
                    value={formData.producaoFio.materiaPrima}
                    onChange={(e) =>
                      handleInputChange(
                        "producaoFio",
                        "materiaPrima",
                        e.target.value
                      )
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="lotes">Lotes</Label>
                  <Input
                    type="text"
                    name="lotes"
                    id="lotes"
                    placeholder="Ex: LT-2024-001"
                    value={formData.producaoFio.lotes}
                    onChange={(e) =>
                      handleInputChange("producaoFio", "lotes", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <FormGroup>
              <Label for="certificacoes">Certificações</Label>
              <Input
                type="textarea"
                name="certificacoes"
                id="certificacoes"
                placeholder="Ex: OEKO-TEX, GOTS, BCI"
                rows={3}
                value={formData.producaoFio.certificacoes}
                onChange={(e) =>
                  handleInputChange(
                    "producaoFio",
                    "certificacoes",
                    e.target.value
                  )
                }
              />
            </FormGroup>
          </Form>
        );

      case 2:
        return (
          <Form>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="cores">Cores</Label>
                  <Input
                    type="text"
                    name="cores"
                    id="cores"
                    placeholder="Ex: Azul Marinho, Branco"
                    value={formData.tinturaria.cores}
                    onChange={(e) =>
                      handleInputChange("tinturaria", "cores", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="lotesTinturaria">Lotes</Label>
                  <Input
                    type="text"
                    name="lotesTinturaria"
                    id="lotesTinturaria"
                    placeholder="Ex: TIN-2024-001"
                    value={formData.tinturaria.lotes}
                    onChange={(e) =>
                      handleInputChange("tinturaria", "lotes", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <FormGroup>
              <Label for="tipoCorante">Tipo de Corante</Label>
              <Input
                type="select"
                name="tipoCorante"
                id="tipoCorante"
                value={formData.tinturaria.tipoCorante}
                onChange={(e) =>
                  handleInputChange("tinturaria", "tipoCorante", e.target.value)
                }
              >
                <option value="reativo">Reativo</option>
                <option value="disperso">Disperso</option>
              </Input>
            </FormGroup>
          </Form>
        );

      case 3:
        return (
          <Form>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="galga">Galga</Label>
                  <Input
                    type="text"
                    name="galga"
                    id="galga"
                    placeholder="Ex: E24, E28"
                    value={formData.malharia.galga}
                    onChange={(e) =>
                      handleInputChange("malharia", "galga", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="tipoMalha">Tipo de Malha</Label>
                  <Input
                    type="text"
                    name="tipoMalha"
                    id="tipoMalha"
                    placeholder="Ex: Jersey, Ribana 1x1"
                    value={formData.malharia.tipoMalha}
                    onChange={(e) =>
                      handleInputChange("malharia", "tipoMalha", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
            <Row>
              <Col md={6}>
                <FormGroup>
                  <Label for="largura">Largura</Label>
                  <Input
                    type="text"
                    name="largura"
                    id="largura"
                    placeholder="Ex: 180cm"
                    value={formData.malharia.largura}
                    onChange={(e) =>
                      handleInputChange("malharia", "largura", e.target.value)
                    }
                  />
                </FormGroup>
              </Col>
              <Col md={6}>
                <FormGroup>
                  <Label for="acabamentos">Acabamentos</Label>
                  <Input
                    type="text"
                    name="acabamentos"
                    id="acabamentos"
                    placeholder="Ex: Amaciamento, Sanforização"
                    value={formData.malharia.acabamentos}
                    onChange={(e) =>
                      handleInputChange(
                        "malharia",
                        "acabamentos",
                        e.target.value
                      )
                    }
                  />
                </FormGroup>
              </Col>
            </Row>
          </Form>
        );

      default:
        return null;
    }
  };

  const progress = (completedSteps.length / 3) * 100;

  return (
    <div className="mb-1">
      <div className="mb-6">
        {/* <h1 className="text-3xl font-bold text-gray-800 mb-2">
          Wizard de Processo Têxtil
        </h1> */}
        <p className="text-gray-600">
          Configure cada etapa do processo de produção
        </p>
      </div>

      {/* Progress Bar */}
      <div className="mb-6">
        <div className="flex justify-between text-sm text-gray-600 mb-2">
          <span>Progresso do processo </span>
          <span>{Math.round(progress)}% concluído</span>
        </div>
        <Progress value={progress} color="primary" className="mb-4" />
      </div>

      {/* Step Navigation */}
      <Nav pills className="mb-6 justify-center">
        {steps.map((step, index) => (
          <NavItem key={step.id} className="mx-1">
            <NavLink
              active={currentStep === step.id}
              className={`cursor-pointer d-flex align-items-center px-4 py-3 rounded-pill transition-all duration-300 ${
                currentStep === step.id
                  ? "bg-blue-600 text-white shadow-lg"
                  : completedSteps.includes(step.id)
                  ? "bg-green-100 text-green-700 hover:bg-green-200"
                  : "bg-gray-100 text-gray-500 hover:bg-gray-200"
              }`}
              onClick={() => handleStepClick(step.id)}
            >
              <div className="me-2">{getStepIcon(step, step.id)}</div>
              <div className="d-none d-md-block">
                <div className="fw-semibold">{step.title}</div>
              </div>
            </NavLink>
          </NavItem>
        ))}
      </Nav>
      <br />
      {/* Main Content */}
      <Card className="shadow-sm border-0">
        <CardHeader className="bg-gradient-to-r from-blue-50 to-blue-50 border-0">
          <div className="d-flex align-items-center">
            <div className="me-3">
              {getStepIcon(steps[currentStep - 1], currentStep)}
            </div>
            <div>
              <h4 className="mb-1 ml-2 text-blue-900 fw-bold">
                {steps[currentStep - 1].title}
              </h4>
              <p className="mb-0 ml-2 text-blue-700">
                {steps[currentStep - 1].description}
              </p>
            </div>
          </div>
        </CardHeader>
        <CardBody className="p-4">
          {!validateStep(currentStep) && (
            <Alert color="warning" className="mb-4">
              <strong>Atenção:</strong> Preencha todos os campos obrigatórios
              antes de continuar.
            </Alert>
          )}

          {renderStepContent()}
        </CardBody>
      </Card>

      {/* Navigation Buttons */}
      <div className="d-flex justify-between mt-4">
        <Button
          color="outline-secondary"
          onClick={handlePrevious}
          disabled={currentStep === 1}
          className="d-flex align-items-center px-4 py-2"
        >
          <ArrowLeft className="w-4 h-4 me-2" />
          Anterior
        </Button>

        <div>
          {currentStep < 3 ? (
            <Button
              color="primary"
              onClick={handleNext}
              disabled={!validateStep(currentStep)}
              className="d-flex align-items-center px-4 py-2"
            >
              Próximo
              <ArrowRight className="w-4 h-4 ms-2" />
            </Button>
          ) : (
            <Button
              color="success"
              onClick={handleSubmit}
              disabled={!validateStep(currentStep)}
              className="d-flex align-items-center px-4 py-2"
            >
              <CheckCircle className="w-4 h-4 me-2" />
              Finalizar Processo
            </Button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProcessWizard;
