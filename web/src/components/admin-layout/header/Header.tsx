import { Avatar, Badge } from "@mui/material";
import { useAppSelector } from "../../../hooks/redux-custom-hooks/useAppSelector";
import { RootState } from "../../../redux/store";
import { Admin } from "../../../data/models/Admin";

const Header = () => {
  let authenticatedAdmin = useAppSelector(
    (state: RootState) => state.authenticatedAdmin.admin
  ) as Admin;
  return (
    <div className="w-full h-20 fixed bg-white flex items-center justify-between px-10 z-50 border-b-2 border-b-primary">
      <img
        src={process.env.PUBLIC_URL + "/assets/images/logo.png"}
        alt="logo"
        className="h-20"
      />
      <div className="h-10 w-2/4 bg-gray-100 border-2 border-primary flex items-center">
        <input type="search" className="w-9/12 h-full outline-none pl-3" />
        <div className="h-full w-3/12 bg-primary text-white uppercase font-raleway flex items-center justify-center font-medium cursor-pointer border-2 border-primary hover:border-gray-300 hover:bg-gray-300 hover:text-primary transition ease-in duration-200">
          Rechercher
        </div>
      </div>
      <div className="py-1 px-3 rounded-full bg-gray-100 flex items-center justify-center font-normal">
        <Badge
          overlap="circular"
          variant="dot"
          className="h-10 w-10 mr-2"
          color="secondary"
        >
          <Avatar className="font-kanit bg-primary">
            {authenticatedAdmin?.FirstName[0]}
          </Avatar>
        </Badge>
        {authenticatedAdmin?.FirstName}
      </div>
    </div>
  );
};

export default Header;
