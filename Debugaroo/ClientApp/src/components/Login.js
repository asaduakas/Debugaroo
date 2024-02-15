import React, {useState} from "react";
import { useAuth } from '../contexts/AuthContext';
import { useNavigate } from 'react-router-dom';

import {
    Button,
    Card,
    CardHeader,
    CardBody,
    CardFooter,
    CardText,
    FormGroup,
    Form,
    Input,
    Row,
    Col,
} from "reactstrap";

const Login = () => {
    const [username, setUsername] = useState('');
    const [password, setPassword] = useState('');
    const auth = useAuth();
    const navigate = useNavigate();
  
    const handleLogin = async (e) => {
        e.preventDefault();
        try{
            await auth.login(username, password);
            navigate('/admin/dashboard'); // Navigate on successful login
        } catch (error) {
            console.error('Login failed:', error);
            // Handle login failure (e.g., display an error message)
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
    </div>
    );
};
  
export default Login;
 