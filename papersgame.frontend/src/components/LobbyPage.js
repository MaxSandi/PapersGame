﻿import { useEffect, useState } from "react";
import { Button, Col, Row, Container, Form, ListGroup, ListGroupItem, InputGroup } from "react-bootstrap";
import { Card } from "react-bootstrap";
import { useLocation, useParams } from "react-router-dom";
import './LobbyPage.css';

export default function LobbyPage(props) {

    let { id } = useParams();

    const connection = props.connection;
    const currentGame = props.currentGame;

    const [players, setPlayers] = useState([]);

    const [characterName, setCharacterName] = useState("");
    const [isPlayerReady, setIsPlayerReady] = useState(false);

    const [isGameStarted, setIsGameStarted] = useState(false);

    useEffect(() => {

        if (currentGame) {
            setPlayers(currentGame.players);
        }

        connection?.on("RecivePlayersList", p => {
            setPlayers(p);
            console.log("GameHub - RecivePlayersList");
        });

        connection?.on("IGameStarted", game => {

            setPlayers(game.players);
            setIsGameStarted(true);

            console.log("GameHub - IGameStarted");
        });
        
    }, [])

    const playersList = () => {
        const currentPlayers = players;
        const list =
            currentPlayers.map(
                (player, index) => <ListGroupItem>{player.name} {String(player.isReady)}</ListGroupItem>);

        return (<ListGroup>{list}</ListGroup>)
    }

    const playerBoard = () => {
        const currentPlayers = players;
        const list =
            currentPlayers.map(
                (player, index) =>
                    <Col>
                        <Card>
                            <Card.Body>
                                <Card.Title>{player.name}</Card.Title>
                                <Card.Text>{player.connectionId == connection.connectionId ? " " : player.character}</Card.Text>
                            </Card.Body>
                        </Card>
                    </Col>);

        return (<Row xs={3}>{list}</Row>)
    }

    const setPlayerReady = async (characterName) => {
        try {

            await connection.invoke("SetPlayerReady", characterName);
            console.log("GameHub - SetPlayerReady");

            setIsPlayerReady(true);

        } catch (e) {
            console.log(e);
        }
    }

    const canStartGame = () => {
        try {

            var result = players.every(x => x.isReady);
            console.log("canStartGame");
            console.log(result);

            return result;

        } catch (e) {
            console.log(e);
        }
    }

    const setPlayerUnready = async () => {
        try {

            await connection.invoke("SetPlayerUnready");
            console.log("GameHub - SetPlayerUnready");

            setIsPlayerReady(false);

        } catch (e) {
            console.log(e);
        }
    }

    const startGame = async () => {
        try {

            await connection.invoke("StartGame");
            console.log("GameHub - StartGame");

        } catch (e) {
            console.log(e);
        }
    }

    return (
        <div>
            {!currentGame
                ?
                <div>
                    <h3>Id: {id}. Game does not exist!</h3>
                </div>
                :
                <div>
                    <h3>Game name: {currentGame.name}. Id {currentGame.id}</h3>
                    {!isGameStarted
                        ?
                        <Container>
                            <Row>
                                <Col>
                                    <h4>Список игроков:</h4>
                                    {playersList()}
                                </Col>

                                
                                <Col>
                                    <Row>
                                        <h2>CHAT!!!</h2>
                                    </Row>
                                    <Row className="row-class">
                                        <InputGroup className="mb-3">
                                            <Form.Label className="align-middle">Имя персонажа:</Form.Label>
                                            <Form.Control name="setCharacter" type="text" placeholder=""
                                                value={characterName} onChange={(event) => setCharacterName(event.target.value)} />
                                            {
                                                !isPlayerReady
                                                    ?
                                                    <Button variant="outline-success" className="mt-1"
                                                        onClick={() => setPlayerReady(characterName)}>
                                                        Готов
                                                    </Button>
                                                    :
                                                    <Button variant="success" className="mt-1"
                                                        onClick={() => setPlayerUnready()}>
                                                        Готов
                                                    </Button>
                                            }
                                        </InputGroup>
                                        <Button className="mt-1" disabled={!canStartGame()} onClick={() => startGame()}>
                                            Начать игру
                                        </Button>
                                    </Row>

                                </Col>
                            </Row>
                        </Container>
                        :
                        <Container>
                            <h3>Game started!!!</h3>
                            {playerBoard()}
                        </Container>
                    }
                </div>
            }
        </div>
    )
}