import { connect } from "react-redux";
import { ClientMainPage } from "./clientMainPage";

const mapStateToProps = (state) => {
    return {
      count: state
    };
  };
  const mapDispatchToProps = (dispatch) => {
    return {
      handleIncrementClick: () => dispatch({ type: 'INCREMENT' }),
      handleDecrementClick: () => dispatch({ type: 'DECREMENT' })
    }
  };

  export const ClientMainPageContainer = connect(mapStateToProps, mapDispatchToProps)(ClientMainPage);