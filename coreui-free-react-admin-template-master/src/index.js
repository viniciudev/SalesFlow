import 'react-app-polyfill/ie9'; // For IE 9-11 support
import 'react-app-polyfill/stable';
// import 'react-app-polyfill/ie11'; // For IE 11 support
import './polyfill'
import React from 'react';
import ReactDOM from 'react-dom';
import './index.css';
import App from './App';
import * as serviceWorker from './serviceWorker';
// import "bootstrap/dist/css/bootstrap.min.css";

ReactDOM.render(<App />, document.getElementById('root'));

// If you want your app to work offline and load faster, you can change
// unregister() to register() below. Note this comes with some pitfalls.
// Learn more about service workers: http://bit.ly/CRA-PWA
serviceWorker.register({

    onUpdate: registration => {
        alert('Nova versão disponível! o sistema será atualizado.');
        if (registration && registration.waiting) {
            console.log("passou no service worker")
            localStorage.clear()
            const waitingWorker = registration && registration.waiting;
            waitingWorker.addEventListener("statechange", event => {
                if (event.target.state === "activated") {
                    window.location.href = process.env.HOST;
                }
            });
            waitingWorker.postMessage({ type: 'SKIP_WAITING' });
        }
        window.location.reload();
    }
});
