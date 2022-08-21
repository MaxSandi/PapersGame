import { useEffect, useState } from "react";
import { Button, Col, Container, Form, ListGroup, ListGroupItem } from "react-bootstrap";
import { useLocation, useParams } from "react-router-dom";

export default function LobbyPage(props) {

    let { id } = useParams();

    const connection = props.connection;
    const currentGame = props.currentGame;

    const [players, setPlayers] = useState([]);

    useEffect(() => {

        if (currentGame) {
            setPlayers(currentGame.players);
        }

        connection?.on("RecivePlayersList", p => {
            setPlayers(p);
            console.log("RecivePlayersList");
        });
        
    }, [])

    const playersList = () => {
        const currentPlayers = players;
        const list =
            currentPlayers.map(
                (player, index) => <ListGroupItem>{player.name}</ListGroupItem>);

        return (<ListGroup>{list}</ListGroup>)
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
                    <Col sm={5}>{playersList()}</Col>
                </div>}
        </div>
    )
}