import React, { Component } from 'react';
import { Button, Modal, ModalHeader, ModalBody, ModalFooter } from 'reactstrap';
import { CgSpinnerTwo } from 'react-icons/cg'
import './modal.css'
export default class Loader extends Component {
    render() {
        return (
            <Modal isOpen={this.props.value}
                centered
                className="bd-modal-dialog"
            >
                <CgSpinnerTwo className='fa fa-spinner fa-spin fa-3x' />
            </Modal>
        )
    }
}