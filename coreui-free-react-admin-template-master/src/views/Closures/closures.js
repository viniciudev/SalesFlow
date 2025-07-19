import Axios from 'axios';
import React, { useState, useEffect } from 'react';
import { Card, CardBody, CardHeader, Button, FormGroup, Input, Table, FormFeedback, Label } from 'reactstrap'
import { URL_Salesman } from '../../services/salesmanService';
import { URL_Closures } from '../../services/closuresService';
import CurrencyInput from 'react-currency-input';
import ConvertToUSD from './../../ConvertCurrency';
import { removeAll } from 'character-remover';

import swal from 'sweetalert';

function WeatherApp() {

    const [location, setLocation] = useState(true);
    const [checkin, setCheckin] = useState(false);
    const [weather, setWeather] = useState(null);
    const [results, setResults] = useState([]);
    const [arraySalesman, setArraySalesman] = useState([]);
    const [text, setText] = useState("")
    const [odometer, setOdometer] = useState(0)
    const [odometerFinal, setOdometerFinal] = useState(0)
    const [value, setValue] = useState(0)
    const [idSalesman, setIdSalesman] = useState(0)
    const [myParam, setMyparam] = useState(window.location.pathname.split('/')[3])
    const [model, setModel] = useState({ id: 0, longInit: '', latInit: '', dateInit: '', odometer: 0 });

    const handleLocationClick = async () => {
        console.log('Abertura', location);
        console.log('odometer', odometer);
        if (odometer > 0 && idSalesman > 0) {

            let map = {
                longInit: location.latitude,
                latInit: location.longitude,
                dateInit: new Date(),
                type: 0,
                idSalesman: idSalesman,
                odometer: odometer
            }
            await Axios.post(URL_Closures, map).then(resp => {
                const { data } = resp
                if (data.id > 0) {
                    swal({ title: "Feito check-in com sucesso", icon: "success" })
                    setCheckin(true)
                    setModel()
                    setModel({
                        ...model, id: data.id,
                        latInit: data.latInit,
                        longInit: data.longInit,
                        dateInit: data.dateInit,
                        odometer: data.odometer
                    })
                }
            })
        } else {
            swal({ title: "Campos obrigatórios", text: "Verifique os campos obrigatórios(*)", icon: "warning" })
        }
    }



    function handleClosedLocationClick() {
        if (odometerFinal > 0) {
            var latFinal = "";
            var longFinal = ""
            navigator.geolocation.getCurrentPosition((position) => {
                latFinal = position.coords.latitude
                longFinal = position.coords.longitude
                console.log('fechamento', longFinal, latFinal);
                let map = {
                    id: model.id,
                    longInit: model.longInit,
                    latInit: model.latInit,
                    dateInit: new Date(model.dateInit),
                    type: 1,
                    idSalesman: idSalesman,
                    longFinal: longFinal,
                    latFinal: latFinal,
                    dateFinal: new Date(),
                    closuresDetails: results,
                    odometer: model.odometer,
                    odometerFinal: odometerFinal
                }
                Axios.put(URL_Closures, map).then(e => {
                    const { data } = e
                    if (data == "OK") {
                        swal({ title: "Salvo", icon: "success", text: "Check-out salvo com sucesso!" })
                    }
                })
                setCheckin(false)
                setResults([])
            });
        } else {
            swal({ title: "Campos obrigatórios", text: "Verifique os campos obrigatórios(*)", icon: "warning" })
        }
    }

    useEffect(() => {
        // Update the document title using the browser API
        document.title = `You clicked ${1} times`;
        navigator.geolocation.getCurrentPosition(success, error);

        consultSalesman();

    }, []);

    const consultSalesman = async () => {
        Axios.get(`${URL_Salesman}/GetAllByGuid`, {
            params: { guid: myParam }
        }).then(resp => {
            const { data } = resp
            setArraySalesman([...data])
        })
    }

    const consultCkeckin = async (e) => {
        setIdSalesman(e)
        await Axios.get(URL_Closures, {
            params: { idsalesman: e }
        }).then(resp => {
            const { data } = resp
            if (data.id > 0) {
                setModel({
                    ...model, id: data.id,
                    latInit: data.latInit,
                    longInit: data.longInit,
                    dateInit: data.dateInit,
                    odometer: data.odometer
                })
                setCheckin(true)
            } else {
                setCheckin(false)
            }
        })
    }

    const addExpense = () => {
        if (text.length > 0) {
            const map = [
                ...results,
                {
                    description: text,
                    value: parseFloat(ConvertToUSD(value))
                }
            ]
            setResults(map)
            setText('')
            setValue(0)
        }
    }

    const removeItem = (index) => {

        if (index > -1) { // only splice array when item is found
            results.splice(index, 1); // 2nd parameter means remove one item only
        }
        setResults([...results]);
    };

    function success(position) {

        const latitude = position.coords.latitude;
        const longitude = position.coords.longitude;
        setLocation({ latitude, longitude });
        // Make API call to OpenWeatherMap
        // fetch(`https://api.openweathermap.org/data/2.5/weather?lat=${latitude}&lon=${longitude}&appid=<YOUR_API_KEY>&units=metric`)
        //     .then(response => response.json())
        //     .then(data => {
        //         setWeather(data);
        //         console.log(data);
        //     })
        //     .catch(error => console.log(error));
    }
    function error() {
        console.log("Unable to retrieve your location");
    }
    return (
        < div >
            <Card>
                <CardHeader>
                    <strong>Registros</strong>
                    <small> de despesas</small>
                </CardHeader>
                <CardBody>
                    <div>
                        <FormGroup >
                            <Label>Vendedor:*</Label>
                            <Input
                                type="select"
                                onChange={e => consultCkeckin(e.target.value)}

                            >
                                <option value={0}>Selecione o vendedor</option>
                                {arraySalesman.map(e => (
                                    <option value={e.id}>{e.name}</option>
                                ))}
                            </Input>

                        </FormGroup>
                        {!checkin ?
                            <div>
                                <FormGroup >
                                    <Label>Hodômetro:*</Label>
                                    <Input
                                        type="number"
                                        placeholder="Km hodômetro..."
                                        onChange={e => setOdometer(e.target.value)}
                                    ></Input>

                                </FormGroup>
                                <Button color='primary' onClick={handleLocationClick}>Salvar local de partida</Button> </div> : null}

                        {weather ? (
                            <div>
                                <p>Location: {weather.name}</p>
                                {/* <p>Temperature: {weather.main.temp} °C</p> */}
                                {/* <p>Weather: {weather.weather[0].description}</p> */}
                            </div>
                        ) : null}

                        {
                            checkin &&
                            <div>
                                <FormGroup >
                                    <Label>Hodômetro final:*</Label>
                                    <Input
                                        type="number"
                                        placeholder="Km hodômetro..."
                                        onChange={e => setOdometerFinal(e.target.value)}
                                    ></Input>

                                </FormGroup>
                                <hr />
                                <div className='card' >
                                    <div className="card-header">
                                        Informações do fechamento
                                    </div>
                                    <div className='card-body'>

                                        <FormGroup >

                                            <Input
                                                type="text"
                                                placeholder="Adicionar despesas..."
                                                onChange={e => setText(e.target.value)}
                                                value={text}
                                            ></Input>

                                        </FormGroup>
                                        <FormGroup >

                                            <CurrencyInput
                                                className="form-control"
                                                type="text"
                                                decimalSeparator=","
                                                thousandSeparator="."
                                                prefix="R$"
                                                onChangeEvent={e => setValue(e.target.value)}
                                                value={value}
                                            >
                                            </CurrencyInput>

                                        </FormGroup>
                                        <FormGroup >
                                            <Button onClick={e => addExpense()}>Adicionar despesa</Button>
                                        </FormGroup>
                                        <FormGroup>
                                            <Table responsive size="sm">
                                                <thead>
                                                    <tr>
                                                        <th>Descrição</th>
                                                        <th>Valor</th>
                                                        <th>Opções</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    {results.map((v, index) => (<tr key={index}>
                                                        <td>{v.description}</td>
                                                        <td>{v.value}</td>
                                                        <td>
                                                            <Button color="secondary" outline
                                                                onClick={e => removeItem(index)}
                                                            >
                                                                <i className="fa fa-trash-o "></i>
                                                            </Button>
                                                        </td>
                                                    </tr>))}
                                                </tbody>
                                            </Table>
                                        </FormGroup>

                                    </div>
                                </div>
                                <Button color='primary' onClick={handleClosedLocationClick}>fechar local</Button>
                            </div>
                        }
                    </div>
                </CardBody>
            </Card>
            {/* {location && <div>{location.latitude}</div>} */}
            {/* {checkin && !weather ? <p>Loading weather data...</p> : null} */}
        </div >
    );
}

export default WeatherApp;