import React, { Component } from 'react';
import { BrowserRouter as Router, Link } from 'react-router-dom';
import "./Main.css"

export class Main extends Component {
  static displayName = Main.name;

  render() {
      return (
          <div class="btn-toolbar d-flex justify-content-center">
              <Link to="/game-create">
                  <button className="btn btn-outline-secondary btn-size">Create game</button>
              </Link>
              <Link to="/game-join">
                  <button className="btn btn-outline-secondary btn-size">Join game</button >
              </Link>
          </div>
    );
  }
}
