import React, {useState} from "react";
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

import {
    Button,
    Card,
    CardHeader,
    CardBody,
    FormGroup,
    Form,
    Input,
    Row,
    Col,
} from "reactstrap";

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const [loginError, setLoginError] = useState('')
    const auth = useAuth();
    const navigate = useNavigate();
  
    const handleLogin = async (e) => {
        e.preventDefault();
        try{
            await auth.login(username, password);
            setLoginError('');
            navigate('/admin/dashboard'); // Navigate on successful login
        } catch (error) {
            console.error('Login failed:', error);
            setLoginError('Wrong username or password!');
        }
    };
  
    return (
        <div className="content centered-container">
        <Card className="centered-box">
            <CardHeader>
                <h1 className="centered">Login</h1>
            </CardHeader>
            <CardBody>
                <Form>
                    <Row>
                        <Col>
                            <FormGroup>
                                <label>Email</label>
                                <Input
                                placeholder="emailaddress@example.com"
                                type="text"
                                value={username}
                                onChange={(e) => setUsername(e.target.value)}
                                required
                                />
                            </FormGroup>
                        </Col>
                    </Row>
                    <Row>
                        <Col>
                            <FormGroup>
                                <label>Password</label>
                                <Input
                                placeholder="Your Password"
                                type="password"
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                                required
                                />
                            </FormGroup>
                        </Col>
                    </Row>
                    <Row>
                        <Col>
                        <Button className="btn-fill" color="primary" type="submit" onClick={handleLogin}>Log in</Button>
                        </Col>
                    </Row>
                </Form>
            </CardBody>
        </Card>
        {loginError && <div class="alert-with-icon alert alert-danger alert-dismissible fade show" role="alert">
                            <button type="button" class="close" aria-label="Close" onClick={()=>setLoginError('')}><span aria-hidden="true">
                                ×</span></button><span class="tim-icons icon-bell-55" data-notify="icon">
                                    </span><span data-notify="message">{loginError}</span>
                        </div>}
    </div>
    );
};
  
export default Login;
 